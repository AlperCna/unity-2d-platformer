using UnityEngine;
using Platformer.Player;

namespace Platformer.DevTools
{
    /// <summary>
    /// Epic 02 - Gorev 2: hazir ayarlar arasinda oyun icinde anlik gecis.
    ///
    /// Neden: ilk oyununu yapan biri "bu zipla iyi mi?" sorusuna cevap veremez,
    /// ama "A mi B mi daha iyi?" sorusuna kolayca cevap verir.
    /// Karsilastirma yapmak, mutlak yargi vermekten cok daha kolaydir.
    ///
    /// Tuslar:  1-4 hazir ayar  |  0 sahnedeki orijinal  |  TAB detay
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class FeelTuner : MonoBehaviour
    {
        [System.Serializable]
        public class Preset
        {
            public string name;
            [TextArea(2, 3)] public string character;
            [TextArea(2, 3)] public string watchFor;
            public PlayerController2D.MovementSettings settings;
        }

        [SerializeField] private bool showPanel = true;

        private PlayerController2D controller;
        private PlayerController2D.MovementSettings original;
        private Preset[] presets;
        private int activeIndex = -1;     // -1 = orijinal
        private bool showDetail;

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            original = controller.GetSettings();
            BuildPresets();
        }

        private void BuildPresets()
        {
            presets = new[]
            {
                new Preset
                {
                    name = "1 · AGIR",
                    character = "Yavas, agirlikli, her zipla bir karar. " +
                                "Castlevania / Limbo cizgisi.",
                    watchFor = "Ziplarken geri donemiyorsun. Hata yapinca duzeltme sansin yok.",
                    settings = new PlayerController2D.MovementSettings
                    {
                        moveSpeed = 6f,
                        accelerationTime = 0.16f,
                        decelerationTime = 0.13f,
                        airControl = 0.55f,
                        jumpHeight = 3.0f,
                        jumpApexTime = 0.44f,
                        fallGravityMultiplier = 1.6f,
                        jumpCutMultiplier = 0.55f,
                        coyoteTime = 0.08f,
                        jumpBufferTime = 0.10f
                    }
                },
                new Preset
                {
                    name = "2 · DENGELI",
                    character = "Orta hiz, affedici zipla, kesif icin yer. " +
                                "Mario cizgisi. Su an secili olan tarz.",
                    watchFor = "Her sey 'normal' hissettiriyor mu? Sikici degil, asiri de degil.",
                    settings = new PlayerController2D.MovementSettings
                    {
                        moveSpeed = 8f,
                        accelerationTime = 0.08f,
                        decelerationTime = 0.06f,
                        airControl = 0.75f,
                        jumpHeight = 3.2f,
                        jumpApexTime = 0.38f,
                        fallGravityMultiplier = 1.9f,
                        jumpCutMultiplier = 0.45f,
                        coyoteTime = 0.10f,
                        jumpBufferTime = 0.12f
                    }
                },
                new Preset
                {
                    name = "3 · KESKIN",
                    character = "Hizli, ani, hassas. Tus birakinca aninda duruyor. " +
                                "Celeste / Super Meat Boy cizgisi.",
                    watchFor = "Kontrol 'anlik' mi? Dusus cok hizli gelip kafa karistiriyor mu?",
                    settings = new PlayerController2D.MovementSettings
                    {
                        moveSpeed = 9.5f,
                        accelerationTime = 0.035f,
                        decelerationTime = 0.03f,
                        airControl = 0.92f,
                        jumpHeight = 3.0f,
                        jumpApexTime = 0.32f,
                        fallGravityMultiplier = 2.3f,
                        jumpCutMultiplier = 0.35f,
                        coyoteTime = 0.12f,
                        jumpBufferTime = 0.15f
                    }
                },
                new Preset
                {
                    name = "4 · UCUCU",
                    character = "Yuksek ve yavas zipla, havada uzun sure. " +
                                "Ay yercekimi hissi.",
                    watchFor = "Havada asili kalmak keyifli mi, yoksa yavas mi geliyor?",
                    settings = new PlayerController2D.MovementSettings
                    {
                        moveSpeed = 8.5f,
                        accelerationTime = 0.14f,
                        decelerationTime = 0.20f,
                        airControl = 0.90f,
                        jumpHeight = 3.8f,
                        jumpApexTime = 0.50f,
                        fallGravityMultiplier = 1.3f,
                        jumpCutMultiplier = 0.60f,
                        coyoteTime = 0.14f,
                        jumpBufferTime = 0.14f
                    }
                }
            };
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) Apply(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Apply(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Apply(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) Apply(3);
            if (Input.GetKeyDown(KeyCode.Alpha0)) RestoreOriginal();
            if (Input.GetKeyDown(KeyCode.Tab)) showDetail = !showDetail;
        }

        private void Apply(int index)
        {
            if (presets == null || index < 0 || index >= presets.Length) return;

            activeIndex = index;
            controller.ApplySettings(presets[index].settings);

            var s = presets[index].settings;
            UnityEngine.Debug.Log(
                $"[Ayar] {presets[index].name}  —  " +
                $"havada {s.AirTime:F2} sn, mesafe {s.JumpDistance:F2} birim, " +
                $"yukseklik {s.jumpHeight:F1} birim");
        }

        private void RestoreOriginal()
        {
            activeIndex = -1;
            controller.ApplySettings(original);
            UnityEngine.Debug.Log("[Ayar] Sahnedeki orijinal degerlere donuldu.");
        }

        // ---------------------------------------------------------------

        private void OnGUI()
        {
            if (!showPanel) return;

            PlayerController2D.MovementSettings s =
                activeIndex >= 0 ? presets[activeIndex].settings : original;

            string title = activeIndex >= 0 ? presets[activeIndex].name : "0 · ORIJINAL";

            float w = 420f;
            float h = showDetail ? 320f : 196f;
            float x = Screen.width - w - 12f;

            GUI.Box(new Rect(x, 12f, w, h), GUIContent.none);

            var head = new GUIStyle(GUI.skin.label)
            {
                fontSize = 17, fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.72f, 0.27f) }
            };
            var body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13, wordWrap = true,
                normal = { textColor = new Color(0.90f, 0.93f, 0.97f) }
            };
            var note = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12, wordWrap = true, fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.62f, 0.82f, 0.68f) }
            };
            var dim = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                normal = { textColor = new Color(0.58f, 0.64f, 0.72f) }
            };

            float cx = x + 16f;
            float cw = w - 32f;
            float y = 20f;

            GUI.Label(new Rect(cx, y, cw, 24f), title, head);
            y += 28f;

            if (activeIndex >= 0)
            {
                GUI.Label(new Rect(cx, y, cw, 40f), presets[activeIndex].character, body);
                y += 42f;
                GUI.Label(new Rect(cx, y, cw, 34f), "» " + presets[activeIndex].watchFor, note);
                y += 38f;
            }
            else
            {
                GUI.Label(new Rect(cx, y, cw, 40f),
                    "Sahnedeki kayitli degerler. 1-4 ile hazir ayarlari dene, " +
                    "begendigini not al.", body);
                y += 46f;
            }

            // Olculer - tarz degisince bu sayilarin nasil degistigini gor
            GUI.Label(new Rect(cx, y, cw, 20f),
                $"Havada kalma : {s.AirTime:F2} sn", body); y += 19f;
            GUI.Label(new Rect(cx, y, cw, 20f),
                $"Zipla mesafesi: {s.JumpDistance:F2} birim", body); y += 19f;
            GUI.Label(new Rect(cx, y, cw, 20f),
                $"Zipla yuksekligi: {s.RealJumpHeight:F2} birim  " +
                $"(ayar: {s.jumpHeight:F2})", body); y += 24f;

            if (showDetail)
            {
                GUI.Label(new Rect(cx, y, cw, 18f), $"Hiz            : {s.moveSpeed:F2}", dim); y += 17f;
                GUI.Label(new Rect(cx, y, cw, 18f), $"Hizlanma       : {s.accelerationTime:F3} sn", dim); y += 17f;
                GUI.Label(new Rect(cx, y, cw, 18f), $"Yavaslama      : {s.decelerationTime:F3} sn", dim); y += 17f;
                GUI.Label(new Rect(cx, y, cw, 18f), $"Havada kontrol : {s.airControl:F2}", dim); y += 17f;
                GUI.Label(new Rect(cx, y, cw, 18f), $"Tepeye cikis   : {s.jumpApexTime:F2} sn", dim); y += 17f;
                GUI.Label(new Rect(cx, y, cw, 18f), $"Dusus carpani  : {s.fallGravityMultiplier:F2}", dim); y += 17f;
                GUI.Label(new Rect(cx, y, cw, 18f), $"Yercekimi      : {s.Gravity:F1}", dim); y += 20f;
            }

            GUI.Label(new Rect(cx, 12f + h - 24f, cw, 18f),
                "1-4 tarz  ·  0 orijinal  ·  TAB detay", dim);
        }
    }
}
