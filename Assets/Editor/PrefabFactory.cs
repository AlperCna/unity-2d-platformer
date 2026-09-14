using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Epic 05 gorev 7 — tekrar eden nesneleri prefab yapar.
    ///
    /// NEDEN SANA LAZIM: bolumler koddan uretiliyor, yani her kurulumda
    /// nesneler SIFIRDAN yaratiliyor. Paranin boyutunu Inspector'dan
    /// buyutursen bir sonraki kurulumda kaybolur.
    ///
    /// Prefab bunu cozer: kurulum artik nesne YARATMIYOR, prefab
    /// KOPYALIYOR. Prefab'i duzenlersin, her bolumde oyle gelir. Kod
    /// degistirmeden ayar yapabilirsin.
    ///
    /// KURAL: var olan prefab'in USTUNE YAZMAZ.
    /// Yoksa yaptigin her ayar bir sonraki kurulumda silinirdi - yani
    /// prefab olmasinin tek sebebi ortadan kalkardi. Sifirlamak icin
    /// prefab'i elle sil, yeniden uretilir.
    ///
    /// Menu: Tools > 2D Platformer > Prefablari Uret
    /// </summary>
    public static class PrefabFactory
    {
        public const string Folder = "Assets/Prefabs";

        public const string Coin = "Coin";
        public const string Spikes = "Spikes";
        public const string Checkpoint = "Checkpoint";
        public const string LevelGoal = "LevelGoal";
        public const string Player = "Player";
        public const string Projectile = "Projectile";
        public const string EnemyPatroller = "Enemy_Patroller";
        public const string EnemyShooter = "Enemy_Shooter";
        public const string EnemyFlyer = "Enemy_Flyer";

        // Epic 07
        public const string RetractingSpikes = "Hazard_RetractingSpikes";
        public const string FireJet = "Hazard_FireJet";
        public const string FallingPlatform = "Platform_Falling";
        public const string OneWayPlatform = "Platform_OneWay";
        public const string JumpPad = "JumpPad";

        [MenuItem("Tools/2D Platformer/Prefablari Uret", false, 23)]
        public static void GenerateMenu()
        {
            if (!EditorGuards.RequireEditMode("Prefablari Uret",
                "Prefab uretimi Play modunda yapilamaz.")) return;

            EnsureAll(verbose: true);
        }

        /// <summary>Eksik prefablari uretir. Var olanlara DOKUNMAZ.</summary>
        internal static void EnsureAll(bool verbose = false)
        {
            EnsureFolder();

            int created = 0;
            created += Ensure(Coin, BuildCoin) ? 1 : 0;
            created += Ensure(Spikes, BuildSpikes) ? 1 : 0;
            created += Ensure(Checkpoint, BuildCheckpoint) ? 1 : 0;
            created += Ensure(LevelGoal, BuildGoal) ? 1 : 0;
            created += Ensure(Player, BuildPlayer) ? 1 : 0;

            // SIRA ONEMLI: Enemy_Shooter, Projectile prefab'ine referans
            // tutuyor. Mermi once var olmali.
            created += Ensure(Projectile, BuildProjectile) ? 1 : 0;
            created += Ensure(EnemyPatroller, BuildPatroller) ? 1 : 0;
            created += Ensure(EnemyShooter, BuildShooter) ? 1 : 0;
            created += Ensure(EnemyFlyer, BuildFlyer) ? 1 : 0;

            created += Ensure(RetractingSpikes, BuildRetractingSpikes) ? 1 : 0;
            created += Ensure(FireJet, BuildFireJet) ? 1 : 0;
            created += Ensure(FallingPlatform, BuildFallingPlatform) ? 1 : 0;
            created += Ensure(OneWayPlatform, BuildOneWayPlatform) ? 1 : 0;
            created += Ensure(JumpPad, BuildJumpPad) ? 1 : 0;

            RepairWiring();

            if (verbose || created > 0)
            {
                Debug.Log($"Prefablar: {created} yeni uretildi, digerlerine " +
                          $"dokunulmadi ({Folder})\n" +
                          $"  Ayar degistirmek icin prefab'i ac; kurulumlar arasinda kalir.\n" +
                          $"  Sifirlamak icin prefab'i sil, yeniden uretilir.");
            }
        }

        // ---------------------------------------------------------------
        // Layer cozumleme
        //
        // NEDEN STATIC KULLANMIYORUZ:
        // LevelBuilder.groundLayer bir static ve sadece ConfigureProject()
        // icinde atraniyor. Unity script derleyince static'ler sifirlanir.
        // "Prefablari Uret" menusu ConfigureProject cagirmadigi icin, o
        // menuden uretilen Player prefab'ine 1 << 0 (Default) gomuldu ve
        // orada KALDI - EnsureAll var olan prefab'in ustune yazmiyor.
        //
        // Sonuc: karakterin zemin algisi yanlis layer'a bakiyordu. Bolum 1'de
        // farkedilmedi (Default'ta altinda bir sey yok), ama dusman test
        // odasinda dusmanlar Default'ta oldugu icin havada IsGrounded true
        // oluyor ve karakter suzuluyordu.
        //
        // Isimden cozmek her zaman dogru: layer'lar proje ayarinda duruyor,
        // derlemeden etkilenmiyor.
        // ---------------------------------------------------------------

        // LevelBuilder artik bunlari tembel cozumluyor - tek kaynak orasi.
        private static int GroundLayer => LevelBuilder.groundLayer;
        private static int PlayerLayer => LevelBuilder.playerLayer;

        /// <summary>
        /// Var olan prefablarda MAKINE'nin sahip oldugu alanlari dogrular.
        ///
        /// EnsureAll bilincli olarak var olan prefabin ustune yazmiyor -
        /// yoksa senin yaptigin ayarlar her kurulumda silinirdi. Ama bu,
        /// yanlis uretilmis bir prefabin sonsuza kadar yanlis kalmasi
        /// demekti; nitekim oyle oldu.
        ///
        /// Ayrim su: BOYUT, RENK, HIZ senin; LAYER ve LAYER MASKESI
        /// makinenin. Ikincileri elle degistirmek icin bir sebep yok ve
        /// yanlis olduklarinda oyun sessizce bozuluyor.
        /// </summary>
        internal static void RepairWiring()
        {
            int fixedCount = 0;

            fixedCount += RepairPlayer();
            fixedCount += RepairLayerMask(EnemyPatroller, "groundLayers",
                                          1 << GroundLayer,
                                          typeof(Gameplay.Patroller));
            fixedCount += RepairLayerMask(Projectile, "blockerLayers",
                                          1 << GroundLayer,
                                          typeof(Gameplay.Projectile));
            fixedCount += RepairShooterReference();
            fixedCount += RepairChildReference(RetractingSpikes, "spikeVisual", "Visual",
                                               typeof(Gameplay.RetractingSpikes));
            fixedCount += RepairChildReference(FireJet, "flameVisual", "Flame",
                                               typeof(Gameplay.FireJet));

            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.LogWarning($"Prefablarda {fixedCount} hatali baglanti duzeltildi. " +
                                 "Ayrintilar yukarida.");
            }
        }

        private static int RepairPlayer()
        {
            GameObject prefab = Load(Player);
            if (prefab == null) return 0;

            int n = 0;

            if (prefab.layer != PlayerLayer)
            {
                Debug.LogWarning($"Player.prefab layer {prefab.layer} -> {PlayerLayer} " +
                                 "(Player) olarak duzeltildi.");
                prefab.layer = PlayerLayer;
                EditorUtility.SetDirty(prefab);
                n++;
            }

            n += RepairLayerMask(Player, "groundLayers", 1 << GroundLayer,
                                 typeof(Platformer.Player.PlayerController2D));
            return n;
        }

        private static int RepairLayerMask(string prefabName, string field,
                                           int expected, System.Type componentType)
        {
            GameObject prefab = Load(prefabName);
            if (prefab == null) return 0;

            var component = prefab.GetComponent(componentType);
            if (component == null) return 0;

            var so = new SerializedObject(component);
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null || prop.intValue == expected) return 0;

            Debug.LogWarning(
                $"{prefabName}.prefab '{field}' = {prop.intValue} " +
                $"(layer {LayerIndexOf(prop.intValue)}) YANLIS -> {expected} " +
                $"(layer {LayerIndexOf(expected)}) olarak duzeltildi.");

            prop.intValue = expected;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(prefab);
            return 1;
        }

        private static string LayerIndexOf(int mask)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) != 0) return $"{i} '{LayerMask.LayerToName(i)}'";
            }
            return "yok";
        }

        /// <summary>Atici, mermi prefabini kaybetmis olabilir.</summary>
        private static int RepairShooterReference()
        {
            GameObject shooter = Load(EnemyShooter);
            GameObject bullet = Load(Projectile);
            if (shooter == null || bullet == null) return 0;

            var component = shooter.GetComponent<Gameplay.ShooterEnemy>();
            if (component == null) return 0;

            var so = new SerializedObject(component);
            SerializedProperty prop = so.FindProperty("projectilePrefab");
            if (prop == null || prop.objectReferenceValue != null) return 0;

            prop.objectReferenceValue = bullet.GetComponent<Gameplay.Projectile>();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(shooter);

            Debug.LogWarning("Enemy_Shooter.prefab mermi referansi bostu, baglandi.");
            return 1;
        }

        /// <summary>
        /// Bir bilesenin cocuk nesne referansi bos kalmissa baglar.
        ///
        /// Bu da "makinenin sahip oldugu" alanlardan: kullanicinin bu
        /// referansi bosaltmak icin bir sebebi yok ve bos oldugunda oyun
        /// her karede hata basiyor.
        /// </summary>
        private static int RepairChildReference(string prefabName, string field,
                                                string childName, System.Type componentType)
        {
            GameObject prefab = Load(prefabName);
            if (prefab == null) return 0;

            var component = prefab.GetComponent(componentType);
            if (component == null) return 0;

            var so = new SerializedObject(component);
            SerializedProperty prop = so.FindProperty(field);
            if (prop == null || prop.objectReferenceValue != null) return 0;

            Transform child = prefab.transform.Find(childName);
            if (child == null) return 0;

            prop.objectReferenceValue = child;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(prefab);

            Debug.LogWarning($"{prefabName}.prefab '{field}' bostu, " +
                             $"'{childName}' cocuguna baglandi.");
            return 1;
        }

        public static string PathOf(string name) => $"{Folder}/{name}.prefab";

        public static GameObject Load(string name) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(PathOf(name));

        /// <summary>
        /// Prefab'i sahneye yerlestirir. PrefabUtility ile - normal
        /// Instantiate baglantiyi koparir ve prefab'i duzenlemen ise yaramaz.
        /// </summary>
        public static GameObject Spawn(string name, Vector2 position, Transform parent)
        {
            GameObject prefab = Load(name);
            if (prefab == null)
            {
                Debug.LogError($"Prefab bulunamadi: {PathOf(name)}. " +
                               "Tools > 2D Platformer > Prefablari Uret calistir.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            return instance;
        }

        // ---------------------------------------------------------------

        private static bool Ensure(string name, System.Func<GameObject> build)
        {
            if (Load(name) != null) return false;      // ELLE YAPILAN AYARLAR KORUNUR

            GameObject temp = build();
            PrefabUtility.SaveAsPrefabAsset(temp, PathOf(name));
            Object.DestroyImmediate(temp);
            return true;
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Folder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
        }

        // ---------------------------------------------------------------
        // Insa tarifleri — her biri eskiden LevelCursor/LevelBuilder icindeydi
        // ---------------------------------------------------------------

        private static GameObject BuildCoin()
        {
            var coin = new GameObject(Coin);

            // Gorsel ayri cocuk nesnede: Epic 14'te para donme/yukselme
            // animasyonu gorseli oynatacak, trigger yerinde kalacak.
            var visual = new GameObject("Visual");
            visual.transform.SetParent(coin.transform);
            visual.transform.localPosition = Vector3.zero;

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("coin");
            renderer.sortingOrder = LevelBuilder.SortItem;

            var trigger = coin.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.45f;

            coin.AddComponent<Gameplay.Coin>();
            return coin;
        }

        /// <summary>
        /// Tek birimlik diken. Kac birim olacagi yerlestirilirken ayarlanir
        /// (renderer.size + collider.size).
        /// </summary>
        private static GameObject BuildSpikes()
        {
            var go = new GameObject(Spikes);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("spike");
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(1f, 1f);
            renderer.sortingOrder = LevelBuilder.SortPlatform + 1;

            // Collider GORSELDEN KUCUK - bilincli bir oyun hissi karari.
            // Oyuncu dikenin ucunu siyirip kurtulabilmeli, "degmedim ki"
            // dememeli. Tilemap'e tasinmamasinin sebebi de bu.
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.75f, 0.5f);
            trigger.offset = new Vector2(0f, -0.18f);

            go.AddComponent<Gameplay.Hazard>();
            return go;
        }

        private static GameObject BuildCheckpoint()
        {
            var go = new GameObject(Checkpoint);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("checkpoint");
            renderer.sortingOrder = LevelBuilder.SortItem;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.2f, 1.5f);

            go.AddComponent<Gameplay.Checkpoint>();
            return go;
        }

        private static GameObject BuildGoal()
        {
            var go = new GameObject(LevelGoal);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("goal");
            renderer.sortingOrder = LevelBuilder.SortItem;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1.3f, 1.75f);

            go.AddComponent<Gameplay.LevelGoal>();
            return go;
        }

        // ---------------------------------------------------------------
        // Tehlikeler ve engeller (Epic 07)
        // ---------------------------------------------------------------

        /// <summary>
        /// Yerden cikan diken. Gorsel AYRI COCUK NESNEDE cunku asagi yukari
        /// hareket eden o; collider ve script kok nesnede sabit kaliyor.
        /// </summary>
        private static GameObject BuildRetractingSpikes()
        {
            var go = new GameObject(RetractingSpikes);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = new Vector3(0f, -0.9f, 0f);  // gizli halde

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("spike");

            // ZEMININ ARKASINDA (-1). Zemin tilemap'i 0'da; diken onun
            // onunde olsaydi gizli haldeyken topragin icinden gorunurdu -
            // ilk denemede oyle oldu.
            //
            // Arkada olmasi yukselmeyi bozmuyor: yuzeyin USTUNDE zaten karo
            // yok, gizleyecek bir sey de yok. Yani diken topraktan cikiyormus
            // gibi gorunuyor, ki dogrusu bu.
            renderer.sortingOrder = LevelBuilder.SortPlatform - 1;

            // Collider gorselden kucuk - "degmedim ki" olmasin
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.75f, 0.55f);
            trigger.offset = new Vector2(0f, 0.28f);

            var spikes = go.AddComponent<Gameplay.RetractingSpikes>();
            var spikesSo = new SerializedObject(spikes);
            spikesSo.FindProperty("spikeVisual").objectReferenceValue = visual.transform;
            spikesSo.ApplyModifiedProperties();
            return go;
        }

        private static GameObject BuildFireJet()
        {
            var go = new GameObject(FireJet);

            // Namlu: alev kapaliyken de nereden cikacagi gorunsun
            var nozzle = new GameObject("Nozzle");
            nozzle.transform.SetParent(go.transform);
            nozzle.transform.localPosition = Vector3.zero;
            var nozzleRenderer = nozzle.AddComponent<SpriteRenderer>();
            nozzleRenderer.sprite = SpriteFactory.Load("square");
            nozzleRenderer.color = SpriteFactory.Metal;
            nozzleRenderer.drawMode = SpriteDrawMode.Sliced;
            nozzleRenderer.size = new Vector2(0.8f, 0.3f);
            nozzleRenderer.sortingOrder = LevelBuilder.SortPlatform + 1;

            var visual = new GameObject("Flame");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = Vector3.zero;

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("flame");
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(0.8f, 0.01f);
            renderer.sortingOrder = LevelBuilder.SortItem;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(0.6f, 0.01f);

            // Referansi ACIKCA bagliyoruz. Otomatik aramaya birakmak, cocuk
            // sirasi degistiginde sessizce yanlis nesneyi bulmak demek.
            var jet = go.AddComponent<Gameplay.FireJet>();
            var jetSo = new SerializedObject(jet);
            jetSo.FindProperty("flameVisual").objectReferenceValue = visual.transform;
            jetSo.ApplyModifiedProperties();
            return go;
        }

        private static GameObject BuildFallingPlatform()
        {
            var go = new GameObject(FallingPlatform);
            go.layer = GroundLayer;                 // uzerine basiliyor

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = TilesetFactory.LoadTile(15);   // tek basina duran karo
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(2f, 1f);
            renderer.sortingOrder = LevelBuilder.SortPlatform;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(2f, 1f);
            box.sharedMaterial = new PhysicsMaterial2D("PlatformFrictionless")
            {
                friction = 0f,
                bounciness = 0f,
            };

            go.AddComponent<Gameplay.FallingPlatform>();
            return go;
        }

        private static GameObject BuildOneWayPlatform()
        {
            var go = new GameObject(OneWayPlatform);
            go.layer = GroundLayer;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = TilesetFactory.LoadTile(15);
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(3f, 0.5f);
            renderer.sortingOrder = LevelBuilder.SortPlatform;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(3f, 0.5f);
            box.sharedMaterial = new PhysicsMaterial2D("PlatformFrictionless")
            {
                friction = 0f,
                bounciness = 0f,
            };

            // Sira: Effector once, cunku OneWayPlatform onu [RequireComponent]
            // ile istiyor ve Awake'de ayarliyor.
            go.AddComponent<PlatformEffector2D>();
            go.AddComponent<Gameplay.OneWayPlatform>();
            return go;
        }

        private static GameObject BuildJumpPad()
        {
            var go = new GameObject(JumpPad);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform);
            visual.transform.localPosition = Vector3.zero;

            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("jumppad");
            renderer.sortingOrder = LevelBuilder.SortItem;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 0.5f);
            trigger.offset = new Vector2(0f, 0.25f);

            go.AddComponent<Gameplay.JumpPad>();
            return go;
        }

        // ---------------------------------------------------------------
        // Dusmanlar (Epic 06)
        // ---------------------------------------------------------------

        private static GameObject BuildProjectile()
        {
            var go = new GameObject(Projectile);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("bullet");
            renderer.sortingOrder = LevelBuilder.SortEnemy + 1;   // dusmanin onunde

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var trigger = go.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.22f;      // gorselden kucuk: "degmedim ki" olmasin

            var projectile = go.AddComponent<Gameplay.Projectile>();
            var so = new SerializedObject(projectile);
            so.FindProperty("blockerLayers").intValue = 1 << GroundLayer;
            so.ApplyModifiedProperties();

            return go;
        }

        private static GameObject BuildPatroller()
        {
            var go = new GameObject(EnemyPatroller);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("enemy");
            renderer.sortingOrder = LevelBuilder.SortEnemy;

            var body = go.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 0.8f);
            box.sharedMaterial = new PhysicsMaterial2D("EnemyFrictionless")
            {
                friction = 0f,
                bounciness = 0f,
            };

            var patroller = go.AddComponent<Gameplay.Patroller>();
            var so = new SerializedObject(patroller);
            so.FindProperty("groundLayers").intValue = 1 << GroundLayer;
            so.ApplyModifiedProperties();

            return go;
        }

        private static GameObject BuildShooter()
        {
            var go = new GameObject(EnemyShooter);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("shooter");
            renderer.sortingOrder = LevelBuilder.SortEnemy;

            // Kinematic: yerinden kimildamiyor ama oyuncu ustune basabilmeli,
            // o yuzden fizik govdesi lazim.
            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.85f, 0.8f);

            var shooter = go.AddComponent<Gameplay.ShooterEnemy>();
            var so = new SerializedObject(shooter);
            so.FindProperty("projectilePrefab").objectReferenceValue =
                Load(Projectile)?.GetComponent<Gameplay.Projectile>();
            so.ApplyModifiedProperties();

            return go;
        }

        private static GameObject BuildFlyer()
        {
            var go = new GameObject(EnemyFlyer);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("enemy");
            renderer.color = new Color(0.62f, 0.85f, 1f);   // devriyeden ayirt edilsin
            renderer.sortingOrder = LevelBuilder.SortEnemy;

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 0.7f);

            go.AddComponent<Gameplay.FlyerEnemy>();
            return go;
        }

        // ---------------------------------------------------------------

        private static GameObject BuildPlayer()
        {
            var go = new GameObject(Player);
            go.tag = "Player";
            go.layer = PlayerLayer;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Load("player");
            renderer.sortingOrder = LevelBuilder.SortPlayer;

            var body = go.AddComponent<Rigidbody2D>();
            body.freezeRotation = true;
            body.gravityScale = 0f;                    // yercekimini kod yonetiyor

            var capsule = go.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.78f, 0.96f);
            capsule.direction = CapsuleDirection2D.Vertical;
            capsule.sharedMaterial = new PhysicsMaterial2D("PlayerFrictionless")
            {
                friction = 0f,
                bounciness = 0f,
            };

            var controller = go.AddComponent<Player.PlayerController2D>();
            var so = new SerializedObject(controller);
            so.FindProperty("groundLayers").intValue = 1 << GroundLayer;
            so.ApplyModifiedProperties();

            go.AddComponent<Player.PlayerHealth>();
            return go;
        }
    }
}
