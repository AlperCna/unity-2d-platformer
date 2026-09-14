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

        [Header("Dash (imza mekanigi)")]
        [Tooltip("Dash sirasindaki hiz. moveSpeed'in ~2 kati iyi bir baslangic.")]
        [SerializeField] private float dashSpeed = 18f;

        [Tooltip("Dash ne kadar surer. Kisa = keskin, uzun = akici.")]
        [SerializeField] private float dashDuration = 0.16f;

        [Tooltip("Iki dash arasi zorunlu bekleme.")]
        [SerializeField] private float dashCooldown = 0.35f;

        [Tooltip("Dash bitince hiz bu orana dusurulur. 1 = hiz korunur (firlamis hissi).")]
        [Range(0.2f, 1f)]
        [SerializeField] private float dashEndSpeedMultiplier = 0.55f;

        [Tooltip("Acik: 8 yonlu dash (yukari dash erisilebilir yuksekligi IKIYE KATLAR, " +
                 "her platform yuksekligini iki kez hesaplaman gerekir). " +
                 "Kapali: sadece yatay - yukseklik yalnizca ziplamayla belirlenir.")]
        [SerializeField] private bool allowDiagonalDash = false;

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

        public bool IsDashing { get; private set; }

        /// <summary>Dash hakki var mi? UI/gorsel geri bildirim icin.</summary>
        public bool DashReady => dashAvailable && dashCooldownLeft <= 0f;

        // Ses, parcacik, animasyon gibi sistemler buraya baglanabilir
        public System.Action OnJumped;
        public System.Action OnLanded;
        public System.Action OnDashed;

        // --- Dahili sayaclar ---
        private float coyoteCounter;
        private float jumpBufferCounter;
        private int jumpsLeft;
        private bool wasGroundedLastFrame;
        private bool frozen;
        private float velocityXSmoothing;

        // --- Dash durumu ---
        private float dashTimeLeft;
        private float dashCooldownLeft;
        private bool dashAvailable = true;
        private Vector2 dashDirection;

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

        // ---------------------------------------------------------------
        // Ayarlari disaridan okuma/yazma
        // Hazir ayar karsilastirmasi ve testler icin (Epic 02, Gorev 2).
        // ---------------------------------------------------------------

        /// <summary>Hareket hissini belirleyen tum degerler tek pakette.</summary>
        [System.Serializable]
        public struct MovementSettings
        {
            public float moveSpeed;
            public float accelerationTime;
            public float decelerationTime;
            public float airControl;
            public float jumpHeight;
            public float jumpApexTime;
            public float fallGravityMultiplier;
            public float jumpCutMultiplier;
            public float coyoteTime;
            public float jumpBufferTime;

            // --- Turetilen olculer (sadece okunur, gosterim icin) ---

            /// <summary>Hesaplanan yercekimi: g = 2h / t^2</summary>
            public float Gravity => (2f * jumpHeight) / (jumpApexTime * jumpApexTime);

            /// <summary>Zipla aninda verilen dikey hiz.</summary>
            public float JumpVelocity => Gravity * jumpApexTime;

            /// <summary>
            /// Oyunda GERCEKTEN ulasilan yukseklik.
            ///
            /// jumpHeight surekli matematigin ideali; fizik ise saniyede 50 ayrik
            /// adimla calisiyor (yari-ortuk Euler). Her adimda once hiz azaliyor,
            /// sonra konum degisiyor - bu, tepe noktasini (v0 * dt) / 2 kadar dusuruyor.
            ///
            /// Bolum tasariminda CETVEL OLARAK BUNU KULLAN, jumpHeight'i degil.
            /// Olculdu ve dogrulandi: 3.20 istenen -> 3.03 gercek.
            /// </summary>
            public float RealJumpHeight =>
                jumpHeight - (JumpVelocity * Time.fixedDeltaTime) * 0.5f;

            /// <summary>Tepeden yere dusme suresi (agir yercekimiyle).</summary>
            public float FallTime => jumpApexTime / Mathf.Sqrt(Mathf.Max(fallGravityMultiplier, 0.01f));

            /// <summary>Toplam havada kalma suresi.</summary>
            public float AirTime => jumpApexTime + FallTime;

            /// <summary>Tam hizda kosarak zipladiginda kat edilen yatay mesafe.</summary>
            public float JumpDistance => moveSpeed * AirTime;
        }

        /// <summary>Su anki ayarlari okur.</summary>
        public MovementSettings GetSettings() => new MovementSettings
        {
            moveSpeed = moveSpeed,
            accelerationTime = accelerationTime,
            decelerationTime = decelerationTime,
            airControl = airControl,
            jumpHeight = jumpHeight,
            jumpApexTime = jumpApexTime,
            fallGravityMultiplier = fallGravityMultiplier,
            jumpCutMultiplier = jumpCutMultiplier,
            coyoteTime = coyoteTime,
            jumpBufferTime = jumpBufferTime
        };

        /// <summary>Ayarlari toptan uygular ve fizigi yeniden hesaplar.</summary>
        public void ApplySettings(MovementSettings s)
        {
            moveSpeed = s.moveSpeed;
            accelerationTime = s.accelerationTime;
            decelerationTime = s.decelerationTime;
            airControl = s.airControl;
            jumpHeight = s.jumpHeight;
            jumpApexTime = s.jumpApexTime;
            fallGravityMultiplier = s.fallGravityMultiplier;
            jumpCutMultiplier = s.jumpCutMultiplier;
            coyoteTime = s.coyoteTime;
            jumpBufferTime = s.jumpBufferTime;

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

            // Dash sirasinda normal hareket ve yercekimi devre disi:
            // karakter duz bir cizgide, sabit hizla gider.
            if (IsDashing)
            {
                UpdateDash();
                return;
            }

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

            // Dash: Shift veya sag fare
            bool dashPressed = Input.GetKeyDown(KeyCode.LeftShift)
                               || Input.GetKeyDown(KeyCode.RightShift)
                               || Input.GetKeyDown(KeyCode.Mouse1);

            if (dashPressed && !IsDashing && DashReady)
            {
                StartDash();
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

            if (dashCooldownLeft > 0f) dashCooldownLeft -= Time.deltaTime;
        }

        /// <summary>
        /// Tamponlanmis zipla istegini iptal eder.
        ///
        /// Tek yonlu platformdan ASAGI inerken gerekiyor: oyuncu ASAGI+ZIPLA
        /// basiyor, ama niyeti ziplamak degil INMEK. Iptal edilmezse hem
        /// platformdan gecer hem yukari ziplar - yani yukari cikip tekrar
        /// ustune duser.
        /// </summary>
        public void CancelBufferedJump()
        {
            jumpBufferCounter = 0f;
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
                dashAvailable = true;   // dash hakki yerde yenilenir
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

        /// <summary>
        /// Dash'i baslatir. Yon: giris varsa o yon, giris yoksa baktigi yon.
        /// </summary>
        private void StartDash()
        {
            float inputX = HorizontalInput;
            float inputY = allowDiagonalDash ? Input.GetAxisRaw("Vertical") : 0f;

            if (Mathf.Abs(inputX) < 0.01f && Mathf.Abs(inputY) < 0.01f)
            {
                // Giris yok -> baktigi yone dash
                dashDirection = FacingRight ? Vector2.right : Vector2.left;
            }
            else
            {
                dashDirection = new Vector2(inputX, inputY).normalized;
            }

            IsDashing = true;
            dashTimeLeft = dashDuration;
            dashAvailable = false;
            dashCooldownLeft = dashCooldown;

            OnDashed?.Invoke();
        }

        private void UpdateDash()
        {
            dashTimeLeft -= Time.fixedDeltaTime;

            if (dashTimeLeft <= 0f)
            {
                IsDashing = false;

                // Hizi kes - yoksa dash bitince firlamis gibi devam eder
                rb.SetVelocity(rb.GetVelocity() * dashEndSpeedMultiplier);

                // Yatay yumusatmayi da sifirla ki SmoothDamp eski hizi hatirlamasin
                velocityXSmoothing = 0f;
                return;
            }

            rb.SetVelocity(dashDirection * dashSpeed);
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
            IsDashing = false;
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

            // Respawn sonrasi dash hakki dolu baslasin
            IsDashing = false;
            dashTimeLeft = 0f;
            dashCooldownLeft = 0f;
            dashAvailable = true;
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
