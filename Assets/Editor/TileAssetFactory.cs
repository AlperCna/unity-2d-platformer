using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 05 — TilesetFactory'nin urettigi 16 sprite'i Tilemap'in
    /// kullanabilecegi Tile varliklarina cevirir.
    ///
    /// Sprite ile Tile ayni sey degil: Tilemap'e Sprite basamazsin, Tile
    /// basarsin. Tile = sprite + collider tipi + (istenirse) davranis.
    ///
    /// colliderType = Grid: karo hucrenin TAMAMI kadar collider verir.
    /// Sprite'in seklini takip etmez. CompositeCollider2D bu kare
    /// colliderlari birlestirip tek bir dis hat cikaracagi icin dogrusu bu.
    ///
    /// Menu: Tools > 2D Platformer > Karo Setini Uret (bunu da calistirir)
    /// </summary>
    public static class TileAssetFactory
    {
        public const string TileFolder = "Assets/Art/Tiles";

        /// <summary>
        /// 16 karo varligini olusturur. Zaten varsa sprite'ini tazeler -
        /// varligi silip yeniden yaratmak, ona referans veren her seyi
        /// (Rule Tile, sahnelerdeki Tilemap verisi) koparirdi.
        /// </summary>
        public static void GenerateAll()
        {
            EnsureFolder();

            int created = 0, updated = 0;

            for (int mask = 0; mask < TilesetFactory.Columns * TilesetFactory.Rows; mask++)
            {
                Sprite sprite = TilesetFactory.LoadTile(mask);
                if (sprite == null) continue;

                string path = TilePath(mask);
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);

                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.Grid;
                    AssetDatabase.CreateAsset(tile, path);
                    created++;
                }
                else if (tile.sprite != sprite || tile.colliderType != Tile.ColliderType.Grid)
                {
                    tile.sprite = sprite;
                    tile.colliderType = Tile.ColliderType.Grid;
                    EditorUtility.SetDirty(tile);
                    updated++;
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Karo varliklari: {created} yeni, {updated} guncellendi " +
                      $"({TileFolder})");

            RuleTileFactory.Generate();
        }

        public static string TilePath(int mask) =>
            $"{TileFolder}/{TilesetFactory.TileName(mask)}.asset";

        public static Tile Load(int mask) =>
            AssetDatabase.LoadAssetAtPath<Tile>(TilePath(mask));

        /// <summary>Varliklar eksikse uretir. Bolum kurucular bunu cagirir.</summary>
        internal static void EnsureGenerated()
        {
            // Sayfa yenilendiyse karo varliklarinin sprite referanslari da
            // tazelenmeli - yoksa Tile'lar eski dilimleri gostermeye calisir.
            bool sheetRebuilt = TilesetFactory.EnsureUpToDate();

            // GenerateAll kendi icinde Rule Tile'i da tazeliyor
            if (sheetRebuilt || Load(0) == null) GenerateAll();
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Art"))
            {
                AssetDatabase.CreateFolder("Assets", "Art");
            }
            if (!AssetDatabase.IsValidFolder(TileFolder))
            {
                AssetDatabase.CreateFolder("Assets/Art", "Tiles");
            }
        }

        // ---------------------------------------------------------------
        // Komsuluk maskesi — karo secme mantiginin tek kaynagi
        // ---------------------------------------------------------------

        /// <summary>
        /// Bir hucrenin hangi karoyu kullanacagini komsularindan hesaplar.
        ///
        /// Rule Tile de ayni isi yapar ama O, ELLE BOYAYAN icin vardir.
        /// Bolum koddan uretildigi icin maskeyi dogrudan hesaplayabiliyoruz:
        /// tahmin yok, "yanlis karo secildi" durumu yok.
        /// </summary>
        public static int MaskFor(System.Func<Vector3Int, bool> isSolid, Vector3Int cell)
        {
            int mask = 0;
            if (!isSolid(cell + Vector3Int.up))    mask |= 1;
            if (!isSolid(cell + Vector3Int.down))  mask |= 2;
            if (!isSolid(cell + Vector3Int.left))  mask |= 4;
            if (!isSolid(cell + Vector3Int.right)) mask |= 8;
            return mask;
        }
    }
}
