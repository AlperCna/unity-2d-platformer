using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Bolumu soldan saga, adim adim insa eder.
    ///
    /// Koordinat yazmak yerine "12 birim zemin, 2,5 birim bosluk, 1,2 birim
    /// basamak" diye tarif edersin. Imlec konumu kendisi takip eder.
    ///
    /// Asil degeri DOGRULAMA: her bosluk ve basamak, Epic 02'de olculen
    /// cetvele karsi kontrol edilir. Gecilemez bir bosluk koyarsan konsola
    /// hata yazar - oyunu oynayip "burasi neden gecilmiyor" demeden once.
    /// </summary>
    public class LevelCursor
    {
        // ---------------------------------------------------------------
        // OLCULEN CETVEL — Epic 02, docs/AYARLAR.md
        // Bu sayilar degisirse dogrulama da degisir.
        // ---------------------------------------------------------------

        /// <summary>Olculen maksimum zipla yuksekligi.</summary>
        public const float MaxJumpHeight = 3.03f;

        /// <summary>En hizli dokunusla bile ulasilan yukseklik.</summary>
        public const float MinJumpHeight = 1.5f;

        /// <summary>Kosarak, dash'siz maksimum mesafe.</summary>
        public const float MaxDistanceNoDash = 5.31f;

        /// <summary>Kosarak + dash maksimum mesafe.</summary>
        public const float MaxDistanceWithDash = 7.62f;

        /// <summary>Bu araligi kullanma: dash'siz imkansiz, dash'li bedava.</summary>
        public const float AmbiguousLow = 5.32f;
        public const float AmbiguousHigh = 5.49f;

        /// <summary>
        /// Zipla yayinin tepesi, yatay mesafenin yuzde kacinda.
        /// Dusus yercekimi cikistan 1,9 kat guclu oldugu icin tepe ortada degil,
        /// biraz ileride: sqrt(1,9) / (sqrt(1,9) + 1) = 0,58.
        /// </summary>
        public const float ApexDistanceFraction = 0.58f;

        // ---------------------------------------------------------------

        private readonly Transform parent;
        private readonly int groundLayer;
        private readonly TilemapRig rig;
        private readonly string levelName;

        /// <summary>Dolu hucreler. Karo maskeleri Paint()'te bundan hesaplanir.</summary>
        private readonly HashSet<Vector3Int> solidCells = new HashSet<Vector3Int>();

        /// <summary>Diken hucreleri.</summary>
        private readonly HashSet<Vector3Int> hazardCells = new HashSet<Vector3Int>();

        /// <summary>Imlecin su anki sag kenari.</summary>
        public float X { get; private set; }

        /// <summary>Su anki zemin ust yuzeyi.</summary>
        public float GroundTop { get; private set; }

        /// <summary>Bolumdeki en yuksek zemin - kamera sinirlari icin.</summary>
        public float MaxGroundTop { get; private set; }

        /// <summary>Bolumdeki en alcak zemin.</summary>
        public float MinGroundTop { get; private set; }

        /// <summary>Son yerlestirilen zemin parcasinin sol kenari ve genisligi.</summary>
        private float lastSegmentStart;
        private float lastSegmentWidth;

        /// <summary>Hemen onceki islem bosluksa genisligi; degilse 0.</summary>
        private float pendingGapWidth;

        /// <summary>Basilabilir zemin araliklari (x = sol, y = sag).</summary>
        private readonly List<Vector2> solidSpans = new List<Vector2>();

        /// <summary>Diken araliklari (x = sol, y = sag).</summary>
        private readonly List<Vector2> spikeSpans = new List<Vector2>();

        private int issueCount;
        private int warningCount;

        /// <summary>
        /// Zeminin yuzeyden asagi kac hucre devam ettigi.
        ///
        /// Oynanisi hic etkilemez - sadece gorseldir. 1 hucre ince bir
        /// tahta gibi duruyordu; 4 hucre toprak kutlesi gibi duruyor ve
        /// bosluklar "ucurum" olarak okunuyor.
        /// </summary>
        private const int GroundDepth = 4;

        public LevelCursor(Transform parent, int groundLayer, TilemapRig rig,
                           string levelName = "Bolum",
                           float startX = 0f, float startGroundTop = 0f)
        {
            this.parent = parent;
            this.groundLayer = groundLayer;
            this.rig = rig;
            this.levelName = levelName;
            X = startX;
            GroundTop = startGroundTop;
        }

        // ---------------------------------------------------------------
        // Izgaraya hizalama
        // ---------------------------------------------------------------

        /// <summary>
        /// Tilemap TAM SAYI hucrelere basar. Kesirli bir genislik verilirse
        /// sessizce yuvarlanmaz - yuvarlanir ve SOYLENIR.
        ///
        /// Sessiz yuvarlama en kotusu olurdu: dokumanda 2,5 yazar, oyunda 3
        /// olur, zorluk yuzdesi tutmaz ve kimse farketmez.
        /// </summary>
        private float Snap(float value, string what)
        {
            int snapped = Mathf.RoundToInt(value);

            if (!Mathf.Approximately(value, snapped))
            {
                warningCount++;
                Debug.LogWarning(
                    $"[{levelName}] x={X:0.0} - {what} {value:0.00} -> {snapped} " +
                    $"hucreye yuvarlandi.\n" +
                    $"  1 birim = 1 hucre; kesirli deger basilamaz. " +
                    $"Tasarima tam sayi yaz ki dokuman ile oyun ayni sey olsun.");
            }

            return snapped;
        }

        // ---------------------------------------------------------------
        // Zemin
        // ---------------------------------------------------------------

        /// <summary>Duz zemin ekler ve imleci ilerletir.</summary>
        public LevelCursor Ground(float width, string name = null)
        {
            pendingGapWidth = 0f;
            width = Snap(width, "zemin genisligi");
            Place(name ?? $"Zemin_{X:0}", X, width, GroundTop);
            lastSegmentStart = X;
            lastSegmentWidth = width;
            X += width;
            return this;
        }

        /// <summary>
        /// Basamak: mevcut zeminden yukari cikan yeni bir kat.
        /// Yukseklik cetvele karsi dogrulanir.
        /// </summary>
        public LevelCursor Step(float height, float width, string name = null)
        {
            height = Snap(height, "basamak yuksekligi");
            width = Snap(width, "basamak genisligi");

            ValidateHeight(height);
            if (pendingGapWidth > 0f) ValidateGapWithRise(pendingGapWidth, height);
            pendingGapWidth = 0f;

            GroundTop += height;
            Place(name ?? $"Basamak_{height:0.0}", X, width, GroundTop);
            lastSegmentStart = X;
            lastSegmentWidth = width;
            X += width;
            return this;
        }

        /// <summary>Asagi inen kat. Dususte sinir yok, sadece olum cizgisi var.</summary>
        public LevelCursor Drop(float height, float width, string name = null)
        {
            pendingGapWidth = 0f;
            height = Snap(height, "inis yuksekligi");
            width = Snap(width, "inis genisligi");
            GroundTop -= height;
            Place(name ?? $"Inis_{height:0.0}", X, width, GroundTop);
            lastSegmentStart = X;
            lastSegmentWidth = width;
            X += width;
            return this;
        }

        /// <summary>
        /// Bosluk: zemin yok, imlec ilerler.
        /// Genislik cetvele karsi dogrulanir.
        /// coinArc > 0 ise boslugun uzerine zipla yayini cizen paralar konur.
        /// </summary>
        public LevelCursor Gap(float width, int coinArc = 0)
        {
            width = Snap(width, "bosluk genisligi");
            ValidateGap(width);

            if (coinArc > 0) PlaceCoinArc(X, width, coinArc);

            pendingGapWidth = width;
            X += width;
            return this;
        }

        // ---------------------------------------------------------------
        // Icerik
        // ---------------------------------------------------------------

        /// <summary>Son zemin parcasinin uzerine yatay para dizisi.</summary>
        public LevelCursor Coins(int count, float heightAboveGround = 1.6f, float spacing = 1.2f)
        {
            float totalWidth = (count - 1) * spacing;
            float startX = lastSegmentStart + (lastSegmentWidth - totalWidth) * 0.5f;

            for (int i = 0; i < count; i++)
            {
                PlaceCoin(new Vector2(startX + i * spacing, GroundTop + heightAboveGround));
            }
            return this;
        }

        /// <summary>Son zemin parcasinin uzerine diken.</summary>
        public LevelCursor Spikes(int count, float offsetFromSegmentStart = -1f)
        {
            float startX = offsetFromSegmentStart >= 0f
                ? lastSegmentStart + offsetFromSegmentStart
                : lastSegmentStart + (lastSegmentWidth - count) * 0.5f;

            spikeSpans.Add(new Vector2(startX, startX + count));

            // NEDEN TILEMAP DEGIL:
            // Epic 05 tehlikeleri Hazards Tilemap'ine koymayi istiyor ve iskelet
            // hazir. Ama Tilemap collider'i hucrenin TAMAMINI kaplar; asagidaki
            // ayarlanmis collider ise gorselin sadece alt yarisini kapliyor.
            //
            // Bu fark bilincli bir oyun hissi karari (Epic 07/09): oyuncu
            // dikenin ucunu siyirip kurtulabilmeli, "degmedim ki" dememeli.
            // Nesne sayisini azaltmak icin bunu feda etmiyoruz - zaten bolumde
            // 4 diken grubu var, kazanc yok.
            //
            // Hazards katmani, hucre boyu collider'in sorun olmadigi
            // tehlikeler (lav, su) icin hazir bekliyor.
            GameObject go = PrefabFactory.Spawn(PrefabFactory.Spikes,
                new Vector2(startX + count * 0.5f, GroundTop + 0.5f), parent);
            if (go == null) return this;

            go.name = $"Diken_{count}";

            // Prefab tek birimlik; kac birim olacagini burada ayarliyoruz.
            // Bunlar prefab USTUNDE degisiklik (override) olarak duruyor -
            // prefab'in geri kalan ayarlari (collider yuksekligi, offset)
            // prefab'dan gelmeye devam ediyor.
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.size = new Vector2(count, 1f);

            var trigger = go.GetComponent<BoxCollider2D>();
            trigger.size = new Vector2(count - 0.25f, trigger.size.y);
            return this;
        }

        public LevelCursor Checkpoint(float offsetFromSegmentStart = 1f)
        {
            PrefabFactory.Spawn(PrefabFactory.Checkpoint,
                new Vector2(lastSegmentStart + offsetFromSegmentStart, GroundTop + 0.75f),
                parent);
            return this;
        }

        public LevelCursor Goal(float offsetFromSegmentStart = -1f)
        {
            float gx = offsetFromSegmentStart >= 0f
                ? lastSegmentStart + offsetFromSegmentStart
                : lastSegmentStart + lastSegmentWidth * 0.6f;

            PrefabFactory.Spawn(PrefabFactory.LevelGoal,
                new Vector2(gx, GroundTop + 0.875f), parent);
            return this;
        }

        // ---------------------------------------------------------------
        // Dogrulama — bu sinifin asil degeri
        // ---------------------------------------------------------------

        private void ValidateGap(float width)
        {
            if (width > MaxDistanceWithDash)
            {
                issueCount++;
                Debug.LogError(
                    $"[{levelName}] x={X:0.0} — {width:0.00} birimlik bosluk GECILEMEZ.\n" +
                    $"  Dash'li maksimum mesafe {MaxDistanceWithDash:0.00}. " +
                    $"Oyuncu deneyip deneyip basaramaz ve oyunu bozuk sanar.");
                return;
            }

            if (width >= AmbiguousLow && width <= AmbiguousHigh)
            {
                warningCount++;
                Debug.LogWarning(
                    $"[{levelName}] x={X:0.0} — {width:0.00} birimlik bosluk BELIRSIZ BOLGEDE.\n" +
                    $"  Dash'siz imkansiz ({MaxDistanceNoDash:0.00}), dash'li bedava. " +
                    $"Hicbir sey ogretmez. {AmbiguousLow - 0.3f:0.0} altina veya " +
                    $"{AmbiguousHigh + 0.3f:0.0} ustune al.");
                return;
            }

            if (width > MaxDistanceNoDash)
            {
                Debug.Log($"[{levelName}] x={X:0.0} — {width:0.00} birim: DASH ZORUNLU " +
                          $"(%{(width / MaxDistanceWithDash * 100f):0} dash'li maksimumun)");
                return;
            }

            float ratio = width / MaxDistanceNoDash;
            if (ratio > 0.92f)
            {
                Debug.Log($"[{levelName}] x={X:0.0} — {width:0.00} birim: " +
                          $"MAKSIMUMA YAKIN (%{ratio * 100f:0}). Seyrek kullan.");
            }
        }

        /// <summary>
        /// Belirli bir YUKSEKLIKTEKI platforma dash'siz ulasilabilecek
        /// en uzun yatay mesafe.
        ///
        /// Duz zeminde 5,31 birim atlarsin. Ama 1,4 birim YUKARIDAKI bir
        /// platforma atlayacaksan, inis noktasina varmadan once o yuksekligin
        /// altina dusmus olmamalisin - yani mesafe kisalir.
        ///
        /// Yorunge tepeden sonra parabol ciziyor:
        ///   x(h) = xTepe + (D - xTepe) * sqrt(1 - h/H)
        /// h = 0 icin D'yi, h = H icin tepeyi verir.
        /// </summary>
        public static float MaxDistanceAtHeight(float height)
        {
            if (height <= 0f) return MaxDistanceNoDash;              // duz veya asagi
            if (height >= MaxJumpHeight) return 0f;                  // cikilamaz

            float apexX = MaxDistanceNoDash * ApexDistanceFraction;
            return apexX + (MaxDistanceNoDash - apexX)
                         * Mathf.Sqrt(1f - height / MaxJumpHeight);
        }

        /// <summary>
        /// Bosluk + hemen ardindan yukari basamak. Ikisi tek tek gecilebilir
        /// olsa da BIRLIKTE gecilemez olabilir - asil tehlike bu.
        /// </summary>
        private void ValidateGapWithRise(float gapWidth, float rise)
        {
            float limit = MaxDistanceAtHeight(rise);

            if (gapWidth > limit)
            {
                issueCount++;
                Debug.LogError(
                    $"[{levelName}] x={X:0.0} — {gapWidth:0.00} birim bosluk + " +
                    $"{rise:0.00} birim yukselis GECILEMEZ.\n" +
                    $"  {rise:0.00} birim yukarida inis icin en fazla {limit:0.00} birim " +
                    $"atlanir (duz zeminde {MaxDistanceNoDash:0.00} olurdu).\n" +
                    $"  Ya boslugu daralt ya basamagi alcalt.");
                return;
            }

            float ratio = gapWidth / limit;
            if (ratio > 0.9f)
            {
                warningCount++;
                Debug.LogWarning(
                    $"[{levelName}] x={X:0.0} — {gapWidth:0.00} bosluk + {rise:0.00} " +
                    $"yukselis: limitin %{ratio * 100f:0}'i ({limit:0.00}). " +
                    $"Ilk bolumler icin cok sert.");
                return;
            }

            Debug.Log($"[{levelName}] x={X:0.0} — {gapWidth:0.00} bosluk + {rise:0.00} " +
                      $"yukselis: %{ratio * 100f:0} (limit {limit:0.00}).");
        }

        private void ValidateHeight(float height)
        {
            if (height > MaxJumpHeight)
            {
                issueCount++;
                Debug.LogError(
                    $"[{levelName}] x={X:0.0} — {height:0.00} birimlik basamak CIKILAMAZ.\n" +
                    $"  Maksimum zipla yuksekligi {MaxJumpHeight:0.00}. " +
                    $"Dash yatay oldugu icin yardimci olmuyor.");
                return;
            }

            if (height < MinJumpHeight)
            {
                Debug.Log($"[{levelName}] x={X:0.0} — {height:0.00} birim basamak: " +
                          $"dokunusla gecilir (min. zipla {MinJumpHeight:0.0}), " +
                          $"ritim icin iyi, meydan okuma degil.");
            }
        }

        /// <summary>
        /// Her dikenin ardinda TAM ziplayan oyuncuya yer var mi?
        ///
        /// Kotu tasarimin sinsi hali: oyuncu dikeni gorur, cekinir, tam
        /// guc ziplar - ve dikenin arkasindaki bosluga duser. Yanlis yaptigi
        /// icin degil, FAZLA dikkatli davrandigi icin olur. Boyle bir olum
        /// oyuncuya "bu oyun bozuk" dedirtir.
        ///
        /// Bolum 1 v2'yi kurarken bu tuzaktan uc tane cikti. Elle bulundu;
        /// bir daha elle aranmasin diye buraya tasindi.
        /// </summary>
        private void ValidateSpikeLandings()
        {
            foreach (Vector2 spike in spikeSpans)
            {
                float takeoff = spike.x - 0.5f;          // dikenin hemen oncesi
                float landing = takeoff + MaxDistanceNoDash;

                if (IsSolidAt(landing)) continue;

                issueCount++;
                Debug.LogError(
                    $"[{levelName}] x={spike.x:0.0} — DIKEN TUZAGI.\n" +
                    $"  Bu dikenden tam guc ziplayan oyuncu {landing:0.0} noktasina " +
                    $"iniyor ve orada zemin YOK.\n" +
                    $"  Dogru ziplayan olur. Dikenden sonra en az " +
                    $"{MaxDistanceNoDash + 0.5f:0.0} birim zemin birak " +
                    $"ya da dikeni geriye al.");
            }
        }

        private bool IsSolidAt(float x)
        {
            foreach (Vector2 span in solidSpans)
            {
                if (x >= span.x - 0.01f && x <= span.y + 0.01f) return true;
            }
            return false;
        }

        /// <summary>Insa bitince cagir: ozet ve sorun sayisi.</summary>
        public void Report()
        {
            ValidateSpikeLandings();

            // 4,2 birim/saniye — OLCULEN deger, uc kosudan:
            //   v1,  83,0 birim / 19,58 sn = 4,24   (ilk kez oynaniyor)
            //   v2, 124,1 birim / 28,86 sn = 4,30   (ilk kez oynaniyor)
            //   v2, 124,1 birim / 22,24 sn = 5,58   (bolum artik BILINIYOR)
            //
            // Ucuncu sayi %30 daha hizli: ayni bolum, ayni oyuncu, sadece
            // ezberlenmis. O yuzden tahmin ILK OYNANIS hizini kullaniyor -
            // "30-90 sn" hedefi de ilk oynanis icin anlamli, tekrar icin degil.
            //
            // Onceki tahmin moveSpeed'i (8) kullaniyordu: oyuncunun hic
            // ziplamadigini, duraksamadigini, para toplamadigini varsayiyordu.
            // Bolum sureleri iki kat kisa gorunuyordu.
            const float MeasuredUnitsPerSecond = 4.2f;
            float estimate = X / MeasuredUnitsPerSecond;

            // Bu bir ALT SINIR: 4,2 degeri v1'den geliyor ve v1 bolumun %54'u
            // bos zemindi. Engel siklastikca oyuncu yavaslar, sure uzar.
            // Bu yuzden sadece kesin hatalari isaretliyoruz.
            string target = estimate > 90f ? "  ** 90 sn ustu: bolum COK UZUN **"
                          : estimate < 20f ? "  ** 20 sn alti: bolum COK KISA **"
                          : "  (hedef 30-90 sn)";

            string status = issueCount > 0
                ? $"{issueCount} GECILEMEZ NOKTA"
                : warningCount > 0
                    ? $"{warningCount} uyari"
                    : "sorun yok";

            Debug.Log(
                $"[{levelName}] insa tamamlandi — {status}\n" +
                $"  uzunluk: {X:0.0} birim\n" +
                $"  oynanis alt siniri: {estimate:0} saniye{target}");
        }

        public int IssueCount => issueCount;

        // ---------------------------------------------------------------
        // Ic yardimcilar
        // ---------------------------------------------------------------

        /// <summary>
        /// Zemin parcasini HUCRE olarak kaydeder. Karoyu hemen basmaz -
        /// bir karonun hangi sprite'i alacagi sag komsusuna da bagli ve
        /// o henuz yerlestirilmemis olabilir. Boyama Paint()'te, her sey
        /// bilindikten sonra tek seferde yapiliyor.
        /// </summary>
        private void Place(string name, float xLeft, float width, float top)
        {
            solidSpans.Add(new Vector2(xLeft, xLeft + width));
            MaxGroundTop = Mathf.Max(MaxGroundTop, top);
            MinGroundTop = Mathf.Min(MinGroundTop, top);

            int x0 = Mathf.RoundToInt(xLeft);
            int x1 = Mathf.RoundToInt(xLeft + width) - 1;   // son hucre dahil
            int yTop = Mathf.RoundToInt(top) - 1;           // yuzey karosu

            for (int x = x0; x <= x1; x++)
            {
                for (int d = 0; d < GroundDepth; d++)
                {
                    solidCells.Add(new Vector3Int(x, yTop - d, 0));
                }
            }
        }

        /// <summary>
        /// Butun hucreleri Tilemap'e basar.
        ///
        /// Her hucrenin karosu dort komsusundan hesaplaniyor - Rule Tile'in
        /// yaptigi isin aynisi, ama tahminsiz: bolum koddan uretildigi icin
        /// hangi hucrenin dolu oldugunu kesin biliyoruz.
        /// </summary>
        /// <summary>
        /// Insa bitince, SAHNE KAYDEDILMEDEN once cagir.
        ///
        /// Onceden Report() icinden cagriliyordu ama Report en sonda,
        /// SaveScene'den SONRA calisiyor - karolar bellekte olusur, dosyaya
        /// yazilmazdi. Oyun oynanirken calisir, sahne kapatilip acilinca
        /// zemin yok olurdu.
        /// </summary>
        public void Build()
        {
            if (rig == null) return;

            var tiles = new Dictionary<int, TileBase>();
            var positions = new List<Vector3Int>(solidCells.Count);
            var chosen = new List<TileBase>(solidCells.Count);

            foreach (Vector3Int cell in solidCells)
            {
                int mask = TileAssetFactory.MaskFor(solidCells.Contains, cell);

                if (!tiles.TryGetValue(mask, out TileBase tile))
                {
                    tile = TileAssetFactory.Load(mask);
                    tiles[mask] = tile;
                }

                if (tile == null) continue;

                positions.Add(cell);
                chosen.Add(tile);
            }

            // Tek tek SetTile yerine toplu basim: 500+ karoda gozle gorulur fark
            rig.Ground.SetTiles(positions.ToArray(), chosen.ToArray());

            Debug.Log($"[{levelName}] {positions.Count} karo basildi " +
                      $"({tiles.Count} farkli karo tipi kullanildi)");
        }

        private void PlaceCoin(Vector2 position)
        {
            PrefabFactory.Spawn(PrefabFactory.Coin, position, parent);
        }

        /// <summary>
        /// Boslugun uzerine zipla yayini cizen paralar.
        /// Oyuncu "su kadar ziplamaliyim" diye dusunmez - paralari takip eder.
        /// </summary>
        private void PlaceCoinArc(float startX, float width, int count)
        {
            // Yayin tepesi: bosluk genisledikce yukselir ama ziplama yuksekligini asmaz
            float peak = Mathf.Min(1.4f + width * 0.25f, MaxJumpHeight * 0.8f);

            for (int i = 0; i < count; i++)
            {
                float t = (i + 0.5f) / count;              // 0..1 arasi, uclardan ickeride
                float parabola = 4f * t * (1f - t);        // 0 -> 1 -> 0
                float x = startX + t * width;
                float y = GroundTop + 1.2f + parabola * peak;

                PlaceCoin(new Vector2(x, y));
            }
        }
    }
}
