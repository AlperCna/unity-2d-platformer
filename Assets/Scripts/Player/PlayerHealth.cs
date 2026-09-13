using System.Collections;
using UnityEngine;
using Platformer.Core;

namespace Platformer.Player
{
    /// <summary>
    /// Karakterin olumu ve yeniden dogusu.
    /// Tehlikeler (Hazard, Patroller, olum cizgisi) Kill() cagirir.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Yeniden Dogus")]
        [Tooltip("Respawn sonrasi kisa dokunulmazlik suresi (ust uste olumu onler).")]
        [SerializeField] private float invulnerabilityAfterRespawn = 0.6f;

        [Tooltip("Olum aninda sprite bu renge doner.")]
        [SerializeField] private Color deathTint = new Color(1f, 0.35f, 0.35f, 1f);

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

        /// <summary>Karakteri oldur. Zaten oluyse veya dokunulmazsa hicbir sey yapmaz.</summary>
        public void Kill()
        {
            if (IsDead || IsInvulnerable) return;

            IsDead = true;

            controller.Freeze();
            if (bodyCollider != null) bodyCollider.enabled = false;
            if (spriteRenderer != null) spriteRenderer.color = deathTint;

            OnDied?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReportPlayerDeath();
                StartCoroutine(RespawnAfterDelay(GameManager.Instance.RespawnDelay));
            }
            else
            {
                StartCoroutine(RespawnAfterDelay(0.9f));
            }
        }

        private IEnumerator RespawnAfterDelay(float delay)
        {
            // Kucuk bir "yukari sicrama" efekti - olum daha okunakli olur
            if (rb != null)
            {
                rb.SetVelocity(new Vector2(0f, 6f));
            }

            yield return new WaitForSeconds(delay);

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
            controller.Unfreeze();

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
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            IsInvulnerable = false;
        }
    }
}
