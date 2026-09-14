using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Ucan dusman — sordugu soru: **"Bunu avantaja cevirebiliyor musun?"**
    ///
    /// Digerleri sadece tehlike; bu ayni zamanda bir FIRSAT. Bosluk uzerinde
    /// gidip gelir: ustune basarsan hem olur hem seni ziplatir, yani
    /// gecilemeyecek bir bosluk gecilebilir hale gelir.
    ///
    /// Epic 06'nin "her dusman farkli soru sorsun" kuralindaki ucuncu soru:
    ///   devriye  -> ne zaman?
    ///   atici    -> nereden?
    ///   ucan     -> cesaret edebiliyor musun?
    ///
    /// Yercekimsiz ve Kinematic hareket eder; yolu konumundan bagimsiz
    /// oldugu icin havada durabiliyor.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class FlyerEnemy : EnemyBase
    {
        [Header("Yatay Gidis Gelis")]
        [Tooltip("Baslangic noktasindan saga ve sola kac birim gider. " +
                 "0 = yatay hareket yok, sadece asagi yukari suzulur.")]
        [SerializeField] private float horizontalRange = 3f;

        [SerializeField] private float horizontalSpeed = 2f;

        [Header("Dikey Suzulme")]
        [Tooltip("Asagi yukari salinim genligi. Kucuk tutun: buyudukce " +
                 "ustune basmak sansa baglanir, beceri olmaktan cikar.")]
        [SerializeField] private float verticalAmplitude = 0.6f;

        [SerializeField] private float verticalFrequency = 1.1f;

        private Vector3 origin;
        private float phase;
        private int direction = 1;

        protected override void OnAwake()
        {
            origin = transform.position;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
            }

            // Her ucan dusman ayni anda ayni noktada olmasin diye baslangic
            // fazi konumdan turetiliyor. Rastgele DEGIL: rastgele olsaydi
            // ayni bolum her oynanista baska turlu olurdu ve "burada
            // ziplayamiyorum" sikayeti tekrar uretilemezdi.
            phase = (origin.x * 0.7f + origin.y * 1.3f) % (Mathf.PI * 2f);
        }

        protected override void OnReset()
        {
            direction = 1;
            transform.position = origin;
        }

        private void FixedUpdate()
        {
            if (IsDead) return;

            float t = Time.time;
            Vector3 position = transform.position;

            // Yatay: baslangic noktasi etrafinda gidip gelme
            if (horizontalRange > 0f)
            {
                position.x += direction * horizontalSpeed * Time.fixedDeltaTime;

                if (position.x > origin.x + horizontalRange)
                {
                    position.x = origin.x + horizontalRange;
                    direction = -1;
                }
                else if (position.x < origin.x - horizontalRange)
                {
                    position.x = origin.x - horizontalRange;
                    direction = 1;
                }
            }

            // Dikey: salinim. Konumu birikmeli degistirmiyoruz, her karede
            // baslangica gore MUTLAK hesapliyoruz - yoksa kucuk hatalar
            // birikip dusman zamanla asagi kayar.
            position.y = origin.y + Mathf.Sin(t * verticalFrequency * Mathf.PI * 2f + phase)
                                    * verticalAmplitude;

            if (rb != null) rb.MovePosition(position);
            else transform.position = position;

            if (spriteRenderer != null) spriteRenderer.flipX = direction < 0;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? origin : transform.position;

            Gizmos.color = new Color(0.6f, 0.4f, 0.9f, 0.8f);
            Gizmos.DrawLine(center + Vector3.left * horizontalRange,
                            center + Vector3.right * horizontalRange);

            // Dikey salinim bandi
            Gizmos.color = new Color(0.6f, 0.4f, 0.9f, 0.35f);
            Gizmos.DrawWireCube(center,
                new Vector3(horizontalRange * 2f, verticalAmplitude * 2f, 0f));
        }
    }
}
