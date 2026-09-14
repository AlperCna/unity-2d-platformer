using UnityEngine;
using Platformer.Player;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Dokununca olduren tehlike (dikenler, lav, testere...).
    /// Hem trigger hem normal collider ile calisir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Hazard : MonoBehaviour
    {
        [Tooltip("Sadece bu yonden gelince olduren tehlikeler icin. Zero = her yon.")]
        [SerializeField] private Vector2 requiredApproachDirection = Vector2.zero;

        [Tooltip("Yon kontrolunun toleransi. 1 = tam isabet, 0 = 90 dereceye kadar.")]
        [Range(0f, 1f)]
        [SerializeField] private float directionTolerance = 0.5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryKill(other, other.transform.position);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Vector2 contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : (Vector2)collision.transform.position;

            TryKill(collision.collider, contactPoint);
        }

        private void TryKill(Collider2D other, Vector2 contactPoint)
        {
            if (!other.CompareTag("Player")) return;

            // Yonlu tehlike: orn. sadece ustunden basinca olduren dikenler
            if (requiredApproachDirection != Vector2.zero)
            {
                Vector2 approach = ((Vector2)other.transform.position - contactPoint).normalized;
                if (Vector2.Dot(approach, requiredApproachDirection.normalized) < directionTolerance)
                {
                    return;
                }
            }

            PlayerHealth health = other.GetComponent<PlayerHealth>();

            // gameObject'i veriyoruz ki olum aninda bu tehlike parlasin -
            // oyuncu NEDEN oldugunu gorsun
            if (health != null) health.Kill(gameObject);
        }
    }
}
