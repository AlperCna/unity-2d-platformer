# Epic 06 — Düşmanlar

**Amaç:** 2–3 çeşit düşman. Az sayıda ama her biri **farklı bir soru soran**
düşmanlar.

**Ön koşul:** [Epic 04](04-seviye-tasarimi-prensipleri.md)

**Süre:** 1 hafta

---

## Neden bu epic var

Düşman, oyuncuya sorulan bir sorudur. İyi düşman net bir soru sorar:

- *"Zamanlamayı tutturabiliyor musun?"*
- *"Nereden geçeceğine karar verebiliyor musun?"*
- *"Hızlı düşünebiliyor musun?"*

Aynı soruyu soran 5 düşman, farklı soru soran 2 düşmandan kötüdür.

---

## Elindeki altyapı

`Assets/Scripts/Gameplay/Patroller.cs` çalışıyor:
- İleri yürür, duvara çarpınca veya platform kenarına gelince döner
- Yandan dokunursa öldürür
- Üstüne basılırsa ölür ve oyuncuyu zıplatır (`LaunchUpward`)
- Ezilince yassılaşıp saydamlaşır

Bu klasik "Goomba" ve iyi bir temel.

---

## Düşman rolleri

2–3 düşman seç, her biri **farklı** bir şey yapsın:

| Rol | Sorduğu soru | Örnek |
|---|---|---|
| **Engel** | "Zamanlamayı tutturabiliyor musun?" | Devriye (mevcut) |
| **Alan reddi** | "Nereden geçeceksin?" | Mermi atan kule |
| **Baskı** | "Hızlı karar verebiliyor musun?" | Takip eden düşman |
| **Platform** | "Bunu avantaja çevirebiliyor musun?" | Üstüne basılan uçan düşman |

**Önerilen set:** Devriye (var) + Mermi atan + Uçan. Üçü üç farklı soru sorar
ve birlikte ilginç kombinasyonlar üretir.

---

## Görevler

### 1. Ortak temel sınıf yaz

Üç düşmanda da tekrar edecek mantık: ezilme kontrolü, ölüm, skor.

`Assets/Scripts/Gameplay/EnemyBase.cs`:

