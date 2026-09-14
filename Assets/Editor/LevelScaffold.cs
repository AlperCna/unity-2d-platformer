using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 05 gorev 8 — bolum sablonu.
    ///
    /// Epic "bos bir sahne yap, Ctrl+D ile kopyala" diyor. Bu, bolumler
    /// ELLE yapiliyorsa dogru. Bizde bolumler koddan uretiliyor ve
    /// kopyalanan sahne KAYAR: birinde kamera ayari duzeltilir digerinde
    /// unutulur, ve bunu kimse farketmez.
    ///
    /// O yuzden sablon bir sahne degil, BU DOSYA. Yeni bolum acmak:
    ///
    ///     var s = LevelScaffold.Create("Bolum 2", levelIndex: 1,
    ///                                  designVersion: 1);
    ///     var c = new LevelCursor(s.Entities, LevelBuilder.groundLayer,
    ///                             s.Rig, "Bolum 2");
    ///     ... tasarim ...
    ///     LevelScaffold.Finish(s, c, "Assets/Scenes/Level02.unity");
    ///
    /// Bir bolum kurucu dosyasi bu ikisinin arasindaki TASARIMDAN ibaret
    /// kaliyor - kurulum tekrari yok, dolayisiyla kayma da yok.
    ///
    /// Elle boyayarak denemek isteyen icin ayrica bos bir sablon SAHNESI
    /// de uretilebiliyor: Tools > 2D Platformer > Bos Bolum Sahnesi.
    /// </summary>
    internal static class LevelScaffold
    {
        internal struct Level
        {
            public Scene Scene;
            public TilemapRig Rig;

            /// <summary>Tilemap'e girmeyen her sey buraya: para, diken, bayrak.</summary>
            public Transform Entities;

            public GameObject Player;
        }

        /// <summary>
        /// Bos bir bolum sahnesi kurar: varliklar, Tilemap iskeleti, oyuncu,
        /// kamera, HUD, yoneticiler. Geriye sadece tasarim kaliyor.
        /// </summary>
        internal static Level Create(string levelName, int levelIndex, int designVersion,
                                     Vector2 playerSpawn)
        {
            LevelBuilder.ConfigureProject();
            SpriteFactory.GenerateAll();
            TileAssetFactory.EnsureGenerated();
            PrefabFactory.EnsureAll();

            var level = new Level
            {
                Scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single),
            };

            LevelBuilder.CreateBackground();

            level.Rig = TilemapRig.Create(LevelBuilder.groundLayer);
            level.Entities = new GameObject("Entities").transform;

            level.Player = LevelBuilder.CreatePlayer(playerSpawn);
            LevelBuilder.CreateCamera(level.Player.transform);
            LevelBuilder.CreateUI();
            LevelBuilder.CreateGameManager(levelIndex, designVersion);

            return level;
        }

        /// <summary>
        /// Bolumu tamamlar: karolari basar, kamera sinirlarini hesaplar,
        /// sahneyi kaydeder, raporu yazar.
        ///
        /// SIRA ONEMLI. Karolar kayittan ONCE basilmali; bir kez ters
        /// sirada oldugu icin karolar bellekte olusup dosyaya yazilmamisti
        /// (oyun calisiyordu ama sahne kapatilinca zemin yok oluyordu).
        /// </summary>
        internal static void Finish(Level level, LevelCursor cursor, string scenePath)
        {
            cursor.Build();
            ApplyCameraBounds(cursor);
            SaveScene(level.Scene, scenePath);
            cursor.Report();

            if (cursor.IssueCount > 0)
            {
                Debug.LogError($"{cursor.IssueCount} gecilemez nokta var! " +
                               "Konsoldaki hatalara bak.");
            }
        }

        // ---------------------------------------------------------------

        /// <summary>
        /// Kamera sinirlarini CameraBoundsTool ile ayni hesaptan alir.
        /// Tek uygulama olmasi onemli: daha once iki ayri formul vardi ve
        /// bolum kurucudaki yanlisti - kamerayi ust sinirina yapistirip
        /// dikey hareketi tamamen durduruyordu.
        /// </summary>
        private static void ApplyCameraBounds(LevelCursor c)
        {
#if UNITY_2023_1_OR_NEWER
            var follow = Object.FindAnyObjectByType<CameraRig.CameraFollow>();
#else
            var follow = Object.FindObjectOfType<CameraRig.CameraFollow>();
#endif
            if (follow == null) return;

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

        private static void SaveScene(Scene scene, string scenePath)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, scenePath);

            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            if (!list.Exists(s => s.path == scenePath))
            {
                list.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = list.ToArray();
            }
        }

        // ---------------------------------------------------------------
        // Elle boyamak icin bos sablon sahnesi
        // ---------------------------------------------------------------

        [MenuItem("Tools/2D Platformer/Bos Bolum Sahnesi", false, 24)]
        private static void CreateTemplateScene()
        {
            if (!EditorGuards.RequireEditMode("Bos Bolum Sahnesi",
                "Yeni sahne olusturmak Play modunda mumkun degil.")) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            const string path = "Assets/Scenes/Level_Template.unity";

            if (System.IO.File.Exists(path) &&
                !EditorUtility.DisplayDialog("Bos Bolum Sahnesi",
                    $"{path} zaten var, uzerine yazilacak.\n\nDevam edilsin mi?",
                    "Yaz", "Vazgec"))
            {
                return;
            }

            Level level = Create("Sablon", levelIndex: 0, designVersion: 1,
                                 playerSpawn: new Vector2(1.5f, 2.5f));

            // Uzerinde durulacak kucuk bir zemin - bos sahnede oyuncu
            // dogrudan olum cizgisine duserdi.
            var tile = TileAssetFactory.Load(1);          // ustu acik yuzey karosu
            for (int x = -2; x <= 12; x++)
            {
                level.Rig.Ground.SetTile(new Vector3Int(x, -1, 0), tile);
            }

            SaveScene(level.Scene, path);

            Debug.Log($"Bos bolum sahnesi hazir: {path}\n" +
                      $"  Window > 2D > Tile Palette ile Ground katmanina " +
                      $"Ground_RuleTile'i boyayabilirsin.\n" +
                      $"  NOT: elle boyanan bolum CETVELE KARSI DOGRULANMAZ - " +
                      $"bosluk genisligi, diken tuzagi kontrolu yapilmaz. " +
                      $"Gercek bolumler icin kod kurucu kullan.");
        }
    }
}
