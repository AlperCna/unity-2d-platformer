using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Aktif edilince karakterin yeniden dogus noktasini gunceller.
    /// Bir kez aktif olur, sonra rengi degisir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Konum")]
        [Tooltip("Yeniden dogus noktasinin checkpoint'e gore kaymasi.")]
        [SerializeField] private Vector3 respawnOffset = new Vector3(0f, 0.6f, 0f);

        [Header("Gorsel")]
        [SerializeField] private Color inactiveColor = new Color(0.55f, 0.58f, 0.62f);
        [SerializeField] private Color activeColor = new Color(0.35f, 0.85f, 0.45f);

        public bool IsActivated { get; private set; }

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.color = inactiveColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsActivated || !other.CompareTag("Player")) return;

            IsActivated = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(transform.position + respawnOffset);
            }

            if (spriteRenderer != null) spriteRenderer.color = activeColor;

            StartCoroutine(ActivationPop());
        }

        private System.Collections.IEnumerator ActivationPop()
        {
            // Kisa bir "zipla" ile oyuncuya kaydedildigini hissettir
            Vector3 baseScale = transform.localScale;
            const float duration = 0.22f;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin((t / duration) * Mathf.PI);
                transform.localScale = baseScale * (1f + k * 0.25f);
                yield return null;
            }

            transform.localScale = baseScale;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + respawnOffset, 0.25f);
        }
    }
}
