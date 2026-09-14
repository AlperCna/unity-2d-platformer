using System.IO;
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Kalici kayit — JSON dosyasi olarak.
    ///
    /// PlayerPrefs KULLANILMIYOR. Kucuk ayarlar icin uygun ama oyun
    /// ilerlemesi icin degil: registry'de tutulur, tasinmaz, yedeklenmez,
    /// boyut siniri vardir ve kullanici gorup duzeltemez.
    ///
    /// Dosya: %USERPROFILE%\AppData\LocalLow\<Company>\<Product>\save.json
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string FileName = "save.json";
        private const int CurrentVersion = 1;

        public SaveData Data { get; private set; }

        public string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        // ===============================================================
        // Yukleme
        // ===============================================================

        public void Load()
        {
            if (!File.Exists(FilePath))
            {
                Data = new SaveData();
                Debug.Log($"Kayit yok, yeni olusturuldu.\n{FilePath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);

                if (loaded == null) throw new System.Exception("JSON null dondu");
                if (loaded.levels == null) loaded.levels = new System.Collections.Generic.List<LevelProgress>();

                if (loaded.version > CurrentVersion)
                {
                    // Oyuncu daha yeni bir surumle oynamis, sonra eskiye donmus.
                    // Okumaya calismak veriyi bozabilir.
                    Debug.LogWarning($"Kayit daha yeni bir surumden (v{loaded.version} > v{CurrentVersion}). " +
                                     "Yeni kayit olusturuluyor, eskisi korunuyor.");
                    BackupFile(".newer");
                    Data = new SaveData();
                    return;
                }

                if (loaded.version < CurrentVersion) Migrate(loaded);

                Data = loaded;
                Debug.Log($"Kayit yuklendi (v{Data.version}, {Data.levels.Count} bolum).");
            }
            catch (System.Exception e)
            {
                // BOZUK KAYIT OYUNU COKERTMEMELI.
                // Oyuncu ne yapacagini bilemez; sessizce sifirdan basla,
                // ama bozuk dosyayi silme - belki kurtarilabilir.
                Debug.LogWarning($"Kayit okunamadi ({e.Message}), yeni kayit olusturuluyor.");
                BackupFile(".corrupt");
                Data = new SaveData();
            }
        }

        /// <summary>Eski surumden gecis. Yeni alanlar varsayilanla dolar.</summary>
        private void Migrate(SaveData old)
        {
            // v1 -> v2 gecisi buraya. Simdilik sadece surumu guncelle.
            Debug.Log($"Kayit v{old.version} -> v{CurrentVersion} tasindi.");
            old.version = CurrentVersion;
        }

        private void BackupFile(string suffix)
        {
            try
            {
                string backup = FilePath + suffix;
                if (File.Exists(backup)) File.Delete(backup);
                File.Move(FilePath, backup);
                Debug.Log($"Eski kayit yedeklendi: {Path.GetFileName(backup)}");
            }
            catch { /* yedekleme basarisiz olursa devam et */ }
        }

        // ===============================================================
        // Kaydetme
        // ===============================================================

        public void Save()
        {
            if (Data == null) return;

            try
            {
                string json = JsonUtility.ToJson(Data, true);
                string temp = FilePath + ".tmp";

                // Once gecici dosyaya yaz, sonra yerine koy.
                //
                // Dogrudan uzerine yazsaydik, yazma sirasinda elektrik
                // kesilmesi veya cokme mevcut kaydi da bozardi. Boylece
                // en kotu ihtimalle son kayit kaybolur, oncekiler durur.
                File.WriteAllText(temp, json);

                if (File.Exists(FilePath))
                {
                    // File.Replace Windows'ta atomik
                    File.Replace(temp, FilePath, null);
                }
                else
                {
                    File.Move(temp, FilePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Kayit yazilamadi: {e.Message}");
            }
        }

        // ===============================================================
        // Yardimcilar
        // ===============================================================

        /// <summary>Bolum bitince cagrilir. En iyi degerleri korur.</summary>
        public void CompleteLevel(int index, int coins, int totalCoins,
                                  bool secret, float time, int deaths)
        {
            LevelProgress p = Data.GetLevel(index);

            p.completed = true;
            p.totalCoins = totalCoins;

            // En iyisini sakla - sonraki deneme daha kotuyse ilerleme geri gitmesin
            p.coinsCollected = Mathf.Max(p.coinsCollected, coins);
            p.secretFound = p.secretFound || secret;
            if (p.bestTime < 0f || time < p.bestTime) p.bestTime = time;

            // Olum sayisi birikir, en iyisi alinmaz - toplam istatistik
            p.deathCount += deaths;
            Data.totalDeaths += deaths;
            Data.totalPlayTime += time;

            Data.lastUnlockedLevel = Mathf.Max(Data.lastUnlockedLevel, index + 1);

            Save();
        }

        public bool IsLevelUnlocked(int index) => index <= Data.lastUnlockedLevel;

        public void ResetProgress()
        {
            Data = new SaveData();
            Save();
            Debug.Log("Ilerleme sifirlandi.");
        }

        // ===============================================================
        // Otomatik kayit noktalari
        // ===============================================================

        private void OnApplicationQuit() => Save();

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
