using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 07 — bes tehlike/engel cesidini art arda denemek icin test odasi.
    ///
    /// Dusman test odasiyla ayni mantik: bolume koyup sonra "bu haksizmis"
    /// demek yerine once burada denenir.
    ///
    /// Her bolum bir parcanin sordugu soruyu izole ediyor:
    ///   aralikli diken -> ne zaman gececeksin?
    ///   ates          -> bekleyebilir misin?
    ///   dusen platform -> durmadan ilerleyebilir misin?
    ///   tek yonlu     -> hangi kattan?
    ///   zipla pedi    -> nereye gidecegini gorebiliyor musun?
    ///
    /// Menu: Tools > 2D Platformer > Tehlike Test Odasi
    /// </summary>
    public static class HazardTestRoom
    {
        private const string ScenePath = "Assets/Scenes/TehlikeTestOdasi.unity";

        [MenuItem("Tools/2D Platformer/Tehlike Test Odasi", false, 5)]
        public static void Build()
        {
            if (!EditorGuards.RequireEditMode("Tehlike Test Odasi",
                "Yeni sahne olusturmak Play modunda mumkun degil.")) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (!EditorUtility.DisplayDialog("Tehlike Test Odasi",
                    $"{ScenePath} olusturulacak.\n\n" +
                    "Bes tehlike/engel cesidi ayri ayri denenebilir.\n\nDevam edilsin mi?",
                    "Olustur", "Vazgec"))
            {
                return;
            }

            // levelIndex 98: gercek bolumlerin kayit yuvalarina dokunmaz.
            LevelScaffold.Level level = LevelScaffold.Create(
                "Tehlike Testi", levelIndex: 98, designVersion: 1,
                playerSpawn: new Vector2(2f, 2.5f));

            var c = new LevelCursor(level.Entities, LevelBuilder.groundLayer,
                                    level.Rig, "Tehlike Testi");

            // --- 1. ARALIKLI DIKEN: "ne zaman gececeksin?" -------------
            // Uc diken, faz kaydirmali (0 / 0,33 / 0,66). Ayni anda
            // cikmiyorlar, dalga gibi. Hepsi ayni fazda olsaydi tek bir
            // "bekle, gec" olurdu; kaydirmali olunca ritim cikiyor.
            c.Ground(10f, "Zemin_Baslangic");
            c.Ground(12f, "Diken_Alani");
            c.TimedSpikes(3f, phaseOffset: 0f);
            c.TimedSpikes(6f, phaseOffset: 0.33f);
            c.TimedSpikes(9f, phaseOffset: 0.66f);

            c.Gap(2f);
            c.Ground(6f, "Nefes_1");
            c.Checkpoint(2f);

            // --- 2. ATES: "bekleyebilir misin?" ------------------------
            // Diken seni HIZLANDIRIR (gecip gitmelisin), ates seni
            // DURDURUR (beklemelisin). Yan yana konunca oyuncu iki farkli
            // refleksi ayni nefeste kullanmak zorunda.
            c.Ground(14f, "Ates_Alani");
            c.Fire(4f, phaseOffset: 0f, length: 3.5f);
            c.Fire(10f, phaseOffset: 0.5f, length: 3.5f);

            // --- 2b. TAVAN DIKENI: "ne kadar ziplayabilirsin?" --------
            // Alcak koridor. Zemin guvenli ama tavanda diken var: tam
            // zipla yaparsan olursun. Ayni prefab, 180 derece dondurulmus.
            //
            // Ayri bir prefab yapmadik cunku dort yon dort ayri prefab
            // demek olurdu ve biri kacinilmaz olarak digerlerinden farkli
            // ayarlanirdi - comert hitbox kurali bozulurdu.
            c.Ground(14f, "Tavan_Koridoru");
            c.Ceiling(clearance: 4f, thickness: 2f);
            c.Spikes(4, offsetFromSegmentStart: 5f,
                     facing: LevelCursor.SpikeFacing.Ceiling, surfaceOffset: 4f);
            c.Coins(3, heightAboveGround: 1.4f);

            c.Gap(2f);
            c.Ground(6f, "Nefes_2");
            c.Checkpoint(2f);

            // --- 3. DUSEN PLATFORM: "durmadan ilerleyebilir misin?" ----
            // Gecilemez bir bosluk degil - 3 birim, ziplanabilir. Ama
            // platformlari kullanmak daha kolay. Tehlikeyi OYUNCU tetikliyor.
            c.Ground(6f, "Dusen_Giris");
            c.Gap(3f);
            c.Falling(atX: c.X - 2f, heightAboveGround: -0.5f, width: 2f);

            c.Ground(4f, "Dusen_Ara");
            c.Gap(3f);
            c.Falling(atX: c.X - 2f, heightAboveGround: -0.5f, width: 2f);

            c.Ground(8f, "Dusen_Cikis");
            c.Checkpoint(2f);

            // --- 4. TEK YONLU PLATFORM: "hangi kattan?" ----------------
            // Iki kat. Alttan zipla ile ustune cikilir, ASAGI + ZIPLA ile
            // geri inilir. Ust katta para var - inmek istemezsen almazsin.
            c.Ground(14f, "TekYonlu_Alani");
            // 2,0 birim: ust yuzey 2,25'e geliyor, yani maksimum ziplamanin
            // (3,03) %74'u. 3,0 vermistim ve ULASILAMIYORDU - platformun
            // kalinligini hesaba katmayi unutmustum.
            c.OneWay(atX: c.X - 10f, heightAboveGround: 2f, width: 4f);
            c.OneWay(atX: c.X - 4f, heightAboveGround: 2f, width: 4f);
            c.Coins(3, heightAboveGround: 3.6f);

            c.Gap(2f);
            c.Ground(6f, "Nefes_3");
            c.Checkpoint(2f);

            // --- 5. ZIPLA PEDI: "nereye gidecegini gorebiliyor musun?" -
            // Ped normal ziplamanin (3,03) iki katini asiyor. Yukarida
            // para var: pedi kullanmadan ulasilamaz.
            c.Ground(10f, "Ped_Alani");
            c.Pad(4f, launchHeight: 6.5f);
            c.Coins(3, heightAboveGround: 5.5f);

            // --- Bitis -------------------------------------------------
            c.Ground(8f, "Zemin_Bitis");
            c.Goal();

            LevelScaffold.Finish(level, c, ScenePath);

            Debug.Log(
                "TEHLIKE TEST ODASI HAZIR - Play'e bas, soldan saga ilerle.\n" +
                "-----------------------------------------------------\n" +
                "1) ARALIKLI DIKEN   \"ne zaman gececeksin?\"\n" +
                "   - Cikmadan ONCE yarim gorunup uyarmali (>= 0,3 sn)\n" +
                "   - Uc diken AYNI ANDA cikmamali - faz kaydirma calisiyor mu?\n" +
                "   - Uyariyi gorup durabiliyor musun, yoksa haksiz mi?\n\n" +
                "2) ATES             \"bekleyebilir misin?\"\n" +
                "   - Once kucuk bir alev cikip uyarmali\n" +
                "   - Iki ates ters fazda - biri kapaliyken digeri acik\n" +
                "   - Alevin kenarini siyirinca olmemeli (collider dar)\n\n" +
                "2b) TAVAN DIKENI    \"ne kadar ziplayabilirsin?\"\n" +
                "   - Koridorda YURU: guvenli olmali\n" +
                "   - Koridorda TAM ZIPLA: tavandaki dikene carpmalisin\n" +
                "   - Diken asagi bakiyor mu, collider da dondu mu?\n\n" +
                "3) DUSEN PLATFORM   \"durmadan ilerleyebilir misin?\"\n" +
                "   - Basinca titremeli, sonra dusmeli\n" +
                "   - Titreme sona dogru SIDDETLENMELI\n" +
                "   - Birkac saniye sonra geri gelmeli\n" +
                "   - Ustunde dururken platformla birlikte dusmemelisin\n\n" +
                "4) TEK YONLU        \"hangi kattan?\"\n" +
                "   - Alttan ziplayinca icinden gecip ustune cikmalisin\n" +
                "   - ASAGI + ZIPLA ile asagi inebilmelisin\n\n" +
                "5) ZIPLA PEDI       \"nereye gidecegini gorebiliyor musun?\"\n" +
                "   - Ustune dusunce firlatmali, yukarideki paralara ulasmalisin\n" +
                "   - Ped basinca ezilme animasyonu oynatmali\n" +
                "   - Yukari cikarken tekrar tetiklenmemeli\n\n" +
                "HER YERDE: ol, checkpoint'ten don - tehlikeler basa sarmis mi?\n" +
                "-----------------------------------------------------");
        }
    }
}
