using System.IO;
using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Oyunun yer tutucu grafiklerini kod ile uretir ve Assets/Art altina
    /// gercek PNG asset'i olarak kaydeder. Boylece disaridan tek bir dosya
    /// indirmeden oynanabilir bir bolum cikiyor.
    /// Kendi grafiklerini yapinca bu PNG'leri degistirmen yeterli.
    /// </summary>
    public static class SpriteFactory
    {
        public const int PixelsPerUnit = 32;
        private const string ArtFolder = "Assets/Art";

        // --- Palet ---
        public static readonly Color Sky = Hex("#1F2B3E");
        public static readonly Color HillFar = Hex("#2C3B52");
        public static readonly Color HillNear = Hex("#3A506B");
        public static readonly Color Grass = Hex("#5FBF77");
        public static readonly Color GrassDark = Hex("#49A05F");
        public static readonly Color Dirt = Hex("#6B4F3A");
        public static readonly Color DirtDark = Hex("#543D2C");

        /// <summary>
        /// Karo setindeki dis kenar cizgisi. DirtDark'tan belirgin sekilde
        /// koyu olmali - amaci silueti okunur kilmak (bulaniklik testi).
        /// Ilk denemede DirtDark kullanildi ve kenarlar gorunmuyordu.
        /// </summary>
        public static readonly Color DirtEdge = Hex("#38291C");
        public static readonly Color PlayerBody = Hex("#F2B544");
        public static readonly Color PlayerDark = Hex("#D9952C");
        public static readonly Color CoinBody = Hex("#FFD75E");
        public static readonly Color CoinLight = Hex("#FFF0B0");
        public static readonly Color SpikeBody = Hex("#E05C5C");
        public static readonly Color SpikeDark = Hex("#B33F3F");
        public static readonly Color EnemyBody = Hex("#9B5DE5");
        public static readonly Color EnemyDark = Hex("#7B41C4");
        public static readonly Color Metal = Hex("#8A94A6");
        public static readonly Color Ink = Hex("#1A1A22");

        /// <summary>
        /// Eksik sprite'lari uretir.
        ///
        /// Var olanlari ATLAR. Onceden her cagrida 8 PNG'yi bastan yazip
        /// zorla yeniden import ettiriyordu; sprite'lar ayni olmasina ragmen.
        /// Bu hem gereksiz yavasti hem de Unity'nin kendi onbellek dosyasiyla
        /// cakisma penceresi yaratiyordu (bir kez Editor'u kapattirdi:
        /// "Opening file VirtualArtifacts/...: Erisim engellendi").
        ///
        /// force = true sadece gercekten yeniden uretmek istedigin zaman.
        /// DIKKAT: kendi cizimlerinin uzerine yazar.
        /// </summary>
        public static void GenerateAll(bool force = false)
        {
            EnsureFolder();

            var creators = new (string name, System.Action create)[]
            {
                ("square",     CreateSquare),
                ("ground",     CreateGroundTile),
                ("player",     CreatePlayer),
                ("coin",       CreateCoin),
                ("gem",        CreateGem),
                ("spike",      CreateSpike),
                ("enemy",      CreateEnemy),
                ("shooter",    CreateShooter),
                ("bullet",     CreateBullet),
                ("flame",      CreateFlame),
                ("jumppad",    CreateJumpPad),
                ("checkpoint", CreateCheckpoint),
                ("goal",       CreateGoalFlag),
            };

            int created = 0;
            foreach ((string name, System.Action create) in creators)
            {
                if (!force && Exists(name)) continue;

                create();
                created++;
            }

            if (created > 0)
            {
                AssetDatabase.Refresh();
                Debug.Log($"SpriteFactory: {created} sprite uretildi, " +
                          $"{creators.Length - created} tanesi zaten vardi.");
            }
        }

        private static bool Exists(string spriteName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{spriteName}.png") != null;
        }

        public static Sprite Load(string spriteName)
        {
            string path = $"{ArtFolder}/{spriteName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (sprite == null)
            {
                Debug.LogError($"SpriteFactory: '{path}' bulunamadi. Once Tools > 2D Platformer > Grafikleri Uret calistir.");
            }

            return sprite;
        }

        // ---------------------------------------------------------------
        // Tek tek sprite'lar
        // ---------------------------------------------------------------

        /// <summary>Duz beyaz kare. Tiled platformlar ve UI icin temel parca.</summary>
        private static void CreateSquare()
        {
            var canvas = new PixelCanvas(32, 32);
            canvas.FillRect(0, 0, 32, 32, Color.white);
            canvas.Save("square");
        }

        /// <summary>Ustu cimen altta toprak olan zemin karosu. Yan yana dizilir.</summary>
        private static void CreateGroundTile()
        {
            var canvas = new PixelCanvas(32, 32);

            // Toprak govde
            canvas.FillRect(0, 0, 32, 26, Dirt);

            // Toprak dokusu - rastgele koyu benekler (sabit seed: her uretimde ayni)
            var random = new System.Random(1337);
            for (int i = 0; i < 26; i++)
            {
                int x = random.Next(0, 32);
                int y = random.Next(0, 24);
                canvas.FillRect(x, y, 2, 2, DirtDark);
            }

            // Cimen bandi
            canvas.FillRect(0, 26, 32, 6, Grass);
            canvas.FillRect(0, 26, 32, 2, GrassDark);

            // Cimenin ustune kucuk tirtiklar
            for (int x = 0; x < 32; x += 4)
            {
                canvas.FillRect(x, 31, 2, 1, GrassDark);
            }

            canvas.Save("ground");
        }

        /// <summary>
        /// Ana karakter.
        ///
        /// SILUET KURALI: disbukey olmamali.
        ///
        /// Ilk surum yuvarlatilmis bir kutuydu ve olculdugunde dusmanla
        /// %78, parayla %74 ortusuyordu - yani siluetten ayirt edilemiyordu.
        /// Oyuncunun kendi karakterini bir lekeden ayiramamasi, sanatin
        /// yapabilecegi en kotu sey.
        ///
        /// Cozum sekil: BACAK ARASI bosluk ve TEPE TUYU. Ikisi de silueti
        /// disbukeylikten cikariyor; hicbir para, hicbir yuvarlak dusman
        /// boyle bir hat cizemez.
        ///
        /// Ayrica paletin EN PARLAK rengi burada - "gozun ilk gittigi yer
        /// oyuncu olmali" kurali (Epic 11, rol dagilimi).
        /// </summary>
        private static void CreatePlayer()
        {
            var canvas = new PixelCanvas(32, 32);

            // Tepe tuyu - silueti yukaridan kiriyor
            canvas.FillRect(12, 28, 3, 4, PlayerDark);
            canvas.FillRect(13, 30, 4, 2, PlayerBody);

            // Kafa: govdeden GENIS - insan silueti okunuyor
            canvas.FillRoundedRect(5, 15, 22, 14, 5, PlayerBody);

            // Govde: daha dar
            canvas.FillRect(9, 6, 14, 10, PlayerBody);
            canvas.FillRect(9, 6, 14, 4, PlayerDark);      // alt golge

            // BACAKLAR - aralarindaki bosluk siluetin en ayirt edici yeri
            canvas.FillRect(9, 0, 5, 7, PlayerDark);
            canvas.FillRect(18, 0, 5, 7, PlayerDark);

            // Gozler
            canvas.FillRect(10, 20, 5, 6, Color.white);
            canvas.FillRect(18, 20, 5, 6, Color.white);
            canvas.FillRect(12, 21, 3, 3, Ink);
            canvas.FillRect(20, 21, 3, 3, Ink);

            canvas.Save("player");
        }

        private static void CreateCoin()
        {
            var canvas = new PixelCanvas(24, 24);

            canvas.FillCircle(12, 12, 11, CoinBody);
            canvas.FillCircle(12, 12, 8, CoinLight);
            canvas.FillCircle(12, 12, 6, CoinBody);

            // Sol ustte parlama
            canvas.FillRect(7, 15, 2, 4, CoinLight);

            canvas.Save("coin");
        }

        /// <summary>Dikenler: yan yana uc ucgen.</summary>
        /// <summary>
        /// Mucevher. Paradan AYIRT EDILEBILIR olmasi sart ve bu sadece
        /// renkle yapilamaz - renk korlugu olan oyuncu sari ile mor'u
        /// ayirt edemeyebilir.
        ///
        /// O yuzden SEKIL de farkli: para yuvarlak, mucevher kosegen
        /// (elmas). Siluetten bile ayirt ediliyor - Epic 07'nin
        /// bulaniklik testinden gecmesi icin sart.
        /// </summary>
        private static void CreateGem()
        {
            var canvas = new PixelCanvas(32, 32);

            Color body = Hex("#5AC8FF");
            Color light = Hex("#B8ECFF");
            Color dark = Hex("#2E8BC0");

            // Elmas: ortadan yukari ve asagi daralan bir eskenar dortgen
            for (int y = 0; y < 32; y++)
            {
                int half = 15 - Mathf.Abs(y - 16);
                if (half <= 0) continue;

                canvas.FillRect(16 - half, y, half * 2, 1, body);
            }

            // Alt yari biraz koyu - hacim hissi
            for (int y = 0; y < 16; y++)
            {
                int half = Mathf.Max(15 - Mathf.Abs(y - 16) - 3, 0);
                if (half <= 0) continue;
                canvas.FillRect(16 - half, y, half * 2, 1, dark);
            }

            // Parlama
            canvas.FillRect(12, 18, 3, 6, light);
            canvas.FillRect(15, 22, 2, 3, light);

            canvas.Save("gem");
        }

        private static void CreateSpike()
        {
            var canvas = new PixelCanvas(32, 32);

            // Taban
            canvas.FillRect(0, 0, 32, 4, SpikeDark);

            // Uc adet sivri
            for (int i = 0; i < 3; i++)
            {
                int baseX = i * 11;
                canvas.FillTriangleUp(baseX, 3, 11, 26, SpikeBody);
                canvas.FillTriangleUp(baseX + 3, 3, 5, 18, SpikeDark);
            }

            canvas.Save("spike");
        }

        /// <summary>Devriye dusmani: yassi blob, iki goz, ofkeli kaslar.</summary>
        /// <summary>
        /// Devriye dusmani.
        ///
        /// Oyuncudan AYRI bir siluet sekli olmali. Ilk surumde ikisi de
        /// yuvarlatilmis kutuydu ve %78 ortusuyorlardi.
        ///
        /// Simdi zit kurulmus: oyuncu DIK ve dar, bu ALCAK ve genis;
        /// oyuncunun tepesinde tuy var, bunun sirtinda diken sirasi.
        /// Gozunu kisip baktiginda bile "bu o degil" diyebilmelisin.
        ///
        /// Renk oyuncudan SONUK - fark edilmeli ama onunla yarismamali.
        /// </summary>
        private static void CreateEnemy()
        {
            var canvas = new PixelCanvas(32, 20);

            // Alcak ve genis govde
            canvas.FillRoundedRect(2, 3, 28, 12, 4, EnemyBody);
            canvas.FillRoundedRect(2, 3, 28, 5, 3, EnemyDark);

            // SIRT DIKENLERI - silueti yukaridan tirtikliyor,
            // ayrica "bu dusman" diye bagiriyor
            for (int i = 0; i < 5; i++)
            {
                canvas.FillTriangleUp(3 + i * 6, 14, 6, 6, EnemyDark);
            }

            // Ayaklar - alttan da tirtikli
            canvas.FillRect(5, 0, 5, 4, EnemyDark);
            canvas.FillRect(22, 0, 5, 4, EnemyDark);

            // Gozler: kizgin, oyuncununkinden kucuk
            canvas.FillRect(8, 8, 5, 4, Color.white);
            canvas.FillRect(19, 8, 5, 4, Color.white);
            canvas.FillRect(10, 9, 3, 3, Ink);
            canvas.FillRect(20, 9, 3, 3, Ink);

            canvas.Save("enemy");
        }

        /// <summary>
        /// Mermi atan dusman. Devriyeden BELIRGIN sekilde farkli gorunmeli:
        /// oyuncu bir bakista "bu ustune basilir mi, uzaktan mi tehlikeli"
        /// ayrimini yapabilmeli.
        ///
        /// Devriye yuvarlak ve mor; bu koseli ve daha koyu, ustunde bir
        /// namlu var.
        /// </summary>
        private static void CreateShooter()
        {
            var canvas = new PixelCanvas(32, 32);

            // Koseli govde - devriyenin yuvarlakligina zit
            canvas.FillRect(4, 2, 24, 22, EnemyDark);
            canvas.FillRect(6, 4, 20, 18, EnemyBody);

            // Namlu: hangi yone atacagini gosteriyor
            canvas.FillRect(0, 10, 6, 6, Metal);
            canvas.FillRect(0, 12, 4, 2, Ink);

            // Tek buyuk goz - "seni goruyorum"
            canvas.FillRect(14, 12, 8, 8, Ink);
            canvas.FillRect(16, 15, 4, 4, Hex("#FF6B6B"));

            // Ayaklar
            canvas.FillRect(6, 0, 5, 3, EnemyDark);
            canvas.FillRect(21, 0, 5, 3, EnemyDark);

            canvas.Save("shooter");
        }

        /// <summary>
        /// Mermi. Kucuk ama YUKSEK KONTRASTLI olmali - arka plan koyu mavi,
        /// mermi parlak turuncu. Oyuncu onu kacirirsa oldugunu anlamaz,
        /// "haksizlik" der.
        /// </summary>
        private static void CreateBullet()
        {
            var canvas = new PixelCanvas(16, 16);

            canvas.FillCircle(8, 8, 6, Hex("#FF8A3D"));
            canvas.FillCircle(8, 8, 4, Hex("#FFC46B"));
            canvas.FillCircle(7, 9, 2, Hex("#FFF0C8"));

            canvas.Save("bullet");
        }

        /// <summary>
        /// Alev sutunu.
        ///
        /// IKI KURAL VAR:
        ///
        /// 1) DIKEYDE KUSURSUZ BIRLESMELI. FireJet gorseli Tiled modda
        ///    cizdiriyor ve boyunu surekli degistiriyor; desen dikeyde
        ///    tekrarlanacagi icin ust ve alt kenarlar uymali. O yuzden
        ///    kenar dalgasinin periyodu 8 (32'yi tam boluyor).
        ///
        /// 2) TAM 32 PIKSEL GENIS OLMALI = 1 birim.
        ///    Ilk surum 16 pikseldi (0,5 birim) ve alev 0,8 birim genis
        ///    ciziliyordu -> 1,6 kopya YAN YANA geliyordu ve alev "iki ince
        ///    cubuk" gibi gorunuyordu, ates gibi degil. Genislik tam bir
        ///    karo olunca yatayda hic tekrar olmuyor.
        /// </summary>
        private static void CreateFlame()
        {
            var canvas = new PixelCanvas(32, 32);

            // Alev oyuncudan BELIRGIN parlak olmali.
            //
            // Olculdugunde ikisinin parlakligi neredeyse esitti (fark 6,5).
            // Renk korlugu olan oyuncu icin bu, tehlikeyi kendi
            // karakterinden ayiramamak demek - ve ates bir TEHLIKE.
            //
            // Cekirdek buyutuldu ve beyaza yaklastirildi: hem daha okunur
            // hem daha ates gibi. Sicak seyler parlak olur.
            Color outer = Hex("#D94E14");
            Color mid = Hex("#FF9A4D");
            Color core = Hex("#FFE0A0");
            Color hot = Hex("#FFFDF2");

            for (int y = 0; y < 32; y++)
            {
                // Kenar dalgasi - alevi duz bir dikdortgen olmaktan cikarir
                int wobble = (y % 8 < 4) ? 0 : 1;

                canvas.FillRect(2 + wobble, y, 28 - wobble * 2, 1, outer);
                canvas.FillRect(5 + wobble, y, 22 - wobble * 2, 1, mid);
                canvas.FillRect(9, y, 14, 1, core);
                canvas.FillRect(12, y, 8, 1, hot);
            }

            canvas.Save("flame");
        }

        /// <summary>
        /// Zipla pedi. Yay gibi gorunmeli ki ne ise yaradigi BAKAR BAKMAZ
        /// anlasilsin - Epic 07'nin okunabilirlik kurali.
        /// </summary>
        private static void CreateJumpPad()
        {
            var canvas = new PixelCanvas(32, 16);

            // Taban
            canvas.FillRect(2, 0, 28, 4, Metal);
            canvas.FillRect(2, 0, 28, 2, Ink);

            // Yay katmanlari
            Color spring = Hex("#4ED2C8");
            Color springDark = Hex("#2FA79E");
            for (int i = 0; i < 3; i++)
            {
                int y = 4 + i * 3;
                canvas.FillRect(5, y, 22, 2, spring);
                canvas.FillRect(5, y, 22, 1, springDark);
            }

            // Ust plaka
            canvas.FillRect(1, 13, 30, 3, spring);
            canvas.FillRect(1, 15, 30, 1, Hex("#8AF0E8"));

            canvas.Save("jumppad");
        }

        private static void CreateCheckpoint()
        {
            var canvas = new PixelCanvas(16, 48);

            // Direk
            canvas.FillRect(6, 0, 4, 48, Color.white);

            // Taban
            canvas.FillRect(2, 0, 12, 4, Color.white);

            // Bayrakcik
            canvas.FillRect(10, 34, 6, 10, Color.white);

            canvas.Save("checkpoint");
        }

        /// <summary>Bolum sonu bayragi.</summary>
        private static void CreateGoalFlag()
        {
            var canvas = new PixelCanvas(40, 56);

            // Direk
            canvas.FillRect(4, 0, 4, 56, Metal);
            canvas.FillRect(0, 0, 12, 5, Metal);

            // Bayrak (beyaz -> SpriteRenderer ile renklendirilir)
            canvas.FillRect(8, 36, 26, 16, Color.white);

            // Bayragin ucundaki centik
            for (int i = 0; i < 6; i++)
            {
                canvas.FillRect(34 - i, 44 - i, 1, 1 + i * 2, Color.white);
            }

            canvas.Save("goal");
        }

        // ---------------------------------------------------------------
        // Yardimcilar
        // ---------------------------------------------------------------

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(ArtFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Art");
            }
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        /// <summary>
        /// Kucuk bir piksel tuvali. Cizimi bitirince Save() ile PNG olarak
        /// diske yazar ve Unity'de Sprite olarak ice aktarilmasini ayarlar.
        /// </summary>
        internal class PixelCanvas
        {
            private readonly int width;
            private readonly int height;
            private readonly Color[] pixels;

            public PixelCanvas(int width, int height)
            {
                this.width = width;
                this.height = height;
                pixels = new Color[width * height];

                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.clear;
                }
            }

            public void SetPixel(int x, int y, Color color)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return;
                if (color.a <= 0f) return;

                pixels[y * width + x] = color;
            }

            public Color GetPixel(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return Color.clear;
                return pixels[y * width + x];
            }

            public void FillRect(int x, int y, int w, int h, Color color)
            {
                for (int py = y; py < y + h; py++)
                {
                    for (int px = x; px < x + w; px++)
                    {
                        SetPixel(px, py, color);
                    }
                }
            }

            public void FillCircle(int cx, int cy, int radius, Color color)
            {
                int rSquared = radius * radius;

                for (int py = cy - radius; py <= cy + radius; py++)
                {
                    for (int px = cx - radius; px <= cx + radius; px++)
                    {
                        int dx = px - cx;
                        int dy = py - cy;
                        if (dx * dx + dy * dy <= rSquared)
                        {
                            SetPixel(px, py, color);
                        }
                    }
                }
            }

            /// <summary>Koseleri yuvarlatilmis dikdortgen.</summary>
            public void FillRoundedRect(int x, int y, int w, int h, int radius, Color color)
            {
                radius = Mathf.Min(radius, Mathf.Min(w, h) / 2);

                for (int py = y; py < y + h; py++)
                {
                    for (int px = x; px < x + w; px++)
                    {
                        // Kose bolgelerinde daire disinda kalan pikselleri atla
                        int dx = 0;
                        int dy = 0;

                        if (px < x + radius) dx = (x + radius) - px;
                        else if (px >= x + w - radius) dx = px - (x + w - radius - 1);

                        if (py < y + radius) dy = (y + radius) - py;
                        else if (py >= y + h - radius) dy = py - (y + h - radius - 1);

                        if (dx > 0 && dy > 0 && dx * dx + dy * dy > radius * radius) continue;

                        SetPixel(px, py, color);
                    }
                }
            }

            /// <summary>Tabani asagida olan ucgen (diken icin).</summary>
            public void FillTriangleUp(int x, int y, int baseWidth, int triangleHeight, Color color)
            {
                for (int row = 0; row < triangleHeight; row++)
                {
                    float t = (float)row / triangleHeight;
                    int rowWidth = Mathf.Max(1, Mathf.RoundToInt(baseWidth * (1f - t)));
                    int rowX = x + (baseWidth - rowWidth) / 2;

                    FillRect(rowX, y + row, rowWidth, 1, color);
                }
            }

            public void Save(string spriteName)
            {
                string path = WritePng(spriteName);
                ConfigureImporter(path);
            }

            /// <summary>
            /// PNG'yi diske yazar ama ice aktarma ayarini YAPMAZ.
            /// Cok sprite'li sayfalar (tileset) kendi dilimlemesini
            /// ayarlayacagi icin bu adimi kendileri yapar.
            /// </summary>
            public string WritePng(string spriteName)
            {
                string path = $"{ArtFolder}/{spriteName}.png";

                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.SetPixels(pixels);
                texture.Apply();

                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                return path;
            }

            private static void ConfigureImporter(string path)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) return;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PixelsPerUnit;
                importer.filterMode = FilterMode.Point;          // keskin pixel-art
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;

                // FullRect: Tiled cizim modu icin sart (platformlar tekrarli cizilsin)
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                settings.spriteExtrude = 0;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }
        }
    }
}
