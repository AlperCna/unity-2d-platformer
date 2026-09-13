using UnityEngine;
using UnityEngine.UI;
using Platformer.Core;

namespace Platformer.UI
{
    /// <summary>
    /// Ekrandaki skor / can gostergesi ve bolum sonu - oyun bitti panelleri.
    /// GameManager'i her karede sorgulamak yerine olaylarina abone olur.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [Header("Gostergeler")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text messageText;

        [Header("Mesajlar")]
        [SerializeField] private string levelCompleteMessage = "BOLUM TAMAMLANDI!\nYeniden baslamak icin R";
        [SerializeField] private string gameOverMessage = "OYUN BITTI\nYeniden baslamak icin R";

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("HudController: sahnede GameManager yok.");
                enabled = false;
                return;
            }

            // Olaylara abone ol
            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.OnLivesChanged += HandleLivesChanged;
            GameManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            GameManager.Instance.OnGameOver += HandleGameOver;

            // Baslangic degerlerini hemen goster
            HandleScoreChanged(GameManager.Instance.Score, GameManager.Instance.TotalCoins);
            HandleLivesChanged(GameManager.Instance.Lives);

            if (messageText != null) messageText.text = string.Empty;
        }

        private void OnDestroy()
        {
            // Abonelikleri birak - sahne degisince hata almamak icin sart
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnLivesChanged -= HandleLivesChanged;
            GameManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        private void HandleScoreChanged(int score, int total)
        {
            if (scoreText != null) scoreText.text = $"Para: {score} / {total}";
        }

        private void HandleLivesChanged(int lives)
        {
            if (livesText != null) livesText.text = $"Can: {lives}";
        }

        private void HandleLevelCompleted()
        {
            if (messageText != null) messageText.text = levelCompleteMessage;
        }

        private void HandleGameOver()
        {
            if (messageText != null) messageText.text = gameOverMessage;
        }
    }
}
