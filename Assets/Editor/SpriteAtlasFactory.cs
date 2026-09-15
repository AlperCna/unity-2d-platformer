using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 11 gorev 7 — Sprite Atlas.
    ///
    /// Amac: her sprite ayri bir texture oldugunda GPU her biri icin ayri
    /// bir cizim cagrisi yapiyor. Hepsi tek bir texture'da toplanirsa
    /// tek cagrida cizilebiliyorlar.
    ///
    /// AMA HER SPRITE ATLASA GIRMEZ.
    ///
    /// DoseneREK cizilen sprite'lar disarida kaliyor. Iki grup var:
    ///
    ///   KARO SETI (tileset.png)
    ///     Zaten tek bir texture - atlaslamanin kazanci sifir. Ustelik
    ///     karolarin arasindaki 2 piksellik dolgu ELLE uretildi (kenar
    ///     pikselleri kopyalanarak). Atlas paketleyici her karoyu yeniden
    ///     yerlestirince o dolgu kayboluyor ve "topragin icinde yesil
    ///     seyler" hatasi geri geliyor - bu hata bir kez yasandi ve
    ///     oynayan kisi hemen farketti.
    ///
    ///   ARKA PLAN KATMANLARI (bg_*)
    ///     SpriteDrawMode.Tiled ile yatayda tekrar ediyorlar. Atlas
    ///     icindeki bir sprite'in UV'leri daha buyuk bir texture'in ortasinda
    ///     kaldigi icin tekrar sinirlarinda sizinti riski var. Kazanci da
    ///     yok: her katman zaten tek cizim cagrisi.
    ///
    /// Ortak kural: **doseneREK veya dilimlenerek cizilen sprite atlasa
    /// girmez.** Tek bir cumle, iki durumu da kapsiyor.
    ///
    /// Menu: Tools > 2D Platformer > Sprite Atlas Uret
    /// </summary>
    internal static class SpriteAtlasFactory
    {
        private const string ArtFolder = "Assets/Art";
        internal const string AtlasPath = ArtFolder + "/GameAtlas.spriteatlas";

        /// <summary>
        /// Sprite'lar arasindaki bosluk. Point filtreleme TEK BASINA
        /// sizintiyi engellemiyor - karo setinde ogrenildi.
        /// </summary>
        private const int Padding = 4;

        [MenuItem("Tools/2D Platformer/Sprite Atlas Uret", false, 22)]
        public static void Generate()
        {
            if (!EditorGuards.RequireEditMode("Sprite Atlas Uret",
                "Atlas paketlemek Play modunda sorun cikarir.")) return;

            EnsureAtlas();
        }

        internal static void EnsureAtlas()
        {
            // Paketleyici kapaliysa atlas dosyasi olusur ama HICBIR ISE
            // YARAMAZ. Sessiz basarisizlik: sahne kurulur, rapor "tamam"
            // der, hicbir sey degismez.
            if (EditorSettings.spritePackerMode != SpritePackerMode.AlwaysOnAtlas)
            {
                EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
                Debug.Log("Sprite paketleyici acildi (AlwaysOnAtlas).");
            }

            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            bool isNew = atlas == null;

            if (isNew)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, AtlasPath);
            }

            atlas.SetIncludeInBuild(true);

            atlas.SetPackingSettings(new SpriteAtlasPackingSettings
            {
                padding = Padding,
                enableRotation = false,      // pixel art'ta dondurme bozuyor
                enableTightPacking = false,  // silueti degil dikdortgeni paketle
                enableAlphaDilation = true,  // seffaf kenarlara renk tasir - sizintiya karsi
            });

            atlas.SetTextureSettings(new SpriteAtlasTextureSettings
            {
                filterMode = FilterMode.Point,   // SANAT-REHBERI standardi
                generateMipMaps = false,
                sRGB = true,
            });

            atlas.SetPlatformSettings(new TextureImporterPlatformSettings
            {
                name = "DefaultTexturePlatform",
                maxTextureSize = 2048,
                format = TextureImporterFormat.RGBA32,   // sikistirma yok
                textureCompression = TextureImporterCompression.Uncompressed,
                overridden = true,
            });

            // Onceki icerigi temizleyip bastan kur: sprite silinmisse
            // atlasta hayalet giris kalmasin.
            Object[] existing = atlas.GetPackables();
            if (existing != null && existing.Length > 0) atlas.Remove(existing);

            List<Object> packables = CollectPackables(out List<string> skipped);
            atlas.Add(packables.ToArray());

            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
            SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);

            Debug.Log($"Sprite Atlas {(isNew ? "olusturuldu" : "guncellendi")}: " +
                      $"{packables.Count} sprite paketlendi.\n" +
                      $"Disarida birakilan ({skipped.Count}): {string.Join(", ", skipped)}\n" +
                      $"Sebep: dosenerek veya dilimlenerek cizilen sprite atlasa girmez.");
        }

        /// <summary>
        /// Atlasa girecek sprite'lari toplar.
        ///
        /// Disarida birakma kurali ISME degil, SPRITE'IN NASIL CIZILDIGINE
        /// bakiyor - boylece yeni sprite eklendiginde liste elle
        /// guncellenmek zorunda kalmiyor.
        /// </summary>
        private static List<Object> CollectPackables(out List<string> skipped)
        {
            var packables = new List<Object>();
            skipped = new List<string>();

            foreach (string rawPath in Directory.GetFiles(ArtFolder, "*.png"))
            {
                string path = rawPath.Replace('\\', '/');
                string name = Path.GetFileNameWithoutExtension(path);

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                // Dilimlenmis sayfa: zaten tek texture, ustelik elle
                // uretilmis dolgusu var.
                if (importer.spriteImportMode == SpriteImportMode.Multiple)
                {
                    skipped.Add($"{name} (dilimlenmis sayfa)");
                    continue;
                }

                // Dosenerek cizilen arka plan katmani.
                if (name.StartsWith(SpriteFactory.BackgroundPrefix))
                {
                    skipped.Add($"{name} (dosenerek ciziliyor)");
                    continue;
                }

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null) packables.Add(texture);
            }

            return packables;
        }
    }
}
