# Epic 02 — Karakter Hissiyatı

> ✅ **TAMAMLANDI — 14 Eylül 2026.** Çıktı: [AYARLAR.md](AYARLAR.md) + zorluk cetveli.
> Dört tarz karşılaştırıldı, "dengeli" seçildi. Ölçümler: 3,03 / 5,31 / 7,62.
> Coyote time ve jump buffer oyun içinde test edildi, ikisi de çalışıyor.

**Amaç:** Karakteri kontrol etmenin kendisini eğlenceli hale getirmek. Bölüm
olmasa bile, boş bir zeminde gezinmek keyifli olmalı.

**Ön koşul:** [Epic 01](01-oyun-vizyonu.md)

**Süre:** 1 hafta. Çoğu zaman kod yazmakla değil, sayı ayarlamakla geçecek.

---

## Neden bu epic var

Platform oyununda oyuncu bütün oyun boyunca **tek bir şey** yapar: yürür ve
zıplar. Bu iki eylem iyi hissettirmiyorsa hiçbir şey kurtarmaz.

Ölçüt şu: **boş bir düzlükte 30 saniye gezin.** Canın sıkılıyorsa sorun var.
Kendini zıplatıp durmaktan keyif alıyorsan doğru yoldasın.

Bu epic'e ayırdığın her saat, sonraki 12 bölümün hepsinde geri döner.

---

## Elindeki altyapı

`Assets/Scripts/Player/PlayerController2D.cs` şunları zaten yapıyor:

| Özellik | Ne yapar | Neden önemli |
|---|---|---|
| **Coyote time** | Platformdan düştükten sonra ~0.1 sn daha zıplayabilirsin | Oyuncu "tam kenardan zıpladım ama olmadı" demez |
| **Jump buffer** | Yere değmeden basılan zıpla hafızada tutulur | Erken basılan tuş kaybolmaz |
| **Değişken zıplama** | Tuşu bırakınca zıplama kesilir | Tek tuşla iki farklı yükseklik |
| **Ayrı düşüş yerçekimi** | Düşerken daha ağır | Zıplama "yapışkan" hissetmez |
| **Havada azaltılmış kontrol** | Zıpladıktan sonra yön değiştirmek zorlaşır | Zıplama bir karar olur, sürekli düzeltme değil |
| **Türetilmiş fizik** | Yükseklik + süre verirsin, yerçekimi hesaplanır | Sihirli sayı denemesi yok |
| **Zemine yapışma** | Yerdeyken düşey hız birikmez | Eğim ve hareketli platformdan kopmaz |

Bu liste çoğu ücretli asset'ten daha kapsamlı. **Sıfırdan yazmana gerek yok,
ayarlaman gerekiyor.**

---

## Türetilmiş fizik nasıl çalışıyor

Kodun kalbindeki fikir şu: yerçekimi ve zıplama hızını elle girmiyorsun.
Onun yerine *ne istediğini* söylüyorsun:

```
jumpHeight   = 3.2   → "3.2 birim yükselsin"
jumpApexTime = 0.38  → "tepeye 0.38 saniyede çıksın"
```

Kod bunlardan fiziği hesaplıyor:

```csharp
// h = (g · t²) / 2   →   g = 2h / t²
gravity      = (2f * jumpHeight) / (jumpApexTime * jumpApexTime);
jumpVelocity = gravity * jumpApexTime;
```

Örnek: `h = 3.2`, `t = 0.38` →
`g = 2 × 3.2 / 0.38² = 44.3 birim/sn²` ve `v = 44.3 × 0.38 = 16.8 birim/sn`

Bunun anlamı: **istediğin hissi doğrudan yazabiliyorsun.** "Daha yükseğe
zıplasın" demek için `jumpHeight`'ı artırırsın; yerçekimi kendini ayarlar.

---

## Görevler

### 1. Test odası yap

Ayar yapmak için bölüm oynamaya gerek yok — hatta zararlı, çünkü bölümün
tasarımı seni yanıltır.

Yeni bir sahne: `Assets/Scenes/TestOdasi.unity`

İçine şunları koy:

