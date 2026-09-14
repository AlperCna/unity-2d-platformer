using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using Platformer.Gameplay;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 07 gorev 6 — okunabilirlik denetimi.
    ///
    /// Epic alti madde sayiyor. Dordu OLCULEBILIR, ikisi gorsel yargi
    /// gerektiriyor. Olculebilirleri goz kararina birakmak, bir bolumde
    /// unutulup fark edilmemesi demek - o yuzden araca donusturuldu.
    ///
    ///   olculebilir  collider gorselden kucuk mu
    ///   olculebilir  uyari suresi >= 0,3 sn mi
    ///   olculebilir  checkpoint'e cok yakin mi
    ///   olculebilir  dongu yeterince yavas mi (desen ogrenilebilsin)
    ///   ELLE         tehlike oldugu bakar bakmaz anlasiliyor mu
    ///   ELLE         dekoratif ogelerle karistirilabilir mi
    ///
    /// Acik sahneyi tarar - koddan uretilmis de olsa elle boyanmis da olsa.
    ///
    /// Menu: Tools > 2D Platformer > Bolumu Denetle
    /// </summary>
    public static class HazardAudit
    {
        /// <summary>Checkpoint'e bu mesafeden yakin tehlike, dogar dogmaz oldurur.</summary>
        private const float CheckpointSafeRadius = 3f;

        /// <summary>Bundan hizli dongu, desenin ogrenilmesine izin vermiyor.</summary>
        private const float MinLearnableCycle = 1.5f;

        [MenuItem("Tools/2D Platformer/Bolumu Denetle", false, 40)]
        public static void Run()
        {
            var report = new StringBuilder();
            int problems = 0;
            int checkedCount = 0;

            List<Vector2> checkpoints = FindCheckpoints();

            report.AppendLine("BOLUM DENETIMI");
            report.AppendLine("--------------------------------------------------");

            // --- Ritimli tehlikeler ---
            foreach (TimedHazard hazard in FindAll<TimedHazard>())
            {
                checkedCount++;
                problems += AuditTimed(hazard, checkpoints, report);
            }

            // --- Statik tehlikeler ---
            foreach (Hazard hazard in FindAll<Hazard>())
            {
                checkedCount++;
                problems += AuditStatic(hazard, checkpoints, report);
            }

            // --- Toplanabilirler (Epic 08) ---
            report.AppendLine();
            report.AppendLine("TOPLANABILIRLER");

            foreach (Gameplay.Collectible item in FindAll<Gameplay.Collectible>())
            {
                checkedCount++;
                problems += AuditCollectible(item, report);
            }

            problems += AuditSecrets(report);

            report.AppendLine("--------------------------------------------------");
            report.AppendLine($"{checkedCount} oge denetlendi, {problems} sorun bulundu.");
            report.AppendLine();
            report.AppendLine("ELLE BAKILACAK IKI MADDE (otomatiklestirilemez):");
            report.AppendLine("  - Tehlike oldugu BAKAR BAKMAZ anlasiliyor mu?");
            report.AppendLine("  - Dekoratif ogelerle karistirilabilir mi?");
            report.AppendLine("  Ikisi icin: ekrana gozunu kisarak bak. Tehlikeler");
            report.AppendLine("  siluetten ayirt edilebiliyorsa gecti.");

            if (problems > 0) Debug.LogWarning(report.ToString());
            else Debug.Log(report.ToString());
        }

        // ---------------------------------------------------------------

        private static int AuditTimed(TimedHazard hazard, List<Vector2> checkpoints,
                                      StringBuilder report)
        {
            int problems = 0;
            var so = new SerializedObject(hazard);

            float warning = so.FindProperty("warningDuration").floatValue;
            float cycle = so.FindProperty("cycleDuration").floatValue;
            float active = so.FindProperty("activeDuration").floatValue;

            if (warning < TimedHazard.MinWarningDuration)
            {
                problems++;
                report.AppendLine($"  [UYARI KISA] {Path(hazard.transform)}: {warning:0.00} sn " +
                                  $"(en az {TimedHazard.MinWarningDuration}). " +
                                  "Insan tepki suresinin altinda - haksiz hissettirir.");
            }

            if (cycle < MinLearnableCycle)
            {
                problems++;
                report.AppendLine($"  [DONGU HIZLI] {Path(hazard.transform)}: {cycle:0.00} sn " +
                                  $"(en az {MinLearnableCycle}). " +
                                  "Oyuncu deseni ogrenemeden tekrarliyor.");
            }

            if (active + warning >= cycle)
            {
                problems++;
                report.AppendLine($"  [KAPANMIYOR] {Path(hazard.transform)}: aktif({active:0.00}) + " +
                                  $"uyari({warning:0.00}) >= dongu({cycle:0.00}). " +
                                  "Guvenli an kalmiyor.");
            }

            problems += CheckCheckpointDistance(hazard.transform, checkpoints, report);

            // FireJet'in collider'i CALISMA ANINDA boyutlaniyor (alev uzayip
            // kisaliyor); edit modunda 0,01. Olcmek anlamsiz olurdu - onun
            // yerine kod, collider'i alevin %75'i yapacagini garanti ediyor.
            if (!(hazard is FireJet))
            {
                problems += CheckColliderSmallerThanVisual(hazard.gameObject, report);
            }

            return problems;
        }

        private static int AuditStatic(Hazard hazard, List<Vector2> checkpoints,
                                       StringBuilder report)
        {
            int problems = 0;
            problems += CheckColliderSmallerThanVisual(hazard.gameObject, report);
            problems += CheckCheckpointDistance(hazard.transform, checkpoints, report);
            return problems;
        }

        /// <summary>
        /// Collider gorselden kucuk mu?
        ///
        /// Epic'in tuzak listesindeki "Collider'i sprite'la ayni yapmak"
        /// maddesi. Oyuncu "degmedim ki!" der ve haklidir. Her zaman
        /// oyuncu lehine comert ol.
        /// </summary>
        /// <summary>
        /// Toplanabilir ULASILABILIR mi?
        ///
        /// Epic 08'in kabul kriteri "hicbir para ulasilamaz yerde degil
        /// (kendin test ettin)" diyor. Elle test etmek demek, bir bolumde
        /// gozden kacirmak demek - 30 parayi tek tek denemez kimse.
        ///
        /// Altina isin atip zemini buluyoruz. Zeminden yukseklik, oyuncunun
        /// erisebilecegi en yuksek noktayi asiyorsa sorun var.
        ///
        /// Erisim hesabi: zipla yuksekligi 3,03 + oyuncunun yari boyu 0,48
        /// + toplama yaricapi 0,45 = 3,96. Pay birakip 3,9 kullaniyoruz.
        /// </summary>
        private const float MaxCollectibleHeight = 3.9f;

        private static int AuditCollectible(Gameplay.Collectible item, StringBuilder report)
        {
            Vector2 origin = item.transform.position;

            // Toplanabilirin KENDI collider'i isini engellemesin
            var self = item.GetComponent<Collider2D>();
            bool wasEnabled = self != null && self.enabled;
            if (self != null) self.enabled = false;

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 30f,
                                                 LayerMask.GetMask("Ground"));

            if (self != null) self.enabled = wasEnabled;

            if (hit.collider == null)
            {
                report.AppendLine($"  [ZEMIN YOK] {Path(item.transform)}: altinda 30 birim " +
                                  "boyunca zemin yok. Dipsiz bosluk uzerinde duruyor.");
                return 1;
            }

            float height = origin.y - hit.point.y;

            if (height <= MaxCollectibleHeight)
            {
                report.AppendLine($"  [tamam] {Path(item.transform)}: zeminden {height:0.00} " +
                                  $"birim (en fazla {MaxCollectibleHeight}).");
                return 0;
            }

            report.AppendLine($"  [ULASILAMAZ] {Path(item.transform)}: zeminden {height:0.00} " +
                              $"birim yukarida, oyuncu en fazla {MaxCollectibleHeight} " +
                              "birime uzanabiliyor.");
            return 1;
        }

        /// <summary>
        /// Her sirrin IPUCUSU var mi?
        ///
        /// Ipucusuz sir, sir degil rastlantidir - oyuncuyu degil sansi
        /// odullendirir. Epic 08 bunu acikca sart kosuyor.
        /// </summary>
        private static int AuditSecrets(StringBuilder report)
        {
            int problems = 0;
            var secrets = FindAll<Gameplay.SecretArea>();

            if (secrets.Length == 0) return 0;

            report.AppendLine();
            report.AppendLine("SIRLAR");

            foreach (Gameplay.SecretArea secret in secrets)
            {
                if (string.IsNullOrWhiteSpace(secret.Hint))
                {
                    problems++;
                    report.AppendLine($"  [IPUCU YOK] {Path(secret.transform)}: " +
                                      "ipucusuz sir, sansi odullendirir.");
                }
                else
                {
                    report.AppendLine($"  [tamam] {Path(secret.transform)}: \"{secret.Hint}\"");
                }
            }

            if (secrets.Length > 1)
            {
                problems++;
                report.AppendLine($"  [COK SIR] Bolumde {secrets.Length} sir var. " +
                                  "Epic 08: bolum basina BIR tane - ikincisi ilkini degersizlestirir.");
            }

            return problems;
        }

        private static int CheckColliderSmallerThanVisual(GameObject go, StringBuilder report)
        {
            var renderer = go.GetComponentInChildren<SpriteRenderer>();
            var collider = go.GetComponent<Collider2D>();

            if (renderer == null || collider == null) return 0;
            if (renderer.sprite == null) return 0;

            Bounds visual = renderer.bounds;
            Bounds hit = collider.bounds;

            // Cok kucuk gorseller (uretilmemis/kapali) olcum disi
            if (visual.size.x < 0.01f || visual.size.y < 0.01f) return 0;

            float ratioX = hit.size.x / visual.size.x;
            float ratioY = hit.size.y / visual.size.y;

            if (ratioX <= 1f && ratioY <= 1f)
            {
                report.AppendLine($"  [tamam] {Path(go.transform)}: collider gorselin " +
                                  $"%{ratioX * 100f:0}x / %{ratioY * 100f:0}y'si.");
                return 0;
            }

            report.AppendLine($"  [COLLIDER BUYUK] {Path(go.transform)}: collider gorselin " +
                              $"%{ratioX * 100f:0}x / %{ratioY * 100f:0}y'si. " +
                              "Oyuncu 'degmedim ki' der ve haklidir.");
            return 1;
        }

        private static int CheckCheckpointDistance(Transform hazard, List<Vector2> checkpoints,
                                                   StringBuilder report)
        {
            foreach (Vector2 cp in checkpoints)
            {
                float distance = Vector2.Distance(cp, hazard.position);
                if (distance > CheckpointSafeRadius) continue;

                report.AppendLine($"  [CHECKPOINT YAKIN] {Path(hazard.transform)}: " +
                                  $"{distance:0.0} birim uzakta (en az {CheckpointSafeRadius}). " +
                                  "Oyuncu dogar dogmaz olur.");
                return 1;
            }

            return 0;
        }

        // ---------------------------------------------------------------

        private static List<Vector2> FindCheckpoints()
        {
            var result = new List<Vector2>();
            foreach (Checkpoint cp in FindAll<Checkpoint>())
            {
                result.Add(cp.transform.position);
            }
            return result;
        }

        private static T[] FindAll<T>() where T : Object
        {
#if UNITY_2023_1_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsInactive.Include,
                                               FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<T>(true);
#endif
        }

        /// <summary>Hiyerarsideki yol - hangi nesne oldugunu bulabilmek icin.</summary>
        private static string Path(Transform t)
        {
            string path = t.name;
            Transform parent = t.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }
    }
}
