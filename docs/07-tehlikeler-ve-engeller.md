# Epic 07 — Tehlikeler, Tuzaklar, Hareketli Parçalar

**Amaç:** Bölümlere çeşitlilik katan, düşman olmayan engeller. Bunlar oyunun
ritmini belirler.

**Ön koşul:** [Epic 05](05-tilemap.md)

**Süre:** 5–6 gün

---

## Neden bu epic var

Düşmanlar tepki verir, tehlikeler vermez — ve bu iyi bir şeydir. Tehlikeler
**ritim** kurar: düzenli, tahmin edilebilir, öğrenilebilir.

Oyuncu bir tehlike desenini çözdüğünde akış hissi (flow) oluşur. Platform
oyununun en tatmin edici anı budur: deseni okuyup kusursuz geçmek.

---

## Elindeki altyapı

| Script | Ne yapıyor |
|---|---|
| `Hazard.cs` | Dokununca öldürür, yönlü olabilir (`requiredApproachDirection`) |
| `MovingPlatform.cs` | Noktalar arası gider-gelir, yolcu taşır, ping-pong/loop |

---

## Görevler

### 1. Ritimli tehlikeler için temel sınıf

Aralıklı diken, ateş püskürtücü, sallanan balta — hepsinin ortak iskeleti.

`Assets/Scripts/Gameplay/TimedHazard.cs`:

```csharp
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Donguyle acilip kapanan tehlikelerin temeli.
    /// Dongu: [bekleme] -> [uyari] -> [aktif] -> [bekleme] ...
    /// Uyari asamasi ZORUNLU - uyarisiz tehlike haksizdir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class TimedHazard : MonoBehaviour
    {
        [Header("Dongu")]
        [Tooltip("Tam dongu suresi (saniye).")]
        [SerializeField] protected float cycleDuration = 2.5f;

        [Tooltip("Tehlikenin aktif oldugu sure.")]
        [SerializeField] protected float activeDuration = 1f;

        [Tooltip("Aktif olmadan onceki uyari suresi. 0.3'un altina INME.")]
        [SerializeField] protected float warningDuration = 0.4f;

        [Tooltip("Yan yana konanlar senkron olmasin diye faz kaydirma (0-1).")]
        [Range(0f, 1f)]
        [SerializeField] protected float phaseOffset = 0f;

        [Header("Baslangic")]
        [Tooltip("Acikken baslasin mi?")]
        [SerializeField] protected bool startActive = false;

        public enum Phase { Idle, Warning, Active }
        public Phase CurrentPhase { get; private set; } = Phase.Idle;

        protected Collider2D hazardCollider;
        private float timer;

        protected virtual void Awake()
        {
            hazardCollider = GetComponent<Collider2D>();
            hazardCollider.isTrigger = true;

            timer = phaseOffset * cycleDuration;
            if (startActive) timer = cycleDuration - activeDuration;
        }

        protected virtual void Update()
        {
            timer += Time.deltaTime;
            if (timer >= cycleDuration) timer -= cycleDuration;

            Phase next = EvaluatePhase();
            if (next != CurrentPhase)
            {
                CurrentPhase = next;
                OnPhaseChanged(next);
            }

            // Sadece aktif fazda oldurur
            hazardCollider.enabled = (CurrentPhase == Phase.Active);
        }

        private Phase EvaluatePhase()
        {
            float activeStart = cycleDuration - activeDuration;
            float warningStart = activeStart - warningDuration;

            if (timer >= activeStart) return Phase.Active;
            if (timer >= warningStart) return Phase.Warning;
            return Phase.Idle;
        }

        /// <summary>Faz degisince gorsel/ses tepkisi. Turetilen sinif doldurur.</summary>
        protected abstract void OnPhaseChanged(Phase phase);

        /// <summary>Uyari fazinda 0'dan 1'e giden ilerleme (gorsel icin).</summary>
        protected float WarningProgress
        {
            get
            {
                if (CurrentPhase != Phase.Warning) return 0f;
                float warningStart = cycleDuration - activeDuration - warningDuration;
                return Mathf.Clamp01((timer - warningStart) / warningDuration);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryKill(other);
        private void OnTriggerStay2D(Collider2D other) => TryKill(other);

        private void TryKill(Collider2D other)
        {
            if (CurrentPhase != Phase.Active || !other.CompareTag("Player")) return;
            other.GetComponent<Player.PlayerHealth>()?.Kill();
        }
    }
}
```

Somut örnek — **aralıklı diken**:

