# Epic 03 — Kamera

> ✅ **TAMAMLANDI — 14 Eylül 2026.** Ayarlar: [AYARLAR.md](AYARLAR.md#kamera-ayarları)
> Dikey takip "son yere değilen yükseklik" mantığına çevrildi. Sarsıntı
> altyapısı kuruldu ve hit stop ile birlikte test edildi. Sınır hesaplama
> aracı ölüm çizgisini ve zıplama yüksekliğini hesaba katıyor.

**Amaç:** Oyuncunun gitmek istediği yeri görebilmesi. İyi kamera fark edilmez;
kötü kamera oyunu bitirir.

**Ön koşul:** [Epic 02](02-karakter-hissiyati.md) — zıplama mesafeleri ölçülmüş olmalı

**Süre:** 2–3 gün

---

## Neden bu epic var

Oyuncular kameradan şikâyet etmez. "Kontroller kötü" derler. Çoğu zaman
kontrol iyidir, kamera yanlış yeri gösteriyordur.

Temel kural:

> **Oyuncu ineceği yeri, zıplamadan önce görmeli.**

Göremiyorsa ölüm haksızdır. Haksız ölüm, oyuncunun oyunu bırakma sebebi
listesinde bir numaradır.

---

## Elindeki altyapı

`Assets/Scripts/Camera/CameraFollow.cs`:

| Özellik | Ne yapar |
|---|---|
| Ölü bölge (dead zone) | Küçük hareketlerde kamera kımıldamaz |
| İleri bakış (look-ahead) | Koştuğun yöne kayar, önünü gösterir |
| Yumuşatma (`SmoothDamp`) | Ani sıçrama yok |
| Sınırlar (bounds) | Bölümün dışını göstermez |
| Gizmo çizimi | Scene view'da ölü bölge ve sınırlar görünür |

---

## Görevler

### 1. Ortografik boyutu belirle

`Main Camera` → `Size`. Bu değer, ekranın **yarı yüksekliğidir** (birim cinsinden).

| Size | Görünen yükseklik | 16:9'da genişlik |
|---|---|---|
| 5 | 10 birim | ~17.8 birim |
| 6 | 12 birim | ~21.3 birim |
| 7 | 14 birim | ~24.9 birim (mevcut) |
| 9 | 18 birim | ~32.0 birim |

**Nasıl karar verilir:** karakterin **2–3 zıplama yüksekliği** görünmeli.

Bu projede ölçülen maksimum zıplama yüksekliği **3,03 birim**:
`3,03 × 3 = 9,1` → Size ≈ 5 yeterli, ama düşüş mesafesi ve önünü görmek için
pay bırak → **7** seçildi (14 birim görüş = 4,6 × zıplama yüksekliği).

| Sorun | Belirti |
|---|---|
| Çok yakın (küçük Size) | Önünü göremezsin, klostrofobik, ani ölümler |
| Çok uzak (büyük Size) | Karakter küçücük nokta, hissiyat kaybolur, tehlikeler okunmaz |

**Pixel-perfect not:** pixel art'ta keskinlik istiyorsan Size'ı şu formülle seç:
`Size = (EkranYüksekliği / 2) / PPU`. 1080p ve PPU 32 için: `1080 / 2 / 32 = 16.875`.
Bu çok uzak olur — bunun yerine `com.unity.2d.pixel-perfect` paketindeki
Pixel Perfect Camera bileşenini kullan, o ölçeği kendisi yönetir.

- [ ] Size belirlendi, gerekçesi `docs/AYARLAR.md`'ye yazıldı

### 2. Dikey takip: ölü bölge değil, "son yere değilen yükseklik"

Yaygın tavsiye "dikey ölü bölgeyi büyüt"tür. **Bu projede denendi ve yetmedi** —
sebebini bilmek işine yarar.

Ölü bölge, kameranın kımıldamadığı bir bant. Zıplama yüksekliği 3,03 birim
olduğu için bandın en az 6 birim olması gerekirdi (karakter merkezden ±3,03
gidiyor). Kamera 14 birim görüyor, yani bandın **%43'ü**. O kadar büyük bir
bant da yeni bir yüksekliğe indiğinde kamerayı geç tepki verdiriyor.

**Uygulanan çözüm:** kamera karakterin anlık yüksekliğini değil, **en son yere
değdiği yüksekliği** takip eder. Normal zıplamada ekran hiç oynamaz — çünkü
zıplarken takip edilen değer hiç değişmez.

Üç istisna:

| Durum | Davranış | Neden |
|---|---|---|
| Yere değdi | Yeni yükseklik kaydedilir | Gerçekten yeni bir kata çıktın |
| Kayıtlı yüksekliğin **altına** hızlı düşüyor | Takip eder | Nereye düştüğünü görmen lazım |
| 4,5 birimden fazla uzaklaştı | Kuralı bozup takip eder | Karakter ekrandan çıkmasın |

İkinci satırdaki **"altına"** şartı kritik ve atlanması çok kolay: normal bir
zıplamanın inişinde de hız eşiği aşılır (düşüş yerçekimi 84 ile 0,1 saniyede
−9'u geçer). O şart olmasa her zıplamada kamera inişe eşlik eder ve önlemeye
çalıştığın zıpzıp hareketi geri gelir.

Yatayda ölü bölge hâlâ var ve işe yarıyor (1,6 birim) — orada zıplama gibi
büyük ve düzenli bir hareket yok.

- [ ] Zıplarken ekran sabit kalıyor
- [ ] Yeni bir kata çıkınca kamera yumuşakça takip ediyor

### 3. İleri bakışı ayarla

`Look Ahead Distance` — koşarken kameranın ileri kayma miktarı.

**Ölçüt:** tam hızda koşarken, önünde ineceğin platformu görebiliyor musun?

Hesap: ölçülen koşarak zıplama mesafesi **5,31**, ekran genişliği ~24,9 birim.
Karakteri merkezden 2,2 birim geriye almak önünde ~14,6 birim görüş bırakıyor —
dash'li en uzun zıplamayı (7,62) bile rahat kapsıyor.

| Değer | Bu projede | Aralık |
|---|---|---|
| `Look Ahead Distance` | **2,2** (mesafenin ~%41'i) | 2,0 – 3,0 |
| `Look Ahead Smooth Time` | **0,5** | 0,4 – 0,6 |

`Look Ahead Smooth Time` çok düşükse her yön değiştirdiğinde kamera savrulur.
0,3'ün altına inme.

- [ ] İleri bakış ayarlandı

### 4. Kamera ne kadar aşağı takip etmeli

Cevap: **ölüm çizgisine kadar.** Altında görülecek bir şey yok — oyuncu zaten öldü.

Ama bu sadece takip mantığıyla çözülmez; **sınırlar** da izin vermeli.
Bu projede tam olarak şu yaşandı:

```
kamera boyutu 7        → 14 birim yükseklik görüyor
alt sınır −6           → kamera merkezi −6 + 7 = +1'in altına inemiyor
sonuç                  → karakter boşluğa düşüyor, kamera yerinde kalıyor
```

Takip mantığı doğru çalışıyordu, `ClampToBounds` engelliyordu.

**Çözüm:** alt sınır `GameManager.killPlaneY`'ye kadar inmeli.
`CameraBoundsTool` artık bunu kendisi okuyor:

```csharp
if (TryGetKillPlane(out float killPlaneY))
{
    min.y = Mathf.Min(min.y, killPlaneY - 1f);
}
```

`Level01`'de ölüm çizgisi −12, alt sınır −13 oluyor, kamera merkezi −6'ya kadar
inebiliyor — zeminin 6 birim altını gösteriyor.

**Dikkat:** bölümün dikey aralığı kameranın gördüğünden darsa kamera dikeyde
hiç hareket edemez (kod ortalayıp sabitler). Test odasında bu yaşandı: içerik
7 birim, kamera 16 birim. Araç artık bu durumda uyarı veriyor.

- [ ] Düşerken aşağısı görünüyor
- [ ] Alt sınır ölüm çizgisini kapsıyor

### 5. Kamera sarsıntısı ekle

Epic 14'te (juice) kullanacaksın, ama altyapıyı şimdi kur.

**Uygulandı:** [`CameraFollow.Shake(duration, magnitude)`](../Assets/Scripts/Camera/CameraFollow.cs)

```csharp
var cam = UnityEngine.Camera.main?.GetComponent<CameraFollow>();
cam?.Shake(0.25f, 0.4f);
```

Üç tasarım kararı, üçü de gerekçeli:

**1. `Time.unscaledDeltaTime` kullanılıyor, `deltaTime` değil.**
Hit stop sırasında (`timeScale = 0`) sarsıntı donmamalı. Aksi halde
Epic 09'da ölüm anına sarsıntı bağladığında efekt hiç görünmez —
çünkü tam o anda oyun donuyor.

**2. Şiddetli sarsıntı zayıfı ezmez.** Aynı anda iki çağrı gelirse
büyük olan kazanır; küçük olan devam eden efekti kesmez.

**3. `ScreenShakeEnabled` statik anahtarı var.** Bazı oyuncular için
ekran sarsıntısı erişilebilirlik meselesi — Epic 15'te ayarlara bağlanacak.

```csharp
public static bool ScreenShakeEnabled = true;
```

**Test:** test odasında Play → **`K`** (sarsıntı) ve **`L`** (sarsıntı +
ekran donması). İkincisi kritik: donma sırasında sarsıntının devam ettiğini
doğrular. [`FeelTuner.cs`](../Assets/Scripts/DevTools/FeelTuner.cs) içinde.

- [ ] `Shake()` çalışıyor ve test edildi

### 6. Bölüm sınırlarını otomatik hesapla

Her bölüm için `Min Bounds` / `Max Bounds` elle ayarlamak sıkıcı ve
unutulmaya açık. Unutulursa kamera bölümün dışındaki boşluğu gösterir.

`Assets/Editor/CameraBoundsTool.cs`:

```csharp
using UnityEditor;
using UnityEngine;
using Platformer.CameraRig;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Sahnedeki tum Renderer'larin kapladigi alani olcup kamera sinirlarina yazar.
    /// Menu: Tools > 2D Platformer > Kamera Sinirlarini Hesapla
    /// </summary>
    public static class CameraBoundsTool
    {
        [MenuItem("Tools/2D Platformer/Kamera Sinirlarini Hesapla", false, 30)]
        public static void Calculate()
        {
            var follow = Object.FindAnyObjectByType<CameraFollow>();
            if (follow == null)
            {
                Debug.LogError("Sahnede CameraFollow bulunamadi.");
                return;
            }

            // Level altindaki her seyi kapsa; yoksa tum Renderer'lari
            GameObject levelRoot = GameObject.Find("Level");
            Renderer[] renderers = levelRoot != null
                ? levelRoot.GetComponentsInChildren<Renderer>()
                : Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);

            if (renderers.Length == 0)
            {
                Debug.LogError("Olculecek Renderer bulunamadi.");
                return;
            }

            Bounds total = renderers[0].bounds;
            foreach (Renderer r in renderers)
            {
                // Arka plan katmanlarini disla - cok genisler
                if (r.gameObject.name.StartsWith("Hills_")) continue;
                total.Encapsulate(r.bounds);
            }

            // Kenarlara biraz pay birak
            const float padding = 2f;

            var so = new SerializedObject(follow);
            so.FindProperty("useBounds").boolValue = true;
            so.FindProperty("minBounds").vector2Value =
                new Vector2(total.min.x - padding, total.min.y - padding);
            so.FindProperty("maxBounds").vector2Value =
                new Vector2(total.max.x + padding, total.max.y + padding);
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(follow);
            Debug.Log($"Kamera sinirlari guncellendi: " +
                      $"min({total.min.x - padding:F1}, {total.min.y - padding:F1}) " +
                      $"max({total.max.x + padding:F1}, {total.max.y + padding:F1})");
        }
    }
}
```

Her yeni bölümde bir kez çalıştır, iş biter.

- [ ] Otomatik sınır hesaplama çalışıyor
- [ ] Mevcut bölümün sınırları doğru

---

## Kabul kriteri

- [ ] Zıplarken ekran yukarı aşağı oynamıyor
- [ ] Tam hızda koşarken ineceğin yeri görebiliyorsun
- [ ] Aşağı düşerken nereye düştüğünü görebiliyorsun
- [ ] Bölümün kenarında boşluk/gökyüzü görünmüyor
- [ ] Yön değiştirirken kamera savrulmuyor
- [ ] `Shake()` çalışıyor ve `timeScale = 0`'da donmuyor
- [ ] Kamera hareketi fark edilmiyor (en iyi işaret)

---

## Tuzaklar

**Kamerayı karaktere tam kilitlemek.** En sık hata. Ölü bölge ve yumuşatma
olmadan kamera karakterin her titremesini kopyalar; oyun mide bulandırır.

**Dikey ölü bölgeyi unutmak.** Her zıplamada ekran zıplar. Amatörlüğün
en görünür işareti.

**Ölü bölgeyi çok büyük yapmak.** Karakter ekranın kenarına gelir, önünü göremez.

**Sınırları unutmak.** Bölüm sonunda kamera boşluğa bakar.

**Look-ahead'i çok hızlı yapmak.** Her yön değiştirişte ekran sallanır.

**Sarsıntıda `Time.deltaTime` kullanmak.** Hit stop sırasında donar,
efekt kaybolur. `unscaledDeltaTime` olmalı.

**Kamerayı `Update`'te hareket ettirmek.** `LateUpdate` olmalı — karakter
o kare hareketini bitirmiş olsun. Mevcut kod doğru yapıyor.

---

## v1'de yapma

- Cinemachine paketi — güçlü, ama öğrenme eğrisi var ve kendi kameran çalışıyor
- Bölge bazlı kamera geçişleri (odadan odaya farklı zoom)
- Sinematik kamera hareketleri, kamera rayları
- Hıza göre dinamik zoom
- Bölünmüş ekran
- Kamera ile bulmaca mekaniği

---

## Sonraki

[Epic 04 — Seviye tasarımı prensipleri](04-seviye-tasarimi-prensipleri.md)
