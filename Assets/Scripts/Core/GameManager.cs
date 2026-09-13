using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Bolumun merkezi durumu: skor, can, checkpoint ve respawn.
    /// Sahnede tek bir tane olur; her yerden GameManager.Instance ile erisilir.
    /// UI ve diger sistemler surekli sorgulamak yerine olaylara (event) abone olur.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Baslangic Degerleri")]
        [SerializeField] private int startingLives = 3;

        [Tooltip("Oldukten sonra yeniden dogana kadar gecen sure.")]
        [SerializeField] private float respawnDelay = 0.9f;

        [Tooltip("Karakter bu Y degerinin altina duserse olur (bosluga dusme).")]
        [SerializeField] private float killPlaneY = -20f;

        // --- Durum ---
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int TotalCoins { get; private set; }
        public bool LevelCompleted { get; private set; }
        public float RespawnDelay => respawnDelay;
        public float KillPlaneY => killPlaneY;

        /// <summary>Son aktif checkpoint. Hic yoksa karakterin baslangic konumu.</summary>
        public Vector3 CheckpointPosition { get; private set; }

        // --- Olaylar (UI bunlara abone olur) ---
        public System.Action<int, int> OnScoreChanged;   // (toplanan, toplam)
        public System.Action<int> OnLivesChanged;
        public System.Action OnLevelCompleted;
        public System.Action OnGameOver;

        private Transform player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Lives = startingLives;
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                CheckpointPosition = player.position;
            }
            else
            {
                Debug.LogWarning("GameManager: 'Player' tag'li bir nesne bulunamadi.");
            }

            // Sahnedeki tum paralari say ki UI "3 / 12" gosterebilsin
#if UNITY_2023_1_OR_NEWER
            TotalCoins = FindObjectsByType<Gameplay.Coin>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
#else
            TotalCoins = FindObjectsOfType<Gameplay.Coin>(true).Length;
#endif

            OnScoreChanged?.Invoke(Score, TotalCoins);
            OnLivesChanged?.Invoke(Lives);
        }

        private void Update()
        {
            // Haritadan asagi dusme kontrolu
            if (player != null && !LevelCompleted && player.position.y < killPlaneY)
            {
                var health = player.GetComponent<Player.PlayerHealth>();
                if (health != null) health.Kill();
            }
        }

        public void AddScore(int amount)
        {
            Score += amount;
            OnScoreChanged?.Invoke(Score, TotalCoins);
        }

        /// <summary>Checkpoint'ler burayi cagirir.</summary>
        public void SetCheckpoint(Vector3 position)
        {
            CheckpointPosition = position;
        }

        /// <summary>PlayerHealth olum aninda burayi cagirir.</summary>
        public void ReportPlayerDeath()
        {
            Lives = Mathf.Max(0, Lives - 1);
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                OnGameOver?.Invoke();
            }
        }

        public void CompleteLevel()
        {
            if (LevelCompleted) return;

            LevelCompleted = true;
            OnLevelCompleted?.Invoke();
        }

        /// <summary>Bolumu bastan baslatir (R tusu veya Game Over ekrani).</summary>
        public void RestartLevel()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnDrawGizmos()
        {
            // Olum cizgisini Scene view'da goster
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            Vector3 left = new Vector3(-500f, killPlaneY, 0f);
            Vector3 right = new Vector3(500f, killPlaneY, 0f);
            Gizmos.DrawLine(left, right);
        }
    }
}
