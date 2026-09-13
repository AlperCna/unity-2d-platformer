using UnityEngine;
using Platformer.Core;

namespace Platformer.Player
{
    /// <summary>
    /// 2D platform karakteri. Iyi bir platformer hissiyatinin bilesenleri burada:
    /// - Ivmeli hareket (aniden durmaz ama kaygan da degil)
    /// - Coyote time: platformdan dustukten sonra kisa bir sure daha ziplayabilirsin
    /// - Jump buffer: yere degmeden hemen once bastigin zipla tusu hafizada tutulur
    /// - Degisken zipla yuksekligi: tusu birakinca zipla kesilir
    /// - Ayri dusme yercekimi: dususte daha agir hissettirir
    /// Bu degerleri Inspector'da oynayarak oyunun karakterini belirlersin.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Yatay Hareket")]
        [Tooltip("Saniyede kac birim (Unity metre) hiz.")]
        [SerializeField] private float moveSpeed = 8f;

        [Tooltip("Hedef hiza ulasma suresi. Kucuk = daha keskin kontrol.")]
        [SerializeField] private float accelerationTime = 0.08f;

        [Tooltip("Durma suresi. Kucuk = aniden durur, buyuk = kayar.")]
        [SerializeField] private float decelerationTime = 0.06f;

        [Tooltip("Havadayken kontrolun ne kadari korunur. 1 = tam kontrol.")]
        [Range(0.05f, 1f)]
        [SerializeField] private float airControl = 0.75f;

        [Header("Zipla")]
        [Tooltip("Ziplamanin tepe noktasi (birim cinsinden yukseklik).")]
        [SerializeField] private float jumpHeight = 3.2f;

        [Tooltip("Tepe noktasina ulasma suresi. Kucuk = hizli, sivri zipla.")]
        [SerializeField] private float jumpApexTime = 0.38f;

        [Tooltip("Dususte yercekimi bu kadar katlanir. 1'den buyuk = daha agir dusus.")]
        [SerializeField] private float fallGravityMultiplier = 1.9f;

        [Tooltip("Tus erken birakilirsa yukari hiz bu oranda kesilir.")]
        [Range(0f, 1f)]
        [SerializeField] private float jumpCutMultiplier = 0.45f;

        [Tooltip("Maksimum dusme hizi (terminal velocity).")]
        [SerializeField] private float maxFallSpeed = 22f;

        [Tooltip("Havada kac kez daha ziplanabilir. 0 = cift zipla yok.")]
        [SerializeField] private int extraJumps = 0;

        [Header("Toleranslar")]
        [Tooltip("Zeminden ayrildiktan sonra hala ziplayabilme suresi.")]
        [SerializeField] private float coyoteTime = 0.1f;

        [Tooltip("Yere inmeden once basilan zipla tusunun hafizada kalma suresi.")]
        [SerializeField] private float jumpBufferTime = 0.12f;

        [Header("Zemin Algilama")]
        [Tooltip("Hangi layer'lar zemin sayilir. Player layer'ini burada ISARETLEME.")]
        [SerializeField] private LayerMask groundLayers = ~0;

