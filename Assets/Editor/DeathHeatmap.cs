using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Platformer.Core;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 17 — olum isi haritasi.
    ///
    /// Epic'in tek cumlesi: **tahmin etme, olc.**
    ///
    /// Tasarimci olarak bolumu 500 kez oynadin ve artik hicbir yeri zor
    /// gelmiyor. Oyuncunun on kez oldugu nokta senin aklina bile gelmez -
    /// cunku sen orada hic olmuyorsun. Bu yuzden zorluk tahminle degil
    /// OLCUMLE dengelenir.
    ///
    /// Bu pencere olculen olumleri sahnenin uzerine ciziyor ve epic'in
    /// "kirmizi bayrak" tablosundaki iki maddeyi otomatik kontrol ediyor:
    ///
    ///   "Ayni noktada 10+ olum" -> engel haksiz veya okunmuyor
    ///   "Hic olmeden bitirme"   -> cok kolay veya cok kisa
    ///
    /// Menu: Tools > 2D Platformer > Olum Isi Haritasi
    /// </summary>
    public class DeathHeatmap : EditorWindow
    {
        /// <summary>Bu yaricap icindeki olumler AYNI NOKTA sayilir.</summary>
        private const float ClusterRadius = 1.5f;

        /// <summary>Bir noktada bu kadar olum varsa kirmizi bayrak.</summary>
        private const int ClusterFlagThreshold = 10;

        private int levelIndex;
        private LevelAnalyticsData data;
        private string status = "Henuz yuklenmedi.";
        private List<(Vector2 center, int count, string topCause)> clusters = new();

        [MenuItem("Tools/2D Platformer/Olum Isi Haritasi", false, 41)]
        public static void Open()
        {
            GetWindow<DeathHeatmap>("Olum Isi Haritasi").minSize = new Vector2(360f, 260f);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Load();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        // ---------------------------------------------------------------

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Olculen olumler", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            int newIndex = EditorGUILayout.IntField("Bolum indeksi", levelIndex);
            if (newIndex != levelIndex)
            {
                levelIndex = newIndex;
                Load();
            }

            if (GUILayout.Button("Yeniden Yukle")) Load();

            EditorGUILayout.Space(6f);
            EditorGUILayout.HelpBox(status, MessageType.None);

            if (data == null) return;

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Ozet", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Deneme      {data.attempts}");
            EditorGUILayout.LabelField($"Bitirme     {data.completions}");
            EditorGUILayout.LabelField($"Olum        {data.deaths.Count}");

            if (data.completionTimes.Count > 0)
            {
                float sum = 0f;
                foreach (float t in data.completionTimes) sum += t;
                EditorGUILayout.LabelField(
                    $"Ortalama sure   {GameManager.FormatTime(sum / data.completionTimes.Count)}");
            }

            EditorGUILayout.Space(8f);
            DrawFlags();

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Olculeri Sifirla (bu bolum)")) ClearData();
        }

        /// <summary>
        /// Epic 17'nin kirmizi bayrak tablosundan OLCULEBILIR olanlar.
        ///
        /// Tablodaki digerleri ("nereye gidecegim?", "duraklatip telefona
        /// bakma") insan gozlemi gerektiriyor - onlar gorev 4'un isi.
        /// </summary>
        private void DrawFlags()
        {
            EditorGUILayout.LabelField("Kirmizi bayraklar", EditorStyles.boldLabel);

            bool anyFlag = false;

            foreach ((Vector2 center, int count, string cause) in clusters)
            {
                if (count < ClusterFlagThreshold) continue;

                anyFlag = true;
                EditorGUILayout.HelpBox(
                    $"x={center.x:0.0} y={center.y:0.0} noktasinda {count} olum " +
                    $"(cogu: {cause}).\n" +
                    "Engel haksiz veya okunmuyor. Kontrasti artir, mesafeyi azalt " +
                    "ya da oncesine checkpoint koy.",
                    MessageType.Warning);
            }

            if (data.completions > 0 && data.deaths.Count == 0)
            {
                anyFlag = true;
                EditorGUILayout.HelpBox(
                    "Bolum hic olmeden bitirildi. Cok kolay veya cok kisa.\n" +
                    "NOT: bolumu sen yaptin ve ezbere biliyorsun - bu bayrak " +
                    "ancak BASKASI oynadiginda anlamli.",
                    MessageType.Warning);
            }

            if (!anyFlag)
            {
                EditorGUILayout.HelpBox("Olculebilir bayrak yok.", MessageType.Info);
            }
        }

        // ---------------------------------------------------------------

        private void Load()
        {
            string path = LevelAnalytics.PathFor(levelIndex);

            if (!File.Exists(path))
            {
                data = null;
                clusters.Clear();
                status = $"Olcum yok.\n{path}\n\nBolumu oynadiktan sonra " +
                         "'Yeniden Yukle'ye bas.";
                Repaint();
                return;
            }

            try
            {
                data = JsonUtility.FromJson<LevelAnalyticsData>(File.ReadAllText(path));
                BuildClusters();
                status = $"Yuklendi: {path}";
            }
            catch (System.Exception e)
            {
                data = null;
                status = $"Okunamadi: {e.Message}";
            }

            SceneView.RepaintAll();
            Repaint();
        }

        /// <summary>
        /// Yakin olumleri tek noktada topluyor.
        ///
        /// Ham noktalar yaniltici: bir engelde olen oyuncu her seferinde
        /// biraz farkli yerde oluyor. Kumelemeden bakarsan "her yerde biraz
        /// olum var" gorunur; kumeleyince "SU noktada 14 olum" cikar.
        /// </summary>
        private void BuildClusters()
        {
            clusters.Clear();
            if (data == null) return;

            var used = new bool[data.deaths.Count];

            for (int i = 0; i < data.deaths.Count; i++)
            {
                if (used[i]) continue;

                var center = new Vector2(data.deaths[i].x, data.deaths[i].y);
                var causes = new Dictionary<string, int>();
                int count = 0;
                Vector2 sum = Vector2.zero;

                for (int j = i; j < data.deaths.Count; j++)
                {
                    if (used[j]) continue;

                    var p = new Vector2(data.deaths[j].x, data.deaths[j].y);
                    if (Vector2.Distance(p, center) > ClusterRadius) continue;

                    used[j] = true;
                    count++;
                    sum += p;

                    string cause = data.deaths[j].cause ?? "bilinmiyor";
                    causes[cause] = causes.TryGetValue(cause, out int n) ? n + 1 : 1;
                }

                string top = "bilinmiyor";
                int best = 0;
                foreach (KeyValuePair<string, int> kv in causes)
                {
                    if (kv.Value > best) { best = kv.Value; top = kv.Key; }
                }

                clusters.Add((sum / count, count, top));
            }

            clusters.Sort((a, b) => b.count.CompareTo(a.count));
        }

        private void ClearData()
        {
            string path = LevelAnalytics.PathFor(levelIndex);

            if (!EditorUtility.DisplayDialog("Olculeri Sifirla",
                    $"{path}\n\nBu bolumun tum olum kayitlari silinecek.\n\n" +
                    "Oyuncunun ilerlemesi (save.json) ETKILENMEZ.",
                    "Sil", "Vazgec"))
            {
                return;
            }

            if (File.Exists(path)) File.Delete(path);
            Load();
        }

        // ---------------------------------------------------------------
        // Sahne cizimi
        // ---------------------------------------------------------------

        private void OnSceneGUI(SceneView view)
        {
            if (data == null || clusters.Count == 0) return;

            foreach ((Vector2 center, int count, string cause) in clusters)
            {
                // Renk olum sayisina gore: 1 olum sari, 10+ kirmizi.
                float t = Mathf.Clamp01((count - 1) / (float)ClusterFlagThreshold);
                Color c = Color.Lerp(new Color(1f, 0.85f, 0.2f), Color.red, t);
                c.a = 0.35f;

                Handles.color = c;
                Handles.DrawSolidDisc(center, Vector3.forward,
                                      0.4f + Mathf.Sqrt(count) * 0.25f);

                if (count >= 3)
                {
                    Handles.Label(center + Vector2.up * 0.8f,
                                  $"{count}x  {cause}", EditorStyles.whiteBoldLabel);
                }
            }
        }
    }
}