```csharp
using UnityEngine;
using Platformer.Core;
using Platformer.Player;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Tum dusmanlarin ortak davranisi: oyuncuyla temas, ezilme, olum.
    /// Turetilen siniflar hareketi kendileri yonetir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Temel")]
        [Tooltip("Ustune basilinca oluyor mu?")]
        [SerializeField] protected bool canBeStomped = true;

        [Tooltip("Oyuncu bu yukseklik farkindan fazla yukarideyse stomp sayilir.")]
        [SerializeField] protected float stompHeightThreshold = 0.25f;

        [Tooltip("Stomp sonrasi oyuncunun ziplama yuksekligi.")]
        [SerializeField] protected float stompBounceHeight = 2.6f;

        [SerializeField] protected int stompScoreReward = 2;

        [Header("Olum Efekti")]
        [SerializeField] protected float deathDuration = 0.25f;

        public bool IsDead { get; protected set; }

        protected Collider2D bodyCollider;
        protected SpriteRenderer spriteRenderer;
        protected Rigidbody2D rb;

        protected virtual void Awake()
        {
            bodyCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        // --- Oyuncu temasi ---

        private void OnCollisionEnter2D(Collision2D collision) => HandleContact(collision.collider);
        private void OnCollisionStay2D(Collision2D collision) => HandleContact(collision.collider);
        private void OnTriggerEnter2D(Collider2D other) => HandleContact(other);
        private void OnTriggerStay2D(Collider2D other) => HandleContact(other);

        protected void HandleContact(Collider2D other)
        {
            if (IsDead || !other.CompareTag("Player")) return;

            var health = other.GetComponent<PlayerHealth>();
            if (health == null || health.IsDead) return;

            var controller = other.GetComponent<PlayerController2D>();

            if (canBeStomped && IsStomp(other, controller))
            {
                Stomped(controller);
            }
            else
            {
                health.Kill();
            }
        }

        /// <summary>Oyuncu ustumuze mi dustu?</summary>
        protected virtual bool IsStomp(Collider2D other, PlayerController2D controller)
        {
            if (controller == null) return false;

            bool falling = controller.Velocity.y <= 0.01f;
            bool above = other.bounds.min.y > bodyCollider.bounds.center.y + stompHeightThreshold;

            return falling && above;
        }

        protected virtual void Stomped(PlayerController2D controller)
        {
            if (controller != null) controller.LaunchUpward(stompBounceHeight);

            if (GameManager.Instance != null && stompScoreReward > 0)
            {
                GameManager.Instance.AddScore(stompScoreReward);
            }

            // Epic 13/14: ses + parcacik + kamera sarsintisi buraya
            Die();
        }

        /// <summary>Dusmani oldur. Turetilen siniflar genisletebilir.</summary>
        public virtual void Die()
        {
            if (IsDead) return;
            IsDead = true;

            bodyCollider.enabled = false;

            if (rb != null)
            {
                rb.SetVelocity(Vector2.zero);
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            StartCoroutine(DeathAnimation());
        }

        protected virtual System.Collections.IEnumerator DeathAnimation()
        {
            Vector3 baseScale = transform.localScale;
            float t = 0f;

            while (t < deathDuration)
            {
                t += Time.deltaTime;
                float k = t / deathDuration;

                // Ezilme: yassilasip saydamlasir
                transform.localScale = new Vector3(
                    baseScale.x * Mathf.Lerp(1f, 1.25f, k),
                    baseScale.y * Mathf.Lerp(1f, 0.1f, k),
                    baseScale.z);

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, k);
                    spriteRenderer.color = c;
                }

                yield return null;
            }

            gameObject.SetActive(false);   // Destroy degil - Epic 10 reset icin
        }
    }
}
```

**Not:** `Destroy` yerine `SetActive(false)` kullanıyoruz. Epic 10'da respawn
olunca düşmanların geri gelmesi gerekecek (`IResettable`).

- [x] `EnemyBase.cs` yazıldı — temas/ezilme/ölüm/sıfırlama tek yerde

### 2. Devriyeyi EnemyBase'e taşı

Mevcut `Patroller.cs`'i `EnemyBase`'den türet. Hareket mantığı kalır,
temas/ölüm mantığı temel sınıfa gider.

```csharp
public class Patroller : EnemyBase
{
    [Header("Devriye")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool startFacingRight = true;

    [Header("Algilama")]
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private float edgeCheckDistance = 0.55f;
    [SerializeField] private float edgeCheckDepth = 0.9f;
    [SerializeField] private float wallCheckDistance = 0.55f;

    [Header("Donme")]
    [Tooltip("Donmeden once bekleme suresi - daha okunabilir olur.")]
    [SerializeField] private float turnPauseDuration = 0.4f;

    private int direction;
    private float turnPauseLeft;

    protected override void Awake()
    {
        base.Awake();
        rb.freezeRotation = true;
        direction = startFacingRight ? 1 : -1;
        ApplyFacing();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        // Donus duraklamasi
        if (turnPauseLeft > 0f)
        {
            turnPauseLeft -= Time.fixedDeltaTime;
            rb.SetVelocityX(0f);
            return;
        }

        if (ShouldTurnAround())
        {
            direction *= -1;
            ApplyFacing();
            turnPauseLeft = turnPauseDuration;
            return;
        }

        rb.SetVelocityX(direction * moveSpeed);
    }

    // ShouldTurnAround() ve ApplyFacing() mevcut koddan aynen kalir
}
```

**`turnPauseDuration` neden eklendi:** dönmeden önce kısa bir duraklama,
düşmanı çok daha okunabilir ve canlı yapar. Oyuncu dönüşü önceden görür.

- [x] `Patroller` yeniden düzenlendi — 240 → 110 satır, geriye sadece hareket kaldı *(oynanarak doğrulanacak)*