        [Tooltip("Ayak altindaki kontrol kutusunun boyutu.")]
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.72f, 0.12f);

        [Tooltip("Kutunun karakter merkezine gore konumu.")]
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.52f);

        // --- Bilesenler ---
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        // --- Durum (baska scriptler okuyabilsin diye public) ---
        public bool IsGrounded { get; private set; }
        public bool IsJumping { get; private set; }
        public float HorizontalInput { get; private set; }
        public bool FacingRight { get; private set; } = true;
        public Vector2 Velocity => rb != null ? rb.GetVelocity() : Vector2.zero;

        // Ses, parcacik, animasyon gibi sistemler buraya baglanabilir
        public System.Action OnJumped;
        public System.Action OnLanded;

        // --- Dahili sayaclar ---
        private float coyoteCounter;
        private float jumpBufferCounter;
        private int jumpsLeft;
        private bool wasGroundedLastFrame;
        private bool frozen;
        private float velocityXSmoothing;

        // jumpHeight ve jumpApexTime'dan turetilen fizik degerleri
        private float gravity;
        private float jumpVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            // Platformer icin dogru Rigidbody ayarlari
            rb.freezeRotation = true;
            rb.gravityScale = 0f;   // yercekimini asagida elle uyguluyoruz
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            RecalculateJumpPhysics();
        }

        private void OnValidate()
        {
            // Inspector'da deger degistirince fizik hemen guncellensin
            RecalculateJumpPhysics();
        }

        /// <summary>
        /// Istenen zipla yuksekligi ve tepe suresinden gereken yercekimini ve
        /// zipla hizini hesaplar. Boylece "3 birim yuksege 0.38 saniyede cik"
        /// diyebiliyoruz; sihirli sayilarla ugrasmiyoruz.
        /// h = (g * t^2) / 2  ->  g = 2h / t^2,  v = g * t
        /// </summary>
        private void RecalculateJumpPhysics()
        {
            if (jumpApexTime < 0.01f) jumpApexTime = 0.01f;
            gravity = (2f * jumpHeight) / (jumpApexTime * jumpApexTime);
            jumpVelocity = gravity * jumpApexTime;
        }

        private void Update()
        {
            if (frozen) return;

            ReadInput();
            UpdateTimers();
        }

        private void FixedUpdate()
        {
            if (frozen) return;

            CheckGround();
            ApplyHorizontalMovement();
            HandleJump();
            ApplyGravity();
        }

        private void ReadInput()
        {
            // Eski Input Manager kullaniyoruz (Active Input Handling = Both veya Old).
            // "Horizontal" ve "Jump" Unity'nin hazir tanimlari: A/D, sol/sag ok, Space.
            HorizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetButtonDown("Jump"))
            {
                jumpBufferCounter = jumpBufferTime;
            }

            // Tus erken birakildiysa ziplamayi kes -> kisa zipla
            if (Input.GetButtonUp("Jump") && IsJumping && rb.GetVelocity().y > 0f)
            {
                rb.SetVelocityY(rb.GetVelocity().y * jumpCutMultiplier);
                IsJumping = false;
            }

            UpdateFacing();
        }

        private void UpdateTimers()
        {
            if (IsGrounded)
            {
                coyoteCounter = coyoteTime;
            }
            else
            {
                coyoteCounter -= Time.deltaTime;
            }

            jumpBufferCounter -= Time.deltaTime;
        }

        private void CheckGround()
        {
            Vector2 origin = (Vector2)transform.position + groundCheckOffset;
            IsGrounded = Physics2D.OverlapBox(origin, groundCheckSize, 0f, groundLayers) != null;

            // Bu karede yeni mi yere indik?
            if (IsGrounded && !wasGroundedLastFrame)
            {
                jumpsLeft = extraJumps;
                IsJumping = false;
                OnLanded?.Invoke();
            }

            wasGroundedLastFrame = IsGrounded;
        }

        private void ApplyHorizontalMovement()
        {
            float targetSpeed = HorizontalInput * moveSpeed;

            // Giris varsa hizlanma suresi, yoksa yavaslama suresi kullanilir
            float smoothTime = Mathf.Abs(HorizontalInput) > 0.01f ? accelerationTime : decelerationTime;

            // Havada kontrol azalir -> zipladiktan sonra yon degistirmek zorlasir
            if (!IsGrounded)
            {
                smoothTime /= airControl;
            }

            float newX = Mathf.SmoothDamp(
                rb.GetVelocity().x,
                targetSpeed,
                ref velocityXSmoothing,
                smoothTime,
                Mathf.Infinity,
                Time.fixedDeltaTime);

            rb.SetVelocityX(newX);
        }

        private void HandleJump()
        {
            bool wantsToJump = jumpBufferCounter > 0f;
            if (!wantsToJump) return;

            bool canGroundJump = coyoteCounter > 0f;
            bool canAirJump = !IsGrounded && jumpsLeft > 0;
            if (!canGroundJump && !canAirJump) return;

            if (!canGroundJump) jumpsLeft--;

            rb.SetVelocityY(jumpVelocity);
            IsJumping = true;

            // Sayaclari sifirla ki tek basista iki zipla harcanmasin
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;

            OnJumped?.Invoke();
        }

        private void ApplyGravity()
        {
            // Yerdeyken asagi hiz biriktirmiyoruz. Bunun yerine kucuk sabit bir
            // asagi hiz veriyoruz: karakter zemine yapisik kalir, egimlerden ve
            // hareketli platformlardan kopmaz.
            if (IsGrounded && rb.GetVelocity().y <= 0f)
            {
                rb.SetVelocityY(-1f);
                return;
            }

            // Dususte daha agir yercekimi -> zipla "yapiskan" hissetmez
            float currentGravity = rb.GetVelocity().y < 0f ? gravity * fallGravityMultiplier : gravity;

            float newY = rb.GetVelocity().y - currentGravity * Time.fixedDeltaTime;
            newY = Mathf.Max(newY, -maxFallSpeed);

            rb.SetVelocityY(newY);
        }

        private void UpdateFacing()
        {
            if (Mathf.Abs(HorizontalInput) < 0.01f) return;

            FacingRight = HorizontalInput > 0f;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !FacingRight;
            }
        }

        /// <summary>Olum / ara sahne aninda kontrolu kes ve karakteri durdur.</summary>
        public void Freeze()
        {
            frozen = true;
            HorizontalInput = 0f;
            if (rb != null) rb.SetVelocity(Vector2.zero);
        }

        /// <summary>Kontrolu geri ver (respawn sonrasi).</summary>
        public void Unfreeze()
        {
            frozen = false;
            velocityXSmoothing = 0f;
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            IsJumping = false;
        }

        /// <summary>Zipla pedi / yay gibi disaridan firlatma icin.</summary>
        public void LaunchUpward(float height)
        {
            rb.SetVelocityY(Mathf.Sqrt(2f * gravity * Mathf.Max(height, 0.01f)));
            IsJumping = true;
            jumpsLeft = extraJumps;
        }

        private void OnDrawGizmosSelected()
        {
            // Scene view'da zemin kontrol kutusunu goster - ayarlamak cok kolaylasir
            Gizmos.color = Application.isPlaying && IsGrounded ? Color.green : Color.red;
            Vector2 origin = (Vector2)transform.position + groundCheckOffset;
            Gizmos.DrawWireCube(origin, groundCheckSize);
        }
    }
}
