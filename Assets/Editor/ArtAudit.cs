using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 11 gorev 3 — uc okunabilirlik testi.
    ///
    /// Epic'in tezi: "Amator gorunen oyunlarin cogu kotu cizildigi icin
    /// degil, TUTARSIZ oldugu icin amator gorunur."
    ///
    /// Tutarlilik olculebilir, guzellik olculemez. Bu arac olculebilir
    /// kismi yapiyor:
    ///
    ///   SILUET     sekiller birbirinden ayirt ediliyor mu
    ///   GRI TONLAMA  parlakliklar ayirt ediliyor mu (renk korlugu testi)
    ///   TUTARLILIK   ayni PPU, ayni filtre, ayni sikistirma
    ///
    /// EN ONEMLI KURAL - UCU BIR TAKIM:
    /// Bir cift siluetten ayirt edilemiyorsa ama parlakliklari cok
    /// farkliysa SORUN YOKTUR. Oyuncu onlari yine ayirt eder.
    /// O yuzden bayrak ancak IKISI BIRDEN basarisizsa kalkiyor.
    ///
    /// Bu ayrim onemli: her testi ayri ayri gecmeye calismak, gereksiz
    /// yere sanati bozmaya yol acar.
    ///
    /// Menu: Tools > 2D Platformer > Sanati Denetle
    /// </summary>
    public static class ArtAudit
    {
        private const string ArtFolder = "Assets/Art";

        /// <summary>Siluetler bu oranin ustunde ortusurse "ayirt edilemiyor".</summary>
        private const float SilhouetteLimit = 0.75f;

        /// <summary>Parlaklik farki bundan azsa "gri tonlamada ayrilmiyor".</summary>
        private const float LuminanceLimit = 25f;

        /// <summary>Siluet karsilastirmasi icin ortak olcek.</summary>
        private const int NormalizeTo = 24;

        private class Art
        {
            public string name;
            public bool[] mask;      // NormalizeTo x NormalizeTo
            public float luminance;
            public int width, height;
            public TextureImporter importer;
        }

        [MenuItem("Tools/2D Platformer/Sanati Denetle", false, 42)]
        public static void Run()
        {
            List<Art> arts = LoadAll();

            if (arts.Count == 0)
            {
                Debug.LogWarning("Assets/Art altinda denetlenecek sprite yok. " +
                                 "Once Tools > 2D Platformer > Sadece Grafikleri Uret calistir.");
                return;
            }

            var report = new StringBuilder();
            report.AppendLine("SANAT DENETIMI");
            report.AppendLine("--------------------------------------------------");

            int problems = 0;
            problems += AuditConsistency(arts, report);
            problems += AuditReadability(arts, report);

            report.AppendLine("--------------------------------------------------");
            report.AppendLine($"{arts.Count} sprite denetlendi, {problems} sorun bulundu.");
            report.AppendLine();
            report.AppendLine("OTOMATIKLESTIRILEMEYEN: bulaniklik testi.");
            report.AppendLine("  Oyunu oynatip ekrana GOZUNU KISARAK bak. Oyuncu,");
            report.AppendLine("  tehlike ve zemin hala ayirt edilebiliyor mu?");

            if (problems > 0) Debug.LogWarning(report.ToString());
            else Debug.Log(report.ToString());
        }

        // ---------------------------------------------------------------

        /// <summary>
        /// Tutarlilik: ayni PPU, ayni filtre, ayni sikistirma.
        ///
        /// Epic'in "amator gorunum" tanisi tam olarak bu: farkli piksel
        /// yogunluklari ve karisik ice aktarma ayarlari.
        /// </summary>
        private static int AuditConsistency(List<Art> arts, StringBuilder report)
        {
            report.AppendLine("TUTARLILIK");
            int problems = 0;

            foreach (Art a in arts)
            {
                var issues = new List<string>();

                if (!Mathf.Approximately(a.importer.spritePixelsPerUnit, SpriteFactory.PixelsPerUnit))
                    issues.Add($"PPU {a.importer.spritePixelsPerUnit} (olmali {SpriteFactory.PixelsPerUnit})");

                if (a.importer.filterMode != FilterMode.Point)
                    issues.Add($"filtre {a.importer.filterMode} (olmali Point)");

                if (a.importer.textureCompression != TextureImporterCompression.Uncompressed)
                    issues.Add("sikistirma acik (olmali None)");

                if (issues.Count > 0)
                {
                    problems++;
                    report.AppendLine($"  [AYAR] {a.name}: {string.Join(", ", issues)}");
                }
            }

            if (problems == 0) report.AppendLine("  [tamam] hepsi ayni standartta.");
            return problems;
        }

        /// <summary>
        /// Siluet + gri tonlama, BIRLIKTE degerlendiriliyor.
        /// </summary>
        private static int AuditReadability(List<Art> arts, StringBuilder report)
        {
            report.AppendLine();
            report.AppendLine("OKUNABILIRLIK");

            int problems = 0;
            var weak = new List<string>();

            for (int i = 0; i < arts.Count; i++)
            {
                for (int j = i + 1; j < arts.Count; j++)
                {
                    float iou = Overlap(arts[i].mask, arts[j].mask);
                    float dl = Mathf.Abs(arts[i].luminance - arts[j].luminance);

                    bool sameShape = iou > SilhouetteLimit;
                    bool sameTone = dl < LuminanceLimit;

                    if (!sameShape || !sameTone) continue;

                    problems++;
                    report.AppendLine(
                        $"  [AYIRT EDILEMIYOR] {arts[i].name} <-> {arts[j].name}\n" +
                        $"      siluet %{iou * 100f:0} ortusuyor VE parlaklik farki " +
                        $"sadece {dl:0.0}.\n" +
                        $"      Ikisinden birini degistir: sekli ayristir " +
                        "ya da tonu ayir.");
                }
            }

            // Bilgi amacli: en yakin ciftler, bayrak olmasa da
            report.AppendLine();
            report.AppendLine("  Parlakliklar (0-255):");
            arts.Sort((a, b) => b.luminance.CompareTo(a.luminance));
            foreach (Art a in arts)
            {
                report.AppendLine($"    {a.name,-12} {a.luminance,6:0.0}   {a.width}x{a.height}");
            }

            if (problems == 0)
            {
                report.AppendLine();
                report.AppendLine("  [tamam] her cift en az bir kanaldan ayirt ediliyor.");
            }

            return problems;
        }

        // ---------------------------------------------------------------

        private static List<Art> LoadAll()
        {
            var result = new List<Art>();

            foreach (string path in Directory.GetFiles(ArtFolder, "*.png"))
            {
                var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
                if (importer == null) continue;

                // Cok sprite'li sayfalar (tileset) tek bir gorsel degil -
                // siluet karsilastirmasi anlamsiz olurdu.
                if (importer.spriteImportMode == SpriteImportMode.Multiple) continue;

                Art art = Analyze(path, importer);
                if (art != null) result.Add(art);
            }

            return result;
        }

        /// <summary>
        /// PNG'yi DISKTEN okuyup cozuyoruz.
        ///
        /// Neden importer'dan degil: texture'dan piksel okumak icin
        /// isReadable acik olmali ve o, calisma aninda bellekte ikinci bir
        /// kopya tutmak demek. Sadece denetim icin butun oyunu
        /// agirlastirmaya degmez.
        /// </summary>
        private static Art Analyze(string path, TextureImporter importer)
        {
            var texture = new Texture2D(2, 2);

            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path)))
            {
                Object.DestroyImmediate(texture);
                return null;
            }

            int w = texture.width, h = texture.height;
            Color32[] pixels = texture.GetPixels32();

            // Parlaklik: sadece opak piksellerin ortalamasi
            float sum = 0f;
            int opaque = 0;

            var full = new bool[w * h];
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a <= 40) continue;

                full[i] = true;
                opaque++;
                sum += 0.2126f * pixels[i].r + 0.7152f * pixels[i].g + 0.0722f * pixels[i].b;
            }

            var art = new Art
            {
                name = Path.GetFileNameWithoutExtension(path),
                width = w,
                height = h,
                importer = importer,
                luminance = opaque > 0 ? sum / opaque : 0f,
                mask = Normalize(full, w, h),
            };

            Object.DestroyImmediate(texture);
            return art;
        }

        /// <summary>
        /// Silueti ortak olcege getiriyor.
        ///
        /// Boyut farki kasten yok sayiliyor: kucuk bir daire ile buyuk bir
        /// daire ayni SEKIL. Soru "ayni buyuklukte mi" degil, "ayni hat mi".
        /// </summary>
        private static bool[] Normalize(bool[] src, int w, int h)
        {
            var dst = new bool[NormalizeTo * NormalizeTo];

            for (int y = 0; y < NormalizeTo; y++)
            {
                for (int x = 0; x < NormalizeTo; x++)
                {
                    int sx = Mathf.Min(x * w / NormalizeTo, w - 1);
                    int sy = Mathf.Min(y * h / NormalizeTo, h - 1);
                    dst[y * NormalizeTo + x] = src[sy * w + sx];
                }
            }

            return dst;
        }

        /// <summary>Kesisim / birlesim - 1'e yaklastikca ayni siluet.</summary>
        private static float Overlap(bool[] a, bool[] b)
        {
            int inter = 0, union = 0;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] && b[i]) inter++;
                if (a[i] || b[i]) union++;
            }

            return union == 0 ? 0f : inter / (float)union;
        }
    }
}
