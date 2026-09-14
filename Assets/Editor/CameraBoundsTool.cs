using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Platformer.CameraRig;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 03 - Gorev 6: sahnedeki her seyi olcup kamera sinirlarini yazar.
    ///
    /// Neden gerekli: her bolumde Min/Max Bounds elle girilmezse kamera bolumun
    /// disindaki bosluga bakar. Unutulmasi cok kolay ve cok amatorce gorunur.
    ///
    /// Menu: Tools > 2D Platformer > Kamera Sinirlarini Hesapla
    /// </summary>
    public static class CameraBoundsTool
    {
        /// <summary>Sinirlarin disina birakilacak pay.</summary>
        private const float Padding = 2f;

        /// <summary>Bu adlarla baslayan nesneler hesaba KATILMAZ (paralaks arka plan).</summary>
        private static readonly string[] IgnorePrefixes = { "Hills_", "Arkaplan", "Background" };

        [MenuItem("Tools/2D Platformer/Kamera Sinirlarini Hesapla", false, 30)]
        public static void Calculate()
        {
            // Play modunda sahne degistirilemez: MarkSceneDirty
            // "InvalidOperationException: This cannot be used during play mode" atar.
            // Ustelik Play'den cikinca degisiklikler zaten geri alinirdi.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Play modunda calismaz",
                    "Kamera sinirlari Play modunda degistirilemez — Play'den cikinca " +
                    "degisiklikler zaten geri alinir.\n\nOnce Play'i durdur, sonra tekrar dene.",
                    "Tamam");
                return;
            }

#if UNITY_2023_1_OR_NEWER
            var follow = Object.FindAnyObjectByType<CameraFollow>();
#else
            var follow = Object.FindObjectOfType<CameraFollow>();