```
40 birim düz zemin  (y = 0)

Basamaklar (soldan sağa, giderek yükselen):
   1.0 birim      x = 6
   2.0 birim      x = 10
   3.0 birim      x = 14
   4.0 birim      x = 18   ← bunu geçememelisin

Boşluklar (giderek genişleyen):
   2 birim        x = 22
   3 birim        x = 27
   4 birim        x = 33
   5 birim        x = 40
   6 birim        x = 48   ← bunu geçememelisin

Tavan testi:
   3 birim yükseklikte tavan, x = 54   (kafa vurma kontrolü)

Dar koridor:
   1.5 birim genişlik, x = 60          (sıkışma kontrolü)
```

Bu sahne oyuna girmeyecek, sadece senin için. Build Settings'e ekleme.

**Hızlı yol:** `LevelBuilder.cs` içine test odası üreten bir menü komutu ekle —
`CreateGround()` yardımcısı zaten var.

- [ ] `TestOdasi.unity` oluşturuldu

### 2. Temel değerleri otur

Inspector'da `Player` → **Player Controller 2D**.

**Kural: bir seferde bir değer.** Beş değeri birden değiştirirsen hangisinin
işe yaradığını asla bilemezsin.

#### Önce yatay hareket

| Değer | Aralık | Ne değişir |
|---|---|---|
| `Move Speed` | 6 (ağır) – 11 (hızlı) | Koşma hızı |
| `Acceleration Time` | 0.02 (anında) – 0.2 (kaygan) | Hızlanma süresi |
| `Deceleration Time` | 0.02 – 0.15 | Durma keskinliği |
| `Air Control` | 0.5 (kısıtlı) – 1.0 (tam) | Havada yön değiştirme |

**Test:** Bir tuşa bas ve bırak. Karakter nerede duruyor? Beklediğin yerde mi?
Kayıyorsa `Deceleration Time` fazla; aniden duruyorsa çok düşük.

Referans noktaları:

| Tarz | Speed | Accel | Decel | Air |
|---|---|---|---|---|
| Hassas (Celeste tarzı) | 9 | 0.04 | 0.03 | 0.9 |
| Ağır (Castlevania tarzı) | 6 | 0.15 | 0.12 | 0.5 |
| Kaygan (buz hissi) | 10 | 0.25 | 0.35 | 0.8 |
| Klasik (Mario tarzı) | 8 | 0.10 | 0.08 | 0.75 |

#### Sonra zıplama

| Değer | Aralık | Ne değişir |
|---|---|---|
| `Jump Height` | 2.5 – 4.5 | Yükseklik (birim) |
| `Jump Apex Time` | 0.30 (sivri) – 0.50 (yumuşak) | Tepeye çıkma süresi |
| `Fall Gravity Multiplier` | 1.4 – 2.5 | Düşüş ağırlığı |
| `Jump Cut Multiplier` | 0.3 – 0.6 | Kısa basınca ne kadar kesilir |
| `Max Fall Speed` | 18 – 28 | Terminal hız |

**Test:** Zıpla ve gözünü kapat. Ne zaman yere değeceğini tahmin edebiliyor
musun? Edemiyorsan `Fall Gravity Multiplier` fazla yüksek olabilir.

- [ ] Yatay hareket oturdu
- [ ] Zıplama oturdu

### 3. Toleransları ayarla

| Değer | Aralık | Not |
|---|---|---|
| `Coyote Time` | 0.08 – 0.15 | 0.15'ten fazlası "havada zıplıyorum" hissi verir |
| `Jump Buffer Time` | 0.10 – 0.15 | 0.2'den fazlası kontrolü gecikmeli hissettirir |

Bu iki değer görünmez ama **en çok fark yaratan** ayarlardır. Sıfıra çekip
oyna, sonra geri al — farkı hemen hissedersin.

- [ ] Toleranslar ayarlandı, ikisi de kapatılıp test edildi

### 4. Sayıları ölç ve yaz — bu adımı atlama

Zıplama değerlerini kesinleştirdikten sonra **ölç**. Bu sayılar bölüm
tasarımının dilbilgisi olacak (Epic 04).

**Ölçme yöntemi — geçici debug script'i:**

