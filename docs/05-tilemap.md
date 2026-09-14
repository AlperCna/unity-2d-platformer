# Epic 05 — Tilemap ile Bölüm İnşası

**Amaç:** Elle tek tek platform yerleştirmeyi bırakıp, fırçayla boyar gibi
bölüm yapmaya geçmek.

**Ön koşul:** [Epic 04](04-seviye-tasarimi-prensipleri.md)

**Süre:** 4–5 gün (öğrenme dahil)

---

## Neden bu epic var

Şu an bölümler `LevelBuilder.cs` içinde koordinat yazarak veya Unity'de elle
sürükleyerek yapılıyor. 1 bölüm için idare eder, **12 bölüm için işkence**.

Tilemap ile 12 bölümü, elle 2 bölüm yapacağın sürede yaparsın. Daha önemlisi:
değiştirmesi kolay olduğu için **daha çok deneme yaparsın** — ve bu, daha iyi
bölüm demektir.

---

## Nasıl uygulandı — epic'ten beş sapma

Epic'in gerekçesi *"bölümler `LevelBuilder.cs` içinde koordinat yazarak veya
elle sürükleyerek yapılıyor"* diyordu. Epic yazıldığında doğruydu; Epic 04'te
`LevelCursor` geldi ve **doğrulama** yapmaya başladı (boşluk genişliği,
birleşik erişim, diken tuzağı).

Tilemap'e geçip fırçayla boyasaydık o doğrulamayı kaybederdik. Değiştirmedik,
**birleştirdik**: `LevelCursor` tasarım + doğrulama katmanı olarak kaldı, ama
artık tek tek sprite nesnesi yaratmak yerine Tilemap'e karo basıyor.

| # | Epic ne diyor | Ne yapıldı | Neden |
|---|---|---|---|
| 1 | "minimum 9 parçalık set" | **16 karo** | Bir karonun görünümünü 4 komşusu belirler → 2⁴ = 16. 9 karoyla 7 durum karşılıksız kalır ve epic'in *kendi tuzak tablosundaki* "Rule Tile yanlış karo seçiyor" satırı ortaya çıkar |
| 2 | Kuralları elle tanımla (~20 dk) | **Kurallar koddan üretiliyor** | Elle tanımlanan kural ile `LevelCursor`'ın hesabı ayrılabilir: aynı bölüm elle boyanınca başka, koddan üretilince başka görünür |
| 3 | "Hazards Tilemap'ine tek Hazard script'i" | **Dikenler prefab kaldı** | Tilemap collider'ı hücrenin tamamını kaplar; ayarlanmış collider görselin alt yarısını kaplıyor. Bu bilinçli bir his kararı (Epic 07/09) |
| 4 | "Boş sahne yap, Ctrl+D ile kopyala" | **Şablon kod** (`LevelScaffold.cs`) | Kopyalanan sahne kayar: birinde kamera ayarı düzeltilir, diğerinde unutulur, kimse farketmez |
| 5 | Filter Mode Point → çizgi sorunu çözülür | **Atlasa dolgu da gerekti** | Point tek başına yetmiyor; taşma filtrelemeden değil texel sınırından geliyor |

### Atlas sızıntısı — epic'in tuzak tablosu eksikti

Kurulumdan sonra zeminde yeşil lekeler ve dikey çizgiler çıktı. Epic'in
çözümü ("Filter Mode → Point, Compression → None") zaten uygulanmıştı.

Üretilen PNG incelendi; sanat temizdi. Sorun atlas yerleşimindeydi:

```
mask 0  (iç karo, hep toprak)  son sütun  x=31
mask 1  (yüzey karosu)         ilk sütun  x=32   <- ÇİMEN
```

İkisi piksel komşusu. Kamera tam sayı piksele oturmadığı anda örnekleme bir
texel taşıyor ve toprak karonun sağ kenarında yeşil çıkıyor.

**Çözüm:** her karonun çevresine 2 piksel dolgu, içine karonun kendi kenar
pikselleri kopyalanıyor (extrude). Sayfa 128×128 → 144×144.

**Doğrulama:** her karonun sanat kenarının hemen dışındaki piksel ile kenar
pikselinin farklı olduğu yer sayısı = **0**.

### Izgaraya geçmenin bedeli ve telafisi

