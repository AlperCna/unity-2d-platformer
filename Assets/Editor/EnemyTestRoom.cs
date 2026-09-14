using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 06 — uc dusmani art arda denemek icin test odasi.
    ///
    /// Bolum 1'de dusman YOK (kasitli: ogretme bolumu). Dolayisiyla
    /// dusmanlari hissetmenin baska yolu yok. Bolume koyup sonra "bu
    /// haksizmis" demek yerine once burada deneniyor.
    ///
    /// Her bolum bir dusmanin sordugu soruyu izole ediyor:
    ///   devriye -> ne zaman?
    ///   atici   -> nereden?
    ///   ucan    -> cesaret edebiliyor musun?
    ///
    /// Menu: Tools > 2D Platformer > Dusman Test Odasi
    /// </summary>
    public static class EnemyTestRoom
    {
        private const string ScenePath = "Assets/Scenes/DusmanTestOdasi.unity";

        [MenuItem("Tools/2D Platformer/Dusman Test Odasi", false, 4)]
        public static void Build()
        {
            if (!EditorGuards.RequireEditMode("Dusman Test Odasi",
                "Yeni sahne olusturmak Play modunda mumkun degil.")) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (!EditorUtility.DisplayDialog("Dusman Test Odasi",
                    $"{ScenePath} olusturulacak.\n\n" +
                    "Uc dusman cesidi ayri ayri denenebilir.\n\nDevam edilsin mi?",
                    "Olustur", "Vazgec"))
            {
                return;
            }

            // levelIndex 99: test odasi BOLUM 1'IN KAYIT YUVASINI KULLANMAMALI.
            // 0 verilseydi, burayi bitirmek Bolum 1'in rekorunu ve para
            // sayisini ezerdi - hem de designVersion farkli oldugu icin
            // "yeniden tasarlanmis" sayilip sifirlardi.
            LevelScaffold.Level level = LevelScaffold.Create(
                "Dusman Testi", levelIndex: 99, designVersion: 1,
                playerSpawn: new Vector2(2f, 2.5f));

            var c = new LevelCursor(level.Entities, LevelBuilder.groundLayer,
                                    level.Rig, "Dusman Testi");

            // --- 1. DEVRIYE: "ne zaman?" -------------------------------
            // Genis duz zemin. Tek soru zamanlama: ne zaman ziplayacaksin,
            // ne zaman ustune basacaksin.
            c.Ground(10f, "Zemin_Baslangic");
            c.Ground(14f, "Devriye_Alani");
            c.Patroller(4f);
            c.Patroller(10f, facingRight: true);

            // --- Nefes -------------------------------------------------
            // BOSLUK SART. Ilk surumde devriye alani ile checkpoint bitisikti
            // ve oynarken cikan sonuc: devriye yuruyup checkpoint'e geliyor,
            // oyuncu her dogdugunda oluyor. Iki dakikada 24 olum.
            //
            // Devriye bosluk gorunce doniyor, yani bosluk onun icin duvar.
            c.Gap(2f);
            c.Ground(6f, "Nefes_1");
            c.Checkpoint(2f);

            // --- 2. ATICI: "nereden?" ----------------------------------
            // Atici koridorun SONUNDA, sola atiyor. Oyuncu koridoru
            // gecerken mermilerin arasindan gecmek zorunda.
            //
            // Atici, checkpoint'ten en az 16 birim uzakta olmali - menzili
            // o kadar. Daha yakin olsaydi oyuncu checkpoint'te dogar dogmaz
            // mermi yerdi.
            c.Ground(20f, "Atici_Alani");
            c.Shooter(18f, fireLeft: true);
            c.Coins(3, heightAboveGround: 2.2f);

            // --- Nefes -------------------------------------------------
            c.Gap(2f);
            c.Ground(6f, "Nefes_2");
            c.Checkpoint(2f);

            // --- 3. UCAN: "cesaret edebiliyor musun?" ------------------
            // Bosluk 4 birim (%75) - ziplanabilir ama rahat degil.
            // Ucan dusman tam ortasinda: ustune basarsan hem onu oldurur
            // hem seni ziplatir, yani bosluk kolaylasir.
            //
            // Iki yol da GECERLI olmali. Ucan dusmani zorunlu yapsaydik
            // "dusman" degil "hareketli platform" olurdu.
            c.Ground(8f, "Ucan_Alani");
            c.Flyer(4f, heightAboveGround: 2.2f, horizontalRange: 2.5f);

            c.Gap(4f, coinArc: 4);

            c.Ground(8f, "Zemin_Inis");
            c.Flyer(4f, heightAboveGround: 3f, horizontalRange: 0f);   // sabit, sadece suzuluyor

            // --- Bitis -------------------------------------------------
            c.Ground(8f, "Zemin_Bitis");
            c.Goal();

            LevelScaffold.Finish(level, c, ScenePath);

            Debug.Log(
                "DUSMAN TEST ODASI HAZIR - Play'e bas, soldan saga ilerle.\n" +
                "-----------------------------------------------------\n" +
                "1) DEVRIYE (x=10..24)  \"ne zaman?\"\n" +
                "   - Yandan dokun -> olmelisin\n" +
                "   - Ustune bas   -> o olmeli, sen ziplamalisin\n" +
                "   - Kenara gelince donmeli, asagi dusmemeli\n\n" +
                "2) ATICI (x=32..52)    \"nereden?\"\n" +
                "   - ATES ONCESI kirmizilasip buyumeli (en az 0,3 sn)\n" +
                "   - Uyariyi gorup kacabiliyor musun? Goremiyorsan sure kisa\n" +
                "   - Ustune basilabiliyor mu?\n" +
                "   - Menzil disina cikinca ates kesmeli\n\n" +
                "3) UCAN (x=60..84)     \"cesaret edebiliyor musun?\"\n" +
                "   - 4 birimlik boslugu ziplayarak gec (gecilebilmeli)\n" +
                "   - Sonra ucan dusmanin ustune basarak gec - daha kolay mi?\n" +
                "   - Ustune basmak iyi hissettiriyor mu?\n\n" +
                "HER YERDE: ol, checkpoint'ten don, dusmanlar yerine donmus mu?\n" +
                "-----------------------------------------------------");
        }
    }
}
