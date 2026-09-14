using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Devriye dusmani — sordugu soru: **"Zamanlamayi tutturabiliyor musun?"**
    ///
    /// Ileri yurur; duvara carpinca veya platformun kenarina gelince doner.
    /// Hareketi tamamen ongorulebilir, o yuzden zorluk refleks degil
    /// zamanlama.
    ///
    /// Temas, ezilme, olum ve sifirlama EnemyBase'de. Burada sadece hareket
    /// var - bir dusman cesidinin tasimasi gereken tek sey bu olmali.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Patroller : EnemyBase
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

        private int direction;

        protected override void OnAwake()
        {
            direction = startFacingRight ? 1 : -1;
            ApplyFacing();
        }

        protected override void OnReset()
        {
            direction = startFacingRight ? 1 : -1;
            ApplyFacing();
        }

        private void FixedUpdate()
        {
            if (IsDead) return;

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
            Vector2 edgeOrigin = center + new Vector2(direction * edgeCheckDistance,
                                                      -halfHeight + 0.05f);
            bool groundAhead = Physics2D.Raycast(edgeOrigin, Vector2.down,
                                                 edgeCheckDepth, groundLayers);

            if (!groundAhead) return true;

            // 2) Onunde duvar var mi?
            return Physics2D.Raycast(center, Vector2.right * direction,
                                     wallCheckDistance, groundLayers);
        }

        private void ApplyFacing()
        {
            if (spriteRenderer != null) spriteRenderer.flipX = direction < 0;
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
            Vector2 edgeOrigin = center + new Vector2(dir * edgeCheckDistance,
                                                      -halfHeight + 0.05f);
            Gizmos.DrawLine(edgeOrigin, edgeOrigin + Vector2.down * edgeCheckDepth);

            // Duvar kontrolu isini
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(center, center + Vector2.right * (dir * wallCheckDistance));
        }
    }
}
