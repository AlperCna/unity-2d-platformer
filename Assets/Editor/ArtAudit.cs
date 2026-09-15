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

        /// <summary>
        /// Arka plan, en sonuk oynanis gorselinden bu kadar daha sonuk olmali.
        ///
        /// Sayi olcumden geliyor: denetime giren en sonuk oynanis gorseli
        /// zemin karosu (94,8). Karo seti daha sonuk (82,4) ama cok-sprite'li
        /// oldugu icin LoadAll onu atliyor. 15 birim aralik, arka planin
        /// zeminle ayni bantta kalmamasini garanti ediyor.
        /// </summary>
        private const float BackgroundHeadroom = 15f;

        /// <summary>
        /// Arka plan kendi icinde bu kadardan fazla kontrast tasimamali.
        ///
        /// Epic'in uyardigi hata: "guzel ama cok belirgin arka plan yuzunden
        /// platformlarin secilememesi". Detayli bir arka plan sonuk olsa bile
        /// yarisir. Karsilastirma icin: karo setinin ic kontrasti 93,6,
        /// zemin karosununki 82. Arka plan bunlardan belirgin sekilde DUZ
        /// olmali.
        /// </summary>
        private const float BackgroundMaxContrast = 60f;

        private class Art
        {
            public string name;
            public bool[] mask;      // NormalizeTo x NormalizeTo
            public float luminance;

            /// <summary>Parlaklik dagiliminin %10-%90 araligi - "ne kadar kalabalik".</summary>
            public float contrast;

            public bool isBackground;
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
            problems += AuditBackgroundRecession(arts, report);
            problems += AuditAtlas(report);

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

            for (int i = 0; i < arts.Count; i++)
            {
                for (int j = i + 1; j < arts.Count; j++)
                {
                    // Arka planlar bu teste GIRMEZ. Birbirlerine benzemeleri
                    // zaten istenen sey; on planla iliskileri ise ayri bir
                    // soru ve asagida ayri olculuyor. Ayrim yapilmasaydi arac
                    // her arka plan cifti icin yanlis bayrak kaldirirdi.
                    if (arts[i].isBackground || arts[j].isBackground) continue;

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
            report.AppendLine("  Parlaklik / ic kontrast (0-255):");
            arts.Sort((a, b) => b.luminance.CompareTo(a.luminance));
            foreach (Art a in arts)
            {
                report.AppendLine($"    {a.name,-12} {a.luminance,6:0.0} {a.contrast,7:0.0}   " +
                                  $"{a.width}x{a.height}{(a.isBackground ? "   [arka plan]" : "")}");
            }

            if (problems == 0)
            {
                report.AppendLine();
                report.AppendLine("  [tamam] her cift en az bir kanaldan ayirt ediliyor.");
            }

            return problems;
        }

        /// <summary>
        /// Epic 11'in kabul kriteri: "Arka plan on planla yarismiyor."
        ///
        /// Bu uzun sure OLCULMEYEN tek kriterdi ve gozle bakinca gecmis
        /// gorunuyordu. Olculdugunde gecmedigi cikti: eski arka plan bandi
        /// 77,3 parlaklikta, zemin karosu 82,4 - arada 5 birim. Ilk oynayan
        /// kisi "ortami begenmedim" dedi ve hakliydi.
        ///
        /// Iki ayri soru soruluyor, cunku arka plan iki farkli sekilde
        /// yarisabilir:
        ///   TON      arka plan on planla ayni parlaklikta mi
        ///   KALABALIK arka plan kendi icinde cok mu detayli
        ///
        /// Ikincisi sinsi: sonuk ama detayli bir arka plan da platformlari
        /// yutuyor.
        /// </summary>
        private static int AuditBackgroundRecession(List<Art> arts, StringBuilder report)
        {
            report.AppendLine();
            report.AppendLine("ARKA PLAN GERI CEKILMESI");

            var backgrounds = new List<Art>();
            Art dimmestGameplay = null;

            foreach (Art a in arts)
            {
                if (a.isBackground) { backgrounds.Add(a); continue; }
                if (dimmestGameplay == null || a.luminance < dimmestGameplay.luminance) dimmestGameplay = a;
            }

            if (backgrounds.Count == 0)
            {
                report.AppendLine("  [atlandi] arka plan katmani yok.");
                return 0;
            }

            if (dimmestGameplay == null)
            {
                report.AppendLine("  [atlandi] karsilastirilacak oynanis gorseli yok.");
                return 0;
            }

            float ceiling = dimmestGameplay.luminance - BackgroundHeadroom;
            report.AppendLine($"  En sonuk oynanis gorseli: {dimmestGameplay.name} " +
                              $"({dimmestGameplay.luminance:0.0}) -> arka plan tavani {ceiling:0.0}");

            int problems = 0;

            foreach (Art bg in backgrounds)
            {
                if (bg.luminance > ceiling)
                {
                    problems++;
                    report.AppendLine(
                        $"  [YARISIYOR] {bg.name}: parlaklik {bg.luminance:0.0}, " +
                        $"tavan {ceiling:0.0}.\n" +
                        $"      SpriteFactory'de PullBack oranini artir.");
                }

                if (bg.contrast > BackgroundMaxContrast)
                {
                    problems++;
                    report.AppendLine(
                        $"  [KALABALIK] {bg.name}: ic kontrast {bg.contrast:0.0}, " +
                        $"sinir {BackgroundMaxContrast:0}.\n" +
                        $"      Sonuk olmasi yetmiyor - detayi da azaltmali.");
                }
            }

            if (problems == 0)
            {
                report.AppendLine($"  [tamam] {backgrounds.Count} katman da geri cekilmis.");
            }

            return problems;
        }

        /// <summary>
        /// Epic 11 gorev 7 — Sprite Atlas var mi ve gercekten calisiyor mu.
        ///
        /// Iki ayri sey soruluyor cunku ikisi de sessizce eksik kalabilir:
        /// atlas dosyasi olmadan da oyun calisir (sadece yavas), ve atlas
        /// dosyasi VARKEN paketleyici kapaliysa dosya hicbir ise yaramaz.
        /// Ikincisi ozellikle sinsi: her sey yerinde gorunur.
        /// </summary>
        private static int AuditAtlas(StringBuilder report)
        {
            report.AppendLine();
            report.AppendLine("SPRITE ATLAS");

            bool exists = File.Exists(SpriteAtlasFactory.AtlasPath);
            bool packerOn = EditorSettings.spritePackerMode != SpritePackerMode.Disabled;

            if (exists && packerOn)
            {
                report.AppendLine($"  [tamam] atlas var, paketleyici acik " +
                                  $"({EditorSettings.spritePackerMode}).");
                return 0;
            }

            if (!exists)
            {
                report.AppendLine("  [YOK] Sprite Atlas uretilmemis.\n" +
                                  "      Tools > 2D Platformer > Sprite Atlas Uret");
            }
            else
            {
                report.AppendLine("  [ETKISIZ] atlas dosyasi var ama sprite paketleyici KAPALI.\n" +
                                  "      Atlas hicbir ise yaramiyor. Ayni menuyu calistir.");
            }

            return 1;
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
            var opaqueLuminances = new List<float>();

            var full = new bool[w * h];
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a <= 40) continue;

                full[i] = true;
                float luminance = 0.2126f * pixels[i].r + 0.7152f * pixels[i].g + 0.0722f * pixels[i].b;
                opaqueLuminances.Add(luminance);
                sum += luminance;
            }

            int opaque = opaqueLuminances.Count;
            string name = Path.GetFileNameWithoutExtension(path);

            var art = new Art
            {
                name = name,
                isBackground = name.StartsWith(SpriteFactory.BackgroundPrefix),
                width = w,
                height = h,
                importer = importer,
                luminance = opaque > 0 ? sum / opaque : 0f,
                contrast = Contrast(opaqueLuminances),
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

        /// <summary>
        /// Parlaklik dagiliminin %10-%90 araligi.
        ///
        /// Min-max degil: tek bir parlak piksel (goz isigi, kivilcim) butun
        /// olcumu bozardi. Yuzdelik dilim o tek pikselden etkilenmiyor.
        /// </summary>
        private static float Contrast(List<float> luminances)
        {
            if (luminances.Count < 10) return 0f;

            luminances.Sort();
            float low = luminances[Mathf.FloorToInt(luminances.Count * 0.10f)];
            float high = luminances[Mathf.FloorToInt(luminances.Count * 0.90f)];

            return high - low;
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
