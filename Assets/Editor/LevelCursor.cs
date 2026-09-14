using UnityEditor;
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

        // ---------------------------------------------------------------

        private readonly Transform parent;
        private readonly int groundLayer;
        private readonly string levelName;

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

        private int issueCount;
        private int warningCount;

        private const float GroundThickness = 1f;

        public LevelCursor(Transform parent, int groundLayer, string levelName = "Bolum",
                           float startX = 0f, float startGroundTop = 0f)
        {
            this.parent = parent;
            this.groundLayer = groundLayer;
            this.levelName = levelName;
            X = startX;
            GroundTop = startGroundTop;
        }

        // ---------------------------------------------------------------
        // Zemin
        // ---------------------------------------------------------------

        /// <summary>Duz zemin ekler ve imleci ilerletir.</summary>
        public LevelCursor Ground(float width, string name = null)
        {
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
            ValidateHeight(height);

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
            ValidateGap(width);

            if (coinArc > 0) PlaceCoinArc(X, width, coinArc);

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

            var go = new GameObject($"Diken_{count}");
            go.transform.position = new Vector3(startX + count * 0.5f, GroundTop + 0.5f, 0f);
            go.transform.SetParent(parent);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("spike");
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(count, 1f);
            renderer.sortingOrder = 1;

            // Collider gorselden KUCUK - oyuncu "degmedim ki" dememeli
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(count - 0.25f, 0.5f);
            trigger.offset = new Vector2(0f, -0.18f);

            go.AddComponent<Gameplay.Hazard>();
            return this;
        }

        public LevelCursor Checkpoint(float offsetFromSegmentStart = 1f)
        {
            var go = new GameObject("Checkpoint");
            go.transform.position = new Vector3(
                lastSegmentStart + offsetFromSegmentStart, GroundTop + 0.75f, 0f);
            go.transform.SetParent(parent);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("checkpoint");
            renderer.sortingOrder = 5;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.2f, 1.5f);

            go.AddComponent<Gameplay.Checkpoint>();
            return this;
        }

        public LevelCursor Goal(float offsetFromSegmentStart = -1f)
        {
            float gx = offsetFromSegmentStart >= 0f
                ? lastSegmentStart + offsetFromSegmentStart
                : lastSegmentStart + lastSegmentWidth * 0.6f;

            var go = new GameObject("LevelGoal");
            go.transform.position = new Vector3(gx, GroundTop + 0.875f, 0f);
            go.transform.SetParent(parent);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("goal");
            renderer.sortingOrder = 5;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.3f, 1.75f);

            go.AddComponent<Gameplay.LevelGoal>();
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

        /// <summary>Insa bitince cagir: ozet ve sorun sayisi.</summary>
        public void Report()
        {
            string status = issueCount > 0
                ? $"{issueCount} GECILEMEZ NOKTA"
                : warningCount > 0
                    ? $"{warningCount} uyari"
                    : "sorun yok";

            Debug.Log(
                $"[{levelName}] insa tamamlandi — {status}\n" +
                $"  uzunluk: {X:0.0} birim\n" +
                $"  yaklasik oynanis: {(X / 8f):0} saniye (kosarak, duraksamadan)");
        }

        public int IssueCount => issueCount;

        // ---------------------------------------------------------------
        // Ic yardimcilar
        // ---------------------------------------------------------------

        private void Place(string name, float xLeft, float width, float top)
        {
            MaxGroundTop = Mathf.Max(MaxGroundTop, top);
            MinGroundTop = Mathf.Min(MinGroundTop, top);

            var go = new GameObject(name);
            go.transform.position = new Vector3(
                xLeft + width * 0.5f, top - GroundThickness * 0.5f, 0f);
            go.transform.SetParent(parent);
            go.layer = groundLayer;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("ground");
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(width, GroundThickness);
            renderer.sortingOrder = 0;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(width, GroundThickness);
        }

        private void PlaceCoin(Vector2 position)
        {
            var coin = new GameObject("Coin");
            coin.transform.position = position;
            coin.transform.SetParent(parent);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(coin.transform);
            visual.transform.localPosition = Vector3.zero;

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("coin");
            renderer.sortingOrder = 5;

            var trigger = coin.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.45f;

            coin.AddComponent<Gameplay.Coin>();
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
