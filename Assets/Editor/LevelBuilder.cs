using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Player;
using Platformer.CameraRig;
using Platformer.UI;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Tek tikla oynanabilir bir ornek bolum kurar: proje ayarlari, grafikler,
    /// karakter, dusmanlar, platformlar, UI ve kamera.
    /// Menu: Tools > 2D Platformer
    ///
    /// Bu script sadece bir baslangic noktasi uretir. Sahne olustuktan sonra
    /// her seyi Unity arayuzunden elle duzenleyebilirsin.
    /// </summary>
    public static class LevelBuilder
    {
        private const string ScenePath = "Assets/Scenes/Level01.unity";
        private const string MaterialPath = "Assets/Art/NoFriction.physicsMaterial2D";

        internal const int SortBackground = -100;
        internal const int SortPlatform = 0;
        internal const int SortItem = 5;
        internal const int SortEnemy = 8;
        internal const int SortPlayer = 10;

        internal static int groundLayer;
        internal static int playerLayer;

        // ===============================================================
        // Menu girisleri
        // ===============================================================

        [MenuItem("Tools/2D Platformer/Ornek Bolumu Olustur", false, 1)]
        public static void BuildEverything()
        {
            if (!EditorGuards.RequireEditMode("Ornek Bolumu Olustur",
                "Yeni sahne olusturmak Play modunda mumkun degil.")) return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            bool proceed = EditorUtility.DisplayDialog(
                "Ornek Bolum Olustur",
                "Yeni bir sahne olusturulacak ve Assets/Scenes/Level01.unity olarak kaydedilecek.\n\n" +
                "Ayrica Assets/Art altina yer tutucu grafikler uretilecek.\n\nDevam edilsin mi?",
                "Olustur", "Vazgec");

            if (!proceed) return;

            ConfigureProject();
            SpriteFactory.GenerateAll();
            BuildScene();
        }

        /// <summary>
        /// BuildEverything'in dialogsuz surumu - komut satirindan calistirmak icin.
        /// Batch modda EditorUtility.DisplayDialog gosterilemez, o yuzden onay
        /// adimlari atlanir ve dogrudan kurulum yapilir.
        ///
        /// Unity.exe -batchmode -quit -projectPath . \
        ///   -executeMethod Platformer.EditorTools.LevelBuilder.BuildEverythingBatch
        /// </summary>
        public static void BuildEverythingBatch()
        {
            Debug.Log("2D Platformer: batch kurulum basliyor...");

            ConfigureProject();
            SpriteFactory.GenerateAll();
            BuildScene();

            Debug.Log("2D Platformer: batch kurulum tamamlandi.");
        }

        [MenuItem("Tools/2D Platformer/Sadece Grafikleri Uret", false, 20)]
        public static void GenerateArtOnly()
        {
            if (!EditorGuards.RequireEditMode("Sadece Grafikleri Uret",
                "Asset yeniden import etmek Play modunda sorun cikarir.")) return;

            // Bu menu bilincli bir istek: var olanlarin uzerine YAZAR.
            bool ok = EditorUtility.DisplayDialog(
                "Grafikleri Yeniden Uret",
                "Assets/Art altindaki 8 PNG YENIDEN uretilecek.\n\n" +
                "Kendi cizimlerini koyduysan UZERINE YAZILIR.\n\nDevam edilsin mi?",
                "Yeniden uret", "Vazgec");

            if (!ok) return;

            SpriteFactory.GenerateAll(force: true);
            Debug.Log("2D Platformer: grafikler Assets/Art altinda yeniden uretildi.");
        }

        [MenuItem("Tools/2D Platformer/Sadece Proje Ayarlarini Uygula", false, 21)]
        public static void ConfigureProjectOnly()
        {
            if (!EditorGuards.RequireEditMode("Sadece Proje Ayarlarini Uygula",
                "Layer ve input ayarlari Play modunda degistirilmemeli.")) return;

            ConfigureProject();
            Debug.Log("2D Platformer: proje ayarlari uygulandi.");
        }

        // ===============================================================
        // Proje ayarlari
        // ===============================================================

        internal static void ConfigureProject()
        {
            groundLayer = EnsureLayer("Ground");
            playerLayer = EnsureLayer("Player");
            EnsureTag("Player");
            EnsureOldInputSystem();
        }

        /// <summary>Layer yoksa bos bir kullanici slotuna ekler, indeksini dondurur.</summary>
        private static int EnsureLayer(string layerName)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return 0;

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null) return 0;

            // Zaten var mi?
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName) return i;
            }

            // 0-7 arasi Unity'nin yerlesik layer'lari; kullanici layer'lari 8'den baslar
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty element = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(element.stringValue))
                {
                    element.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return i;
                }
            }

            Debug.LogWarning($"LevelBuilder: bos layer slotu kalmadi, '{layerName}' eklenemedi.");
            return 0;
        }

        private static void EnsureTag(string tagName)
        {
            // Yerlesik tag'ler ('Player' dahil) bu listede zaten gorunur
            foreach (string existing in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (existing == tagName) return;
            }

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            if (tags == null) return;

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
            tagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Scriptler eski Input Manager'i kullaniyor (Input.GetAxisRaw).
        /// Proje "Input System Package (New)" moduna ayarliysa oyun hata verir,
        /// bu yuzden "Both" moduna aliyoruz.
        /// </summary>
        private static void EnsureOldInputSystem()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;

            var settings = new SerializedObject(assets[0]);
            SerializedProperty handler = settings.FindProperty("activeInputHandler");
            if (handler == null) return;

            // 0 = Old, 1 = New, 2 = Both
            if (handler.intValue == 1)
            {
                handler.intValue = 2;
                settings.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();

                // Batch modda dialog gosterilemez - log'a yaz
                if (Application.isBatchMode)
                {
                    Debug.Log("2D Platformer: Active Input Handling 'Both' yapildi. " +
                              "Etkili olmasi icin Editor'u yeniden baslat.");
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Input ayari degistirildi",
                        "Active Input Handling 'Both' olarak ayarlandi.\n\n" +
                        "Bu ayarin etkili olmasi icin Unity'yi yeniden baslatman gerekiyor.",
                        "Tamam");
                }
            }
        }

        // ===============================================================
        // Sahne kurulumu
        // ===============================================================

        private static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateBackground();
            GameObject player = CreatePlayer(new Vector2(1f, 1.5f));
            CreateCamera(player.transform);
            CreateLevelGeometry();
            CreateUI();
            CreateGameManager();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.SaveScene(scene, ScenePath);

            // Sahneyi Build Settings'e ekle (yoksa)
            var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!buildScenes.Exists(s => s.path == ScenePath))
            {
                buildScenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            Debug.Log("2D Platformer: bolum hazir! Play tusuna basip oynayabilirsin.\n" +
                      "Kontroller: A/D veya ok tuslari = hareket, Space = zipla, R = yeniden basla");

            EditorSceneManager.MarkSceneDirty(scene);
        }

        // --- Kamera ---------------------------------------------------

        internal static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";

            var cam = cameraObject.AddComponent<UnityEngine.Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = SpriteFactory.Sky;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 2f, -10f);

            cameraObject.AddComponent<AudioListener>();

            var follow = cameraObject.AddComponent<CameraFollow>();
            var so = new SerializedObject(follow);
            so.FindProperty("target").objectReferenceValue = target;
            so.FindProperty("minBounds").vector2Value = new Vector2(-4f, -6f);
            so.FindProperty("maxBounds").vector2Value = new Vector2(114f, 20f);
            so.ApplyModifiedProperties();
        }

        // --- Karakter -------------------------------------------------

        /// <summary>
        /// Oyuncuyu prefab'dan yerlestirir.
        ///
        /// Eskiden burada sifirdan yaratiliyordu; o zaman Inspector'dan
        /// yapilan her ayar bir sonraki kurulumda siliniyordu. Artik
        /// Assets/Prefabs/Player.prefab duzenlenebiliyor ve kaliyor.
        /// </summary>
        internal static GameObject CreatePlayer(Vector2 position)
        {
            PrefabFactory.EnsureAll();
            return PrefabFactory.Spawn(PrefabFactory.Player, position, null);
        }

        /// <summary>
        /// Surtunmesiz fizik materyali. Olmazsa karakter duvarlara yapisip
        /// havada asili kalabilir.
        /// </summary>
        private static PhysicsMaterial2D CreateFrictionlessMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(MaterialPath);
            if (existing != null) return existing;

            var material = new PhysicsMaterial2D("NoFriction")
            {
                friction = 0f,
                bounciness = 0f
            };

            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        // --- Arka plan ------------------------------------------------

        internal static void CreateBackground()
        {
            var root = new GameObject("Background");

            // Uzak tepe bandi - kamerayla neredeyse birlikte hareket eder
            GameObject far = CreateStretchedSprite("Hills_Far", new Vector2(50f, 4f),
                new Vector2(200f, 22f), SpriteFactory.HillFar, SortBackground, root.transform);
            SetPrivateFloat(far.AddComponent<ParallaxLayer>(), "parallaxFactor", 0.2f);

            // Yakin tepe bandi
            GameObject near = CreateStretchedSprite("Hills_Near", new Vector2(50f, -1f),
                new Vector2(200f, 14f), SpriteFactory.HillNear, SortBackground + 5, root.transform);
            SetPrivateFloat(near.AddComponent<ParallaxLayer>(), "parallaxFactor", 0.5f);
        }

        // --- Bolum geometrisi ----------------------------------------

        private static void CreateLevelGeometry()
        {
            var root = new GameObject("Level");
            Transform parent = root.transform;

            // ---- Bolum 1: baslangic, kolay zemin ----
            CreateGround("Ground_Start", new Vector2(6f, -0.5f), new Vector2(16f, 1f), parent);
            CreateCoinRow(new Vector2(4f, 1.6f), 4, 1.2f, parent);

            // Kucuk basamak - ilk zipla
            CreateGround("Step_1", new Vector2(11f, 1f), new Vector2(3f, 0.7f), parent);
            CreateCoin(new Vector2(11f, 2.4f), parent);

            // ---- Bolum 2: bosluk + yardimci platform ----
            CreateGround("Platform_Float_1", new Vector2(16f, 2.2f), new Vector2(3f, 0.6f), parent);
            CreateCoin(new Vector2(16f, 3.6f), parent);

            CreateGround("Ground_B", new Vector2(24f, -0.5f), new Vector2(12f, 1f), parent);
            CreateEnemy(new Vector2(24f, 0.6f), true, parent);
            CreateCoinRow(new Vector2(21f, 1.6f), 3, 1.2f, parent);

            // Dikenler - ustune basma!
            CreateSpikes("Spikes_1", new Vector2(28f, 0.5f), 2, parent);

            // ---- Bolum 3: hareketli platform ucurumu ----
            CreateMovingPlatform("MovingPlatform_H", new Vector2(31.5f, 1.2f), new Vector2(3f, 0.6f),
                new Vector2(6.5f, 0f), 2.2f, parent);
            CreateCoin(new Vector2(35f, 2.6f), parent);

            CreateGround("Ground_C", new Vector2(45f, -0.5f), new Vector2(14f, 1f), parent);

            // Checkpoint - buraya kadar geldiysen artik burada dogarsin
            CreateCheckpoint(new Vector2(40f, 0f), parent);

            CreateSpikes("Spikes_2", new Vector2(46f, 0.5f), 3, parent);
            CreateEnemy(new Vector2(50f, 0.6f), false, parent);
            CreateCoinRow(new Vector2(42f, 1.6f), 3, 1.2f, parent);

            // ---- Bolum 4: yukari tirmanis ----
            CreateGround("Climb_1", new Vector2(55f, 1.5f), new Vector2(3f, 0.6f), parent);
            CreateMovingPlatform("MovingPlatform_V", new Vector2(59f, 2.5f), new Vector2(3f, 0.6f),
                new Vector2(0f, 3.5f), 1.8f, parent);
            CreateGround("Climb_3", new Vector2(63f, 6f), new Vector2(3f, 0.6f), parent);

            CreateCoin(new Vector2(55f, 2.9f), parent);
            CreateCoin(new Vector2(63f, 7.4f), parent);

            // ---- Bolum 5: yuksek kat ----
            CreateGround("Ground_High", new Vector2(74f, 7f), new Vector2(16f, 1f), parent);
            CreateCheckpoint(new Vector2(68f, 7.5f), parent);
            CreateEnemy(new Vector2(76f, 8.1f), true, parent);
            CreateCoinRow(new Vector2(71f, 9.1f), 5, 1.2f, parent);
            CreateSpikes("Spikes_3", new Vector2(80f, 8f), 2, parent);

            // ---- Bolum 6: inis ve bitis ----
            CreateGround("Descend_1", new Vector2(86f, 5f), new Vector2(3f, 0.6f), parent);
            CreateGround("Descend_2", new Vector2(90f, 3f), new Vector2(3f, 0.6f), parent);
            CreateCoin(new Vector2(86f, 6.4f), parent);
            CreateCoin(new Vector2(90f, 4.4f), parent);

            CreateGround("Ground_Finish", new Vector2(100f, -0.5f), new Vector2(16f, 1f), parent);
            CreateCoinRow(new Vector2(96f, 1.6f), 3, 1.2f, parent);
            CreateGoal(new Vector2(105f, 0f), parent);
        }

        // ===============================================================
        // Nesne fabrikalari
        // ===============================================================

        private static GameObject CreateGround(string objectName, Vector2 center, Vector2 size, Transform parent)
        {
            GameObject platform = CreateTiledSprite(objectName, "ground", center, size, Color.white, SortPlatform, parent);
            platform.layer = groundLayer;

            var collider = platform.AddComponent<BoxCollider2D>();
            collider.size = size;

            return platform;
        }

        private static void CreateMovingPlatform(string objectName, Vector2 start, Vector2 size,
            Vector2 travel, float speed, Transform parent)
        {
            GameObject platform = CreateTiledSprite(objectName, "ground", start, size, new Color(0.75f, 0.9f, 1f), SortPlatform, parent);
            platform.layer = groundLayer;

            var collider = platform.AddComponent<BoxCollider2D>();
            collider.size = size;

            // Kinematik: fizik onu itemez ama karakter ustunde durabilir
            var body = platform.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;

            var mover = platform.AddComponent<MovingPlatform>();
            var so = new SerializedObject(mover);

            SerializedProperty waypoints = so.FindProperty("waypoints");
            waypoints.arraySize = 2;
            waypoints.GetArrayElementAtIndex(0).vector2Value = Vector2.zero;
            waypoints.GetArrayElementAtIndex(1).vector2Value = travel;

            so.FindProperty("speed").floatValue = speed;
            so.FindProperty("passengerLayers").intValue = 1 << playerLayer;
            so.ApplyModifiedProperties();
        }

        private static void CreateCoin(Vector2 position, Transform parent)
        {
            var coin = new GameObject("Coin");
            coin.transform.position = position;
            coin.transform.SetParent(parent);

            // Gorsel ayri bir cocuk nesne: donerken collider donmesin
            var visual = new GameObject("Visual");
            visual.transform.SetParent(coin.transform);
            visual.transform.localPosition = Vector3.zero;

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("coin");
            renderer.sortingOrder = SortItem;

            var trigger = coin.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.45f;

            coin.AddComponent<Coin>();
        }

        private static void CreateCoinRow(Vector2 start, int count, float spacing, Transform parent)
        {
            for (int i = 0; i < count; i++)
            {
                CreateCoin(new Vector2(start.x + i * spacing, start.y), parent);
            }
        }

        private static void CreateSpikes(string objectName, Vector2 center, int count, Transform parent)
        {
            GameObject spikes = CreateTiledSprite(objectName, "spike", center,
                new Vector2(count, 1f), Color.white, SortPlatform + 1, parent);

            // Collider sprite'tan kucuk: sadece sivri uclara degince olsun
            var trigger = spikes.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(count - 0.2f, 0.55f);
            trigger.offset = new Vector2(0f, -0.15f);

            spikes.AddComponent<Hazard>();
        }

        private static void CreateEnemy(Vector2 position, bool facingRight, Transform parent)
        {
            var enemy = new GameObject("Enemy");
            enemy.transform.position = position;
            enemy.transform.SetParent(parent);

            var renderer = enemy.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("enemy");
            renderer.sortingOrder = SortEnemy;

            var body = enemy.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = 3f;   // dusman icin normal fizik yeterli

            var collider = enemy.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.9f, 0.72f);
            collider.sharedMaterial = CreateFrictionlessMaterial();

            var patroller = enemy.AddComponent<Patroller>();
            var so = new SerializedObject(patroller);
            so.FindProperty("groundLayers").intValue = 1 << groundLayer;
            so.FindProperty("startFacingRight").boolValue = facingRight;
            so.ApplyModifiedProperties();
        }

        private static void CreateCheckpoint(Vector2 position, Transform parent)
        {
            var checkpoint = new GameObject("Checkpoint");
            // Sprite'in pivotu ortasinda; tabani zemine otursun diye yariyuksekligi kadar kaldir
            checkpoint.transform.position = position + new Vector2(0f, 0.75f);
            checkpoint.transform.SetParent(parent);

            var renderer = checkpoint.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("checkpoint");
            renderer.sortingOrder = SortItem;

            var trigger = checkpoint.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.2f, 1.5f);

            checkpoint.AddComponent<Checkpoint>();
        }

        private static void CreateGoal(Vector2 position, Transform parent)
        {
            var goal = new GameObject("LevelGoal");
            goal.transform.position = position + new Vector2(0f, 0.875f);
            goal.transform.SetParent(parent);

            var renderer = goal.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("goal");
            renderer.sortingOrder = SortItem;

            var trigger = goal.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.3f, 1.75f);

            goal.AddComponent<LevelGoal>();
        }

        /// <summary>
        /// Tek parca halinde esnetilmis sprite. Duz renk arka plan bantlari icin
        /// kullanilir: Tiled mod burada binlerce gereksiz karo uretirdi.
        /// </summary>
        private static GameObject CreateStretchedSprite(string objectName, Vector2 center,
            Vector2 size, Color color, int sortingOrder, Transform parent)
        {
            var go = new GameObject(objectName);
            go.transform.position = center;
            if (parent != null) go.transform.SetParent(parent);

            // 'square' sprite'i tam 1x1 birim oldugu icin olcek dogrudan boyut demek
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("square");
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            return go;
        }

        /// <summary>Tiled cizim modunda sprite olusturur - esnemek yerine tekrarlanir.</summary>
        private static GameObject CreateTiledSprite(string objectName, string spriteName, Vector2 center,
            Vector2 size, Color color, int sortingOrder, Transform parent)
        {
            var go = new GameObject(objectName);
            go.transform.position = center;
            if (parent != null) go.transform.SetParent(parent);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load(spriteName);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            return go;
        }

        // --- UI -------------------------------------------------------

        internal static void CreateUI()
        {
            var canvasObject = new GameObject("HUD Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            // EventSystem olmadan UI uyari verir
#if UNITY_2023_1_OR_NEWER
            bool hasEventSystem = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null;
#else
            bool hasEventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null;
#endif
            if (!hasEventSystem)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Text scoreText = CreateText(canvasObject.transform, "ScoreText", "Para: 0 / 0",
                new Vector2(0f, 1f), new Vector2(40f, -40f), TextAnchor.UpperLeft, 40);

            // Can degil olum SAYACI - sinirsiz deneme var (Epic 09)
            Text deathText = CreateText(canvasObject.transform, "DeathText", "Olum: 0",
                new Vector2(0f, 1f), new Vector2(40f, -100f), TextAnchor.UpperLeft, 40);

            Text timeText = CreateText(canvasObject.transform, "TimeText", "0:00",
                new Vector2(1f, 1f), new Vector2(-40f, -40f), TextAnchor.UpperRight, 40);

            Text messageText = CreateText(canvasObject.transform, "MessageText", string.Empty,
                new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter, 64);

            var hud = canvasObject.AddComponent<HudController>();
            var so = new SerializedObject(hud);
            so.FindProperty("scoreText").objectReferenceValue = scoreText;
            so.FindProperty("deathText").objectReferenceValue = deathText;
            so.FindProperty("timeText").objectReferenceValue = timeText;
            so.FindProperty("messageText").objectReferenceValue = messageText;
            so.ApplyModifiedProperties();
        }

        private static Text CreateText(Transform parent, string objectName, string content,
            Vector2 anchor, Vector2 offset, TextAnchor alignment, int fontSize)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(anchor.x, anchor.y);
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(900f, 220f);

            var text = textObject.AddComponent<Text>();
            text.text = content;
            text.font = GetBuiltinFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // Koyu golge - arka plan ne olursa olsun okunakli kalsin
            var shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
            shadow.effectDistance = new Vector2(2f, -2f);

            return text;
        }

        /// <summary>Unity surumune gore yerlesik fontu bulur.</summary>
        private static Font GetBuiltinFont()
        {
            // Unity 2022+ 'LegacyRuntime.ttf', daha eskiler 'Arial.ttf' kullanir
            Font font = null;

            try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { /* bu surumde yok, asagida denenecek */ }

            if (font == null)
            {
                try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
                catch { /* ikisi de yoksa null kalir, Unity varsayilani kullanir */ }
            }

            return font;
        }

        // --- GameManager ----------------------------------------------

        internal static void CreateGameManager(int levelIndex = 0, int designVersion = 1)
        {
            var managerObject = new GameObject("GameManager");
            var manager = managerObject.AddComponent<GameManager>();

            var so = new SerializedObject(manager);
            so.FindProperty("killPlaneY").floatValue = -12f;
            so.FindProperty("levelIndex").intValue = levelIndex;
            so.FindProperty("levelDesignVersion").intValue = designVersion;
            so.ApplyModifiedProperties();

            // Kalici yoneticiler AYRI nesnelerde: DontDestroyOnLoad ile sahneler
            // arasi yasiyorlar. GameManager ise bolume ozel (skor, checkpoint
            // her bolumde sifirlanmali), o yuzden ayni nesnede olamazlar.
            var timeObject = new GameObject("TimeController");
            timeObject.AddComponent<TimeController>();

            var saveObject = new GameObject("SaveManager");
            saveObject.AddComponent<SaveManager>();
        }

        // --- Kucuk yardimci -------------------------------------------

        private static void SetPrivateFloat(Object target, string propertyName, float value)
        {
            var so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(propertyName);
            if (property == null) return;

            property.floatValue = value;
            so.ApplyModifiedProperties();
        }
    }
}
