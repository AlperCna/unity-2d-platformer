using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Platformer.CameraRig;
using Platformer.DevTools;
using Platformer.Player;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 02 - Gorev 1: karakter hissiyatini ayarlamak icin olculu test odasi.
    ///
    /// Bu sahne oyuna GIRMEZ. Sadece "bu zipla iyi hissettiriyor mu" ve
    /// "kac birim atlayabiliyorum" sorularini cevaplamak icin.
    ///
    /// Menu: Tools > 2D Platformer > Test Odasi Olustur
    /// </summary>
    public static class TestRoomBuilder
    {
        private const string ScenePath = "Assets/Scenes/TestOdasi.unity";

        // Zemin ust yuzeyi her zaman y = 0
        private const float GroundTop = 0f;
        private const float GroundThickness = 1f;

        private static int groundLayer;
        private static int playerLayer;

        [MenuItem("Tools/2D Platformer/Test Odasi Olustur", false, 2)]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            bool ok = EditorUtility.DisplayDialog(
                "Test Odasi Olustur",
                "Olculu bir test odasi olusturulup Assets/Scenes/TestOdasi.unity " +
                "olarak kaydedilecek.\n\n" +
                "Bu sahne oyuna girmez - sadece karakter ayari icin.\n\n" +
                "Devam edilsin mi?",
                "Olustur", "Vazgec");

            if (!ok) return;

            groundLayer = LayerMask.NameToLayer("Ground");
            playerLayer = LayerMask.NameToLayer("Player");

            if (groundLayer < 0 || playerLayer < 0)
            {
                EditorUtility.DisplayDialog("Layer eksik",
                    "'Ground' ve 'Player' layer'lari bulunamadi.\n\n" +
                    "Once Tools > 2D Platformer > Sadece Proje Ayarlarini Uygula calistir.",
                    "Tamam");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("TestOdasi");
            Transform t = root.transform;

            BuildRunwaySection(t);
            BuildStepSection(t);
            BuildGapSection(t);
            BuildCeilingSection(t);
            BuildCorridorSection(t);

            GameObject player = CreatePlayer(new Vector2(-26f, 1.5f));
            CreateCamera(player.transform);
            CreateBackdrop();

            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log(
                "TEST ODASI HAZIR — Play'e bas ve soldan saga ilerle.\n" +
                "-----------------------------------------------------\n" +
                "OLCUM PISTI (x=-30..0)  30 birim tamamen temiz duz zemin\n" +
                "   Maks. mesafe icin burada kos ve zipla - engel yok,\n" +
                "   kot farki olusmaz, olcum 'duz zipla' olarak sayilir\n\n" +
                "BASAMAKLAR (x=6..20)   1.0 / 2.0 / 3.0 / 4.0 birim\n" +
                "   En yuksek cikabildigin basamak = maks. zipla yuksekligin\n\n" +
                "BOSLUKLAR (x=24..70)   2.0 / 3.0 / 4.0 / 5.0 / 6.0 birim\n" +
                "   Kosarak gecebildigin en genis bosluk = maks. zipla mesafen\n" +
                "   Dash'li ve dash'siz AYRI AYRI dene, ikisini de not al\n\n" +
                "TAVAN (x=74)           3.0 birim yukseklikte - kafa vurma testi\n" +
                "DAR KORIDOR (x=82)     1.5 birim genislik - sikisma testi\n" +
                "-----------------------------------------------------\n" +
                "Olculeri docs/AYARLAR.md icindeki tabloya yaz.");
        }

        // ---------------------------------------------------------------
        // Bolumler
        // ---------------------------------------------------------------

        /// <summary>
        /// Uzun, tamamen temiz duz zemin. Maks. zipla mesafesini olcmek icin.
        ///
        /// Neden ayri bir bolum: basamaklarin oldugu zeminde tam mesafeli bir zipla
        /// mutlaka bir basamaga denk geliyor, kot farki olusuyor ve olcum "duz zipla"
        /// sayilmiyor. Burada 30 birim boyunca hicbir engel yok.
        /// </summary>
        private static void BuildRunwaySection(Transform parent)
        {
            var section = new GameObject("0_OlcumPisti");
            section.transform.SetParent(parent);

            CreateGround("Zemin_OlcumPisti", -30f, 30f, section.transform);

            // Her 5 birimde bir referans isareti - gozle de takip edebilesin
            for (int i = 0; i <= 5; i++)
            {
                float x = -28f + i * 5f;
                var mark = new GameObject($"Isaret_{i * 5}birim");
                mark.transform.SetParent(section.transform);
                mark.transform.position = new Vector3(x, GroundTop, 0f);
                AddLabel(mark.transform, $"{i * 5}", 0.8f);
            }

            var info = new GameObject("_PistBilgi");
            info.transform.SetParent(section.transform);
            info.transform.position = new Vector3(-20f, GroundTop + 4f, 0f);
            info.AddComponent<TestRoomLabel>().text =
                "OLCUM PISTI — burada kos ve zipla (Space basili)";
        }

        /// <summary>Giderek yukselen basamaklar: maks. zipla yuksekligini olcer.</summary>
        private static void BuildStepSection(Transform parent)
        {
            var section = new GameObject("1_Basamaklar");
            section.transform.SetParent(parent);

            // Taban zemin
            CreateGround("Zemin_Baslangic", -4f, 22f, section.transform);

            // Basamaklar: yukseklik, x konumu
            float[] heights = { 1.0f, 2.0f, 3.0f, 4.0f };
            float x = 6f;

            foreach (float h in heights)
            {
                var step = CreateBlock(
                    $"Basamak_{h:0.0}birim",
                    new Vector2(x, GroundTop + h * 0.5f),
                    new Vector2(2.5f, h),
                    section.transform);

                // Yukseklige gore renk: alcak yesil -> yuksek kirmizi
                float k = (h - 1f) / 3f;
                SetColor(step, Color.Lerp(
                    new Color(0.45f, 0.78f, 0.50f),
                    new Color(0.85f, 0.38f, 0.35f), k));

                AddLabel(step.transform, $"{h:0.0}", h * 0.5f + 0.6f);
                x += 4f;
            }
        }

        /// <summary>Giderek genisleyen bosluklar: maks. zipla mesafesini olcer.</summary>
        private static void BuildGapSection(Transform parent)
        {
            var section = new GameObject("2_Bosluklar");
            section.transform.SetParent(parent);

            float[] gaps = { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f };

            // Ilk platform kosu mesafesi icin uzun
            float x = 24f;
            const float platformWidth = 5f;

            CreateGround("Platform_Kosu", x, platformWidth + 3f, section.transform);
            x += platformWidth + 3f;

            foreach (float gap in gaps)
            {
                // Boslugu isaretle
                var marker = new GameObject($"Bosluk_{gap:0.0}birim");
                marker.transform.SetParent(section.transform);
                marker.transform.position = new Vector3(x + gap * 0.5f, GroundTop + 0.1f, 0f);
                AddLabel(marker.transform, $"{gap:0.0}", 1.2f);

                x += gap;

                var platform = CreateGround($"Platform_{gap:0.0}sonrasi", x, platformWidth, section.transform);

                // Genislige gore renk
                float k = (gap - 2f) / 4f;
                SetColor(platform, Color.Lerp(
                    new Color(0.45f, 0.78f, 0.50f),
                    new Color(0.85f, 0.38f, 0.35f), k));

                x += platformWidth;
            }
        }

        /// <summary>Alcak tavan: zipla sirasinda kafa vurma davranisini test eder.</summary>
        private static void BuildCeilingSection(Transform parent)
        {
            var section = new GameObject("3_Tavan");
            section.transform.SetParent(parent);

            CreateGround("Zemin_Tavan", 72f, 10f, section.transform);

            var ceiling = CreateBlock("Tavan_3birim",
                new Vector2(77f, GroundTop + 3f + 0.5f),
                new Vector2(8f, 1f),
                section.transform);

            SetColor(ceiling, new Color(0.55f, 0.58f, 0.68f));
            AddLabel(ceiling.transform, "tavan 3.0", 1.2f);
        }

        /// <summary>Dar dikey koridor: sikisma ve duvar surtunmesi testi.</summary>
        private static void BuildCorridorSection(Transform parent)
        {
            var section = new GameObject("4_DarKoridor");
            section.transform.SetParent(parent);

            CreateGround("Zemin_Koridor", 84f, 10f, section.transform);

            // 1.5 birim aralikli iki duvar
            var left = CreateBlock("Duvar_Sol",
                new Vector2(86f, GroundTop + 3f), new Vector2(1f, 6f), section.transform);
            var right = CreateBlock("Duvar_Sag",
                new Vector2(87.5f, GroundTop + 3f), new Vector2(1f, 6f), section.transform);

            SetColor(left, new Color(0.55f, 0.58f, 0.68f));
            SetColor(right, new Color(0.55f, 0.58f, 0.68f));
            AddLabel(left.transform, "koridor 1.5", 3.6f);
        }

        // ---------------------------------------------------------------
        // Yardimcilar
        // ---------------------------------------------------------------

        /// <summary>Ust yuzeyi y=0 olan zemin parcasi. x = sol kenar.</summary>
        private static GameObject CreateGround(string name, float xLeft, float width, Transform parent)
        {
            return CreateBlock(name,
                new Vector2(xLeft + width * 0.5f, GroundTop - GroundThickness * 0.5f),
                new Vector2(width, GroundThickness),
                parent);
        }

        private static GameObject CreateBlock(string name, Vector2 center, Vector2 size, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            go.transform.SetParent(parent);
            go.layer = groundLayer;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("ground");
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = size;
            renderer.sortingOrder = 0;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = size;

            return go;
        }

        private static void SetColor(GameObject go, Color color)
        {
            var r = go.GetComponent<SpriteRenderer>();
            if (r != null) r.color = color;
        }

        /// <summary>Scene view'da okunabilir olcu etiketi.</summary>
        private static void AddLabel(Transform parent, string text, float heightOffset)
        {
            var label = new GameObject($"[{text}]");
            label.transform.SetParent(parent);
            label.transform.localPosition = new Vector3(0f, heightOffset, 0f);
            label.AddComponent<TestRoomLabel>().text = text;
        }

        private static GameObject CreatePlayer(Vector2 position)
        {
            var go = new GameObject("Player");
            go.tag = "Player";
            go.layer = playerLayer;
            go.transform.position = position;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("player");
            renderer.sortingOrder = 10;

            var body = go.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = 0f;

            var capsule = go.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.78f, 0.96f);
            capsule.direction = CapsuleDirection2D.Vertical;

            var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(
                "Assets/Art/NoFriction.physicsMaterial2D");
            if (material != null) capsule.sharedMaterial = material;

            var controller = go.AddComponent<PlayerController2D>();
            var so = new SerializedObject(controller);
            so.FindProperty("groundLayers").intValue = 1 << groundLayer;
            so.ApplyModifiedProperties();

            // Olcum yardimcisi - Epic 02 Gorev 4
            go.AddComponent<JumpMeasure>();

            // Karsilastirma araci - Epic 02 Gorev 2.
            // Sadece Play modunda gecici ayar uygular, sahneyi DEGISTIRMEZ.
            // Istege bagli; kullanmak istemezsen bileseni kapatabilirsin.
            go.AddComponent<FeelTuner>();

            // Bosluga dusunce basa don - Play'i yeniden baslatmaya gerek kalmasin
            go.AddComponent<TestRoomRespawn>();

            return go;
        }

        private static void CreateCamera(Transform target)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";

            var cam = go.AddComponent<UnityEngine.Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8f;          // test odasinda biraz genis gorus
            cam.backgroundColor = SpriteFactory.Sky;
            cam.clearFlags = CameraClearFlags.SolidColor;
            go.transform.position = new Vector3(0f, 3f, -10f);

            go.AddComponent<AudioListener>();

            var follow = go.AddComponent<CameraFollow>();
            var so = new SerializedObject(follow);
            so.FindProperty("target").objectReferenceValue = target;
            // Alt siniri cok asagi aliyoruz: bosluga dusunce kamera takip edebilsin.
            // Test odasinin icerigi sadece ~7 birim yuksek; kamera ise 16 birim
            // goruyor. Sinirlari icerige gore daraltirsak kamera dikeyde hic
            // hareket edemez (kod ortalayip sabitler).
            so.FindProperty("minBounds").vector2Value = new Vector2(-34f, -26f);
            so.FindProperty("maxBounds").vector2Value = new Vector2(100f, 20f);
            so.ApplyModifiedProperties();
        }

        private static void CreateBackdrop()
        {
            var go = new GameObject("Arkaplan");
            go.transform.position = new Vector3(45f, 2f, 0f);
            go.transform.localScale = new Vector3(140f, 30f, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("square");
            renderer.color = SpriteFactory.HillFar;
            renderer.sortingOrder = -100;
        }
    }
}