```csharp
using UnityEngine;
using Platformer.Player;
using Platformer.Core;

/// Test odasina ekle, oyna, Console'a bak. Olcum bitince sil.
public class JumpMeasure : MonoBehaviour
{
    private PlayerController2D controller;
    private Rigidbody2D rb;

    private float startY, maxY, startX;
    private bool measuring;

    private void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Zipla basladi
        if (!measuring && !controller.IsGrounded)
        {
            measuring = true;
            startY = transform.position.y;
            startX = transform.position.x;
            maxY = startY;
        }

        if (measuring)
        {
            maxY = Mathf.Max(maxY, transform.position.y);

            // Yere indi
            if (controller.IsGrounded)
            {
                measuring = false;
                float height = maxY - startY;
                float distance = Mathf.Abs(transform.position.x - startX);
                Debug.Log($"Yukseklik: {height:F2} birim | Mesafe: {distance:F2} birim");
            }
        }
    }
}
```

Şunları ölç ve tabloya yaz:

| Ölçüm | Nasıl | Değerin |
|---|---|---|
| Maksimum zıplama yüksekliği | Dur, zıpla, tuşu basılı tut | ___ birim |
| Minimum zıplama yüksekliği | Dur, zıpla, tuşu hemen bırak | ___ birim |
| Yerinden zıplama mesafesi | Dur, zıpla, havada yön ver | ___ birim |
| **Koşarak zıplama mesafesi** | Tam hızda koş, zıpla | ___ birim |
| Tepeye çıkma süresi | Inspector'daki `Jump Apex Time` | ___ sn |
| Toplam havada kalma | Zıpla, yere değene kadar | ___ sn |

**En önemlisi "koşarak zıplama mesafesi".** Bölümlerindeki hiçbir boşluk
bundan geniş olmayacak.

- [ ] Altı ölçüm yapıldı ve `docs/AYARLAR.md` içine yazıldı

### 5. İmza mekaniğini ekle

Epic 01'de seçtiğin mekaniği şimdi ekle. Aşağıda en yaygın ikisinin
**çalışır kodu** var.

#### Seçenek A — Dash (havada ileri atılma)

`PlayerController2D.cs` içine ekle:

```csharp
[Header("Dash")]
[Tooltip("Dash sirasindaki hiz (birim/sn).")]
[SerializeField] private float dashSpeed = 20f;

[Tooltip("Dash ne kadar surer.")]
[SerializeField] private float dashDuration = 0.14f;

[Tooltip("Iki dash arasi bekleme.")]
[SerializeField] private float dashCooldown = 0.35f;

[Tooltip("Dash sonrasi hiz bu orana dusurulur (1 = hiz korunur).")]
[Range(0.2f, 1f)]
[SerializeField] private float dashEndSpeedMultiplier = 0.55f;

// --- Dash durumu ---
public bool IsDashing { get; private set; }
public System.Action OnDashed;

private float dashTimeLeft;
private float dashCooldownLeft;
private bool dashAvailable = true;
private Vector2 dashDirection;
```

`ReadInput()` içine, `UpdateFacing()` çağrısından önce:

```csharp
// Dash girisi - Shift veya sag fare
if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Mouse1))
    && dashAvailable && dashCooldownLeft <= 0f && !IsDashing)
{
    StartDash();
}
```

`UpdateTimers()` içine:

```csharp
if (dashCooldownLeft > 0f) dashCooldownLeft -= Time.deltaTime;
```

Yeni metotlar:

```csharp
private void StartDash()
{
    // Yon: giris varsa o yon, yoksa baktigi yon
    float inputX = HorizontalInput;
    float inputY = Input.GetAxisRaw("Vertical");

    if (Mathf.Abs(inputX) < 0.01f && Mathf.Abs(inputY) < 0.01f)
    {
        dashDirection = FacingRight ? Vector2.right : Vector2.left;
    }
    else
    {
        dashDirection = new Vector2(inputX, inputY).normalized;
    }

    IsDashing = true;
    dashTimeLeft = dashDuration;
    dashAvailable = false;
    dashCooldownLeft = dashCooldown;

    OnDashed?.Invoke();
}

private void UpdateDash()
{
    dashTimeLeft -= Time.fixedDeltaTime;

    if (dashTimeLeft <= 0f)
    {
        IsDashing = false;
        // Dash bitince hizi kes - yoksa firlamis gibi devam eder
        rb.SetVelocity(rb.GetVelocity() * dashEndSpeedMultiplier);
        return;
    }

    rb.SetVelocity(dashDirection * dashSpeed);
}
```

`FixedUpdate()`'i şöyle değiştir:

