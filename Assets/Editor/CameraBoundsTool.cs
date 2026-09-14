using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Platformer.CameraRig;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 03 — kamera sinirlarini bolumun gercek olculerinden hesaplar.
    ///
    /// Menu: Tools > 2D Platformer > Kamera Sinirlarini Hesapla
    ///
    /// Level01Builder de ayni hesabi kullanir - tek uygulama, iki cagiran.
    /// </summary>
    public static class CameraBoundsTool
    {
        private const float Padding = 2f;

        /// <summary>Paralaks arka planlar olcume katilmaz - bolumden cok genisler.</summary>
        private static readonly string[] IgnorePrefixes = { "Hills_", "Arkaplan", "Background" };

        // ===============================================================
        // Menu
        // ===============================================================

        [MenuItem("Tools/2D Platformer/Kamera Sinirlarini Hesapla", false, 30)]
        public static void Calculate()
        {
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

            if (!TryMeasureScene(out Bounds content, out int counted))
            {
                EditorUtility.DisplayDialog("Olculecek nesne yok",
                    "Sahnede sinir hesabina girecek Renderer bulunamadi.", "Tamam");
                return;
            }

            var so = new SerializedObject(follow);
            Vector2 previousMin = so.FindProperty("minBounds").vector2Value;
            Vector2 previousMax = so.FindProperty("maxBounds").vector2Value;

            Compute(follow, content, content.min.y, content.max.y,
                    out Vector2 min, out Vector2 max, out string note);

            Undo.RecordObject(follow, "Kamera Sinirlarini Hesapla");
            so.FindProperty("useBounds").boolValue = true;
            so.FindProperty("minBounds").vector2Value = min;
            so.FindProperty("maxBounds").vector2Value = max;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(follow);
            var scene = follow.gameObject.scene;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log(
                $"Kamera sinirlari guncellendi ({counted} nesne olculdu)\n" +
                $"  ONCE : min({previousMin.x:F1}, {previousMin.y:F1})  max({previousMax.x:F1}, {previousMax.y:F1})\n" +
                $"  SONRA: min({min.x:F1}, {min.y:F1})  max({max.x:F1}, {max.y:F1})\n" +
                note);

            bool save = EditorUtility.DisplayDialog(
                "Kamera sinirlari guncellendi",
                $"ONCE\n  min ({previousMin.x:F1}, {previousMin.y:F1})\n" +
                $"  max ({previousMax.x:F1}, {previousMax.y:F1})\n\n" +
                $"SONRA\n  min ({min.x:F1}, {min.y:F1})\n" +
                $"  max ({max.x:F1}, {max.y:F1})\n\n{note}\n\n" +
                "Sahneyi simdi kaydedeyim mi?",
                "Kaydet", "Simdilik kaydetme");

            if (save)
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                Debug.Log("Sahne kaydedildi.");
            }
        }

        // ===============================================================
        // Hesap — tek uygulama
        // ===============================================================

        /// <summary>
        /// Kamera sinirlarini hesaplar.
        ///
        /// Iki ayri sart var ve ikisi de saglanmali:
        ///
        /// 1) BOSLUK GOSTERME — sinirlar icerigi kapsamali, disinda bosluk
        ///    gorunmemeli. Ust tarafa zipla yuksekligi kadar pay birakilir.
        ///
        /// 2) KAMERA YERINE ULASABILMELI — kamera "zemin + kayma" konumuna
        ///    gitmek istiyor. Sinir bunu engellerse kamera surekli kirpilir
        ///    ve HIC HAREKET ETMEZ. Bolum dikeyde kisaysa bu sart 1'den
        ///    daha genis sinir gerektirir; o zaman biraz bosluk gormek
        ///    kameranin donmasindan iyidir.
        ///
        /// Alt sinir ayrica olum cizgisine kadar iner - oyuncu nereye
        /// dustugunu gormeli.
        /// </summary>
        internal static void Compute(
            CameraFollow follow, Bounds content,
            float lowestGroundTop, float highestGroundTop,
            out Vector2 min, out Vector2 max, out string note)
        {
            var lines = new List<string>();

            float jumpHeight = TryGetJumpHeight(out float jh) ? jh : 3f;
            float topPadding = Mathf.Max(Padding, jumpHeight + 1f);
            lines.Add($"  ust pay {topPadding:F1} (zipla {jumpHeight:F2} + 1)");

            // --- Sart 1: icerigi kapsa ---
            min = new Vector2(content.min.x - Padding, content.min.y - Padding);
            max = new Vector2(content.max.x + Padding, content.max.y + topPadding);

            // --- Olum cizgisi: asagi dusus gorunur olsun ---
            if (TryGetKillPlane(out float killPlaneY))
            {
                min.y = Mathf.Min(min.y, killPlaneY - 1f);
                lines.Add($"  olum cizgisi {killPlaneY:F1} — alt sinir oraya indirildi");
            }

            // --- Sart 2: kamera dogal konumuna ulasabilsin ---
            var cam = follow.GetComponent<UnityEngine.Camera>();
            if (cam != null && cam.orthographic)
            {
                float half = cam.orthographicSize;
                float offsetY = GetCameraOffsetY(follow);

                // En yuksek zeminde kameranin gitmek istedigi yer
                float highestDesired = highestGroundTop + offsetY;
                float neededMax = highestDesired + half;

                // En alcak zeminde
                float lowestDesired = lowestGroundTop + offsetY;
                float neededMin = lowestDesired - half;

                if (neededMax > max.y)
                {
                    lines.Add($"  ust sinir {max.y:F1} -> {neededMax:F1} " +
                              $"(kamera en yuksek katta {highestDesired:F1}'e ulasabilsin)");
                    max.y = neededMax;
                }

                if (neededMin < min.y)
                {
                    lines.Add($"  alt sinir {min.y:F1} -> {neededMin:F1}");
                    min.y = neededMin;
                }

                // Yataydaki ayni sart
                float neededMaxX = content.max.x - half * cam.aspect;
                float neededMinX = content.min.x + half * cam.aspect;
                if (neededMinX > neededMaxX)
                {
                    lines.Add($"  NOT: bolum kameradan dar ({content.size.x:F1} birim), " +
                              "kamera yatayda ortalanacak");
                }
            }

            note = string.Join("\n", lines);
        }

        // ===============================================================
        // Olcum
        // ===============================================================

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

        /// <summary>CameraFollow'un dikey kaymasini okur (private SerializeField).</summary>
        private static float GetCameraOffsetY(CameraFollow follow)
        {
            var so = new SerializedObject(follow);
            SerializedProperty prop = so.FindProperty("offset");
            return prop != null ? prop.vector2Value.y : 1.2f;
        }

        /// <summary>Oyuncunun GERCEK zipla yuksekligi (ayrik fizik kaybi dahil).</summary>
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
    }
}
