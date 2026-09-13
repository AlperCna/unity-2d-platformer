using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Toplanabilir para. Trigger'a giren "Player" tag'li nesne icin skor ekler.
    /// Collider2D'nin "Is Trigger" secili olmasi gerekir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Coin : MonoBehaviour
    {
        [Header("Deger")]
        [SerializeField] private int scoreValue = 1;

        [Header("Gorsel Hareket")]
        [Tooltip("Yukari asagi salinim mesafesi.")]
        [SerializeField] private float bobAmplitude = 0.18f;

        [Tooltip("Salinim hizi.")]
        [SerializeField] private float bobSpeed = 2.5f;

        [Tooltip("Kendi ekseninde donme hizi (derece/saniye).")]
        [SerializeField] private float spinSpeed = 120f;

        [Header("Toplanma Efekti")]
        [SerializeField] private float popDuration = 0.18f;
        [SerializeField] private float popScale = 1.6f;

        private Vector3 startPosition;
        private float phaseOffset;
        private bool collected;
        private Transform visual;

        private void Awake()
        {
            startPosition = transform.position;

            // Her para farkli fazda salinsin - hepsi ayni anda hareket etmesin
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);

            // Gorseli dondururken collider'i dondurmemek icin cocuk sprite kullaniyoruz
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            visual = sr != null ? sr.transform : transform;
        }

        private void Update()
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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
            }

            // Collider'i hemen kapat ki iki kez sayilmasin
            GetComponent<Collider2D>().enabled = false;

            StartCoroutine(PopAndDisappear());
        }

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

            Destroy(gameObject);
        }
    }
}
