using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 05 gorev 3 + 5 — bir bolumun Tilemap iskeletini kurar.
    ///
    /// Katmanlar ayri Tilemap'ler: her birinin collider ihtiyaci, layer'i ve
    /// siralamasi farkli. Hepsini tek Tilemap'e koyarsan dikeni zeminden
    /// ayiramazsin.
    /// </summary>
    public class TilemapRig
    {
        public Grid Grid { get; private set; }

        /// <summary>Basilabilen her sey. Composite collider, layer Ground.</summary>
        public Tilemap Ground { get; private set; }

        /// <summary>Oldurur. Trigger collider, uzerinde tek Hazard script'i.</summary>
        public Tilemap Hazards { get; private set; }

        /// <summary>Collider yok, arkada durur.</summary>
        public Tilemap Background { get; private set; }

        /// <summary>Collider yok, onde durur (ot, tas, detay).</summary>
        public Tilemap Decoration { get; private set; }

        public static TilemapRig Create(int groundLayer)
        {
            var rig = new TilemapRig();

            var gridObject = new GameObject("Grid");
            rig.Grid = gridObject.AddComponent<Grid>();
            rig.Grid.cellSize = new Vector3(1f, 1f, 0f);      // 1 birim = 1 hucre
            rig.Grid.cellLayout = GridLayout.CellLayout.Rectangle;

            rig.Background = CreateLayer(gridObject.transform, "Background", -10);
            rig.Ground     = CreateLayer(gridObject.transform, "Ground", 0);
            rig.Hazards    = CreateLayer(gridObject.transform, "Hazards", 1);
            rig.Decoration = CreateLayer(gridObject.transform, "Decoration", 5);

            ConfigureGround(rig.Ground, groundLayer);
            ConfigureHazards(rig.Hazards);

            return rig;
        }

        private static Tilemap CreateLayer(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var map = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sortingOrder;

            // Chunk: komsu karolari tek mesh'te birlestirir, cizim cagrisi
            // sayisini dusurur. Individual sadece karolarin birbirini
            // ortmesi gereken durumlarda gerekiyor; bizde oyle bir sey yok.
            renderer.mode = TilemapRenderer.Mode.Chunk;

            return map;
        }

        /// <summary>
        /// Zemin katmani: Composite Collider.
        ///
        /// Epic 05'in "bir numarali sorunu": Composite olmadan her karo AYRI
        /// bir collider olur ve karakter karolarin birlestigi yerdeki gorunmez
        /// koselere takilir. Duz bir zeminde kosarken durup kalmak gibi.
        /// Insanlar bunu "fizik bozuk" saniyor; degil, collider sayisi sorunu.
        ///
        /// Composite hepsini tek bir dis hatta eritiyor: duz zemin = tek cizgi.
        /// </summary>
        private static void ConfigureGround(Tilemap map, int groundLayer)
        {
            GameObject go = map.gameObject;
            go.layer = groundLayer;                  // PlayerController2D buna bakiyor

            // SIRA ONEMLI: compositeOperation ancak ayni nesnede bir
            // CompositeCollider2D varken atanabilir. Once bilesenler,
            // sonra baglanti.
            var collider = go.AddComponent<TilemapCollider2D>();

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;  // Dynamic olursa zemin duser

            var composite = go.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Outlines;
            composite.generationType = CompositeCollider2D.GenerationType.Synchronous;

            collider.compositeOperation = Collider2D.CompositeOperation.Merge;

            // Surtunmesiz: karakterin duvara yapismasini engeller.
            // Oyuncunun kendi fizik materyali de var ama zemin tarafi da
            // surtunmesiz olmali, yoksa ikisinin ortalamasi alinir.
            var material = new PhysicsMaterial2D("GroundFrictionless")
            {
                friction = 0f,
                bounciness = 0f,
            };
            composite.sharedMaterial = material;
        }

        /// <summary>
        /// Tehlike katmani: tek trigger, tek script.
        ///
        /// Epic 05: "Hazards Tilemap'ine tek bir Hazard script'i ekle - tum
        /// dikenler tek seferde calisir." 40 dikenli bir bolumde 40 nesne
        /// yerine 1 nesne.
        /// </summary>
        private static void ConfigureHazards(Tilemap map)
        {
            GameObject go = map.gameObject;

            var collider = go.AddComponent<TilemapCollider2D>();
            collider.isTrigger = true;

            go.AddComponent<Gameplay.Hazard>();
        }

        // ---------------------------------------------------------------
        // Boyama yardimcilari
        // ---------------------------------------------------------------

        /// <summary>Dunya koordinatini hucreye cevirir.</summary>
        public Vector3Int CellOf(Vector2 worldPosition) =>
            Grid.WorldToCell(worldPosition);

        /// <summary>
        /// Zemine dikdortgen doldurur. x ve y HUCRE indeksi, dunya birimi degil
        /// (bu iki sey 1 birim = 1 hucre oldugu icin ayni sayi ama karismasin).
        /// </summary>
        public void FillGround(int xFrom, int xTo, int yFrom, int yTo, TileBase tile)
        {
            for (int y = yFrom; y <= yTo; y++)
            {
                for (int x = xFrom; x <= xTo; x++)
                {
                    Ground.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
}
