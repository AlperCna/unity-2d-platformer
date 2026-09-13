using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Bolum sonu bayragi. Karakter degince bolum tamamlanir.
    /// Istege bagli olarak tum paralar toplanmadan acilmayacak sekilde ayarlanabilir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelGoal : MonoBehaviour
    {
        [Tooltip("Acik ise bolum sonu icin tum paralarin toplanmasi gerekir.")]
        [SerializeField] private bool requireAllCoins = false;

        [SerializeField] private Color lockedColor = new Color(0.6f, 0.6f, 0.6f);
        [SerializeField] private Color openColor = new Color(1f, 0.85f, 0.3f);

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.color = IsOpen() ? openColor : lockedColor;
        }

        private bool IsOpen()
        {
            if (!requireAllCoins) return true;
            if (GameManager.Instance == null) return true;

            return GameManager.Instance.Score >= GameManager.Instance.TotalCoins;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (!IsOpen()) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }
    }
}
