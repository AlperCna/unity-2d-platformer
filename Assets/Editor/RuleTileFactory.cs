using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 05 gorev 4 — zemin icin Rule Tile uretir.
    ///
    /// NE ISE YARAR: Tile Palette ile ELLE boyarken hangi karonun nereye
    /// gelecegine kendisi karar verir. Sen sadece sekli cizersin.
    ///
    /// Bolumler koddan uretildigi icin oyun bunu KULLANMIYOR - LevelCursor
    /// maskeyi kendisi hesaplayip dogru karoyu basiyor. Rule Tile, elle
    /// deneme yapmak istedigin zamanlar icin.
    ///
    /// ONEMLI: kurallari elle tanimlamiyoruz. Epic "9 kural tanimlaman ~20
    /// dakika surer" diyor; ama elle tanimlanan kurallar ile LevelCursor'in
    /// hesabi birbirinden AYRILABILIR - ayni bolum elle boyaninca baska,
    /// koddan uretilince baska gorunur. Ikisi de ayni maskeden uretiliyor,
    /// o yuzden ayrilamazlar.
    ///
    /// Menu: Tools > 2D Platformer > Karo Setini Uret (bunu da calistirir)
    /// </summary>
    public static class RuleTileFactory
    {
        public const string AssetPath = "Assets/Art/Tiles/Ground_RuleTile.asset";

        /// <summary>
        /// RuleTile'in komsu kodlari. Paket sabitleriyle ayni degerler;
        /// burada isimlendirilmis olmasi okunurluk icin.
        /// </summary>
        private const int Filled = 1;      // RuleTile.TilingRuleOutput.Neighbor.This
        private const int Empty = 2;       // RuleTile.TilingRuleOutput.Neighbor.NotThis

        public static void Generate()
        {
            var tile = AssetDatabase.LoadAssetAtPath<RuleTile>(AssetPath);
            bool isNew = tile == null;

            if (isNew) tile = ScriptableObject.CreateInstance<RuleTile>();

            tile.m_DefaultSprite = TilesetFactory.LoadTile(0);
            tile.m_DefaultColliderType = Tile.ColliderType.Grid;
            tile.m_TilingRules = new List<RuleTile.TilingRule>(16);

            for (int mask = 0; mask < 16; mask++)
            {
                Sprite sprite = TilesetFactory.LoadTile(mask);
                if (sprite == null) return;

                tile.m_TilingRules.Add(BuildRule(mask, sprite));
            }

            if (isNew) AssetDatabase.CreateAsset(tile, AssetPath);
            else EditorUtility.SetDirty(tile);

            AssetDatabase.SaveAssets();

            Debug.Log($"Rule Tile hazir: {AssetPath}\n" +
                      $"  16 kural, hepsi dort komsuyu da acikca belirtiyor -> " +
                      $"kurallar birbirini dislar, sira onemsiz.");
        }

        /// <summary>
        /// Tek bir maske icin kural.
        ///
        /// Dort komsunun DORDUNU de acikca belirtiyoruz ("fark etmez"
        /// birakmiyoruz). Bunun sonucu onemli: kurallar birbirini karsilikli
        /// disliyor, yani her hucre durumuna TAM BIR kural uyuyor.
        ///
        /// Epic'in tuzak tablosundaki "Rule Tile yanlis karo seciyor -
        /// kurallar yukaridan asagi kontrol edilir, ozel kurallari uste al"
        /// sorunu bu sekilde hic dogmuyor: sira onemli degil.
        /// </summary>
        private static RuleTile.TilingRule BuildRule(int mask, Sprite sprite)
        {
            bool topOpen    = (mask & 1) != 0;
            bool bottomOpen = (mask & 2) != 0;
            bool leftOpen   = (mask & 4) != 0;
            bool rightOpen  = (mask & 8) != 0;

            return new RuleTile.TilingRule
            {
                m_Sprites = new[] { sprite },
                m_ColliderType = Tile.ColliderType.Grid,
                m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single,
                m_RuleTransform = RuleTile.TilingRule.Transform.Fixed,

                m_NeighborPositions = new List<Vector3Int>
                {
                    Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
                },
                m_Neighbors = new List<int>
                {
                    topOpen    ? Empty : Filled,
                    bottomOpen ? Empty : Filled,
                    leftOpen   ? Empty : Filled,
                    rightOpen  ? Empty : Filled,
                },
            };
        }
    }
}
