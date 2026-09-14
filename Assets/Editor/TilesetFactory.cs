using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 05 — zemin karo seti uretir.
    ///
    /// TASARIM KARARI: 9 degil 16 karo.
    ///
    /// Epic 05 "minimum 9 parcalik set" diyor (kose/kenar/orta). Ama bir
    /// karonun nasil gorunecegini belirleyen sey DORT komsusu: ust, alt,
    /// sol, sag. Her biri ya dolu ya bos -> 2^4 = 16 durum.
    ///
    /// 9 karoyla bu 16 durumun 7'si karsilanmaz ve Rule Tile "en yakini"
    /// secmek zorunda kalir. Epic'in tuzaklar tablosundaki "Rule Tile yanlis
    /// karo seciyor" satiri tam olarak budur. 16 karo ile o tuzak
    /// MATEMATIKSEL OLARAK imkansiz hale geliyor - her duruma birebir
    /// karsilik gelen bir karo var.
    ///
    /// Karo indeksi bir bit maskesi:
    ///   bit 0 (1)  = ust komsu BOS   -> ustte cimen
    ///   bit 1 (2)  = alt komsu BOS   -> altta koyu kenar
    ///   bit 2 (4)  = sol komsu BOS   -> solda koyu kenar
    ///   bit 3 (8)  = sag komsu BOS   -> sagda koyu kenar
    ///
    /// Yani tile_00 = her yani dolu (ic karo), tile_15 = tek basina duran karo.
    /// RuleTileFactory bu maskeyi dogrudan kural olarak kullaniyor.
    ///
    /// Menu: Tools > 2D Platformer > Karo Setini Uret
    /// </summary>
    public static class TilesetFactory
    {
        public const int TileSize = 32;
        public const int Columns = 4;
        public const int Rows = 4;
        public const string SheetName = "tileset";
        public const string SheetPath = "Assets/Art/tileset.png";

        /// <summary>
        /// Karolar arasindaki dolgu (piksel).
        ///
        /// NEDEN VAR: dolgusuz atlasta mask 0'in (ic karo, hep toprak)
        /// son sutunu, mask 1'in (yuzey karosu) CIMEN sutununa piksel
        /// komsusu oluyordu. Kamera tam sayi piksele oturmadigi anda
        /// ornekleme bir texel tasiyor ve toprak karonun kenarinda YESIL
        /// cikiyordu. Ayni sebep zeminde dikey ek yeri cizgileri de
        /// yapiyordu.
        ///
        /// Cozum: her karonun cevresine 2 piksel dolgu, icine karonun kendi
        /// kenar pikselleri kopyalanir. Tasan ornekleme artik komsu karoyu
        /// degil, karonun kendi rengini buluyor.
        ///
        /// Filter Mode Point oldugu icin "olmaz" sanilir; olur - tasma
        /// filtrelemeden degil, texel sinirindan kaynaklaniyor.
        /// </summary>
        private const int Padding = 2;

        /// <summary>Bir karonun sayfada kapladigi toplam yer: sanat + iki yan dolgu.</summary>
        private const int CellStride = TileSize + Padding * 2;

        /// <summary>Kenar serit kalinligi (piksel).</summary>
        private const int EdgeThickness = 3;

        /// <summary>Ust yuzeydeki cimen bandi kalinligi.</summary>
        private const int GrassThickness = 6;

        [MenuItem("Tools/2D Platformer/Karo Setini Uret", false, 22)]
        public static void Generate()
        {
            if (!EditorGuards.RequireEditMode("Karo Setini Uret",
                "Asset uretimi Play modunda yapilamaz.")) return;

            GenerateInternal();
            TileAssetFactory.GenerateAll();     // sprite -> Tile varliklari
            RuleTileFactory.Generate();         // elle boyama icin

            Debug.Log($"Karo seti uretildi: {SheetPath}\n" +
                      $"  {Columns}x{Rows} = {Columns * Rows} karo, {TileSize}x{TileSize} piksel (+{Padding} dolgu)\n" +
                      $"  indeks = ust(1) | alt(2) | sol(4) | sag(8), bit set ise o yon BOS");
        }

        /// <summary>
        /// Menusuz cagri - bolum kurucular kullanir.
        /// Yeniden uretti ise true doner.
        ///
        /// Sadece "dosya var mi" bakmak yetmiyor: dolgu eklendiginde sayfa
        /// 128x128'den 144x144'e buyudu ve eski dosya yerinde kalsaydi
        /// bolum bayat atlasla kurulurdu. Boyut beklenenden farkliysa
        /// dosya eskidir, yenilenir.
        /// </summary>
        internal static bool EnsureUpToDate()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(SheetPath);
            int expected = Columns * CellStride;

            if (existing != null && existing.width == expected && existing.height == expected)
            {
                return false;
            }

            if (existing != null)
            {
                Debug.Log($"Karo seti eski duzende ({existing.width}x{existing.height}, " +
                          $"beklenen {expected}x{expected}) - yeniden uretiliyor.");
            }

            GenerateInternal();
            return true;
        }

        private static void GenerateInternal()
        {
            var canvas = new SpriteFactory.PixelCanvas(Columns * CellStride, Rows * CellStride);

            for (int mask = 0; mask < Columns * Rows; mask++)
            {
                GetCell(mask, out int ox, out int oy);
                DrawTile(canvas, ox, oy, mask);
                Extrude(canvas, ox, oy);
            }

            string path = canvas.WritePng(SheetName);
            ConfigureSheet(path);
        }

        // ---------------------------------------------------------------
        // Yerlesim
        // ---------------------------------------------------------------

        /// <summary>
        /// Maskenin SANAT alaninin sol-alt kosesi (dolgu haric).
        /// Sprite dilimi de tam olarak burasi.
        /// </summary>
        private static void GetCell(int mask, out int x, out int y)
        {
            int col = mask % Columns;
            int row = mask / Columns;
            x = col * CellStride + Padding;
            y = row * CellStride + Padding;   // PixelCanvas'ta y=0 ALTTIR
        }

        /// <summary>
        /// Karonun kenar piksellerini dolgu alanina kopyalar (clamp).
        /// Koseler de dolduruluyor, yoksa capraz ornekleme bos piksel bulur.
        /// </summary>
        private static void Extrude(SpriteFactory.PixelCanvas c, int ox, int oy)
        {
            for (int p = 1; p <= Padding; p++)
            {
                for (int i = 0; i < TileSize; i++)
                {
                    // sol / sag
                    c.SetPixel(ox - p,            oy + i, c.GetPixel(ox,                oy + i));
                    c.SetPixel(ox + TileSize - 1 + p, oy + i, c.GetPixel(ox + TileSize - 1, oy + i));
                    // alt / ust
                    c.SetPixel(ox + i, oy - p,            c.GetPixel(ox + i, oy));
                    c.SetPixel(ox + i, oy + TileSize - 1 + p, c.GetPixel(ox + i, oy + TileSize - 1));
                }
            }

            // Dort kose
            for (int px = 1; px <= Padding; px++)
            {
                for (int py = 1; py <= Padding; py++)
                {
                    c.SetPixel(ox - px, oy - py, c.GetPixel(ox, oy));
                    c.SetPixel(ox + TileSize - 1 + px, oy - py,
                               c.GetPixel(ox + TileSize - 1, oy));
                    c.SetPixel(ox - px, oy + TileSize - 1 + py,
                               c.GetPixel(ox, oy + TileSize - 1));
                    c.SetPixel(ox + TileSize - 1 + px, oy + TileSize - 1 + py,
                               c.GetPixel(ox + TileSize - 1, oy + TileSize - 1));
                }
            }
        }

        public static string TileName(int mask) => $"{SheetName}_{mask:00}";

        // ---------------------------------------------------------------
        // Cizim
        // ---------------------------------------------------------------

        private static void DrawTile(SpriteFactory.PixelCanvas c, int ox, int oy, int mask)
        {
            bool topOpen    = (mask & 1) != 0;
            bool bottomOpen = (mask & 2) != 0;
            bool leftOpen   = (mask & 4) != 0;
            bool rightOpen  = (mask & 8) != 0;

            // --- Toprak govde ---
            c.FillRect(ox, oy, TileSize, TileSize, SpriteFactory.Dirt);

            // Doku benekleri. Seed maskeye bagli: her karo farkli gorunur ama
            // her uretimde AYNI kalir - PNG degismezse Unity yeniden ice
            // aktarmaz. (Bir kez bu yuzden editor kendini kapatmisti.)
            var random = new System.Random(1337 + mask);
            for (int i = 0; i < 22; i++)
            {
                int px = random.Next(0, TileSize - 2);
                int py = random.Next(0, TileSize - GrassThickness - 2);
                c.FillRect(ox + px, oy + py, 2, 2, SpriteFactory.DirtDark);
            }

            // --- Ust yuzey: cimen ---
            // Sadece ustu acik karolarda. Icteki karolarda cimen olsa
            // toprak yiginin ortasinda yesil bantlar gorunurdu.
            if (topOpen)
            {
                int gy = oy + TileSize - GrassThickness;
                c.FillRect(ox, gy, TileSize, GrassThickness, SpriteFactory.Grass);
                c.FillRect(ox, gy, TileSize, 2, SpriteFactory.GrassDark);

                for (int x = 0; x < TileSize; x += 4)
                {
                    c.FillRect(ox + x, oy + TileSize - 1, 2, 1, SpriteFactory.GrassDark);
                }
            }

            // --- Kenarlar: acik yonlere koyu serit ---
            // Silueti okunur kilar. Bulaniklik testinde zeminin nerede
            // bittigi bu seritlerden anlasilir.
            if (bottomOpen)
            {
                c.FillRect(ox, oy, TileSize, EdgeThickness, SpriteFactory.DirtEdge);
            }

            // Sol/sag seritler cimenin ALTINDAN baslar; cimen kismini ayrica
            // koyu yesille kapatiyoruz. Boylece karonun cevresi kesintisiz
            // bir cizgiyle ciziliyor ama cimen topraga donusmuyor.
            int sideTop = topOpen ? TileSize - GrassThickness : TileSize;
            if (leftOpen)
            {
                c.FillRect(ox, oy, EdgeThickness, sideTop, SpriteFactory.DirtEdge);
                if (topOpen)
                {
                    c.FillRect(ox, oy + sideTop, EdgeThickness, GrassThickness,
                               SpriteFactory.GrassDark);
                }
            }
            if (rightOpen)
            {
                c.FillRect(ox + TileSize - EdgeThickness, oy, EdgeThickness, sideTop,
                           SpriteFactory.DirtEdge);
                if (topOpen)
                {
                    c.FillRect(ox + TileSize - EdgeThickness, oy + sideTop,
                               EdgeThickness, GrassThickness, SpriteFactory.GrassDark);
                }
            }
        }

        // ---------------------------------------------------------------
        // Ice aktarma: cok sprite'li sayfa olarak dilimle
        // ---------------------------------------------------------------

        private static void ConfigureSheet(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"Karo seti ice aktarilamadi: {path}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = TileSize;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;                 // karolar arasi sizinti olmasin
            importer.SetTextureSettings(settings);

            var slices = new List<SpriteMetaData>(Columns * Rows);
            for (int mask = 0; mask < Columns * Rows; mask++)
            {
                GetCell(mask, out int x, out int y);
                slices.Add(new SpriteMetaData
                {
                    name = TileName(mask),
                    rect = new Rect(x, y, TileSize, TileSize),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                });
            }

#pragma warning disable CS0618 // spritesheet eski API ama Unity 6'da hala calisan
            importer.spritesheet = slices.ToArray();    // en kisa yol; yenisi
#pragma warning restore CS0618 // ISpriteEditorDataProvider, cok daha uzun

            importer.SaveAndReimport();
        }

        /// <summary>Uretilmis karolardan birini maskesiyle yukler.</summary>
        public static Sprite LoadTile(int mask)
        {
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(SheetPath);
            string want = TileName(mask);

            foreach (Object o in all)
            {
                if (o is Sprite s && s.name == want) return s;
            }

            Debug.LogError($"Karo bulunamadi: {want}. Once " +
                           "Tools > 2D Platformer > Karo Setini Uret calistir.");
            return null;
        }
    }
}
