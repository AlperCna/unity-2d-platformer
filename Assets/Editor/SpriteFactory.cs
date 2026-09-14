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
                ("spike",      CreateSpike),
                ("enemy",      CreateEnemy),
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

        /// <summary>Ana karakter: yuvarlatilmis govde, gozler, ayaklar.</summary>
        private static void CreatePlayer()
        {
            var canvas = new PixelCanvas(32, 32);

            // Govde
            canvas.FillRoundedRect(5, 2, 22, 27, 6, PlayerBody);

            // Alt golge - hacim hissi
            canvas.FillRoundedRect(5, 2, 22, 7, 5, PlayerDark);

            // Gozler
            canvas.FillRect(11, 17, 4, 6, Color.white);
            canvas.FillRect(19, 17, 4, 6, Color.white);
            canvas.FillRect(12, 18, 3, 3, Ink);
            canvas.FillRect(20, 18, 3, 3, Ink);

            // Agiz
            canvas.FillRect(14, 12, 5, 2, Ink);

            canvas.Save("player");
        }

        /// <summary>Toplanabilir para: halka seklinde madeni para.</summary>
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
        private static void CreateEnemy()
        {
            var canvas = new PixelCanvas(32, 24);

            canvas.FillRoundedRect(2, 1, 28, 21, 8, EnemyBody);
            canvas.FillRoundedRect(2, 1, 28, 6, 5, EnemyDark);

            // Gozler
            canvas.FillRect(9, 11, 5, 6, Color.white);
            canvas.FillRect(18, 11, 5, 6, Color.white);
            canvas.FillRect(11, 12, 3, 3, Ink);
            canvas.FillRect(20, 12, 3, 3, Ink);

            // Kaslar - kizgin ifade
            canvas.FillRect(9, 18, 5, 2, EnemyDark);
            canvas.FillRect(18, 18, 5, 2, EnemyDark);

            // Disler
            canvas.FillRect(12, 6, 3, 3, Color.white);
            canvas.FillRect(17, 6, 3, 3, Color.white);

            canvas.Save("enemy");
        }

        /// <summary>Checkpoint diregi. Rengi script tarafindan degistirilir.</summary>
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
        private class PixelCanvas
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
                string path = $"{ArtFolder}/{spriteName}.png";

                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.SetPixels(pixels);
                texture.Apply();

                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);

                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                ConfigureImporter(path);
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