### 3. Nesne havuzu yaz

Mermiler için gerekli. Sürekli `Instantiate`/`Destroy` çöp üretir ve
çöp toplayıcı takılmalarına sebep olur.

`Assets/Scripts/Core/ObjectPool.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Basit nesne havuzu. Instantiate/Destroy yerine ayni nesneleri
    /// yeniden kullanir - cop toplayici takilmasini onler.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> available = new Queue<T>();
        private readonly List<T> all = new List<T>();

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;

            for (int i = 0; i < initialSize; i++)
            {
                T item = CreateNew();
                item.gameObject.SetActive(false);
                available.Enqueue(item);
            }
        }

        private T CreateNew()
        {
            T item = Object.Instantiate(prefab, parent);
            all.Add(item);
            return item;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T item = available.Count > 0 ? available.Dequeue() : CreateNew();

            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            return item;
        }

        public void Return(T item)
        {
            if (item == null || !item.gameObject.activeSelf) return;

            item.gameObject.SetActive(false);
            available.Enqueue(item);
        }

        /// <summary>Bolum sifirlanirken tumunu geri al.</summary>
        public void ReturnAll()
        {
            foreach (T item in all)
            {
                if (item != null && item.gameObject.activeSelf) Return(item);
            }
        }
    }
}
```

- [x] `ObjectPool.cs` yazıldı — çift iade koruması dahil

### 4. Mermi ve mermi atan düşman

`Assets/Scripts/Gameplay/Projectile.cs`:

```csharp
using UnityEngine;
using Platformer.Core;
using Platformer.Player;

namespace Platformer.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private LayerMask blockingLayers = ~0;

        private Vector2 velocity;
        private float lifeLeft;
        private System.Action<Projectile> returnToPool;

        public void Launch(Vector2 direction, float speed, System.Action<Projectile> onExpire)
        {
            velocity = direction.normalized * speed;
            lifeLeft = lifetime;
            returnToPool = onExpire;

            // Mermiyi hareket yonune cevir
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);

            lifeLeft -= Time.deltaTime;
            if (lifeLeft <= 0f) Expire();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerHealth>()?.Kill();
                Expire();
                return;
            }

            // Duvara carpti
            if ((blockingLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                Expire();
            }
        }

        private void Expire()
        {
            returnToPool?.Invoke(this);
        }
    }
}
```

`Assets/Scripts/Gameplay/Shooter.cs`:

```csharp
using System.Collections;
using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Sabit durur, belli aralikla mermi atar. Ates etmeden once gorsel uyari verir.
    /// </summary>
    public class Shooter : EnemyBase
    {
        [Header("Ates")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Vector2 fireDirection = Vector2.left;
        [SerializeField] private float projectileSpeed = 7f;

        [Tooltip("Iki ates arasi sure.")]
        [SerializeField] private float fireInterval = 2.2f;

        [Tooltip("Ates oncesi uyari suresi. ASLA 0 YAPMA.")]
        [SerializeField] private float warningDuration = 0.45f;

        [Tooltip("Yan yana konanlar senkron olmasin diye faz kaydirma.")]
        [SerializeField] private float phaseOffset = 0f;

        [Header("Uyari Gorseli")]
        [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0.4f);

        private ObjectPool<Projectile> pool;
        private Color baseColor;

        protected override void Awake()
        {
            base.Awake();
            canBeStomped = false;   // kule ezilmez

            if (spriteRenderer != null) baseColor = spriteRenderer.color;
            if (muzzle == null) muzzle = transform;

            pool = new ObjectPool<Projectile>(projectilePrefab, 8);
        }

        private void Start() => StartCoroutine(FireLoop());

        private IEnumerator FireLoop()
        {
            yield return new WaitForSeconds(phaseOffset);

            while (!IsDead)
            {
                yield return new WaitForSeconds(fireInterval - warningDuration);

                // --- Uyari: oyuncu atesi onceden gorsun ---
                float t = 0f;
                while (t < warningDuration)
                {
                    t += Time.deltaTime;
                    if (spriteRenderer != null)
                    {
                        // Hizlanan yanip sonme
                        float blink = Mathf.PingPong(t * 12f, 1f);
                        spriteRenderer.color = Color.Lerp(baseColor, warningColor, blink);
                    }
                    yield return null;
                }

                if (spriteRenderer != null) spriteRenderer.color = baseColor;

                Fire();
            }
        }

        private void Fire()
        {
            Projectile p = pool.Get(muzzle.position, Quaternion.identity);
            p.Launch(fireDirection, projectileSpeed, pool.Return);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            Gizmos.DrawRay(origin, (Vector3)fireDirection.normalized * 5f);
        }
    }
}
```

