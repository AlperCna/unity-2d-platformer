using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Basit nesne havuzu. Instantiate/Destroy yerine ayni nesneleri
    /// yeniden kullanir.
    ///
    /// NEDEN: bir mermi atan dusman saniyede 1 mermi atsa, 60 saniyelik
    /// bolumde 60 Instantiate + 60 Destroy olur. Her Destroy cop birakir;
    /// cop toplayici devreye girdiginde oyun bir kare takilir. Platform
    /// oyununda tek kare takilma, kacirilmis bir zipla demek.
    ///
    /// Havuz bunu sifira indiriyor: nesneler bir kez yaratilir, sonra
    /// aktif/pasif arasinda gidip gelir.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly bool canGrow;

        /// <summary>Su an bosta bekleyenler.</summary>
        private readonly Queue<T> available = new Queue<T>();

        /// <summary>Havuzun urettigi her sey - ReturnAll icin.</summary>
        private readonly List<T> all = new List<T>();

        /// <summary>
        /// Su an disarida olanlar.
        ///
        /// Ayni nesnenin IKI KEZ iade edilmesini engelliyor. Iki kez iade
        /// edilseydi kuyruga iki kez girerdi ve iki farkli cagiran AYNI
        /// nesneyi alirdi - biri digerinin mermisini ucururdu. Bulunmasi
        /// cok zor bir hata olurdu.
        /// </summary>
        private readonly HashSet<T> inUse = new HashSet<T>();

        public int CountAll => all.Count;
        public int CountActive => inUse.Count;

        public ObjectPool(T prefab, int initialSize, Transform parent = null,
                          bool canGrow = true)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.canGrow = canGrow;

            for (int i = 0; i < initialSize; i++)
            {
                Create();
            }
        }

        private T Create()
        {
            T item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            all.Add(item);
            available.Enqueue(item);
            return item;
        }

        /// <summary>
        /// Havuzdan bir nesne alir ve konumlandirir.
        /// Havuz bos ve buyuyemiyorsa null doner.
        /// </summary>
        public T Get(Vector2 position, Quaternion rotation)
        {
            T item = null;

            // Sahne degisiminde havuzdaki nesneler yok edilmis olabilir;
            // Unity'nin null'i ozel oldugu icin == null ile kontrol sart.
            while (available.Count > 0 && item == null)
            {
                item = available.Dequeue();
            }

            if (item == null)
            {
                if (!canGrow) return null;
                item = Create();
                available.Dequeue();        // Create kuyruga ekledi, geri al
            }

            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            inUse.Add(item);
            return item;
        }

        /// <summary>Nesneyi havuza geri verir. Iki kez cagirmak zararsiz.</summary>
        public void Return(T item)
        {
            if (item == null) return;
            if (!inUse.Remove(item)) return;     // zaten havuzdaydi

            item.gameObject.SetActive(false);
            available.Enqueue(item);
        }

        /// <summary>
        /// Bolum sifirlanirken tumunu geri al.
        ///
        /// Checkpoint'e donuldugunde havada asili kalan mermiler
        /// temizlenmeli; yoksa oyuncu dogar dogmaz eski bir mermiye carpar.
        /// </summary>
        public void ReturnAll()
        {
            for (int i = all.Count - 1; i >= 0; i--)
            {
                T item = all[i];

                if (item == null)
                {
                    all.RemoveAt(i);            // sahne degisiminde yok olmus
                    continue;
                }

                Return(item);
            }
        }
    }
}
