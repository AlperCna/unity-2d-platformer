using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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

            LevelScaffold.Level level = LevelScaffold.Create(
                "Bolum 1",
                levelIndex: 0,
                // designVersion 4 = mucevher + sir eklendi (Epic 08). Bolum yeniden
                // tasarlandiginda ARTIR: eski rekor otomatik sifirlanir.
                designVersion: 4,
                playerSpawn: new Vector2(1.5f, 1.5f));

            LevelCursor c = BuildLayout(level.Entities, level.Rig);

            LevelScaffold.Finish(level, c, ScenePath);
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

            // MUCEVHER — Epic 08. Dikenin tam ustunde, paralardan yukarida.
            // Almak icin koridoru gecerken tam guc ziplamak gerekiyor;
            // alcak zipla yeter ama mucevheri kacirir.
            //
            // Yani mucevher bir odul degil, bir SORU: "riske girer misin?"
            c.Gem(offsetFromSegmentStart: 4f, heightAboveGround: 3.4f);

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

            // SIR — Epic 08. Bolumun en genis boslugunun DIBINDE bir oda.
            //
            // Oyuncu butun bolum boyunca "bosluk = olum" ogrendi. Burada
            // degil: dibinde toprak gibi gorunen ama gecilebilen bir perde
            // ve arkasinda mucevherler var.
            //
            // Ipucu: para yayinin son parcasi digerlerinden ALCAKTA duruyor.
            // Onu almak icin asagi uzanmak gerekiyor ve o an oyuncu "burasi
            // neden bu kadar asagida" diye soruyor.
            c.Secret(hint: "Final boslugunun para yayindaki son para, " +
                           "digerlerinden belirgin sekilde alcakta duruyor. " +
                           "Onu almaya calisan oyuncu perdeye degiyor.",
                     depth: 5f, gemCount: 2);

            c.Step(1f, 6f, "Zemin_Final");
            c.Coins(3);

            // --- 13. BITIS ----------------------------------------------
            c.Ground(5f, "Zemin_Bitis");
            c.Goal();

            return c;
        }
    }
}