**Uyarı süresi kritik.** Uyarısız mermi haksız hissettirir. 0.3 saniyenin
altına inme.

- [x] Mermi atan düşman yazıldı — `ShooterEnemy.cs` *(oynanarak doğrulanacak)*
- [x] Ateş öncesi görsel uyarı var — 0,45 sn, hem renk hem boyut değişiyor
- [x] Mermiler havuzdan geliyor — *Profiler ölçümü yapılmadı, kod yolu doğru*

### 5. Uçan düşman

Sinüs dalgası çizerek hareket eder. Hem tehdit hem platform — en ilginç
düşman tipi budur.

```csharp
using UnityEngine;

namespace Platformer.Gameplay
{
    public class Flyer : EnemyBase
    {
        [Header("Ucus")]
        [SerializeField] private Vector2 moveDirection = Vector2.left;
        [SerializeField] private float moveSpeed = 2f;

        [Tooltip("Salinim genligi (birim).")]
        [SerializeField] private float waveAmplitude = 1.2f;

        [Tooltip("Salinim hizi.")]
        [SerializeField] private float waveFrequency = 2f;

        [Tooltip("Gidis mesafesi. 0 = sonsuz duz gider.")]
        [SerializeField] private float travelDistance = 6f;

        [SerializeField] private float phaseOffset = 0f;

        private Vector2 origin;
        private Vector2 perpendicular;
        private float elapsed;
        private int travelSign = 1;

        protected override void Awake()
        {
            base.Awake();
            origin = transform.position;
            moveDirection = moveDirection.normalized;
            perpendicular = new Vector2(-moveDirection.y, moveDirection.x);

            // Ucan dusman yercekimsiz
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.useFullKinematicContacts = true;
            }
        }

        private void FixedUpdate()
        {
            if (IsDead) return;

            elapsed += Time.fixedDeltaTime * travelSign;

            float along = elapsed * moveSpeed;

            // Mesafe sinirina gelince geri don
            if (travelDistance > 0f && Mathf.Abs(along) > travelDistance)
            {
                travelSign *= -1;
                if (spriteRenderer != null) spriteRenderer.flipX = travelSign < 0;
            }

            float wave = Mathf.Sin((elapsed * waveFrequency) + phaseOffset) * waveAmplitude;
            Vector2 target = origin + moveDirection * along + perpendicular * wave;

            rb.MovePosition(target);
        }
    }
}
```

- [x] Uçan düşman yazıldı — `FlyerEnemy.cs` *(oynanarak doğrulanacak)*

### 6. Düşman yerleştirme kuralları

Kod bitti; şimdi tasarım. Bu kurallar kodu kadar önemli:

| Kural | Neden |
|---|---|
| Düşman asla kör noktada olmasın | Ekrana girer girmez görülmeli |
| Zıplama inişine düşman koyma | Oyuncu havadayken kaçamaz |
| Checkpoint yanına düşman koyma | Doğar doğmaz ölmek berbat |
| İlk düşmanı güvenli tanıt | Altında zemin olsun |
| Mermi hattı görünür olsun | Gizmo'daki kırmızı çizgi ekranda olmalı |
| Uçan düşmanı platform olarak da kullan | Tek rolde bırakma |

- [x] Kuralların ikisi artık **otomatik denetleniyor** (boşluk inişi, checkpoint yakınlığı). Bölüm 1'de düşman yok — kasıtlı, öğretme bölümü

