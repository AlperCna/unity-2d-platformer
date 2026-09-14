# Epic 19 — Performans

**Amaç:** Oyunun eski bilgisayarlarda da akıcı çalışması.

**Ön koşul:** [Epic 18](18-test-ve-hata-ayiklama.md)

**Süre:** 2–3 gün

---

## Neden bu epic var

Takılan bir oyun, yavaş bir oyundan daha kötüdür. Sabit 30 FPS oynanabilir;
60 FPS'te saniyede bir takılan oyun oynanamaz — çünkü takılma tam zıplama
anında gelirse oyuncu ölür ve bunu kendi hatası sanmaz.

Asıl hedef yüksek FPS değil, **düzgün FPS**.

---


## Önce bir uyarı

**2D platform oyununda muhtemelen performans sorunun olmayacak.** Senin oyunun
ekranda birkaç yüz sprite gösteriyor; modern bir bilgisayar bunu zorlanmadan
yapar.

Bu epic'i **"ölçmeden hiçbir şey optimize etme"** prensibiyle oku. Erken
optimizasyon, kodu okunmaz hale getirip hiçbir şey kazandırmaz.

Ama iki gerçek risk var, onlara bakacağız:
1. **Çöp toplayıcı (GC) takılmaları** — 60 FPS'te bile oyunu "takık" yapar
2. **Draw call patlaması** — çok sayıda sprite tek tek çizilirse

---

## Görevler

### 1. Önce ölç

`Window → Analysis → Profiler` (veya `Ctrl+7`)

Bakılacak metrikler:

| Metrik | Hedef | Nerede |
|---|---|---|
| FPS | Sabit 60, dalgalanma yok | Üst grafik |
| CPU (ms) | < 16 ms | CPU Usage |
| Draw Calls / Batches | < 100 | Rendering |
| **GC Alloc (kare başına)** | **0 B** | CPU Usage → GC Alloc sütunu |
| Bellek | Sabit, artmıyor | Memory |

**En önemli satır GC Alloc.** Kare başına sürekli tahsis varsa, çöp toplayıcı
düzenli olarak devreye girer ve her seferinde birkaç kare takılma yaratır.
Ortalama FPS 60 görünür ama oyun "takık" hissettirir.

**Deep Profile** ile hangi metodun ne kadar sürdüğünü görürsün (profiler'ı
yavaşlatır, sadece teşhis için aç).

**Build'de profil al:** `Development Build` + `Autoconnect Profiler` işaretle.
Editor'deki sayılar gerçeği yansıtmaz.

- [ ] Profiler ile ölçüm yapıldı, sayılar not edildi

### 2. GC tahsisini sıfırla

`Update`/`FixedUpdate` içinde kare başına tahsis yapan kalıplar:

| Kötü | İyi | Neden |
|---|---|---|
| `GetComponent<T>()` her karede | `Awake`'te cache'le | Arama maliyetli |
| `FindObjectOfType<T>()` | Referansı sakla | Çok pahalı |
| `Camera.main` her karede | Cache'le | İçeride `FindGameObjectWithTag` çağırır |
| `"Para: " + x` her karede | Değer değişince güncelle | String tahsisi |
| `Physics2D.OverlapBoxAll` | `OverlapBoxNonAlloc` | Dizi yeniden kullanılır |
| `foreach` bazı koleksiyonlarda | `for` döngüsü | Enumerator tahsisi |
| `new List<T>()` döngüde | Alanı yeniden kullan | Tahsis |
| `LINQ` (`.Where`, `.Select`) | Elle döngü | Çok tahsis üretir |

**Projendeki iyi örnek** — `MovingPlatform`:

```csharp
// Her karede yeni dizi olusturmak yerine tek diziyi yeniden kullaniyor
private readonly Collider2D[] passengerBuffer = new Collider2D[8];

int count = Physics2D.OverlapBoxNonAlloc(
    checkCenter, checkSize, 0f, passengerBuffer, passengerLayers);
```

