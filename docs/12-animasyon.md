# Epic 12 — Animasyon

**Amaç:** Karakteri ve dünyayı canlandırmak. Az sayıda ama doğru zamanlanmış
animasyon.

**Ön koşul:** [Epic 11](11-sanat-ve-sprite.md)

**Süre:** 1 hafta

---

## Neden bu epic var

Animasyon "güzellik" değil, **geri bildirim**. Oyuncu karakterin ne yaptığını
animasyondan okur: zıplıyor mu, düşüyor mu, yere değdi mi, hız kazanıyor mu?

**En kritik kural:**

> Animasyon oynanışı beklemez. Önce hareket başlar, animasyon onu takip eder.

Tersi olursa kontrol "ağır" ve "gecikmeli" hissettirir — ve oyuncu bunu
"kontroller kötü" diye yorumlar.

---

## Elindeki altyapı

`PlayerController2D` animasyon için gereken her şeyi dışarı açıyor:

```csharp
public bool IsGrounded { get; }
public bool IsJumping { get; }
public float HorizontalInput { get; }
public Vector2 Velocity { get; }
public bool FacingRight { get; }
public System.Action OnJumped;
public System.Action OnLanded;
```

Bunları okuyan bir `PlayerAnimator` yazacaksın.

---

## Görevler

### 1. Animasyon listesi

Minimum set — bundan fazlası v1'de gereksiz:

| Animasyon | Kare | FPS | Döngü | Öncelik |
|---|---|---|---|---|
| Idle (bekleme) | 2–4 | 6–8 | ✔ | Zorunlu |
| Run (koşma) | 4–6 | 10–12 | ✔ | Zorunlu |
| Jump (yükselme) | 1–2 | — | ✘ | Zorunlu |
| Fall (düşme) | 1–2 | — | ✘ | Zorunlu |
| Land (iniş) | 2–3 | 15 | ✘ | Önerilen |
| Death (ölüm) | 3–4 | 12 | ✘ | Önerilen |

**6 animasyon yeterli.** Duvara tutunma, dönme, eğilme — hepsi sonra.

Idle bile 2 kare olabilir (hafif nefes alma). Hiç hareket etmeyen karakter
ölü görünür — bu 2 kare çok fark yaratır.

- [ ] Liste çıkarıldı

### 2. Sprite sheet hazırla

Tüm kareler tek PNG'de, eşit aralıklı ızgara.

```
┌────┬────┬────┬────┬────┬────┐
│idle│idle│idle│idle│    │    │  satır 1
├────┼────┼────┼────┼────┼────┤
│run │run │run │run │run │run │  satır 2
├────┼────┼────┼────┼────┼────┤
│jump│fall│land│land│    │    │  satır 3
└────┴────┴────┴────┴────┴────┘
```

**Import ayarları:**

| Ayar | Değer |
|---|---|
| Sprite Mode | **Multiple** |
| Pixels Per Unit | 32 |
| Filter Mode | Point |
| Pivot | **Bottom** |

Sprite Editor → `Slice` → **Grid By Cell Size** → 32×32 → Apply

**Pivot'u Bottom yapmak önemli:** Center olursa karakterin ayağı zemine
gömülü veya havada görünür ve her animasyonda kayar.

- [ ] Sprite sheet hazır ve dilimlenmiş

### 3. Animator Controller kur

`Assets → Create → Animation → Animator Controller` → `PlayerAnimator.controller`

**Parametreler:**

| Ad | Tip | Kaynak |
|---|---|---|
| `Speed` | Float | `Mathf.Abs(HorizontalInput)` |
| `IsGrounded` | Bool | `controller.IsGrounded` |
| `VelocityY` | Float | `controller.Velocity.y` |
| `Jump` | Trigger | `OnJumped` olayı |
| `Land` | Trigger | `OnLanded` olayı |
| `Death` | Trigger | `PlayerHealth.OnDied` |

**Geçişler:**

```
Idle  → Run    : Speed > 0.1
Run   → Idle   : Speed < 0.1
Any   → Jump   : Jump (trigger)
Jump  → Fall   : VelocityY < 0
Fall  → Land   : IsGrounded = true
Land  → Idle   : Exit Time (animasyon bitince)
Any   → Death  : Death (trigger)
```

**Her geçiş için:**