Tilemap tam sayı hücrelere basar. Bölüm 1 v2'nin ölçüleri kesirliydi
(2,5 / 2,8 / 3,2 / 4,4). Sadece boşluk genişliğiyle dört kademe kalıyor:
%38 (2 hücre), %56 (3), %75 (4), %94 (5). v2'nin dalgası bu dörde sığmıyor.

**Telafi: zorluğu boşluk + yükseliş birleşiminden almak.** Epic 04'te yazılan
`MaxDistanceAtHeight()` sayesinde dokuz kademe çıkıyor:

| boşluk | yükseliş | limit | zorluk |
|---|---|---|---|
| 2 | 0 | 5,31 | %38 |
| 2 | 1 | 4,91 | %41 |
| 2 | 2 | 4,38 | %46 |
| 3 | 0 | 5,31 | %56 |
| 3 | 1 | 4,91 | %61 |
| **3** | **2** | **4,38** | **%68** |
| 4 | 0 | 5,31 | %75 |
| **4** | **1** | **4,91** | **%82** |
| 5 | 0 | 5,31 | %94 |

Sonuç — eğri neredeyse birebir korundu:

```
v2 (kesirli)  47 → 53 → 56 → 49 → 68 → 68 → 83
v3 (ızgara)   38 → 56 → 56 → 38 → 68 → 75 → 82
```

### Piksel yoğunluğu neden 1 birim = 1 hücre

Yarım birimlik hücre daha ince ayar verirdi ama karoların 16×16 piksel
olmasını gerektirirdi. Karakter 32 piksel/birim; karolar 64 olurdu ve
karakterden daha yüksek çözünürlüklü görünürlerdi. Tutarlılık ince ayardan
önemli — hele ince ayarın telafisi varken.

---

## Görevler

### 1. 2D Tilemap Extras paketini kur

`Window → Package Manager → Unity Registry` → **2D Tilemap Extras** → Install

Bu paket Rule Tile'ı getiriyor. Onsuz her köşe karosunu elle seçmen gerekir.

- [x] Paket kuruldu — `com.unity.2d.tilemap.extras` **4.1.1** (4.2+ Unity 6000.1 istiyor, bizde 6000.0)

### 2. Tile set hazırla

Minimum 9 parçalık set — köşeler, kenarlar, orta:

```
┌───┬───┬───┐
│ ↖ │ ↑ │ ↗ │   sol-üst, üst, sağ-üst
├───┼───┼───┤
│ ← │ ■ │ → │   sol, orta (dolu), sağ
├───┼───┼───┤
│ ↙ │ ↓ │ ↘ │   sol-alt, alt, sağ-alt
└───┴───┴───┘
```

Pratikte 13–16 parça daha iyi olur (tek başına duran, yatay uç, dikey uç,
tek karo). Ama 9 ile başla.

`Assets/Art/tileset.png` — 32×32'lik karelerden oluşan ızgara.
9 parça için 96×96 piksel (3×3).

**Import ayarları:**

| Ayar | Değer |
|---|---|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | **Multiple** |
| Pixels Per Unit | **32** |
| Filter Mode | **Point (no filter)** |
| Compression | None |
| Mesh Type | Full Rect |

Sonra **Sprite Editor** → `Slice` → Type: **Grid By Cell Size** → 32×32 → Slice → Apply

- [x] Tile set hazır ve dilimlenmiş — `TilesetFactory.cs`, 16 karo + dolgu

### 3. Tilemap kur

`GameObject → 2D Object → Tilemap → Rectangular`

Bu üç şey oluşturur: **Grid** (üst nesne), **Tilemap**, **Tilemap Renderer**.

Zemin Tilemap'ine şu bileşenleri ekle:

| Bileşen | Ayar |
|---|---|
| `Tilemap Collider 2D` | **Used By Composite** ✔ |
| `Composite Collider 2D` | Geometry Type: Outlines |
| `Rigidbody2D` | Body Type: **Static** |

**Composite Collider neden şart:** olmadan her karo ayrı bir collider olur ve
karakter karo birleşim yerlerindeki görünmez köşelere takılır. Bu, Tilemap'le
yaşanan **bir numaralı sorundur** ve insanlar genelde "fizik bozuk" sanır.

