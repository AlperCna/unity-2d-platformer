# 2D Platform Oyunu — Unity Başlangıç Projesi

Çalışır durumda bir 2D platform oyunu iskeleti. Unity'yi kurup projeyi açtıktan
sonra **tek menü tıklamasıyla** oynanabilir bir bölüm üretiliyor: karakter,
düşmanlar, hareketli platformlar, dikenler, paralar, checkpoint'ler ve bitiş bayrağı.

Dışarıdan tek bir asset indirmene gerek yok — grafikler kod ile üretiliyor.

---

## Başlangıç

**Gereken:** Unity **6000.0.83f1** (Unity 6 LTS). Proje bu sürüme sabitlenmiş
(`ProjectSettings/ProjectVersion.txt`). Farklı bir sürümün varsa Unity Hub
"farklı sürümle aç" diye sorar — sorun değil.

1. Depoyu klonla
2. Unity Hub → **Add** → **Add project from disk** → bu klasör
3. Aç (ilk açılış birkaç dakika sürer, `Library/` oluşturuluyor)
4. `Assets/Scenes/Level01.unity` sahnesine çift tıkla
5. **Play**

Sahne depoda hazır geliyor — 84 nesne, oynanabilir durumda.

> Sürüm eşleme için `setup.ps1` çalıştırabilirsin: kurulu en yeni Unity'yi
> bulup `ProjectVersion.txt`'yi ona göre günceller.

### Doğrulandı

| | |
|---|---|
| Derlendiği sürüm | Unity 6000.0.83f1 |
| Derleme sonucu | 0 hata, 0 uyarı |
| Örnek bölüm | 84 nesne, batch modda üretildi ve test edildi |
| Harici bağımlılık | Yok — sprite'lar kod ile üretiliyor |

### Bölümü yeniden üretmek

Sahneyi bozarsan veya sıfırdan başlamak istersen:

**Tools → 2D Platformer → Ornek Bolumu Olustur**

Bu işlem `Ground`/`Player` layer'larını oluşturur, grafikleri `Assets/Art/`
altına üretir ve bölümü kurup `Level01.unity` olarak kaydeder. **Mevcut
sahnenin üzerine yazar.**

### Komut satırından

Editor'ü hiç açmadan, terminalden de çalıştırılabilir:

```bash
unity open .
```

Bölümü baştan kurmak için (Editor kapalıyken):

```bash
"C:\Program Files\Unity\Hub\Editor\6000.0.83f1\Editor\Unity.exe" -batchmode -quit -projectPath . -executeMethod Platformer.EditorTools.LevelBuilder.BuildEverythingBatch -logFile build.log
```

`BuildEverythingBatch`, menü komutunun diyalogsuz sürümü — batch modda
`EditorUtility.DisplayDialog` çalışmadığı için ayrı bir giriş noktası gerekti.

---

## Kontroller

| Tuş | İşlev |
|---|---|
| `A` / `D` veya `←` / `→` | Hareket |
| `Space` | Zıpla (basılı tutarsan daha yükseğe) |
| `R` | Bölümü yeniden başlat |

Düşmanların **üstüne** basarsan ölürler ve seni zıplatırlar. Yandan
dokunursan sen ölürsün.

---

## Projede ne var

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs            Skor, can, checkpoint, respawn
│   │   └── Rigidbody2DExtensions.cs  Unity 2022 / Unity 6 uyumluluğu
│   ├── Player/
│   │   ├── PlayerController2D.cs     Hareket ve zıplama (asıl önemli dosya)
│   │   └── PlayerHealth.cs           Ölüm ve yeniden doğma
│   ├── Camera/
│   │   └── CameraFollow.cs           Ölü bölge + ileri bakış + sınırlar
│   ├── Gameplay/
│   │   ├── Coin.cs                   Toplanabilir para
│   │   ├── Hazard.cs                 Dikenler, lav vb.
│   │   ├── Patroller.cs              Devriye düşmanı, üstüne basılabilir
│   │   ├── MovingPlatform.cs         Yolcu taşıyan hareketli platform
│   │   ├── Checkpoint.cs             Ara kayıt noktası
│   │   ├── LevelGoal.cs              Bölüm sonu bayrağı
│   │   └── ParallaxLayer.cs          Arka plan derinliği
│   └── UI/
│       └── HudController.cs          Skor / can göstergesi
└── Editor/
    ├── SpriteFactory.cs              Grafikleri kod ile üretir
    └── LevelBuilder.cs               Örnek bölümü kurar (Tools menüsü)