| Ayar | Değer | Neden |
|---|---|---|
| `Has Exit Time` | **✘ kapalı** (Land→Idle hariç) | Açıksa animasyon bitene kadar beklenir, kontrol geç hissettirir |
| `Transition Duration` | **0** | Pixel art'ta blend bulanık ara kareler üretir ve gecikme yaratır |
| `Can Transition To Self` | ✘ | Trigger tekrar tetiklenince baştan başlamasın |

- [ ] Animator kuruldu
- [ ] Tüm geçişlerde Exit Time kapalı, süre 0

### 4. PlayerAnimator yaz

`Assets/Scripts/Player/PlayerAnimator.cs`:

```csharp
using UnityEngine;

namespace Platformer.Player
{
    /// <summary>
    /// PlayerController2D'nin durumunu Animator'a aktarir.
    /// Ayrica squash & stretch yapar - kare cizmeden canlilik katar.
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerAnimator : MonoBehaviour
    {
        // string yerine hash - her karede string karsilastirmak israf
        private static readonly int SpeedHash      = Animator.StringToHash("Speed");
        private static readonly int GroundedHash   = Animator.StringToHash("IsGrounded");
        private static readonly int VelocityYHash  = Animator.StringToHash("VelocityY");
        private static readonly int JumpHash       = Animator.StringToHash("Jump");
        private static readonly int LandHash       = Animator.StringToHash("Land");
        private static readonly int DeathHash      = Animator.StringToHash("Death");

        [Header("Squash & Stretch")]
        [Tooltip("Zipla aninda dikey uzama.")]
        [SerializeField] private Vector2 jumpSquash = new Vector2(0.82f, 1.25f);

        [Tooltip("Inis aninda yatay ezilme.")]
        [SerializeField] private Vector2 landSquash = new Vector2(1.25f, 0.75f);

        [Tooltip("Normale donme hizi. Buyuk = daha hizli.")]
        [SerializeField] private float squashRecoverySpeed = 12f;

        [Header("Referanslar")]
        [Tooltip("Bos birakilirsa cocuk SpriteRenderer kullanilir.")]
        [SerializeField] private Transform visual;

        private PlayerController2D controller;
        private PlayerHealth health;
        private Animator animator;
        private Vector3 currentSquash = Vector3.one;

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            health = GetComponent<PlayerHealth>();
            animator = GetComponentInChildren<Animator>();

            if (visual == null)
            {
                var sr = GetComponentInChildren<SpriteRenderer>();
                visual = sr != null ? sr.transform : transform;
            }
        }

        private void OnEnable()
        {
            controller.OnJumped += HandleJump;
            controller.OnLanded += HandleLand;
            if (health != null) health.OnDied += HandleDeath;
        }

        private void OnDisable()
        {
            // ZORUNLU: abonelik birakilmazsa sahne degisiminde hata alirsin
            controller.OnJumped -= HandleJump;
            controller.OnLanded -= HandleLand;
            if (health != null) health.OnDied -= HandleDeath;
        }

        private void Update()
        {
            UpdateAnimatorParameters();
            UpdateSquash();
        }

        private void UpdateAnimatorParameters()
        {
            if (animator == null) return;

            animator.SetFloat(SpeedHash, Mathf.Abs(controller.HorizontalInput));
            animator.SetBool(GroundedHash, controller.IsGrounded);
            animator.SetFloat(VelocityYHash, controller.Velocity.y);
        }

        private void UpdateSquash()
        {
            // Deformasyon yumusak sekilde normale doner
            currentSquash = Vector3.Lerp(
                currentSquash, Vector3.one, Time.deltaTime * squashRecoverySpeed);

            visual.localScale = currentSquash;
        }

        private void HandleJump()
        {
            animator?.SetTrigger(JumpHash);
            currentSquash = new Vector3(jumpSquash.x, jumpSquash.y, 1f);
        }

        private void HandleLand()
        {
            animator?.SetTrigger(LandHash);
            currentSquash = new Vector3(landSquash.x, landSquash.y, 1f);
        }

        private void HandleDeath()
        {
            animator?.SetTrigger(DeathHash);
        }
    }
}
```

**`Animator.StringToHash` neden:** her karede string ile parametre aramak
gereksiz maliyet. Hash bir kez hesaplanır, sonra tamsayı karşılaştırması olur.

- [ ] `PlayerAnimator.cs` yazıldı ve çalışıyor

### 5. Squash & stretch'i ayarla

Kare çizmeden animasyon hissi veren en güçlü teknik. Yukarıdaki kod zaten
yapıyor; şimdi ayarla:

