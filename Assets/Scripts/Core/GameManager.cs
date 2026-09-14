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

        [Header("Bolum")]
        [Tooltip("0'dan baslayan bolum numarasi. Kayit sistemi bunu kullanir.")]
        [SerializeField] private int levelIndex = 0;

        [Header("Olum")]
        [Tooltip("Oldukten sonra yeniden dogana kadar gecen sure. " +
                 "0.6'nin uzerine CIKMA - 30 kez olecek oyuncu icin her 0.1 sn 3 sn demek.")]
        [SerializeField] private float respawnDelay = 0.45f;

        [Tooltip("Karakter bu Y degerinin altina duserse olur (bosluga dusme).")]
        [SerializeField] private float killPlaneY = -12f;

        [Header("Hit Stop")]
        [Tooltip("Olum aninda ekranin donma suresi. Darbeyi hissettirir.")]
        [SerializeField] private float deathHitStop = 0.08f;

        // --- Durum ---
        public int Score { get; private set; }
        public int TotalCoins { get; private set; }
        public bool LevelCompleted { get; private set; }
        public float RespawnDelay => respawnDelay;
        public float DeathHitStop => deathHitStop;
        public float KillPlaneY => killPlaneY;

        /// <summary>
        /// Bu bolumde kac kez olundu.
        ///
        /// CAN DEGIL - sinirsiz deneme var, "Game Over" yok. Vizyondaki
        /// "Adalet" sutunu geregi: zorluk tekrar ettirmekten degil,
        /// ogretmekten gelmeli. Can azaltmak oyunu zorlastirmaz, sadece
        /// oyuncuyu ayni kolay kisimlari tekrar oynamaya zorlar.
        /// </summary>
        public int DeathCount { get; private set; }

        /// <summary>Olum konumlari - Epic 17'de isi haritasi icin.</summary>
        public System.Collections.Generic.List<Vector2> DeathPositions { get; }
            = new System.Collections.Generic.List<Vector2>();

        /// <summary>Bolum suresi. Duraklatmada ve bolum bitince saymaz.</summary>
        public float LevelTime { get; private set; }

        /// <summary>Son aktif checkpoint. Hic yoksa karakterin baslangic konumu.</summary>
        public Vector3 CheckpointPosition { get; private set; }

        // --- Olaylar (UI bunlara abone olur) ---
        public System.Action<int, int> OnScoreChanged;   // (toplanan, toplam)
        public System.Action<int> OnDeathCountChanged;
        public System.Action OnLevelCompleted;

        private Transform player;
        private IResettable[] resettables;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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

            CacheResettables();

            OnScoreChanged?.Invoke(Score, TotalCoins);
            OnDeathCountChanged?.Invoke(DeathCount);
        }

        private void Update()
        {
            // Bolum suresi - duraklatmada ve bolum bitince saymaz
            if (!LevelCompleted && Time.timeScale > 0f)
            {
                LevelTime += Time.deltaTime;
            }

            // Haritadan asagi dusme kontrolu
            if (player != null && !LevelCompleted && player.position.y < killPlaneY)
            {
                var health = player.GetComponent<Player.PlayerHealth>();
                if (health != null) health.Kill();   // bosluga dusme - olduren nesne yok
            }
        }

        public static string FormatTime(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.FloorToInt(seconds % 60f);
            return $"{m}:{s:00}";
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

        /// <summary>
        /// PlayerHealth olum aninda burayi cagirir.
        /// Can azaltmaz - sinirsiz deneme var. Sadece sayar.
        /// </summary>
        public void ReportPlayerDeath(Vector2 position)
        {
            DeathCount++;
            DeathPositions.Add(position);
            OnDeathCountChanged?.Invoke(DeathCount);
        }

        /// <summary>
        /// Respawn'da cagrilir: bolumdeki hareketli parcalar basa donsun.
        ///
        /// Paralar ve checkpoint'ler IResettable UYGULAMADIGI icin
        /// sifirlanmaz - toplanmis/aktif kalirlar.
        /// </summary>
        public void ResetLevelState()
        {
            if (resettables == null) return;

            foreach (IResettable r in resettables)
            {
                // Yok edilmis nesneleri atla. Unity'nin null kontrolu
                // Destroy edilmis MonoBehaviour'lari da yakalar.
                if (r is MonoBehaviour mb && mb == null) continue;

                r.ResetToInitialState();
            }
        }

        /// <summary>
        /// Sahnedeki tum IResettable'lari bir kez bulur ve saklar.
        ///
        /// Her respawn'da aramak pahali olurdu: FindObjectsByType tum
        /// sahneyi tarar ve olum 30 kez tekrarlanacak bir olay.
        /// </summary>
        private void CacheResettables()
        {
#if UNITY_2023_1_OR_NEWER
            var behaviours = FindObjectsByType<MonoBehaviour>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var behaviours = FindObjectsOfType<MonoBehaviour>(true);
#endif

            var list = new System.Collections.Generic.List<IResettable>();
            foreach (MonoBehaviour b in behaviours)
            {
                if (b is IResettable r) list.Add(r);
            }

            resettables = list.ToArray();
        }

        public void CompleteLevel()
        {
            if (LevelCompleted) return;

            LevelCompleted = true;

            // Karakteri dondur - bayraga degdikten sonra kosmaya devam etmesin
            if (player != null)
            {
                player.GetComponent<Player.PlayerController2D>()?.Freeze();
            }

            // Kalici kayit. Sir sistemi Epic 08'de gelecek, simdilik false.
            SaveManager.Instance?.CompleteLevel(
                levelIndex, Score, TotalCoins,
                secret: false,
                time: LevelTime,
                deaths: DeathCount);

            OnLevelCompleted?.Invoke();
        }

        /// <summary>Bolumu bastan baslatir (R tusu).</summary>
        public void RestartLevel()
        {
            // ZORUNLU: hit stop sirasinda R'ye basilirsa yeni sahne donuk acilir.
            // timeScale sahne degisimini asar, kendiliginden duzelmez.
            TimeController.Instance?.ResetTimeScale();
            Time.timeScale = 1f;

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