Tilemap nesnesinin **Layer**'ını `Ground` yap — `PlayerController2D`'nin
zemin algılaması buna bakıyor.

- [x] Tilemap kuruldu, layer `Ground` — `TilemapRig.cs`
- [x] Composite Collider çalışıyor, takılma yok — oynanarak doğrulandı

### 4. Rule Tile oluştur

`Assets → Create → 2D → Tiles → Rule Tile`

Rule Tile, hangi karonun nereye geleceğine **kendisi karar verir**. Sen sadece
şekli çizersin; köşeleri ve kenarları otomatik seçer.

Kural tanımlama mantığı:

```
Her kural için 3×3 ızgarada komşuları işaretlersin:
  ✔ = burada karo OLMALI
  ✘ = burada karo OLMAMALI
  (boş) = fark etmez

Örnek — "üst kenar" karosu:
  ┌───┬───┬───┐
  │   │ ✘ │   │   üstte karo yok
  ├───┼───┼───┤
  │ ✔ │   │ ✔ │   sağ ve solda var
  ├───┼───┼───┤
  │   │ ✔ │   │   altta var
  └───┴───┴───┘
```

9 kural tanımlaman ~20 dakika sürer, sonra ömür boyu vakit kazandırır.

**Kısayol:** Tilemap Extras içinde hazır örnek Rule Tile'lar var
(`Packages/2D Tilemap Extras/Samples`), birini kopyalayıp sprite'larını
değiştirebilirsin.

- [x] Rule Tile hazır — `RuleTileFactory.cs`, 16 kural koddan üretiliyor

### 5. Katmanları ayır

Birden fazla Tilemap kullan, her biri ayrı işe:

| Tilemap | Collider | Layer | Sorting Order | İçerik |
|---|---|---|---|---|
| `Background` | ✘ | Default | −10 | Duvar dokusu, dekor |
| `Ground` | ✔ Composite | **Ground** | 0 | Basılabilen her şey |
| `Platforms_OneWay` | ✔ + Effector | **Ground** | 0 | Tek yönlü platformlar |
| `Decoration` | ✘ | Default | 5 | Ot, taş, ön plan detayı |
| `Hazards` | ✔ Trigger | Default | 1 | Diken vb. |

**`Hazards` Tilemap'ine tek bir `Hazard` script'i ekle** — tüm dikenler tek
seferde çalışır, her dikeni ayrı nesne yapmana gerek kalmaz.

`Hazard.cs` bir `Collider2D` bekliyor; `Tilemap Collider 2D` + `Is Trigger`
işaretli olması yeterli.

- [x] Katmanlar ayrıldı ve doğru yapılandırıldı

### 6. Tile Palette ile çiz

`Window → 2D → Tile Palette`

Yeni palet: `Create New Palette` → adını ver → `Assets/Art/Palettes/` altına kaydet.
Tile set'ini palete sürükle.

**Kısayollar** — öğren, çok vakit kazandırır:

| Tuş | Araç |
|---|---|
| `B` | Fırça |
| `E` | Silgi (veya Shift + fırça) |
| `U` | Kutu doldurma (dikdörtgen) |
| `I` | Renk seçici — sahnedeki karoyu seç |
| `G` | Kova (bağlı alanı doldur) |
| `S` | Seçim |

- [x] Bölüm 1 Tilemap ile yeniden yapıldı — 416 karo, ızgara tasarımı (v3)

### 7. Nesneleri prefab'e çevir

Tilemap zemin içindir. Düşman, para, checkpoint gibi şeyler ayrı nesne kalır.
Hepsini **prefab** yap.

`Assets/Prefabs/` altına:
- `Coin`
- `Enemy_Patroller`
- `Spikes`
- `Checkpoint`
- `MovingPlatform_H` / `MovingPlatform_V`
- `LevelGoal`
- `Player`

Sahnedeki bir nesneyi Project panelindeki `Prefabs` klasörüne sürükle.

**Neden kritik:** paranın rengini değiştirmek istersen, 300 paranın hepsini
tek tek değil, prefab'i düzenlersin. Prefab yapmadan 200 nesne yerleştirirsen
sonradan dönüştürmek çok daha zordur.

- [x] Tüm tekrar eden nesneler prefab — `PrefabFactory.cs`

### 8. Bölüm şablonu oluştur

