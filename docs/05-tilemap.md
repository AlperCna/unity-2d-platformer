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

## Görevler

### 1. 2D Tilemap Extras paketini kur

`Window → Package Manager → Unity Registry` → **2D Tilemap Extras** → Install

Bu paket Rule Tile'ı getiriyor. Onsuz her köşe karosunu elle seçmen gerekir.

- [ ] Paket kuruldu

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

- [ ] Tile set hazır ve dilimlenmiş

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

- [ ] Tilemap kuruldu, layer `Ground`
- [ ] Composite Collider çalışıyor, takılma yok

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

- [ ] Rule Tile hazır ve doğru karo seçiyor

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

- [ ] Katmanlar ayrıldı ve doğru yapılandırıldı

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

- [ ] Bölüm 1 Tilemap ile yeniden yapıldı

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

- [ ] Tüm tekrar eden nesneler prefab

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

- [ ] `Assets/Scenes/Level_Template.unity` hazır

---

## Kabul kriteri

- [ ] Tilemap ile 10 dakikada bir bölüm iskeleti çizebiliyorsun
- [ ] Karakter karo birleşim yerlerine takılmıyor
- [ ] Rule Tile köşeleri otomatik seçiyor
- [ ] Tehlikeler ayrı Tilemap'te ve tek script'le çalışıyor
- [ ] Tüm tekrar eden nesneler prefab
- [ ] Bölüm şablonu var ve kopyalanarak yeni bölüm açılabiliyor

---

## Tuzaklar ve çözümleri

| Belirti | Sebep | Çözüm |
|---|---|---|
| Karakter görünmez engellere takılıyor | Composite Collider yok | `Used By Composite` ✔ + Composite ekle |
| Tilemap aşağı düşüyor | Rigidbody2D Dynamic | Body Type → **Static** |
| Karakter zeminde zıplayamıyor | Tilemap layer'ı yanlış | Layer → **Ground** |
| Karolar arasında ince çizgiler | Texture bleeding | Filter Mode → Point, Compression → None |
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