**Kabul edilebilir örnek** — `HudController`:

```csharp
scoreText.text = $"Para: {score} / {total}";
```

Bu string tahsis eder, **ama her karede değil** — sadece skor değiştiğinde
olay tetiklendiğinde. Kare başına olmadığı sürece string tahsisi sorun değildir.

**Kural:** `Update` içinde tahsis = sorun. Olay içinde tahsis = sorun değil.

- [ ] Kare başına GC Alloc = 0

### 3. Nesne havuzu kullan

Epic 06'da `ObjectPool<T>` yazdın. Şunlarda kullan:

- Mermiler (Epic 06) ✔
- Parçacık efektleri (Epic 14) ✔
- Skor göstergesi yazıları
- Ses kaynakları (çok sayıda eşzamanlı ses varsa)

Unity 2021+ içinde hazır `UnityEngine.Pool.ObjectPool<T>` da var; kendi
yazdığın da iş görür.

- [ ] Sık oluşturulan her şey havuzdan geliyor

### 4. Draw call azalt

2D'de en büyük performans faktörü.

**Sprite Atlas** (Epic 11'de oluşturdun):
Aynı atlastan gelen sprite'lar tek draw call'da çizilir.

`Window → 2D → Sprite Atlas` → oluşturduğun atlası kontrol et:
- `Objects for Packing`: `Assets/Art` klasörü
- `Include in Build` ✔
- `Allow Rotation` ✘ (pixel art'ta sorun çıkarır)
- `Tight Packing` ✘

**Diğer ipuçları:**
- Gereksiz Sorting Layer yaratma — her katman batch'i böler
- Aynı sorting order'daki sprite'lar daha iyi gruplanır
- Tilemap zaten verimli (chunk halinde çizer)
- UI'da `Canvas` sayısını az tut; her Canvas ayrı batch

**Kontrol:** Profiler → Rendering → `SetPass Calls` ve `Batches`.
Atlas öncesi/sonrası farkı gör.

- [ ] Sprite Atlas çalışıyor, draw call düştü

### 5. Ekran dışını pasifleştir

Bölüm uzunsa, ekran dışındaki nesnelerin script'leri boşuna çalışır.

Unity sprite'ları **render** tarafında zaten culling yapıyor — sorun
script'lerde: 50 düşmanın hepsi her karede raycast atıyorsa israf.

```csharp
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Ekran disina cikinca belirtilen script'leri kapatir.
    /// Renderer gerektirir (OnBecameVisible/Invisible icin).
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class OffscreenDisabler : MonoBehaviour
    {
        [Tooltip("Ekran disinda kapatilacak bilesenler.")]
        [SerializeField] private MonoBehaviour[] componentsToDisable;

        [Tooltip("Ekran disinda fizik de dursun mu?")]
        [SerializeField] private bool disableRigidbody = true;

        private Rigidbody2D rb;

        private void Awake() => rb = GetComponent<Rigidbody2D>();

        private void OnBecameVisible() => SetActive(true);
        private void OnBecameInvisible() => SetActive(false);

        private void SetActive(bool active)
        {
            foreach (MonoBehaviour c in componentsToDisable)
            {
                if (c != null) c.enabled = active;
            }

            if (disableRigidbody && rb != null)
            {
                rb.simulated = active;
            }
        }
    }
}
```

**Dikkat:** `OnBecameInvisible` Scene view kamerasını da sayar — Editor'de
tuhaf davranabilir. Build'de sorun yok.

- [ ] Ekran dışı düşmanlar pasif

### 6. Fizik ayarları

`Edit → Project Settings → Physics 2D`

**Layer Collision Matrix:** çarpışmaması gereken layer çiftlerinin işaretini
kaldır:

| | Player | Ground | Enemy | Coin | Projectile |
|---|---|---|---|---|---|
| Player | ✘ | ✔ | ✔ | ✔ | ✔ |
| Ground | | ✘ | ✔ | ✘ | ✔ |
| Enemy | | | **✘** | ✘ | ✘ |
| Coin | | | | **✘** | ✘ |
| Projectile | | | | | **✘** |

Düşman-düşman, para-para, mermi-mermi çarpışması gereksiz ve maliyetli.

`Edit → Project Settings → Time`:
- `Fixed Timestep`: **0.02** (50 Hz) — varsayılan, 2D için yeterli
- Düşürme (0.01 vb.) — fizik 2 kat çalışır, kazancı yok

- [ ] Collision matrix temizlendi

### 7. Bellek sızıntısı kontrolü

30 dakika oyna, Profiler → Memory'ye bak. Bellek sürekli artıyorsa sızıntı var.

En yaygın sebepler:

| Sebep | Belirti | Çözüm |
|---|---|---|
| `OnDisable`'da abonelik bırakılmamış | Sahne değişiminde artış | `-=` ekle |
| `static` liste temizlenmiyor | Sürekli artış | Sahne başında temizle |
| `Destroy` edilen nesneye referans | Yavaş artış | Referansı `null`la |
| Havuz sınırsız büyüyor | Kademeli artış | Maksimum boyut koy |

Test: aynı bölümü 10 kez yeniden başlat, bellek başlangıçtaki seviyeye
dönüyor mu?

- [ ] 30 dakika oynayınca bellek artmıyor

### 8. Build boyutu

`Build Settings → Player Settings`:

| Ayar | Değer |
|---|---|
| Managed Stripping Level | **Medium** |
| Scripting Backend | Mono (hızlı build) veya IL2CPP (hızlı çalışma) |

Diğer:
- Kullanılmayan paketleri `manifest.json`'dan çıkar
- Ses: müzik Vorbis (%70), efektler ADPCM
- Texture: pixel art için compression **None** kalmalı (dosyalar zaten küçük)

Hedef: **100 MB altı.** 2D platformer için çok rahat ulaşılır.

Build sonrası `Editor.log` içinde boyut dökümü var — hangi asset'in ne kadar
yer kapladığını görebilirsin (`Build Report` diye ara).

- [ ] Build boyutu makul

---

## Kabul kriteri

- [ ] Sabit 60 FPS, takılma yok
- [ ] Kare başına GC Alloc = 0
- [ ] Draw call < 100
- [ ] 30 dakika oynayınca bellek artmıyor
- [ ] **Eski/zayıf bir bilgisayarda test edildi**
- [ ] Build boyutu 100 MB altı
- [ ] Collision matrix temizlendi

---

## Tuzaklar

**Ölçmeden optimize etmek.** Zamanının %90'ını yanlış yere harcarsın ve
kodu okunmaz hale getirirsin.

**`Update`'te `GetComponent`.** En yaygın performans hatası.

**`Camera.main` her karede.** İçeride `FindGameObjectWithTag` çağırır.
`Awake`'te cache'le.

**Sadece kendi bilgisayarında test etmek.** Seninki RTX 3060'lı; oyuncunun
entegre grafikli dizüstüsü olabilir.

**Editor'de profil alıp sonuca güvenmek.** Editor overhead'i gerçeği gizler.

**`Destroy` sonrası referans tutmak.** Bellek sızıntısı ve
`MissingReferenceException`.

**Bellek sızıntısını göz ardı etmek.** "Kapatıp açarlar" — 1 saat oynayan
oyuncu için oyun çöker.

**LINQ'i `Update` içinde kullanmak.** Her çağrıda tahsis üretir.

---

## v1'de yapma

- Burst / Jobs / ECS (2D platformer için tamamen gereksiz)
- Custom shader optimizasyonu
- LOD sistemi
- Asset Bundle / Addressables
- Çok iş parçacıklı özel sistemler
- GPU instancing
- Occlusion culling (2D'de anlamsız)

---

## Sonraki

[Epic 20 — Build ve yayınlama](20-build-ve-yayinlama.md)