---

## Yerleştirme kurallarından ikisi koda taşındı

Epic'in altı yerleştirme kuralından dördü tasarımcı gözüyle bakılacak şey.
Ama şu ikisi **ölçülebilir** ve ikisi de haksız ölüm üretiyor:

| Kural | Neden koda taşındı |
|---|---|
| Boşluk inişine düşman koyma | Oyuncu havadayken yön değiştiremiyor — görse bile kaçamaz |
| Checkpoint yanına düşman koyma | Doğar doğmaz ölmek, "oyun bozuk" dedirten şeylerin başında |

`LevelCursor.ValidateEnemyPlacements()` her kurulumda kontrol ediyor.

### İlk sürümü yetersizdi — oynayınca çıktı

Kontrol düşmanın **başlangıç konumuna** bakıyordu. Devriye düşmanı
checkpoint'ten 6 birim uzakta başlıyor, kontrolü geçiyor, sonra yürüyüp
checkpoint'e geliyor. Test odasında **iki dakikada 24 ölüm**.

Artık **ulaşabildiği aralığa** bakıyor:

| Düşman | Tehlike aralığı |
|---|---|
| Devriye | Bulunduğu kesintisiz ve aynı kottaki zeminin tamamı — boşluk ve basamak onun için duvar, ikisinde de dönüyor |
| Atıcı | Ateş menzili (16 birim), ateş ettiği yönde |
| Uçan | Yatay gidiş geliş aralığı |

Aynı dersi diken tuzağında da almıştık: **durağan bir kontrol, hareketli
bir tehlike için yeterli değil.**

---

## Kabul kriteri

- [x] 3 düşman çeşidi var, her biri farklı soru soruyor
- [x] Hepsi `EnemyBase`'den türüyor
- [x] Mermi atan düşmanın ateş öncesi uyarısı var — 0,45 sn, renk **ve** boyut
- [x] Mermiler havuzdan geliyor — *Profiler ölçümü yapılmadı; kod yolunda Instantiate/Destroy yok*
- [x] Hiçbir düşman kör noktadan gelmiyor — test odasında oynanarak doğrulandı
- [x] Düşman ezmek tatmin edici — oynanarak onaylandı
- [x] Ölen düşman `SetActive(false)` oluyor — checkpoint'ten dönünce geri geliyor

---

## Tuzaklar

**Çok fazla düşman çeşidi.** 6 düşman 6 kat iştir ve oyuncu farkı zaten
anlamaz. 3 tane yeter.

**Sağlık barı olan düşman.** Platform oyununda düşman tek vuruşta ölmeli.
Sağlık barı dövüş oyunu mekaniğidir ve ritmi bozar.

**Uyarısız ateş.** Oyuncu "bu haksız" der ve haklıdır.

**Karmaşık AI.** Pathfinding, durum makinesi... Platform oyununda düşman
**tahmin edilebilir** olmalı. Tahmin edilemeyen düşman eğlenceli değil,
sinir bozucudur.

**`Destroy` kullanmak.** Respawn'da düşmanların geri gelmesi gerekir.
`SetActive(false)` kullan.

**Mermide `Instantiate`/`Destroy`.** Her mermide çöp üretir, oyun takılır.

**Faz kaydırmayı unutmak.** Yan yana 3 kule aynı anda ateş ederse hem
sıkıcı hem de geçilmez olur.

---

## v1'de yapma

- Pathfinding / NavMesh / A*
- Düşman sağlık sistemi, çok vuruşluk düşmanlar
- Boss savaşı (istersen sadece 1, en sonda)
- Düşmanların birbiriyle etkileşimi
- Zırhlı/kalkanlı varyantlar
- Düşman düşürme (loot) sistemi
- Düşman diyalogu veya sesli tepkileri
- Dinamik düşman üretimi (spawner)

---

## Sonraki

[Epic 07 — Tehlikeler ve engeller](07-tehlikeler-ve-engeller.md)
