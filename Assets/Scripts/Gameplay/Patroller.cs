using UnityEngine;
using Platformer.Core;
using Platformer.Player;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Basit devriye dusmani. Ileri yurur; duvara carpinca veya platformun
    /// kenarina gelince geri doner. Yandan dokunursa oldurur, ustune
    /// basilirsa (stomp) olur ve karakteri ziplatir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Patroller : MonoBehaviour
    {
        [Header("Hareket")]
        [SerializeField] private float moveSpeed = 2f;

        [Tooltip("Basta saga mi yurusun?")]
        [SerializeField] private bool startFacingRight = true;

        [Header("Algilama")]
        [Tooltip("Zemin ve duvar olarak sayilacak layer'lar.")]
        [SerializeField] private LayerMask groundLayers = ~0;

        [Tooltip("Kenar kontrolu icin onden ne kadar ileriye bakilir.")]
        [SerializeField] private float edgeCheckDistance = 0.55f;

        [Tooltip("Kenar kontrolu icin asagi dogru isin uzunlugu.")]
        [SerializeField] private float edgeCheckDepth = 0.9f;

        [Tooltip("Duvar kontrolu icin ileri isin uzunlugu.")]
        [SerializeField] private float wallCheckDistance = 0.55f;

        [Header("Ustune Basilma (Stomp)")]
        [Tooltip("Karakter bu yukseklik farkindan fazla yukarideyse stomp sayilir.")]
        [SerializeField] private float stompHeightThreshold = 0.25f;

        [Tooltip("Stomp sonrasi karakterin ziplama yuksekligi.")]
        [SerializeField] private float stompBounceHeight = 2.6f;

        [SerializeField] private int stompScoreReward = 2;

        private Rigidbody2D rb;
        private Collider2D bodyCollider;
        private SpriteRenderer spriteRenderer;
        private int direction;
        private bool isDead;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            rb.freezeRotation = true;
            direction = startFacingRight ? 1 : -1;
            ApplyFacing();
        }

        private void FixedUpdate()
        {
            if (isDead) return;

            if (ShouldTurnAround())
            {
                direction *= -1;
                ApplyFacing();
            }

            // Y hizina dokunmuyoruz ki yercekimi normal calissin
            rb.SetVelocityX(direction * moveSpeed);
        }

        /// <summary>Onunde zemin yoksa veya duvar varsa donmeli.</summary>
        private bool ShouldTurnAround()
        {
            Vector2 center = bodyCollider.bounds.center;
            float halfHeight = bodyCollider.bounds.extents.y;

            // 1) Onunde ucurum var mi?
            Vector2 edgeOrigin = center + new Vector2(direction * edgeCheckDistance, -halfHeight + 0.05f);
            bool groundAhead = Physics2D.Raycast(edgeOrigin, Vector2.down, edgeCheckDepth, groundLayers);

            if (!groundAhead) return true;

            // 2) Onunde duvar var mi?
            bool wallAhead = Physics2D.Raycast(center, Vector2.right * direction, wallCheckDistance, groundLayers);

            return wallAhead;
        }

        private void ApplyFacing()
        {
            if (spriteRenderer != null) spriteRenderer.flipX = direction < 0;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandlePlayerContact(collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            // Karakter dusmanin ustune "oturursa" Enter tetiklenmeyebilir
            HandlePlayerContact(collision.collider);
        }

        private void HandlePlayerContact(Collider2D other)
        {
            if (isDead || !other.CompareTag("Player")) return;

            PlayerHealth health = other.GetComponent<PlayerHealth>();
            PlayerController2D controller = other.GetComponent<PlayerController2D>();
            if (health == null || health.IsDead) return;

            bool fallingOnto = controller != null && controller.Velocity.y <= 0.01f;
            bool aboveEnemy = other.bounds.min.y > bodyCollider.bounds.center.y + stompHeightThreshold;

            if (fallingOnto && aboveEnemy)
            {
                Stomped(controller);
            }
            else
            {
                health.Kill();
            }
        }

        private void Stomped(PlayerController2D controller)
        {
            isDead = true;

            if (controller != null) controller.LaunchUpward(stompBounceHeight);

            if (GameManager.Instance != null && stompScoreReward > 0)
            {
                GameManager.Instance.AddScore(stompScoreReward);
            }

            // Artik kimseye zarar vermesin ve fizigi etkilemesin
            bodyCollider.enabled = false;
            rb.SetVelocity(Vector2.zero);
            rb.bodyType = RigidbodyType2D.Kinematic;

            StartCoroutine(SquashAndDisappear());
        }

        private System.Collections.IEnumerator SquashAndDisappear()
        {
            Vector3 baseScale = transform.localScale;
            const float duration = 0.25f;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = t / duration;

                // Ezilme efekti: yassilasip saydamlassin
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

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col == null) return;

            int dir = Application.isPlaying ? direction : (startFacingRight ? 1 : -1);
            Vector2 center = col.bounds.center;
            float halfHeight = col.bounds.extents.y;

            // Kenar kontrolu isini
            Gizmos.color = Color.yellow;
            Vector2 edgeOrigin = center + new Vector2(dir * edgeCheckDistance, -halfHeight + 0.05f);
            Gizmos.DrawLine(edgeOrigin, edgeOrigin + Vector2.down * edgeCheckDepth);

            // Duvar kontrolu isini
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(center, center + Vector2.right * (dir * wallCheckDistance));
        }
    }
}