```

---

## Oyunun hissiyatını ayarlamak

Platform oyununda her şey "hissiyat"tan ibaret. `Player` nesnesini seçip
Inspector'daki **Player Controller 2D** bileşeninde oynayacağın değerler:

| Değer | Ne yapar | Denemeye değer |
|---|---|---|
| `Move Speed` | Koşma hızı | 6 (ağır) – 11 (hızlı) |
| `Acceleration Time` | Hızlanma süresi | 0.02 (keskin) – 0.2 (kaygan) |
| `Jump Height` | Zıplama yüksekliği | 2.5 – 4.5 |
| `Jump Apex Time` | Tepeye çıkma süresi | 0.3 (sivri) – 0.5 (yumuşak) |
| `Fall Gravity Multiplier` | Düşüş ağırlığı | 1.5 – 2.5 |
| `Coyote Time` | Kenardan sonra zıplama toleransı | 0.08 – 0.15 |
| `Extra Jumps` | Çift zıplama için `1` yap | 0 veya 1 |

Zıplama yüksekliği ve süresi **doğrudan** ayarlanıyor — yerçekimi ve zıplama
hızı bunlardan hesaplanıyor. Yani "3 birim yüksek, 0.38 saniyede" diyebiliyorsun;
sihirli sayılarla uğraşmıyorsun.

Play modundayken de değiştirebilirsin, ama **Play'den çıkınca değişiklikler
kaybolur** — beğendiğin değerleri not al.

---

## Kendi bölümünü yapmak

`LevelBuilder.cs` içindeki `CreateLevelGeometry()` metodu bölümü tarif ediyor.
Orada satır ekleyip menüyü tekrar çalıştırabilirsin:

```csharp
CreateGround("Zemin", new Vector2(merkezX, merkezY), new Vector2(genişlik, kalınlık), parent);
CreateCoin(new Vector2(x, y), parent);
CreateEnemy(new Vector2(x, y), sağaBakıyorMu, parent);
CreateSpikes("Dikenler", new Vector2(x, y), kaçTane, parent);
CreateMovingPlatform("Platform", başlangıç, boyut, gideceğiMesafe, hız, parent);
```

Ama asıl yöntem bu değil — **Unity arayüzünde elle yapmak** çok daha hızlı.
Sahnedeki bir nesneyi `Ctrl+D` ile kopyalayıp sürükle. `LevelBuilder` sadece
sana bir başlangıç noktası veriyor.

Bölümü büyütürsen `Main Camera` → **Camera Follow** → `Max Bounds` değerini de
büyütmeyi unutma, yoksa kamera bölümün sonunu göstermez.

---

## Kendi grafiklerini koymak

`Assets/Art/` altındaki PNG'leri kendi çizimlerinle değiştir — dosya adları aynı
kalsın, sahnedeki referanslar bozulmaz. Import ayarları:

- **Texture Type:** Sprite (2D and UI)
- **Pixels Per Unit:** 32 (bu projedeki ölçek)
- **Filter Mode:** Point (pixel-art için; yumuşak çizimlerde Bilinear)
- **Mesh Type:** Full Rect (platformların Tiled modu için şart)

> `Tools → 2D Platformer → Sadece Grafikleri Uret` menüsü bu PNG'lerin
> **üzerine yazar**. Kendi çizimlerini koyduktan sonra o menüyü çalıştırma.

---

## Sık karşılaşılan sorunlar

**"InvalidOperationException: You are trying to read Input using the UnityEngine.Input class..."**
Active Input Handling yanlış. `Edit → Project Settings → Player → Active Input
Handling` → **Both** yap, Unity'yi yeniden başlat.

**Karakter zemine düşüp duruyor / zıplayamıyor**
`Player` nesnesinin **Ground Layers** maskesinde `Ground` işaretli mi, ve zemin
nesnelerinin layer'ı `Ground` mu? Player'ı seçince Scene view'da ayaklarının
altındaki kutu görünür — yere değdiğinde yeşile döner.

**Karakter duvarlara yapışıyor**
Collider'ında **NoFriction** fizik materyali yok. `Assets/Art/NoFriction.physicsMaterial2D`
dosyasını Capsule Collider 2D'nin `Material` alanına sürükle.

**Hareketli platform karakteri taşımıyor**
Platformun **Passenger Layers** maskesinde `Player` işaretli olmalı.

**Konsolda "SpriteFactory: ... bulunamadi" hatası**
Önce `Tools → 2D Platformer → Sadece Grafikleri Uret` çalıştır.

---

## Tam yol haritası

Bu iskeleti **bitmiş, yayınlanmış bir oyuna** dönüştürmenin planı
[`docs/`](docs/00-YOL-HARITASI.md) altında — 20 epic, 4 kilometre taşı.

Oradan başla: [docs/00-YOL-HARITASI.md](docs/00-YOL-HARITASI.md)

---

## Hızlı sonraki adımlar

Plana girmeden önce küçük şeylerle oynamak istersen:

1. **Ses** — `AudioSource` ekle, `PlayerController2D` içindeki `OnJumped` ve
   `OnLanded` olaylarına bağla (bu olaylar tam bunun için duruyor)
2. **Animasyon** — karakter sprite sheet'i + `Animator`; `IsGrounded`,
   `HorizontalInput`, `Velocity` zaten dışarıya açık
3. **Tilemap** — elle platform dizmek yerine `Window → 2D → Tile Palette`
4. **Menü sahnesi** — başlangıç ekranı ve bölüm seçimi
5. **Build** — `File → Build Settings → Windows → Build`
   Editor temel kurulumla geldi; Mono ile Windows .exe alabilirsin. IL2CPP
   (daha hızlı, daha küçük çıktı) istersen Hub → Installs → sürümün yanındaki
   dişli → **Add modules** → *Windows Build Support (IL2CPP)*.

---

## Notlar

Bu proje `Library/`, `Temp/`, `obj/` klasörleri olmadan geliyor — Unity ilk
açılışta bunları kendisi üretir, birkaç dakika sürer. `.gitignore` zaten bu
klasörleri dışlıyor, projeyi doğrudan git'e ekleyebilirsin.

`setup.ps1` isteğe bağlıdır: kurulu Unity sürümlerini tarar, en yenisini bulur
ve `ProjectSettings/ProjectVersion.txt` dosyasını ona göre günceller. Projeyi
farklı bir Unity sürümüyle açmak istediğinde işini kolaylaştırır.

```bash
powershell -ExecutionPolicy Bypass -File setup.ps1
```
