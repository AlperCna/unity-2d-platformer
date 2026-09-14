using System.Collections;
using UnityEngine;
using Platformer.Core;
using Platformer.CameraRig;

namespace Platformer.Player
{
    /// <summary>
    /// Karakterin olumu ve yeniden dogusu.
    ///
    /// Tasarim ilkesi: OLUMUN MALIYETI ZAMAN DEGIL, BILGI OLMALI.
    /// Oyuncu neden oldugunu anlamali ve hemen tekrar denemeli.
    ///
    /// Olum dizisi (toplam ~0.45 sn):
    ///   0.00  hit stop (0.08 sn donma) + kamera sarsintisi + olduren vurgulanir
    ///   0.08  karakter yukari siçrar, rengi degisir
    ///   0.45  checkpoint'te dogar, ANINDA kontrol edilebilir
    ///   +0.5  kisa dokunulmazlik (yanip sonerek)
    ///
    /// Tum zamanlamalar UNSCALED - hit stop suresi respawn'i geciktirmesin.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Yeniden Dogus")]
        [Tooltip("Respawn sonrasi kisa dokunulmazlik. 1 sn'nin uzerine cikma - " +
                 "oyuncu tehlikeden 'gecip gitmeye' baslar ve tasarim bozulur.")]
        [SerializeField] private float invulnerabilityAfterRespawn = 0.5f;

        [Header("Olum Efekti")]
        [SerializeField] private Color deathTint = new Color(1f, 0.35f, 0.35f, 1f);

        [Tooltip("Olum aninda yukari sicrama - olumu okunakli yapar.")]
        [SerializeField] private float deathPopVelocity = 6f;

        [Header("Kamera Sarsintisi")]
        [SerializeField] private float shakeDuration = 0.25f;
        [SerializeField] private float shakeMagnitude = 0.4f;

        [Header("Olduren Nesneyi Vurgula")]
        [Tooltip("Oyuncu NEDEN oldugunu anlamali. Olduren nesne kisa sure parlar.")]
        [SerializeField] private float killerHighlightDuration = 0.4f;

        public bool IsDead { get; private set; }
        public bool IsInvulnerable { get; private set; }

        public System.Action OnDied;
        public System.Action OnRespawned;

        private PlayerController2D controller;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Collider2D bodyCollider;
        private Color originalColor = Color.white;

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        /// <summary>
        /// Karakteri oldur.
        /// </summary>
        /// <param name="killer">
        /// Olduren nesne. Verilirse kisa sure parlatilir - oyuncu neden
        /// oldugunu gorur. Bosluga dusmede null (ortada bir nesne yok).
        /// </param>
        public void Kill(GameObject killer = null)
        {
            if (IsDead || IsInvulnerable) return;

            IsDead = true;

            // --- 1. ZAMAN: darbeyi hissettir ---
            float hitStop = GameManager.Instance != null
                ? GameManager.Instance.DeathHitStop
                : 0.08f;
            TimeController.Instance?.HitStop(hitStop);

            // --- 2. KAMERA ---
            var cam = UnityEngine.Camera.main != null
                ? UnityEngine.Camera.main.GetComponent<CameraFollow>()
                : null;
            cam?.Shake(shakeDuration, shakeMagnitude);

            // --- 3. KONTROL ---
            controller.Freeze();
            if (bodyCollider != null) bodyCollider.enabled = false;

            // --- 4. GORSEL ---
            if (spriteRenderer != null) spriteRenderer.color = deathTint;
            if (killer != null) StartCoroutine(HighlightKiller(killer));

            // Epic 14: parcacik patlamasi + ekran kirmizi flasi buraya

            OnDied?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReportPlayerDeath(transform.position);
                StartCoroutine(DeathSequence(GameManager.Instance.RespawnDelay));
            }
            else
            {
                StartCoroutine(DeathSequence(0.45f));
            }
        }

        // ---------------------------------------------------------------

        private IEnumerator DeathSequence(float delay)
        {
            // Kucuk yukari sicrama - olum okunakli olur, karakter "yikildi" gorunur
            if (rb != null) rb.SetVelocity(new Vector2(0f, deathPopVelocity));

            // UNSCALED: hit stop suresi respawn'i geciktirmesin.
            // Aksi halde 0.08 sn donma + 0.45 sn bekleme = 0.53 sn olurdu
            // ve hit stop'u uzattikca olum yavaslar.
            yield return new WaitForSecondsRealtime(delay);

            Respawn();
        }

        private void Respawn()
        {
            Vector3 target = GameManager.Instance != null
                ? GameManager.Instance.CheckpointPosition
                : transform.position;

            transform.position = target;
            if (rb != null) rb.SetVelocity(Vector2.zero);

            if (bodyCollider != null) bodyCollider.enabled = true;
            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            IsDead = false;

            // ANINDA kontrol - bekleme yok. Oyuncu olur olmaz tekrar denemeli.
            controller.Unfreeze();

            // Hareketli platformlar, dusmanlar basa donsun (Epic 10)
            GameManager.Instance?.ResetLevelState();

            OnRespawned?.Invoke();

            StartCoroutine(InvulnerabilityWindow());
        }

        private IEnumerator InvulnerabilityWindow()
        {
            IsInvulnerable = true;

            // Yanip sonme - oyuncuya dokunulmaz oldugunu gosterir
            float elapsed = 0f;
            const float blinkInterval = 0.08f;

            while (elapsed < invulnerabilityAfterRespawn)
            {
                if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSecondsRealtime(blinkInterval);
                elapsed += blinkInterval;
            }

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            IsInvulnerable = false;
        }

        /// <summary>
        /// Olduren nesneyi kisa sure parlatir.
        ///
        /// Anlasilmayan olum, haksiz olumdur. Oyuncu "ne oldu?" derse
        /// hatayi kendinde degil oyunda arar.
        /// </summary>
        private IEnumerator HighlightKiller(GameObject killer)
        {
            var sr = killer.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) yield break;

            Color original = sr.color;
            float t = 0f;

            while (t < killerHighlightDuration)
            {
                // unscaled: hit stop sirasinda da gorunsun
                t += Time.unscaledDeltaTime;

                if (sr == null) yield break;   // nesne yok olmus olabilir

                float blink = Mathf.PingPong(t * 10f, 1f);
                sr.color = Color.Lerp(original, Color.white, blink);
                yield return null;
            }

            if (sr != null) sr.color = original;
        }
    }
}
