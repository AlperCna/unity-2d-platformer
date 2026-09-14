using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Platformer.CameraRig;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 04 — Bolum 1.
    ///
    /// Tasarim belgesi: docs/BOLUM-01.md
    /// Kullanilan cetvel: docs/AYARLAR.md (Epic 02'de olculdu)
    ///
    /// Asagidaki kod, tasarim belgesindeki beat sheet'in birebir karsiligi.
    /// Koordinat yok - "12 birim zemin, 2,5 birim bosluk" diye okunuyor.
    /// Her bosluk ve basamak LevelCursor tarafindan cetvele karsi dogrulaniyor.
    ///
    /// Menu: Tools > 2D Platformer > Bolum 1'i Kur
    /// </summary>
    public static class Level01Builder
    {
        private const string ScenePath = "Assets/Scenes/Level01.unity";

        [MenuItem("Tools/2D Platformer/Bolum 1'i Kur", false, 3)]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            bool ok = EditorUtility.DisplayDialog(
                "Bolum 1'i Kur",
                "Assets/Scenes/Level01.unity YENIDEN olusturulacak.\n\n" +
                "Tasarim: docs/BOLUM-01.md\n" +
                "Mevcut sahnenin uzerine yazilir.\n\nDevam edilsin mi?",
                "Kur", "Vazgec");

            if (!ok) return;

            LevelBuilder.ConfigureProject();
            SpriteFactory.GenerateAll();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            LevelBuilder.CreateBackground();

            var root = new GameObject("Level");
            LevelCursor c = BuildLayout(root.transform);

            GameObject player = LevelBuilder.CreatePlayer(new Vector2(1.5f, 1.5f));
            LevelBuilder.CreateCamera(player.transform);
            LevelBuilder.CreateUI();
            LevelBuilder.CreateGameManager();

            ApplyCameraBounds(c);
            SaveScene(scene);

            c.Report();

            if (c.IssueCount > 0)
            {
                Debug.LogError($"Bolum 1'de {c.IssueCount} gecilemez nokta var! " +
                               "Konsoldaki hatalara bak.");
            }
        }

        // ===============================================================
        // TASARIM — docs/BOLUM-01.md beat sheet'i
        // ===============================================================

        private static LevelCursor BuildLayout(Transform parent)
        {
            var c = new LevelCursor(parent, LevelBuilder.groundLayer, "Bolum 1");

            // --- 1. GIRIS (0-6 sn) --------------------------------------
            // Saga gitmekten baska secenek yok. Olum imkansiz.
            c.Ground(12f, "Zemin_Giris");
            c.Coins(3);

            // --- 2. ILK ZIPLAMA (6-10 sn) -------------------------------
            // 1,2 birim: minimum zipla 1,5 oldugu icin tusa DOKUNMAK yeter.
            // Oyuncu "zipla var" bilgisini bedavaya ogrenir.
            c.Step(1.2f, 4f, "Basamak_Tanitim");
            c.Coins(2);

            // --- 3. GERCEK ZIPLAMA (10-16 sn) ---------------------------
            // 2,2 birim (%73): artik gercek bir zipla gerekiyor.
            c.Step(2.2f, 5f, "Basamak_Uygulama");
            c.Coins(3);

            // --- 4. ILK BOSLUK (16-22 sn) -------------------------------
            // 2,5 birim (%47 - "cok kolay"). Ilk olum ihtimali burada.
            // Para yayi ziplamanin yorungesini cizip yol gosteriyor.
            c.Gap(2.5f, coinArc: 4);

            // --- 5. NEFES (22-27 sn) ------------------------------------
            // Tehlikesiz duz zemin. Checkpoint burada: zor kisimlardan ONCE.
            c.Ground(9f, "Zemin_Nefes");
            c.Coins(3);
            c.Checkpoint(1.5f);

            // --- 6. DIKEN TANITIMI (27-34 sn) ---------------------------
            // Tek diken, 10 birimlik genis zeminde. Gorursun, rahatca
            // ustunden ziplarsin. Olum mumkun ama kacinmasi kolay.
            c.Ground(10f, "Zemin_DikenTanitim");
            c.Spikes(1, offsetFromSegmentStart: 5f);

            // --- 7. ZORLUK ARTISI (34-41 sn) ----------------------------
            // 3,5 birim (%66 - "kolay"). Bir onceki bosluktan genis.
            c.Gap(3.5f, coinArc: 5);

            // --- Inis: gorsel cesitlilik, zorluk eklemez ----------------
            c.Drop(2f, 8f, "Inis");
            c.Coins(3);

            // --- 8. BIRLESTIRME (41-48 sn) ------------------------------
            // Once diken (ogrenileni tekrar), sonra bolumun en genis boslugu.
            // Bosluktan once 4 birim duz kosu mesafesi var - tam hiza ulasmak
            // icin ~2 birim gerekiyor, yani rahat.
            c.Ground(8f, "Zemin_DikenUygulama");
            c.Spikes(2, offsetFromSegmentStart: 2f);

            c.Gap(4f, coinArc: 5);   // %75 - bolum 1'in tavani

            // Inisin hemen ardinda diken YOK; once yer ver, sonra tehlike.
            c.Ground(7f, "Zemin_SonDiken");
            c.Spikes(1, offsetFromSegmentStart: 4.5f);

            // --- 9. BITIS (48-53 sn) ------------------------------------
            // Rahat yaklasim, bayrak uzaktan gorunuyor.
            c.Ground(10f, "Zemin_Bitis");
            c.Coins(3);
            c.Goal();

            return c;
        }

        // ===============================================================

        /// <summary>
        /// Kamera sinirlarini CameraBoundsTool ile ayni hesaptan alir.
        /// Tek uygulama olmasi onemli: daha once bu iki yerde ayri formuller
        /// vardi ve buradaki yanlisti - kamerayi ust sinirina yapistirip
        /// dikey hareketi tamamen durduruyordu.
        /// </summary>
        private static void ApplyCameraBounds(LevelCursor c)
        {
#if UNITY_2023_1_OR_NEWER
            var follow = Object.FindAnyObjectByType<CameraFollow>();
#else
            var follow = Object.FindObjectOfType<CameraFollow>();
#endif
            if (follow == null) return;

            // Bolumun kapladigi alan: zemin parcalari + uzerlerindeki icerik
            var content = new Bounds();
            content.SetMinMax(
                new Vector3(0f, c.MinGroundTop - 1f, 0f),
                new Vector3(c.X, c.MaxGroundTop + 2.5f, 0f));

            CameraBoundsTool.Compute(follow, content, c.MinGroundTop, c.MaxGroundTop,
                                     out Vector2 min, out Vector2 max, out string note);

            var so = new SerializedObject(follow);
            so.FindProperty("useBounds").boolValue = true;
            so.FindProperty("minBounds").vector2Value = min;
            so.FindProperty("maxBounds").vector2Value = max;
            so.ApplyModifiedProperties();

            Debug.Log($"Kamera sinirlari: min({min.x:0.0}, {min.y:0.0}) " +
                      $"max({max.x:0.0}, {max.y:0.0})\n{note}");
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, ScenePath);

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            if (!list.Exists(s => s.path == ScenePath))
            {
                list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = list.ToArray();
            }
        }
    }
}
