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
            if (!EditorGuards.RequireEditMode("Bolum 1'i Kur",
                "Yeni sahne olusturmak Play modunda mumkun degil.")) return;

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
            TileAssetFactory.EnsureGenerated();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            LevelBuilder.CreateBackground();

            TilemapRig rig = TilemapRig.Create(LevelBuilder.groundLayer);

            // Tilemap'e girmeyen her sey (para, checkpoint, bayrak, diken)
            // burada toplaniyor - Epic 05'in katman ayrimi.
            var root = new GameObject("Entities");
            LevelCursor c = BuildLayout(root.transform, rig);

            GameObject player = LevelBuilder.CreatePlayer(new Vector2(1.5f, 1.5f));
            LevelBuilder.CreateCamera(player.transform);
            LevelBuilder.CreateUI();
            // designVersion 3 = izgaraya tasinmis tasarim (v3). Bolum yeniden
            // tasarlandiginda ARTIR: eski rekor otomatik sifirlanir.
            LevelBuilder.CreateGameManager(levelIndex: 0, designVersion: 3);

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

        private static LevelCursor BuildLayout(Transform parent, TilemapRig rig)
        {
            var c = new LevelCursor(parent, LevelBuilder.groundLayer, rig, "Bolum 1");

            // Butun olculer TAM SAYI: 1 birim = 1 Tilemap hucresi.
            // Kesirli deger verilirse LevelCursor yuvarlar ve uyarir.

            // --- 1. GIRIS -----------------------------------------------
            // Saga gitmekten baska secenek yok, olum imkansiz.
            c.Ground(9f, "Zemin_Giris");
            c.Coins(3);

            // --- 2. ILK ZIPLAMA -----------------------------------------
            // 1 birim: en kisa zipla bile 1,5 oldugu icin tusa DOKUNMAK yeter.
            c.Step(1f, 4f, "Basamak_Tanitim");
            c.Coins(2);

            // --- 3. GERCEK ZIPLAMA --------------------------------------
            // 2 birim (%66): artik gercek bir zipla gerekiyor.
            c.Step(2f, 5f, "Basamak_Uygulama");
            c.Coins(3);

            // --- 4. ILK BOSLUK ------------ %38 --------------------------
            c.Gap(2f, coinArc: 3);

            // --- 5. RITIM ----------------- %56 %56 %38 -----------------
            // Uc ardisik bosluk. Ilk ikisi AYNI genislikte - ritim
            // tutarliliktir, cesitlilik degil. Ucuncusu kasitli kucuk:
            // oyuncu ritimden kazanarak cikar, tokatlanarak degil.
            c.Ground(4f, "Ritim_1");
            c.Gap(3f, coinArc: 3);
            c.Ground(3f, "Ritim_2");
            c.Gap(3f, coinArc: 3);
            c.Ground(4f, "Ritim_3");
            c.Gap(2f, coinArc: 3);
            c.Ground(4f, "Ritim_4");

            // --- 6. NEFES + CHECKPOINT ----------------------------------
            c.Ground(8f, "Zemin_Nefes");
            c.Coins(3);
            c.Checkpoint(1.5f);

            // --- 7. DIKEN TANITIMI --------------------------------------
            // Zemin 11 birim, diken 5'te. Daha dar yapsam, dikenden TAM guc
            // ziplayan oyuncu bir sonraki bosluga dusup olurdu.
            // (LevelCursor.ValidateSpikeLandings bunu her kurulumda kontrol ediyor.)
            c.Ground(11f, "Zemin_DikenTanitim");
            c.Spikes(1, offsetFromSegmentStart: 5f);

            // --- 8. TIRMANIS -------------- %68 --------------------------
            // 3 birimlik bosluk duz zeminde %56'dir. Ama karsi taraf 2 birim
            // YUKARIDA oldugu icin gecerli limit 5,31 degil 4,38 - yani %68.
            // Ayni bosluk, daha zor. Bu, izgaraya gecince kaybedilen ince
            // ayari geri kazandiran sey: zorluk sadece genislikten degil,
            // genislik+yukselis birlesiminden geliyor.
            c.Gap(3f, coinArc: 4);
            c.Step(2f, 5f, "Tirmanis_1");
            c.Coins(2);
            c.Step(1f, 4f, "Tirmanis_2");
            c.Coins(2);

            // --- 9. DIKEN KORIDORU --------------------------------------
            // Iki birimlik kesintisiz diken. Bosluktan farki: zemini
            // GORUYORSUN ama basamiyorsun.
            c.Ground(9f, "Zemin_DikenKoridoru");
            c.Spikes(2, offsetFromSegmentStart: 3f);
            c.Coins(3, heightAboveGround: 2.8f, spacing: 1.1f);

            // --- 10. INIS + CHECKPOINT ----------------------------------
            c.Drop(3f, 7f, "Inis");
            c.Coins(3);
            c.Checkpoint(1.5f);

            // --- 11. BIRLESTIRME ---------- %75 --------------------------
            c.Ground(8f, "Zemin_DikenUygulama");
            c.Spikes(2, offsetFromSegmentStart: 2f);

            c.Gap(4f, coinArc: 4);

            c.Ground(8f, "Zemin_SonDiken");
            c.Spikes(1, offsetFromSegmentStart: 2f);

            // --- 12. FINAL ---------------- %82 --------------------------
            // Bolumun tepesi: 4 birim bosluk + 1 birim yukselis.
            // Duz olsaydi %75 olurdu - onceki bosluktan farksiz.
            c.Gap(4f, coinArc: 5);
            c.Step(1f, 6f, "Zemin_Final");
            c.Coins(3);

            // --- 13. BITIS ----------------------------------------------
            c.Ground(5f, "Zemin_Bitis");
            c.Goal();

            return c;
        }

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