```csharp
private void FixedUpdate()
{
    if (frozen) return;

    CheckGround();

    // Dash sirasinda normal hareket ve yercekimi devre disi
    if (IsDashing)
    {
        UpdateDash();
        return;
    }

    ApplyHorizontalMovement();
    HandleJump();
    ApplyGravity();
}
```

`CheckGround()` içinde, yere inince dash hakkını yenile:

```csharp
if (IsGrounded && !wasGroundedLastFrame)
{
    jumpsLeft = extraJumps;
    IsJumping = false;
    dashAvailable = true;   // ← EKLE
    OnLanded?.Invoke();
}
```

**Ayarlama rehberi:**

| His | dashSpeed | dashDuration | cooldown |
|---|---|---|---|
| Kısa, keskin (Celeste) | 22 | 0.12 | 0.20 |
| Uzun, akıcı | 18 | 0.20 | 0.40 |
| Nadir, güçlü | 26 | 0.15 | 0.80 |

**Dikkat edilecekler:**
- Dash sırasında yerçekimi kapalı olmalı (yukarıdaki kod bunu yapıyor)
- Yere değince hak yenilensin (havada sonsuz dash olmasın)
- Dash'in görsel geri bildirimi olmalı (Epic 14 — iz, parçacık, kısa donma)

#### Seçenek B — Duvar sekmesi

```csharp
[Header("Duvar")]
[SerializeField] private LayerMask wallLayers = ~0;
[SerializeField] private float wallCheckDistance = 0.45f;
[SerializeField] private float wallSlideSpeed = 3f;
[SerializeField] private Vector2 wallJumpForce = new Vector2(12f, 16f);
[Tooltip("Duvar ziplamasi sonrasi yatay kontrolun kilitli kaldigi sure.")]
[SerializeField] private float wallJumpLockTime = 0.18f;

public bool IsTouchingWall { get; private set; }
public bool IsWallSliding { get; private set; }

private int wallDirection;
private float wallJumpLockLeft;
```

```csharp
private void CheckWall()
{
    Vector2 origin = transform.position;
    bool right = Physics2D.Raycast(origin, Vector2.right, wallCheckDistance, wallLayers);
    bool left  = Physics2D.Raycast(origin, Vector2.left,  wallCheckDistance, wallLayers);

    IsTouchingWall = right || left;
    wallDirection = right ? 1 : (left ? -1 : 0);

    // Duvara yapisik + havada + asagi gidiyorsa kayiyor
    IsWallSliding = IsTouchingWall
                    && !IsGrounded
                    && rb.GetVelocity().y < 0f
                    && Mathf.Abs(HorizontalInput) > 0.01f;

    if (IsWallSliding)
    {
        rb.SetVelocityY(Mathf.Max(rb.GetVelocity().y, -wallSlideSpeed));
    }
}

private void WallJump()
{
    rb.SetVelocity(new Vector2(-wallDirection * wallJumpForce.x, wallJumpForce.y));
    wallJumpLockLeft = wallJumpLockTime;
    IsJumping = true;
    jumpBufferCounter = 0f;
    OnJumped?.Invoke();
}
```

`HandleJump()` başına:

```csharp
if (jumpBufferCounter > 0f && IsTouchingWall && !IsGrounded)
{
    WallJump();
    return;
}
```

`ApplyHorizontalMovement()` başına (kilit süresince yatay girdiyi yoksay):

```csharp
if (wallJumpLockLeft > 0f)
{
    wallJumpLockLeft -= Time.fixedDeltaTime;
    return;
}
```

**Kilit süresi neden gerekli:** olmadan, duvardan zıplayıp hemen duvara doğru
tuşa basarsan aynı duvara geri yapışırsın ve yukarı tırmanmak bedavaya gelir.

- [ ] İmza mekaniği çalışıyor
- [ ] Test odasında denendi ve ayarlandı

### 6. Kontrol tepkiselliğini doğrula

Tuşa bastığın an ile ekranda bir şey olduğu an arasındaki gecikme
**3 kareyi (~50 ms) geçmemeli.**

Kontrol listesi:

- [ ] Girdi `Update`'te okunuyor, `FixedUpdate`'te değil ✓ (kod zaten öyle)
- [ ] Hareket, animasyon tetiklenmesini beklemiyor
- [ ] `Rigidbody2D` → `Interpolation` = **Interpolate** ✓ (kod ayarlıyor)
- [ ] `Rigidbody2D` → `Collision Detection` = **Continuous** ✓
- [ ] V-Sync açık (Project Settings → Quality) — takılma olmasın