```csharp
using UnityEngine;

namespace Platformer.Gameplay
{
    public class RetractingSpikes : TimedHazard
    {
        [Header("Gorsel")]
        [SerializeField] private Transform spikeVisual;
        [SerializeField] private float hiddenOffsetY = -0.9f;
        [SerializeField] private float riseSpeed = 14f;

        private Vector3 shownPosition;
        private Vector3 hiddenPosition;
        private Vector3 targetPosition;

        protected override void Awake()
        {
            base.Awake();
            if (spikeVisual == null) spikeVisual = transform;

            shownPosition = spikeVisual.localPosition;
            hiddenPosition = shownPosition + new Vector3(0f, hiddenOffsetY, 0f);
            targetPosition = hiddenPosition;
            spikeVisual.localPosition = hiddenPosition;
        }

        protected override void Update()
        {
            base.Update();

            // Uyari fazinda titre - oyuncu gelmekte oldugunu gorsun
            Vector3 goal = targetPosition;
            if (CurrentPhase == Phase.Warning)
            {
                float shake = Mathf.Sin(Time.time * 50f) * 0.05f * WarningProgress;
                goal += new Vector3(shake, 0f, 0f);
            }

            spikeVisual.localPosition = Vector3.MoveTowards(
                spikeVisual.localPosition, goal, riseSpeed * Time.deltaTime);
        }

        protected override void OnPhaseChanged(Phase phase)
        {
            targetPosition = (phase == Phase.Active) ? shownPosition : hiddenPosition;
            // Epic 13: ses buraya
        }
    }
}
```

- [ ] `TimedHazard` yazıldı
- [ ] En az 2 ritimli tehlike çalışıyor
- [ ] Hepsinde görsel uyarı var

### 2. Düşen platform

Basınca titrer, düşer, bir süre sonra geri gelir. Oyuncuyu ilerlemeye zorlar —
çok etkili bir tasarım aracıdır.

```csharp
using System.Collections;
using UnityEngine;

namespace Platformer.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class FallingPlatform : MonoBehaviour
    {
        [Header("Zamanlama")]
        [Tooltip("Basildiktan sonra dusmeye kadar gecen sure.")]
        [SerializeField] private float shakeDuration = 0.55f;

        [Tooltip("Dustukten sonra yok olmaya kadar.")]
        [SerializeField] private float destroyDelay = 1.5f;

        [Tooltip("Geri gelme suresi. 0 = geri gelmez.")]
        [SerializeField] private float respawnDelay = 3f;

        [Header("Gorsel")]
        [SerializeField] private float shakeMagnitude = 0.06f;

        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer spriteRenderer;
        private Vector3 startPosition;
        private bool triggered;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            startPosition = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (triggered || !collision.collider.CompareTag("Player")) return;

            // Sadece ustune basildiysa tetiklensin
            if (collision.collider.bounds.min.y < col.bounds.max.y - 0.1f) return;

            triggered = true;
            StartCoroutine(FallSequence());
        }

        private IEnumerator FallSequence()
        {
            // 1. Titre - oyuncuya "kac" de
            float t = 0f;
            while (t < shakeDuration)
            {
                t += Time.deltaTime;
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                transform.position = startPosition + new Vector3(x, y, 0f);
                yield return null;
            }
            transform.position = startPosition;

            // 2. Dus
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;

            yield return new WaitForSeconds(destroyDelay);

            // 3. Gizle
            SetVisible(false);

            // 4. Geri gel
            if (respawnDelay > 0f)
            {
                yield return new WaitForSeconds(respawnDelay);
                ResetPlatform();
            }
        }

        private void SetVisible(bool visible)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = visible;
            col.enabled = visible;
        }

        public void ResetPlatform()
        {
            StopAllCoroutines();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.SetVelocity(Vector2.zero);
            transform.position = startPosition;
            SetVisible(true);
            triggered = false;
        }
    }
}
```

`ResetPlatform()` public — Epic 10'daki `IResettable` bunu çağıracak.

- [ ] Düşen platform çalışıyor

### 3. Tek yönlü platform

Aşağıdan zıplayınca geçilir, üstünde durulur. Unity'nin hazır bileşeni var.

**Kurulum:**
1. Platform nesnesine `Platform Effector 2D` ekle
2. `Surface Arc` = **180**
3. `Use One Way` = ✔
4. Collider'da **`Used By Effector`** = ✔

**Aşağı inme (↓ + Zıpla):** `PlayerController2D` içine:

```csharp
[Header("Tek Yonlu Platform")]
[SerializeField] private float dropThroughDuration = 0.35f;
private PlatformEffector2D currentEffector;

// ReadInput() icine:
if (Input.GetAxisRaw("Vertical") < -0.5f && Input.GetButtonDown("Jump") && IsGrounded)
{
    TryDropThrough();
}

private void TryDropThrough()
{
    Vector2 origin = (Vector2)transform.position + groundCheckOffset;
    Collider2D hit = Physics2D.OverlapBox(origin, groundCheckSize, 0f, groundLayers);
    if (hit == null) return;

    var effector = hit.GetComponent<PlatformEffector2D>();
    if (effector == null) return;

    StartCoroutine(DropThrough(effector));
}

private System.Collections.IEnumerator DropThrough(PlatformEffector2D effector)
{
    float originalArc = effector.surfaceArc;
    effector.surfaceArc = 0f;          // gecici olarak katilastir -> gecersin
    yield return new WaitForSeconds(dropThroughDuration);
    effector.surfaceArc = originalArc;
}
```