#endif
            if (follow == null)
            {
                EditorUtility.DisplayDialog("Kamera bulunamadi",
                    "Sahnede CameraFollow bileseni olan bir kamera yok.", "Tamam");
                return;
            }

            if (!TryMeasureScene(out Bounds total, out int counted))
            {
                EditorUtility.DisplayDialog("Olculecek nesne yok",
                    "Sahnede sinir hesabina girecek Renderer bulunamadi.", "Tamam");
                return;
            }

            // Ust pay ziplama yuksekligini kapsamali: oyuncu en yuksek platformdan
            // zipladiginda tepe noktasi gorunur olmali. Sabit bir pay verirsek
            // (eskiden 2 birimdi) bu ancak tesaduf eseri yeterli olur.
            float topPadding = Padding;
            string topPaddingNote = $"ust pay {Padding:F1} (sabit)";

            if (TryGetJumpHeight(out float jumpHeight))
            {
                topPadding = Mathf.Max(Padding, jumpHeight + 1f);
                topPaddingNote = $"ust pay {topPadding:F1} (zipla {jumpHeight:F2} + 1)";
            }

            var min = new Vector2(total.min.x - Padding, total.min.y - Padding);
            var max = new Vector2(total.max.x + Padding, total.max.y + topPadding);

            // --- Alt siniri olum cizgisine kadar indir ---
            //
            // Oyuncu bosluga duserken NEREYE dustugunu gormeli. Alt siniri sadece
            // zeminin altina koyarsak kamera orada takilir ve karakter ekrandan
            // kaybolur. Olum cizgisinin altinda gorulecek bir sey yok - oyuncu
            // zaten oldu - ama o noktaya kadar takip etmeli.
            string killPlaneNote = "GameManager yok — varsayilan pay kullanildi";
            float killPlaneY;

            if (TryGetKillPlane(out killPlaneY))
            {
                min.y = Mathf.Min(min.y, killPlaneY - 1f);
                killPlaneNote = $"olum cizgisi {killPlaneY:F1} — alt sinir oraya indirildi";
            }
            else
            {
                // GameManager yoksa (test sahnesi) yine de dusmeyi gorunur kil
                min.y = Mathf.Min(min.y, total.min.y - 14f);
            }

            // --- Kamera bu sinirlara sigiyor mu? ---
            var cam = follow.GetComponent<UnityEngine.Camera>();
            string fitNote = "";
            if (cam != null && cam.orthographic)
            {
                float neededHeight = cam.orthographicSize * 2f;
                float availableHeight = max.y - min.y;

                if (availableHeight < neededHeight)
                {
                    fitNote =
                        $"\n  UYARI: bolum {availableHeight:F1} birim yuksek ama kamera " +
                        $"{neededHeight:F1} birim goruyor.\n" +
                        $"  Kamera dikeyde hic hareket edemeyecek (ortalanacak).\n" +
                        $"  Cozum: Orthographic Size'i {(availableHeight / 2f):F1} veya altina indir.";
                }
            }

            Undo.RecordObject(follow, "Kamera Sinirlarini Hesapla");

            var so = new SerializedObject(follow);
            so.FindProperty("useBounds").boolValue = true;
            Vector2 previousMin = so.FindProperty("minBounds").vector2Value;
            Vector2 previousMax = so.FindProperty("maxBounds").vector2Value;
            so.FindProperty("minBounds").vector2Value = min;
            so.FindProperty("maxBounds").vector2Value = max;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(follow);
            var scene = follow.gameObject.scene;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log(
                $"Kamera sinirlari guncellendi ({counted} nesne olculdu)\n" +
                $"  ONCE : min({previousMin.x:F1}, {previousMin.y:F1})  " +
                $"max({previousMax.x:F1}, {previousMax.y:F1})\n" +
                $"  SONRA: min({min.x:F1}, {min.y:F1})  max({max.x:F1}, {max.y:F1})\n" +
                $"  bolum olcusu: {(max.x - min.x):F1} x {(max.y - min.y):F1} birim\n" +
                $"  {killPlaneNote}\n" +
                $"  {topPaddingNote}" + fitNote);

            // Kaydetmeyi hatirlat - yoksa degisiklik bellekte kalir
            bool save = EditorUtility.DisplayDialog(
                "Kamera sinirlari guncellendi",
                $"ONCE\n  min ({previousMin.x:F1}, {previousMin.y:F1})\n" +
                $"  max ({previousMax.x:F1}, {previousMax.y:F1})\n\n" +
                $"SONRA\n  min ({min.x:F1}, {min.y:F1})\n" +
                $"  max ({max.x:F1}, {max.y:F1})\n\n" +
                $"{killPlaneNote}\n{topPaddingNote}{fitNote}\n\n" +
                "Sahneyi simdi kaydedeyim mi?",
                "Kaydet", "Simdilik kaydetme");

            if (save)
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                Debug.Log("Sahne kaydedildi.");
            }
        }

        /// <summary>
        /// Oyuncunun GERCEK zipla yuksekligini okur.
        /// RealJumpHeight, ayrik fizik kaybini hesaba katiyor - ayardaki
        /// jumpHeight degil, oyunda ulasilan yukseklik.
        /// </summary>
        private static bool TryGetJumpHeight(out float jumpHeight)
        {
            jumpHeight = 0f;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return false;

            var controller = player.GetComponent<Platformer.Player.PlayerController2D>();
            if (controller == null) return false;

            jumpHeight = controller.GetSettings().RealJumpHeight;
            return jumpHeight > 0.01f;
        }

        /// <summary>Sahnedeki GameManager'dan olum cizgisini okur.</summary>
        private static bool TryGetKillPlane(out float killPlaneY)
        {
            killPlaneY = 0f;

#if UNITY_2023_1_OR_NEWER
            var manager = Object.FindAnyObjectByType<Platformer.Core.GameManager>();
#else
            var manager = Object.FindObjectOfType<Platformer.Core.GameManager>();
#endif
            if (manager == null) return false;

            var so = new SerializedObject(manager);
            SerializedProperty prop = so.FindProperty("killPlaneY");
            if (prop == null) return false;

            killPlaneY = prop.floatValue;
            return true;
        }

        /// <summary>
        /// Sahnedeki tum Renderer'larin kapladigi toplam alani olcer.
        /// Paralaks arka planlar haric tutulur - onlar bolumden cok daha genis
        /// olduklari icin siniri anlamsiz sekilde buyuturler.
        /// </summary>
        private static bool TryMeasureScene(out Bounds total, out int counted)
        {
            total = new Bounds();
            counted = 0;

#if UNITY_2023_1_OR_NEWER
            Renderer[] renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
#else
            Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
#endif

            var included = new List<Renderer>();
            foreach (Renderer r in renderers)
            {
                if (r == null || !r.enabled) continue;
                if (ShouldIgnore(r.transform)) continue;
                included.Add(r);
            }

            if (included.Count == 0) return false;

            total = included[0].bounds;
            foreach (Renderer r in included) total.Encapsulate(r.bounds);

            counted = included.Count;
            return true;
        }

        /// <summary>Nesne veya ust nesnelerinden biri disarida birakilmis mi?</summary>
        private static bool ShouldIgnore(Transform t)
        {
            while (t != null)
            {
                foreach (string prefix in IgnorePrefixes)
                {
                    if (t.name.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                t = t.parent;
            }
            return false;
        }
    }
}