| Değer | Ne yapar | Aralık |
|---|---|---|
| `jumpSquash` | Zıplarken dikey uzama | (0.80, 1.30) – (0.90, 1.15) |
| `landSquash` | İnerken yatay ezilme | (1.15, 0.85) – (1.35, 0.65) |
| `squashRecoverySpeed` | Normale dönme hızı | 8 (yumuşak) – 18 (keskin) |

**Abartma.** 0.5/1.5 gibi değerler karakteri lastik gibi gösterir.
Hafif olması daha iyi — fark edilmesin ama hissedilsin.

**Hacim korunumu:** `x × y ≈ 1` olsun. `(0.82, 1.25)` → `1.025` ✓

- [ ] Squash & stretch ayarlandı, abartılı değil

### 6. Düşman ve çevre animasyonları

| Nesne | Animasyon | Not |
|---|---|---|
| Devriye düşman | Yürüme (2–4 kare) | Dönerken duraklama animasyonu |
| Uçan düşman | Kanat çırpma (2 kare) | Hızlı |
| Para | Dönme (4–6 kare) | `Coin.cs` şu an Y ekseninde döndürüyor, sprite daha iyi |
| Checkpoint | Aktifleşme (bayrak açılma) | Tek seferlik |
| Aralıklı diken | Çıkma/inme | `RetractingSpikes` konumla yapıyor, yeterli |
| Su/lav yüzeyi | Dalgalanma (3–4 kare) | Döngü |

**Para için sprite animasyonu:** mevcut kod `visual.Rotate(0, spinSpeed, 0)`
ile 3D dönüş yapıyor; bu unlit sprite'ta kabul edilebilir görünür. Sprite
sheet ile yaparsan daha iyi olur ama zorunlu değil.

- [ ] Düşman animasyonları
- [ ] Çevre animasyonları

### 7. Animasyon olayları (Animation Events)

Belirli bir karede ses/efekt tetiklemek için:

Animation penceresinde animasyonu aç → zaman çizgisinde sağ tık → `Add Animation Event`
→ çağrılacak metodu seç.

Kullanım örnekleri:
- Koşma animasyonunun 2. ve 5. karesinde adım sesi
- İniş animasyonunun 1. karesinde toz parçacığı
- Ölüm animasyonunun son karesinde respawn tetikleme

Metot `public void` olmalı ve animator'ın olduğu nesnede bulunmalı.

- [ ] (İsteğe bağlı) Adım sesi animasyon olayıyla bağlandı

---

## Kabul kriteri

- [ ] Karakterin 6 temel animasyonu var
- [ ] Animasyon geçişleri gecikme hissettirmiyor
- [ ] Kontrol animasyonu beklemiyor (tuşa basınca hemen hareket)
- [ ] Squash & stretch çalışıyor ve abartılı değil
- [ ] Pivot ayakta, karakter zemine tam basıyor
- [ ] `StringToHash` kullanılıyor
- [ ] `OnDisable`'da abonelikler bırakılıyor
- [ ] Düşman ve para animasyonlu

---

## Tuzaklar

**Animasyonun oynanışı bekletmesi.** En kritik hata. Zıpla tuşuna basınca
karakter hemen zıplamalı; "hazırlanma" animasyonu oynatıp sonra zıplarsa
kontrol bozuk hissettirir.

**`Transition Duration > 0`.** Pixel art'ta bulanık ara kareler oluşur
ve tepki gecikir.

**`Has Exit Time` açık bırakmak.** Animasyon bitene kadar tepki verilmez.
Unity'nin varsayılanı **açık** — her geçişte kapatmayı unutma.

**Pivot Center bırakmak.** Karakter zeminle hizalanmaz, her animasyonda kayar.

**Çok fazla animasyon.** 20 animasyonlu bir karakter çizmek aylar alır.

**`SetFloat("Speed", ...)` her karede string ile.** Hash kullan.

**`OnDisable`'da abonelik bırakmamak.** Sahne değişince
`MissingReferenceException` yağmuru.

**Squash & stretch'i abartmak.** Karakter lastik gibi görünür.

---

## v1'de yapma

- İskelet (bone) animasyon — kare kare çok daha basit
- IK (ayakların zemine uyması)
- Yüz ifadeleri, göz takibi
- Kıyafet/ekipman değişimi
- Prosedürel animasyon
- Blend Tree'ler (basit geçişler yeterli)
- Animation Rigging paketi
- Kumaş/saç simülasyonu

---

## Sonraki

[Epic 13 — Ses ve müzik](13-ses-ve-muzik.md)
