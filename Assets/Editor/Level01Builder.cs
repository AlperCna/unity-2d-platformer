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

            // --- 1. GIRIS -----------------------------------------------
            // Saga gitmekten baska secenek yok, olum imkansiz.
            // v1'de 12 birimdi: hicbir sey olmadan 3 saniye yuruyordun.
            c.Ground(9f, "Zemin_Giris");
            c.Coins(3);

            // --- 2. ILK ZIPLAMA -----------------------------------------
            // 1,2 birim: en kisa zipla bile 1,5 oldugu icin tusa DOKUNMAK yeter.
            // Oyuncu "zipla var" bilgisini bedavaya ogrenir.
            c.Step(1.2f, 4f, "Basamak_Tanitim");
            c.Coins(2);

            // --- 3. GERCEK ZIPLAMA --------------------------------------
            // 2,2 birim (%73): artik gercek bir zipla gerekiyor.
            c.Step(2.2f, 5f, "Basamak_Uygulama");
            c.Coins(3);

            // --- 4. ILK BOSLUK ------------ %47 --------------------------
            // Ilk olum ihtimali. Para yayi yorungeyi cizip yol gosteriyor.
            c.Gap(2.5f, coinArc: 4);

            // --- 5. RITIM (YENI) -------- %53 %56 %49 -------------------
            // Uc ardisik bosluk: zipla-in-zipla-in-zipla.
            //
            // Fiil ayni (ziplamak) ama HIS bambaska. Tek bosluk "engel"dir;
            // ardisik bosluk "ritim"dir, oyuncu akisa girer. v1'in en buyuk
            // eksigi buydu - her engel tek basina duruyordu, akis yoktu.
            //
            // Son bosluk KASITLI olarak dahakucuk (%49): oyuncu ritimden
            // kazanarak cikar, tokatlanarak degil.
            c.Ground(3.5f, "Ritim_1");
            c.Gap(2.8f, coinArc: 3);
            c.Ground(3f, "Ritim_2");
            c.Gap(3f, coinArc: 3);
            c.Ground(3.5f, "Ritim_3");
            c.Gap(2.6f, coinArc: 3);
            c.Ground(4f, "Ritim_4");

            // --- 6. NEFES + CHECKPOINT ----------------------------------
            // Ritimden sonra dinlenme. Checkpoint zor kisimlardan ONCE.
            c.Ground(8f, "Zemin_Nefes");
            c.Coins(3);
            c.Checkpoint(1.5f);

            // --- 7. DIKEN TANITIMI --------------------------------------
            // Tek diken, genis zeminde. Gorursun, rahatca ustunden ziplarsin.
            //
            // Zemin 11 birim, diken 5'te: dikenden TAM zipla atilirsa
            // 10,8'e iniyor - hala zeminde. Daha dar yapsam, dogru ziplayan
            // oyuncu bir sonraki bosluga dusup olurdu. Dogru oynayani
            // cezalandiran tasarim en kotusudur.
            c.Ground(11f, "Zemin_DikenTanitim");
            c.Spikes(1, offsetFromSegmentStart: 5f);

            // --- 8. TIRMANIS (YENI) ------ %68 --------------------------
            // Bosluk + hemen ardindan yukari basamak.
            //
            // 3,2 birim duz zeminde %60'tir; ama 1,4 birim YUKARIYA inecegin
            // icin gecerli limit 5,31 degil 4,72 - yani %68. Ayni bosluk,
            // daha zor. LevelCursor bunu artik kendisi hesapliyor.
            c.Gap(3.2f, coinArc: 4);
            c.Step(1.4f, 4f, "Tirmanis_1");
            c.Coins(2);
            c.Step(1.4f, 4f, "Tirmanis_2");
            c.Coins(2);

            // --- 9. DIKEN KORIDORU (YENI) -------------------------------
            // Iki birimlik kesintisiz diken. Bosluktan farki: zemini
            // GORUYORSUN ama basamiyorsun. v1'deki her diken tekti ve
            // tek adimda geciliyordu; bu ilk defa mesafe olcturuyor.
            //
            // Ustundeki paralar ziplamanin nereden baslamasi gerektigini
            // soyluyor - alcak zipla yetmez.
            c.Ground(9f, "Zemin_DikenKoridoru");
            c.Spikes(2, offsetFromSegmentStart: 4.5f);
            c.Coins(3, heightAboveGround: 2.8f, spacing: 1.1f);

            // --- 10. INIS + CHECKPOINT ----------------------------------
            // Nefes. Asagi inmek bedava, zorluk eklemez.
            //
            // Ikinci checkpoint: bolum v1'in 1,5 kati uzunlukta, tek
            // checkpoint'le en sondaki bosluktan olmek 70 birim geri
            // gondermek demekti.
            c.Drop(2.8f, 7f, "Inis");
            c.Coins(3);
            c.Checkpoint(1.5f);

            // --- 11. BIRLESTIRME --------- %68 --------------------------
            // Diken korudoru + genis bosluk art arda. Ogrenilen iki sey
            // ayni nefeste.
            c.Ground(8f, "Zemin_DikenUygulama");
            c.Spikes(2, offsetFromSegmentStart: 1.5f);

            c.Gap(3.6f, coinArc: 4);

            c.Ground(8f, "Zemin_SonDiken");
            c.Spikes(1, offsetFromSegmentStart: 1.5f);

            // --- 12. FINAL --------------- %83 --------------------------
            // Bolumun tepesi. v1'in en zor ani %75'ti; bolum kendi
            // tavanina hic yaklasmadan bitiyordu.
            c.Gap(4.4f, coinArc: 5);
            c.Ground(6f, "Zemin_Final");
            c.Coins(3);

            // --- 13. BITIS ----------------------------------------------
            // Kisa. v1'de finalden sonra 17 birim bos zemin vardi; bolum
            // en zayif notasinda bitiyordu.
            c.Ground(5f, "Zemin_Bitis");
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
