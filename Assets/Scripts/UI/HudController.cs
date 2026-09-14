using UnityEngine;
using UnityEngine.UI;
using Platformer.Core;

namespace Platformer.UI
{
    /// <summary>
    /// Ekrandaki gostergeler.
    ///
    /// CAN GOSTERMIYOR - oyunda sinirsiz deneme var. Onun yerine olum
    /// SAYACI var: ceza degil, istatistik. Oyuncu kendi gelisimini gorur,
    /// sen de hangi bolumun bozuk oldugunu (Epic 17).
    ///
    /// GameManager'i her karede sorgulamak yerine olaylarina abone olur.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [Header("Gostergeler")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text deathText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text messageText;

        [Header("Mesajlar")]
        [SerializeField] private string levelCompleteMessage = "BOLUM TAMAMLANDI!";

        [Header("Sure")]
        [Tooltip("Sureyi ekranda goster. Kapaliysa yine olculur, sadece gorunmez.")]
        [SerializeField] private bool showTimer = true;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("HudController: sahnede GameManager yok.");
                enabled = false;
                return;
            }

            GameManager.Instance.OnScoreChanged += HandleScoreChanged;
            GameManager.Instance.OnDeathCountChanged += HandleDeathCountChanged;
            GameManager.Instance.OnLevelCompleted += HandleLevelCompleted;

            HandleScoreChanged(GameManager.Instance.Score, GameManager.Instance.TotalCoins);
            HandleDeathCountChanged(GameManager.Instance.DeathCount);

            if (messageText != null) messageText.text = string.Empty;
            if (timeText != null) timeText.gameObject.SetActive(showTimer);
        }

        private void OnDestroy()
        {
            // Abonelikleri birak - sahne degisince hata almamak icin sart
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnScoreChanged -= HandleScoreChanged;
            GameManager.Instance.OnDeathCountChanged -= HandleDeathCountChanged;
            GameManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            if (showTimer && timeText != null && !GameManager.Instance.LevelCompleted)
            {
                timeText.text = GameManager.FormatTime(GameManager.Instance.LevelTime);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                GameManager.Instance.RestartLevel();
            }
        }

        private void HandleScoreChanged(int score, int total)
        {
            if (scoreText != null) scoreText.text = $"Para: {score} / {total}";
        }

        private void HandleDeathCountChanged(int deaths)
        {
            if (deathText != null) deathText.text = $"Olum: {deaths}";
        }

        private void HandleLevelCompleted()
        {
            if (messageText == null) return;

            GameManager gm = GameManager.Instance;
            messageText.text =
                $"{levelCompleteMessage}\n\n" +
                $"Sure   {GameManager.FormatTime(gm.LevelTime)}\n" +
                $"Para   {gm.Score} / {gm.TotalCoins}\n" +
                $"Olum   {gm.DeathCount}\n\n" +
                "Yeniden baslamak icin R";
        }
    }
}