**Ölçme:** `Time.frameCount` ile tuşa basılan kareyi ve hızın değiştiği
kareyi logla; fark 1–2 kare olmalı.

- [ ] Gecikme hissedilmiyor

### 7. Değerleri kaydet

Play modunda yapılan değişiklikler **kaybolur**. Ve bir gün her şeyi bozup
geri dönmek isteyeceksin.

`docs/AYARLAR.md` oluştur:

```markdown
# Karakter Ayarları — son güncelleme: [tarih]

## Hareket
| Değer | Ayar |
|---|---|
| Move Speed | |
| Acceleration Time | |
| Deceleration Time | |
| Air Control | |

## Zıplama
| Değer | Ayar |
|---|---|
| Jump Height | |
| Jump Apex Time | |
| Fall Gravity Multiplier | |
| Jump Cut Multiplier | |
| Max Fall Speed | |
| Extra Jumps | |

## Toleranslar
| Değer | Ayar |
|---|---|
| Coyote Time | |
| Jump Buffer Time | |

## İmza mekaniği: [ad]
| Değer | Ayar |
|---|---|

## ÖLÇÜMLER (bölüm tasarımı için)
| Ölçüm | Değer |
|---|---|
| Maks. zıplama yüksekliği | |
| Min. zıplama yüksekliği | |
| Koşarak zıplama mesafesi | |
| Havada kalma süresi | |
```

- [ ] `docs/AYARLAR.md` dolduruldu

---

## Kabul kriteri

- [ ] Test odasında 30 saniye gezmek sıkıcı değil
- [ ] Zıplama mesafesi tahmin edilebilir
- [ ] Kenardan düşerken hâlâ zıplayabiliyorsun (coyote time)
- [ ] Yere inmeden basılan zıpla kaybolmuyor (jump buffer)
- [ ] Tuşu kısa basınca alçak, uzun basınca yüksek zıplıyorsun
- [ ] İmza mekaniği çalışıyor ve keyifli
- [ ] Altı ölçüm tabloya yazıldı
- [ ] Ayarlar `docs/AYARLAR.md` içinde
- [ ] Kontrol gecikmesi hissedilmiyor

---

## Tuzaklar

**Hepsini aynı anda değiştirmek.** Bir değeri değiştir, oyna, karar ver.
Beşini birden değiştirirsen hangisinin işe yaradığını bilemezsin.

**Gerçekçilik peşinde koşmak.** Mario'nun zıplaması fiziksel olarak imkânsız
(havada yön değiştiriyor, düşerken hızlanıyor, tuşu bırakınca duruyor).
Gerçekçi değil, **iyi** hissettirmeli.

**Çok erken cila.** Animasyon ve parçacık olmadan da hissiyat oturmalı.
Yer tutucu kareyle oynadığında keyifli değilse, animasyon kurtarmaz.

**Başkasının sayılarını kopyalamak.** "Celeste'te şu değerler" diye almak
işe yaramaz — onların karakter boyu, kamera mesafesi, PPU'su ve bölüm ölçeği
farklı. Referans al, kopyalama.

**Ölçüm adımını atlamak.** Bölüm tasarımına ölçüsüz başlarsan, her boşluğu
deneme-yanılmayla ayarlarsın ve tutarsız zorluk elde edersin.

**Play modunda ayarlayıp kaydetmemek.** En sinir bozucu kayıp.

**`Time.deltaTime`'ı `FixedUpdate`'te kullanmak.** Orada `Time.fixedDeltaTime`
olmalı. Mevcut kod doğru yapıyor; eklerken dikkat et.

---

## v1'de yapma

- Duvar tırmanma + dash + kanca'nın üçü birden
- Koşma/yürüme ayrımı (Shift ile hızlanma) — imza mekaniğinle çakışır
- Eğimli zeminler (ciddi ek karmaşıklık, ayrı bir sistem)
- Su, buz, bataklık gibi yüzey çeşitleri
- Combo / zincirleme puan sistemi
- Çömelme, sürünme
- Çift yönlü dash (8 yön) — 2 yön yeterli

---

## Sonraki

[Epic 03 — Kamera](03-kamera.md)
