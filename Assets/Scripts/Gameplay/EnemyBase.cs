using UnityEngine;
using Platformer.Core;
using Platformer.Player;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Butun dusmanlarin ortak davranisi: oyuncuyla temas, ezilme, olum,
    /// respawn'da sifirlanma.
    ///
    /// Turetilen siniflar SADECE hareketi yazar. Bir dusman cesidi eklerken
    /// yazman gereken tek sey "bu nasil hareket eder" - temas kurallari,
    /// olum efekti ve checkpoint sifirlamasi burada, bir kez.
    ///
    /// NEDEN ONEMLI: stomp kurali (ne zaman ezme sayilir) uc ayri dusmanda
    /// uc ayri sekilde yazilirsa oyuncu tutarsizlik hisseder ama nedenini
    /// soyleyemez - "bazen ezebiliyorum bazen oluyorum" der. Tek yerde olmasi
    /// bunu imkansiz kiliyor.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour, IResettable
    {
        [Header("Ustune Basilma (Stomp)")]
        [Tooltip("Ustune basilinca oluyor mu? Dikenli dusmanlarda kapat.")]
        [SerializeField] protected bool canBeStomped = true;

        [Tooltip("Oyuncu bu yukseklik farkindan fazla yukaridaysa stomp sayilir.")]
        [SerializeField] protected float stompHeightThreshold = 0.25f;

        [Tooltip("Stomp sonrasi oyuncunun ziplama yuksekligi.")]
        [SerializeField] protected float stompBounceHeight = 2.6f;

        [SerializeField] protected int stompScoreReward = 2;

        [Header("Olum")]
        [Tooltip("Ezilme animasyonunun suresi.")]
        [SerializeField] protected float deathDuration = 0.25f;

        public bool IsDead { get; protected set; }

        protected Rigidbody2D rb;                 // ucan dusmanda olmayabilir
        protected Collider2D bodyCollider;
        protected SpriteRenderer spriteRenderer;

        // Respawn'da geri donulecek baslangic durumu
        private Vector3 initialPosition;
        private Vector3 initialScale;
        private Color initialColor = Color.white;

        /// <summary>
        /// Baslangictaki govde tipi. Sabit "Dynamic" YAZILAMAZ: ucan dusman
        /// Kinematic ve yercekimsiz calisiyor; sifirlama onu Dynamic yapsaydi
        /// checkpoint'ten sonra yere duserdi.
        /// </summary>
        private RigidbodyType2D initialBodyType = RigidbodyType2D.Dynamic;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (rb != null)
            {
                rb.freezeRotation = true;
                initialBodyType = rb.bodyType;
            }

            initialPosition = transform.position;
            initialScale = transform.localScale;
            if (spriteRenderer != null) initialColor = spriteRenderer.color;

            OnAwake();
        }

        /// <summary>Turetilen sinif kendi kurulumunu buraya yazar.</summary>
        protected virtual void OnAwake() { }

        // ---------------------------------------------------------------
        // Sifirlama (IResettable)
        // ---------------------------------------------------------------

        /// <summary>
        /// Respawn'da baslangic durumuna doner.
        ///
        /// Olurken Destroy degil SetActive(false) kullaniliyor; Destroy
        /// edilseydi burada geri getirilecek nesne kalmazdi ve GameManager'in
        /// onbellekteki referansi olu kalirdi.
        /// </summary>
        public void ResetToInitialState()
        {
            // Devam eden ezilme animasyonunu durdur, yoksa sifirladigimiz
            // olcegi ve rengi tekrar bozar
            StopAllCoroutines();

            IsDead = false;

            transform.position = initialPosition;
            transform.localScale = initialScale;

            if (bodyCollider != null) bodyCollider.enabled = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = initialColor;
                spriteRenderer.enabled = true;
            }

            if (rb != null)
            {
                rb.bodyType = initialBodyType;
                rb.SetVelocity(Vector2.zero);
            }

            gameObject.SetActive(true);
            OnReset();
        }

        /// <summary>Turetilen sinif kendi durumunu buraya sifirlar.</summary>
        protected virtual void OnReset() { }

        // ---------------------------------------------------------------
        // Oyuncuyla temas
        // ---------------------------------------------------------------
        //
        // Dort giris noktasi var cunku dusmanlar farkli collider tipleri
        // kullaniyor: devriye kati govdeli (Collision), ucan dusman trigger
        // olabilir. Stay de dinleniyor cunku oyuncu dusmanin ustune
        // "oturursa" Enter bir daha tetiklenmez.

        private void OnCollisionEnter2D(Collision2D c) => HandlePlayerContact(c.collider);
        private void OnCollisionStay2D(Collision2D c) => HandlePlayerContact(c.collider);
        private void OnTriggerEnter2D(Collider2D c) => HandlePlayerContact(c);
        private void OnTriggerStay2D(Collider2D c) => HandlePlayerContact(c);

        protected void HandlePlayerContact(Collider2D other)
        {
            if (IsDead || other == null || !other.CompareTag("Player")) return;

            var health = other.GetComponent<PlayerHealth>();
            if (health == null || health.IsDead) return;

            var controller = other.GetComponent<PlayerController2D>();

            if (canBeStomped && IsStomp(other, controller))
            {
                Stomped(controller);
            }
            else
            {
                health.Kill(gameObject);
            }
        }

        /// <summary>
        /// Ezme mi, carpma mi?
        ///
        /// Iki sart birden: oyuncu DUSUYOR olmali ve dusmanin merkezinden
        /// belirgin sekilde YUKARIDA olmali. Sadece "yukarida" yetmez -
        /// yandan zipla ile gelen oyuncu bir an yukarida gorunur.
        /// </summary>
        protected virtual bool IsStomp(Collider2D other, PlayerController2D controller)
        {
            // 1) AYAKLAR DUSMANIN USTUNDE MI?
            //
            // Bu kontrol sonradan eklendi ve sebebi soyleydi: ucan dusmanin
            // ustune basinca dusman olmuyor, OYUNCU oluyordu.
            //
            // Sebep asagidaki hiz kontrolu. Ucan dusman yukari asagi
            // suzuluyor; yukari cikarken oyuncuya carpip onu ITIYOR, yani
            // oyuncunun dikey hizi POZITIF oluyor. "Dusmuyor" sayiliyor ve
            // yandan carpma muamelesi goruyordu.
            //
            // Oysa ayaklarin dusmanin tepesindeyse, nasil geldigin onemli
            // degil - ustune basmissin demektir.
            float enemyTop = bodyCollider.bounds.max.y;
            if (other.bounds.min.y >= enemyTop - stompHeightThreshold) return true;

            // 2) Klasik kontrol: dusuyor ve belirgin sekilde yukarida
            bool falling = controller != null && controller.Velocity.y <= 0.01f;
            bool above = other.bounds.min.y > bodyCollider.bounds.center.y + stompHeightThreshold;
            return falling && above;
        }

        private void Stomped(PlayerController2D controller)
        {
            if (controller != null) controller.LaunchUpward(stompBounceHeight);

            if (GameManager.Instance != null && stompScoreReward > 0)
            {
                GameManager.Instance.AddScore(stompScoreReward);
            }

            Die();
        }

        /// <summary>
        /// Dusmani oldurur. Ezilme disindaki sebepler (ornegin ileride bir
        /// silah) de bunu cagirabilir.
        /// </summary>
        public virtual void Die()
        {
            if (IsDead) return;
            IsDead = true;

            // Artik kimseye zarar vermesin ve fizigi etkilemesin
            if (bodyCollider != null) bodyCollider.enabled = false;

            if (rb != null)
            {
                rb.SetVelocity(Vector2.zero);
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            OnDeath();
            StartCoroutine(SquashAndDisappear());
        }

        /// <summary>Turetilen sinif olum aninda ek is yapacaksa buraya.</summary>
        protected virtual void OnDeath() { }

        private System.Collections.IEnumerator SquashAndDisappear()
        {
            Vector3 baseScale = transform.localScale;
            float t = 0f;

            while (t < deathDuration)
            {
                t += Time.deltaTime;
                float k = t / deathDuration;

                // Yassilasip saydamlassin
                transform.localScale = new Vector3(
                    baseScale.x * Mathf.Lerp(1f, 1.25f, k),
                    baseScale.y * Mathf.Lerp(1f, 0.1f, k),
                    baseScale.z);

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, k);
                    spriteRenderer.color = c;
                }

                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