- [ ] Tek yönlü platform çalışıyor
- [ ] ↓ + Zıpla ile aşağı inilebiliyor

### 4. Zıplama pedi

`PlayerController2D.LaunchUpward()` zaten hazır — sadece bir trigger yeter.

```csharp
using UnityEngine;
using Platformer.Player;

namespace Platformer.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class JumpPad : MonoBehaviour
    {
        [Tooltip("Firlatma yuksekligi (birim).")]
        [SerializeField] private float launchHeight = 6f;

        [Tooltip("Ayni oyuncuyu tekrar firlatmadan once bekleme.")]
        [SerializeField] private float cooldown = 0.25f;

        [SerializeField] private Transform visual;

        private float cooldownLeft;

        private void Update()
        {
            if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;

            // Yaylanma animasyonu
            if (visual != null)
            {
                visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one, Time.deltaTime * 10f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (cooldownLeft > 0f || !other.CompareTag("Player")) return;

            var controller = other.GetComponent<PlayerController2D>();
            if (controller == null) return;

            controller.LaunchUpward(launchHeight);
            cooldownLeft = cooldown;

            // Ezilme efekti - Lerp ile geri acilacak
            if (visual != null) visual.localScale = new Vector3(1.3f, 0.55f, 1f);
            // Epic 13: ses buraya
        }
    }
}
```

- [ ] Zıplama pedi çalışıyor

### 5. Statik tehlikeleri gözden geçir

**Dikenler (mevcut):**
- Collider sprite'tan **küçük** olmalı. Oyuncu "değmedim ama öldüm" dememeli.
  Mevcut değer 0.55 yükseklik — bu iyi, daha da cömert yapabilirsin.
- Duvara ve tavana yerleştirilebilen döndürülmüş varyantlar yap.

**Lav / asit / dipsiz boşluk:**
- Geniş alan tehlikesi, `Hazard` script'i yeter
- Yüzey animasyonu ekle (Epic 12)
- `GameManager.killPlaneY` zaten dipsiz boşluğu hallediyor

- [ ] Diken collider'ı cömert
- [ ] Duvar/tavan varyantları var

### 6. Okunabilirlik denetimi

Tüm tehlikeler için:

- [ ] Tehlike olduğu **bakar bakmaz** anlaşılıyor
- [ ] Aktifleşmeden önce uyarı var (≥0.3 sn)
- [ ] Collider görselden **küçük**
- [ ] Ekran dışından gelmiyor
- [ ] Checkpoint'in hemen yanında değil
- [ ] Dekoratif öğelerle karıştırılamıyor

---

## Kabul kriteri

- [ ] En az 5 farklı tehlike/engel çeşidi var
- [ ] Her ritimli tehlikenin görünür uyarısı var
- [ ] Faz kaydırma parametresi çalışıyor (yan yana konanlar senkron değil)
- [ ] Tek yönlü platform hem yukarı hem aşağı çalışıyor
- [ ] Düşen platform titreyip düşüyor ve geri geliyor
- [ ] Hiçbir tehlike "haksız" hissettirmiyor
- [ ] Collider'lar görselden küçük

---

## Tuzaklar

**Uyarısız tehlike.** En sık ve en ölümcül hata. Oyuncu ölümü gelmeden
görebilmeli.

**Collider'ı sprite'la aynı yapmak.** Oyuncu "değmedim ki!" der ve haklıdır.
Her zaman oyuncu lehine cömert ol.

**Çok hızlı döngü.** Oyuncunun deseni öğrenmesi için en az 1.5–2 sn gerekir.

**Hareketli platformda `transform.position` kullanmak.** Karakter titrer.
Kinematik Rigidbody + `MovePosition` doğru yöntem — `MovingPlatform` zaten öyle.

**Faz kaydırmayı unutmak.** Yan yana 3 diken aynı anda çıkarsa hem sıkıcı
hem geçilmez olur.

**Tehlikeyi süs sanmak (veya tersi).** Arka plandaki dekoratif ateş,
tehlikeymiş gibi görünür ve oyuncuyu durdurur.

**Düşen platformda `Destroy` kullanmak.** Respawn'da geri gelmesi gerekir.

---

## v1'de yapma

- Fizik tabanlı yıkılan yapılar
- Su/sıvı simülasyonu, yüzme
- Zincirleme tepki tuzakları
- Oyuncunun yerleştirebildiği platformlar
- Yerçekimi değiştiren bölgeler (imza mekaniğin değilse)
- Işınlanma kapıları
- Hareket eden duvarlar / ezici presler (zamanlama karmaşık)

---

## Sonraki

[Epic 08 — Toplanabilirler](08-toplanabilirler.md)
