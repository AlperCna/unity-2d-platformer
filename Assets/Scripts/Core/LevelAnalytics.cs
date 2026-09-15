using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>Tek bir olumun kaydi.</summary>
    [System.Serializable]
    public class DeathRecord
    {
        public float x;
        public float y;

        /// <summary>Bolume girdikten kac saniye sonra oldu.</summary>
        public float timeIntoLevel;

        /// <summary>Olduren nesnenin adi. Bosluga dusmede "bosluk".</summary>
        public string cause;
    }

    [System.Serializable]
    public class LevelAnalyticsData
    {
        public int levelIndex;
        public int designVersion;

        /// <summary>Bolume kac kez girildi.</summary>
        public int attempts;

        /// <summary>Kac kez bitirildi.</summary>
        public int completions;

        public List<DeathRecord> deaths = new List<DeathRecord>();

        /// <summary>Bitirilen kosularin sureleri - ortalama icin.</summary>
        public List<float> completionTimes = new List<float>();
    }

    /// <summary>
    /// Epic 17 — oynanis verisi toplar.
    ///
    /// Epic'in tek cumlelik ozeti: **tahmin etme, olc.**
    ///
    /// "Cogu amator oyun cok zordur ve yapimcisi bunu bilmez. Sebep basit:
    /// sen oyununu 500 kez oynadin, oyuncu ilk kez goruyor."
    ///
    /// Bu, tahminle cozulemez. Nerede olundugu OLCULMELI - cunku tasarimci
    /// olarak senin "kolay" dedigin yer, oyuncunun on kez oldugu yer olabilir
    /// ve sen orayi hic zor bulmadigin icin aklina bile gelmez.
    ///
    /// Veri bolum bazinda ayri dosyalara yaziliyor:
    ///   %USERPROFILE%/AppData/LocalLow/DefaultCompany/2d-platformer/
    ///     analytics-level0.json
    ///
    /// KAYITTAN AYRI TUTULUYOR: oyuncunun ilerlemesi ile gelistirme verisi
    /// ayri seyler. Kayit silinince olcumler kaybolmamali, olcumler silinince
    /// oyuncunun ilerlemesi bozulmamali.
    /// </summary>
    public static class LevelAnalytics
    {
        private static LevelAnalyticsData current;
        private static string currentPath;

        public static LevelAnalyticsData Current => current;

        public static string PathFor(int levelIndex) =>
            Path.Combine(Application.persistentDataPath, $"analytics-level{levelIndex}.json");

        /// <summary>Bolum acilinca cagrilir.</summary>
        public static void BeginLevel(int levelIndex, int designVersion)
        {
            currentPath = PathFor(levelIndex);
            current = Load(currentPath);

            // Tasarim degistiyse eski olcumler artik baska bir bolume ait.
            // Ayni sebeple kayit sistemi de rekoru sifirliyor (Epic 10).
            if (current.designVersion != designVersion)
            {
                current = new LevelAnalyticsData
                {
                    levelIndex = levelIndex,
                    designVersion = designVersion,
                };
            }

            current.attempts++;
            Save();
        }

        public static void RecordDeath(Vector2 position, float timeIntoLevel, string cause)
        {
            if (current == null) return;

            current.deaths.Add(new DeathRecord
            {
                x = position.x,
                y = position.y,
                timeIntoLevel = timeIntoLevel,
                cause = string.IsNullOrEmpty(cause) ? "bilinmiyor" : cause,
            });

            Save();
        }

        public static void RecordCompletion(float time)
        {
            if (current == null) return;

            current.completions++;
            current.completionTimes.Add(time);
            Save();
        }

        // ---------------------------------------------------------------

        private static LevelAnalyticsData Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var data = JsonUtility.FromJson<LevelAnalyticsData>(File.ReadAllText(path));
                    if (data != null)
                    {
                        data.deaths ??= new List<DeathRecord>();
                        data.completionTimes ??= new List<float>();
                        return data;
                    }
                }
            }
            catch (System.Exception e)
            {
                // Bozuk olcum dosyasi oyunu DURDURMAMALI. Kayit dosyasindan
                // farki bu: oyuncunun ilerlemesi degil, gelistirme verisi.
                Debug.LogWarning($"Olcum dosyasi okunamadi ({path}): {e.Message}. " +
                                 "Yeniden baslatiliyor.");
            }

            return new LevelAnalyticsData();
        }

        private static void Save()
        {
            if (current == null || string.IsNullOrEmpty(currentPath)) return;

            try
            {
                File.WriteAllText(currentPath, JsonUtility.ToJson(current, true));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Olcum dosyasi yazilamadi: {e.Message}");
            }
        }
    }
}
