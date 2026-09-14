using UnityEngine;
using Platformer.Player;

namespace Platformer.DevTools
{
    /// <summary>
    /// Epic 02 - Gorev 4: zipla yuksekligini ve mesafesini olcer.
    ///
    /// Iki onemli detay:
    ///
    /// 1) Olcum OnJumped olayiyla baslar, "yerden ayrildi" aniyla degil.
    ///    Yerden ayrilmayi bekleseydik bir fizik karesi (~0.02 sn) gec kalirdik
    ///    ve karakter o sirada zaten ~0.3 birim yukselmis olurdu. Her olcum
    ///    eksik cikardi.
    ///
    /// 2) Sadece DUZ ziplamalar kayda gecer (kalkis ve inis yuksekligi yakinsa).
    ///    Basamaktan asagi ziplarsan havada daha uzun kalirsin ve mesafe sisirilir;
    ///    bu sayiyi bolum tasarimina cetvel yapamazsin.
    ///
    /// Olcum bittikten sonra bu bileseni silebilirsin.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class JumpMeasure : MonoBehaviour
    {
        [Header("Gosterim")]
        [SerializeField] private bool showOnScreen = true;
        [SerializeField] private bool logToConsole = true;

        [Header("Olcum")]
        [Tooltip("Kalkis-inis yukseklik farki bundan kucukse 'duz zipla' sayilir.")]
        [SerializeField] private float flatJumpTolerance = 0.4f;

        private PlayerController2D controller;

        // --- Su anki zipla ---
        private bool measuring;
        private bool leftGround;      // karakter gercekten havalandi mi
        private float startX, startY, maxY;
        private bool dashUsedThisJump;

        // --- Son olcum ---
        private float lastHeight, lastDistance, lastDrop;
        private bool lastUsedDash, lastWasFlat;

        // --- En iyiler (sadece duz ziplamalar) ---
        private float bestHeight;
        private float bestDistanceNoDash;
        private float bestDistanceWithDash;

        private void Awake() => controller = GetComponent<PlayerController2D>();

        private void OnEnable() => controller.OnJumped += BeginMeasure;
        private void OnDisable() => controller.OnJumped -= BeginMeasure;

        /// <summary>Zipla tetiklendigi anda baslar - tam dogru baslangic noktasi.</summary>
        private void BeginMeasure()
        {
            measuring = true;
            leftGround = false;
            startX = transform.position.x;
            startY = transform.position.y;
            maxY = startY;
            dashUsedThisJump = false;
        }

        private void Update()
        {
            if (!measuring) return;

            maxY = Mathf.Max(maxY, transform.position.y);
            if (controller.IsDashing) dashUsedThisJump = true;

            // OnJumped, zemin kontrolunun ARDINDAN ayni fizik karesinde tetikleniyor,
            // yani o an IsGrounded hala true. Once gercekten havalanmayi bekle,
            // yoksa zipla daha baslamadan "yere indi" sanip 0 olceriz.
            if (!leftGround)
            {
                if (!controller.IsGrounded) leftGround = true;
                return;
            }

            if (!controller.IsGrounded) return;

            // --- Yere indik: sonucu degerlendir ---
            measuring = false;

            lastHeight = maxY - startY;
            lastDistance = Mathf.Abs(transform.position.x - startX);
            lastDrop = transform.position.y - startY;      // negatif = asagi indik
            lastUsedDash = dashUsedThisJump;
            lastWasFlat = Mathf.Abs(lastDrop) <= flatJumpTolerance;

            // Sadece duz ziplamalar cetvele girer
            if (lastWasFlat)
            {
                if (lastUsedDash)
                {
                    bestDistanceWithDash = Mathf.Max(bestDistanceWithDash, lastDistance);
                }
                else
                {
                    bestHeight = Mathf.Max(bestHeight, lastHeight);
                    bestDistanceNoDash = Mathf.Max(bestDistanceNoDash, lastDistance);
                }
            }

            if (logToConsole)
            {
                string tag = lastWasFlat ? "" : $"  [EGIMLI {lastDrop:+0.00;-0.00} — sayilmadi]";
                UnityEngine.Debug.Log(
                    $"Zipla — yukseklik {lastHeight:F2} | mesafe {lastDistance:F2}" +
                    (lastUsedDash ? " [dash]" : "") + tag);
            }
        }

        private void ResetRecords()
        {
            bestHeight = bestDistanceNoDash = bestDistanceWithDash = 0f;
            lastHeight = lastDistance = lastDrop = 0f;
            lastWasFlat = true;
        }

        private void OnGUI()
        {
            if (!showOnScreen) return;

            if (Event.current.type == EventType.KeyDown
                && Event.current.keyCode == KeyCode.Backspace)
            {
                ResetRecords();
            }

            const float w = 400f, h = 210f;
            GUI.Box(new Rect(12f, 12f, w, h), GUIContent.none);

            var title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16, fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            var line = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = new Color(0.90f, 0.93f, 0.97f) }
            };
            var warn = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12, wordWrap = true,
                normal = { textColor = new Color(0.95f, 0.62f, 0.45f) }
            };
            var dim = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11, wordWrap = true,
                normal = { textColor = new Color(0.60f, 0.66f, 0.74f) }
            };

            float x = 26f, cw = w - 28f, y = 20f;

            GUI.Label(new Rect(x, y, cw, 22f), "OLCUM  (duz ziplamalar)", title);
            y += 26f;

            // Son zipla - egimliyse uyar
            string dashTag = lastUsedDash ? "  [dash]" : "";
            GUI.Label(new Rect(x, y, cw, 20f),
                $"Son: {lastHeight:F2} yuksek / {lastDistance:F2} uzak{dashTag}", line);
            y += 22f;

            if (!lastWasFlat && lastDistance > 0.01f)
            {
                GUI.Label(new Rect(x, y, cw, 30f),
                    $"↑ {lastDrop:+0.00;-0.00} birim kot farki — cetvele sayilmadi", warn);
                y += 30f;
            }
            else
            {
                y += 6f;
            }

            GUI.Label(new Rect(x, y, cw, 20f),
                $"Maks. yukseklik          : {bestHeight:F2}", line); y += 21f;
            GUI.Label(new Rect(x, y, cw, 20f),
                $"Maks. mesafe  (dash'siz) : {bestDistanceNoDash:F2}", line); y += 21f;
            GUI.Label(new Rect(x, y, cw, 20f),
                $"Maks. mesafe  (dash'li)  : {bestDistanceWithDash:F2}", line); y += 24f;

            string hint;
            if (bestHeight < 0.01f)
                hint = "Duz zeminde dur, Space'i BASILI TUT, yere inene kadar birakma.";
            else if (bestDistanceNoDash < 0.01f)
                hint = "Duz zeminde tam hizda kos, sonra zipla (Space basili).";
            else if (bestDistanceWithDash < 0.01f)
                hint = "Ayni ziplamayi yap, tepe noktasinda Shift'e bas.";
            else
                hint = $"dash kazanci: +{(bestDistanceWithDash - bestDistanceNoDash):F2} birim" +
                       "   ·   Backspace: sifirla";

            GUI.Label(new Rect(x, 12f + h - 34f, cw, 30f), hint, dim);
        }
    }
}
