using System;
using System.Collections.Generic;

namespace Platformer.Core
{
    /// <summary>Tek bir bolumun ilerlemesi.</summary>
    [Serializable]
    public class LevelProgress
    {
        public int levelIndex;
        public bool completed;

        public int coinsCollected;
        public int totalCoins;

        public int gemsCollected;
        public int totalGems;

        public bool secretFound;

        /// <summary>En iyi sure. -1 = hic bitirilmedi.</summary>
        public float bestTime = -1f;

        /// <summary>Bu bolumde toplam kac kez olundu (tum denemeler).</summary>
        public int deathCount;

        /// <summary>
        /// Bolumun TASARIM surumu. Bolum yeniden tasarlaninca artar.
        ///
        /// Neden gerekli: Bolum 1 baştan tasarlandi (83 birim -> 124 birim)
        /// ama eski rekor (19,58 sn) kayitta kaldi. Yeni bolum daha uzun
        /// oldugu icin hicbir kosu o rekoru kiramaz - yani rekor sonsuza
        /// kadar ULASILAMAZ ve YANLIS bir sayi olarak durur.
        ///
        /// Tasarim degisince rekorlar sifirlanir; sure ve para sayisi
        /// ancak ayni bolume aitse karsilastirilabilir.
        /// </summary>
        public int designVersion;
    }

    /// <summary>
    /// Diske yazilan her sey.
    ///
    /// JsonUtility ile serilestirilir, o yuzden:
    ///   - Alanlar PUBLIC olmali (property degil)
    ///   - Dictionary desteklenmez, List kullanilir
    ///   - null List'ler bos olarak geri gelir, sorun degil
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Kayit formati degisirse bu artar. ASLA SILME.
        ///
        /// Ileride alan ekleyip cikardiginda eski kayitlari okuyabilmek
        /// icin tek dayanagin bu. Versiyonsuz kayit formati degistiginde
        /// oyuncularin ilerlemesi sessizce silinir.
        /// </summary>
        public int version = 1;

        // --- Ilerleme ---
        public int lastUnlockedLevel;
        public List<LevelProgress> levels = new List<LevelProgress>();

        // --- Ayarlar (Epic 15'te menuye baglanacak) ---
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1f;
        public bool fullscreen = true;
        public bool screenShake = true;

        // --- Istatistik ---
        public int totalDeaths;
        public float totalPlayTime;

        /// <summary>Bolum kaydini bul, yoksa olustur.</summary>
        public LevelProgress GetLevel(int index)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelIndex == index) return levels[i];
            }

            var created = new LevelProgress { levelIndex = index };
            levels.Add(created);
            return created;
        }
    }
}
