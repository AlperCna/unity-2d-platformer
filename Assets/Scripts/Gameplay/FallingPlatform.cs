using System.Collections;
using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Ustune basinca titreyip dusen platform — sordugu soru:
    /// **"Durmadan ilerleyebilir misin?"**
    ///
    /// Digerlerinden farki: tehlikeyi OYUNCU tetikliyor. Diken kendi
    /// dongusunde calisir, bu sen basana kadar bekler. Bu yuzden "acele et"
    /// hissi veriyor - platform oyununun en guclu ritim araclarindan biri.
    ///
    /// Titreme suresi kritik: cok kisa olursa tepki veremezsin (haksiz),
    /// cok uzun olursa aciliyet kaybolur (anlamsiz).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class FallingPlatform : MonoBehaviour, IResettable
    {
        [Header("Zamanlama")]
        [Tooltip("Basildiktan sonra dusmeye kadar gecen sure. " +
                 "0,3'un altina inme - insan tepki suresinin altinda kalir.")]
        [SerializeField] private float shakeDuration = 0.45f;

        [Tooltip("Dustukten sonra kaybolana kadar gecen sure.")]
        [SerializeField] private float fallDuration = 1.2f;

        [Tooltip("Kaybolduktan sonra geri gelene kadar gecen sure. " +
                 "0 = hic geri gelmez.")]
        [SerializeField] private float respawnDelay = 2.5f;

        [Header("Titreme")]
        [Tooltip("Titreme genligi (birim).")]
        [SerializeField] private float shakeMagnitude = 0.06f;

        [SerializeField] private float shakeFrequency = 34f;

        private Rigidbody2D rb;
        private Collider2D platformCollider;
        private SpriteRenderer spriteRenderer;

        private Vector3 initialPosition;
        private bool triggered;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            platformCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            initialPosition = transform.position;
        }

        public void ResetToInitialState()
        {
            StopAllCoroutines();

            triggered = false;
            transform.position = initialPosition;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.SetVelocity(Vector2.zero);

            platformCollider.enabled = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            gameObject.SetActive(true);
        }

        private void OnCollisionEnter2D(Collision2D collision) => TryTrigger(collision);
        private void OnCollisionStay2D(Collision2D collision) => TryTrigger(collision);

        private void TryTrigger(Collision2D collision)
        {
            if (triggered || !collision.collider.CompareTag("Player")) return;

            // Sadece USTUNE basilinca. Altindan kafa vurmak veya yandan
            // surtmek tetiklememeli - oyuncu neyi tetikledigini bilmeli.
            if (collision.contactCount > 0 &&
                collision.GetContact(0).normal.y > -0.5f) return;

            triggered = true;
            StartCoroutine(ShakeThenFall());
        }

        private IEnumerator ShakeThenFall()
        {
            Vector3 basePosition = transform.position;

            float t = 0f;
            while (t < shakeDuration)
            {
                t += Time.deltaTime;

                // Titreme sona dogru SIDDETLENIYOR: "birazdan" degil
                // "simdi" diyor. Sabit genlikli titreme aciliyeti
                // iletmiyordu.
                float intensity = Mathf.Lerp(0.35f, 1f, t / shakeDuration);
                float offset = Mathf.Sin(t * shakeFrequency) * shakeMagnitude * intensity;

                transform.position = basePosition + new Vector3(offset, 0f, 0f);
                yield return null;
            }

            transform.position = basePosition;

            // Dusus: artik fizik halletsin
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.2f;

            // Collider hemen kapaniyor: ustundeki oyuncu platformla birlikte
            // dusmesin, birakilsin. Birlikte duserse oyuncu "hala uzerinde"
            // sanip ziplamaya calisir.
            platformCollider.enabled = false;

            float fade = 0f;
            while (fade < fallDuration)
            {
                fade += Time.deltaTime;

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, fade / fallDuration);
                    spriteRenderer.color = c;
                }

                yield return null;
            }

            if (respawnDelay <= 0f)
            {
                gameObject.SetActive(false);
                yield break;
            }

            // Geri gelme
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.SetVelocity(Vector2.zero);
            transform.position = initialPosition;

            if (spriteRenderer != null) spriteRenderer.enabled = false;

            yield return new WaitForSeconds(respawnDelay);

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            platformCollider.enabled = true;
            triggered = false;
        }
    }
}
