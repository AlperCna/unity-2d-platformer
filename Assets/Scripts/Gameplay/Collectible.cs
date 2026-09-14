using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Toplanabilirlerin ortak davranisi: salinim, donme, toplanma efekti.
    ///
    /// Epic 08'in temel fikri: toplanabilir IKI is yapar ve ikincisi daha
    /// onemlidir.
    ///   1. Odul     — toplamak tatmin edici
    ///   2. YONLENDIRME — nereye gidecegini soyler, tek kelime yazmadan
    ///
    /// Bir para dizisi zipla yayini cizdiginde oyuncu farkinda olmadan
    /// dogru rotayi izler. Oyun tasariminin en zarif araci ve bedava.
    ///
    /// Turetilen siniflar sadece "kac puan" ve "hangi sayaca" sorularini
    /// cevaplar. Gorsel davranis burada, bir kez.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class Collectible : MonoBehaviour
    {
        [Header("Gorsel Hareket")]
        [Tooltip("Yukari asagi salinim mesafesi.")]
        [SerializeField] protected float bobAmplitude = 0.18f;

        [Tooltip("Salinim hizi.")]
        [SerializeField] protected float bobSpeed = 2.5f;

        [Tooltip("Kendi ekseninde donme hizi (derece/saniye).")]
        [SerializeField] protected float spinSpeed = 120f;

        [Header("Toplanma Efekti")]
        [SerializeField] protected float popDuration = 0.18f;
        [SerializeField] protected float popScale = 1.6f;

        protected Vector3 startPosition;
        protected Transform visual;
        private float phaseOffset;
        private bool collected;

        protected virtual void Awake()
        {
            startPosition = transform.position;

            // Her toplanabilir farkli fazda salinsin - hepsi ayni anda
            // hareket ederse "canli" degil "mekanik" gorunur.
            //
            // Konumdan turetiliyor, Random'dan DEGIL: rastgele olsaydi ayni
            // bolum her acilista baska turlu gorunurdu ve bir hatayi tekrar
            // uretmek imkansizlasirdi.
            phaseOffset = (startPosition.x * 0.7f + startPosition.y * 1.3f) % (Mathf.PI * 2f);

            // Gorseli dondururken collider'i dondurmemek icin cocuk sprite
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            visual = sr != null ? sr.transform : transform;
        }

        protected virtual void Update()
        {
            if (collected) return;

            float y = startPosition.y + Mathf.Sin(Time.time * bobSpeed + phaseOffset) * bobAmplitude;
            transform.position = new Vector3(startPosition.x, y, startPosition.z);

            if (visual != null)
            {
                visual.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.Self);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || !other.CompareTag("Player")) return;

            collected = true;

            // Collider'i hemen kapat ki iki kez sayilmasin
            GetComponent<Collider2D>().enabled = false;

            OnCollected();
            StartCoroutine(PopAndDisappear());
        }

        /// <summary>Turetilen sinif sayaci burada artirir.</summary>
        protected abstract void OnCollected();

        private System.Collections.IEnumerator PopAndDisappear()
        {
            Vector3 baseScale = transform.localScale;
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            Color baseColor = sr != null ? sr.color : Color.white;

            float t = 0f;
            while (t < popDuration)
            {
                t += Time.deltaTime;
                float k = t / popDuration;

                // Buyurken saydamlas - klasik "toplandi" efekti
                transform.localScale = baseScale * Mathf.Lerp(1f, popScale, k);
                transform.position += Vector3.up * (2.5f * Time.deltaTime);

                if (sr != null)
                {
                    baseColor.a = Mathf.Lerp(1f, 0f, k);
                    sr.color = baseColor;
                }

                yield return null;
            }

            // Destroy, SetActive(false) DEGIL.
            //
            // Dusmanlar respawn'da geri geliyor ama toplananlar GELMIYOR -
            // bu bilincli: ayni parayi yirmi kez toplamak iskence olurdu
            // (Epic 10'un tuzak listesi). O yuzden Coin/Gem IResettable
            // uygulamiyor ve yok edilebiliyor.
            Destroy(gameObject);
        }
    }
}