Her bölümde olması gerekenleri içeren boş bir sahne:

```
Level_Template
├── Grid
│   ├── Background        (Tilemap, collider yok)
│   ├── Ground            (Tilemap + Composite, layer: Ground)
│   ├── Platforms_OneWay  (Tilemap + Effector)
│   ├── Hazards           (Tilemap + trigger + Hazard script)
│   └── Decoration        (Tilemap, collider yok)
├── Entities              (boş — düşman/para/platform buraya)
├── Background_Parallax
│   ├── Hills_Far         (ParallaxLayer 0.2)
│   └── Hills_Near        (ParallaxLayer 0.5)
├── Player                (prefab)
├── Main Camera           (CameraFollow)
├── GameManager
└── HUD Canvas
```

Yeni bölüm açarken bu sahneyi `Ctrl+D` ile kopyala, adını değiştir.

- [x] Şablon hazır — kod olarak `LevelScaffold.cs`; elle boyamak için `Tools > Bos Bolum Sahnesi`

---

## Kabul kriteri

- [x] Bir bölüm iskeleti 10 dakikada çıkıyor — *fırçayla değil, ~60 satır
      okunabilir kodla; üstelik cetvele karşı doğrulanarak*
- [x] Karakter karo birleşim yerlerine takılmıyor — **oynanarak doğrulandı**
- [x] Rule Tile 16 kuralla üretiliyor ve kurallar birbirini dışlıyor
- [ ] Rule Tile elle boyarken doğru karoyu seçiyor — *`Tools > Bos Bolum
      Sahnesi` ile denenecek; oyun bu yolu kullanmadığı için bloke değil*
- [~] ~~Tehlikeler ayrı Tilemap'te ve tek script'le çalışıyor~~ — **bilinçli
      olarak yapılmadı.** Tilemap collider'ı hücrenin tamamını kaplıyor;
      dikenin ayarlanmış collider'ı görselin alt yarısını kaplıyor ve bu bir
      oyun hissi kararı. Katman kuruldu, hücre boyu collider'ın sorun olmadığı
      tehlikeler (lav, su) için hazır
- [x] Tüm tekrar eden nesneler prefab *(düşman/hareketli platform Epic 06–07'de,
      gerçekten yerleştirildiklerinde)*
- [x] Bölüm şablonu var — `LevelScaffold.cs`. Yeni bölüm = `Create()` +
      tasarım + `Finish()`. Sahne kopyalamak yerine kod paylaşıldığı için
      bölümler arasında kayma olamaz

---

## Tuzaklar ve çözümleri

| Belirti | Sebep | Çözüm |
|---|---|---|
| Karakter görünmez engellere takılıyor | Composite Collider yok | `Used By Composite` ✔ + Composite ekle |
| Tilemap aşağı düşüyor | Rigidbody2D Dynamic | Body Type → **Static** |
| Karakter zeminde zıplayamıyor | Tilemap layer'ı yanlış | Layer → **Ground** |
| Karolar arasında ince çizgiler | Texture bleeding | Filter Mode → Point, Compression → None **+ atlasa dolgu** |
| Karonun kenarında komşu karonun rengi | Atlasta karolar bitişik | Her karonun çevresine 2px dolgu, kenar pikselleri kopyalanır |
| Rule Tile yanlış karo seçiyor | Kural sırası | Kurallar yukarıdan aşağı kontrol edilir; özel kuralları üste al |
| Tehlike ile zemin ayrılamıyor | Hepsi tek Tilemap'te | Ayrı Tilemap'lere böl |
| Tek yönlü platform çalışmıyor | Effector ayarı | `Platform Effector 2D` + Collider'da `Used By Effector` ✔ |

**Prefab yapmayı ertelemek** de bir tuzak: 200 nesne yerleştirdikten sonra
prefab'e çevirmek, baştan yapmaktan çok daha zordur.

---

## v1'de yapma

- Isometric veya hexagonal tilemap
- Animasyonlu karolar
- Prosedürel tilemap üretimi
- Oyun içi seviye editörü
- Yıkılabilir/değişebilen zemin (Tilemap'te karo silme runtime'da pahalı)
- Otomatik bölüm doğrulama araçları

---

## Sonraki

[Epic 06 — Düşmanlar](06-dusmanlar.md)
