# Epic 11 — Sanat ve Sprite Üretimi

**Amaç:** Oyunun tutarlı bir görünüşü olması. Güzel olmasından **önce**
tutarlı olması.

**Ön koşul:** [Epic 05](05-tilemap.md)

**Süre:** 1–2 hafta (kendin çiziyorsan daha uzun)

---

## Neden bu epic var

Amatör görünen oyunların çoğu kötü çizildiği için değil, **tutarsız** olduğu
için amatör görünür: farklı piksel yoğunlukları, uyumsuz renkler, karışık
kontur kalınlıkları, farklı ışık yönleri.

> Vasat ama tutarlı sanat, güzel ama karmakarışık sanattan iyi görünür.

Çizim yeteneğin yoksa bile tutarlılık başarabilirsin — ve o yeterli.

---

## Elindeki altyapı

`Assets/Editor/SpriteFactory.cs` tüm grafikleri kod ile üretiyor:
`square`, `ground`, `player`, `coin`, `spike`, `enemy`, `checkpoint`, `goal`.

Palet dosyanın başında tanımlı ve iyi bir başlangıç noktası:

```csharp
Sky        #1F2B3E    HillFar    #2C3B52    HillNear   #3A506B
Grass      #5FBF77    GrassDark  #49A05F
Dirt       #6B4F3A    DirtDark   #543D2C
PlayerBody #F2B544    PlayerDark #D9952C
CoinBody   #FFD75E    CoinLight  #FFF0B0
SpikeBody  #E05C5C    SpikeDark  #B33F3F
EnemyBody  #9B5DE5    EnemyDark  #7B41C4
Metal      #8A94A6    Ink        #1A1A22
```

⚠️ **Uyarı:** `Tools → 2D Platformer → Sadece Grafikleri Uret` menüsü
`Assets/Art/` altındaki PNG'lerin **üzerine yazar.** Kendi çizimlerini
koyduktan sonra o menüyü bir daha çalıştırma.

---

## Görevler

### 1. Teknik standartları belirle ve yaz

Bunları bir kere kararlaştır, **hiç değiştirme**:

| Karar | Öneri | Neden |
|---|---|---|
| Pixels Per Unit | **32** | Projede zaten bu |
| Karakter boyu | 32×32 px = 1 birim | Ölçek referansı |
| Karo boyu | 32×32 px | PPU ile aynı |
| Filter Mode | **Point (no filter)** | Pixel art keskin kalsın |
| Compression | **None** | Küçük dosyalar zaten, bozulma olmasın |
| Mesh Type | **Full Rect** | Tiled çizim için şart |
| Pivot | Bottom (karakter), Center (diğer) | Zemin hizalaması |
| Palet | 16–24 renk | Tutarlılığın anahtarı |
| Kontur | Var **veya** yok — birini seç | Karışık olmasın |
| Işık yönü | Sol üst | Tüm gölgeler aynı yöne |

`docs/SANAT-REHBERI.md` diye yaz. Her yeni asset'te buraya bak.

- [ ] Standartlar yazıldı

### 2. Renk paleti seç

**Kendi paletini sıfırdan yapma.** Hazır paletler var ve çok iyiler:

| Kaynak | Not |
|---|---|
| [lospec.com/palette-list](https://lospec.com/palette-list) | Binlerce hazır palet, filtrelenebilir |
| *PICO-8* (16 renk) | Çok kısıtlı, çok tutarlı sonuç |
| *Endesga 32* | Genel amaçlı, çok popüler |
| *Sweetie 16* | Canlı, oyun dostu |
| *Apollo* (46 renk) | Daha geniş, doğa tonları güçlü |

Palet seçtikten sonra **sadece o renkleri kullan.** Bu tek kural bile
oyunu profesyonel gösterir.

**Rol dağılımı yap:**

| Rol | Özellik | Neden |
|---|---|---|
| Arka plan | Soğuk, koyu, düşük doygunluk | Geri çekilsin |
| Zemin | Orta ton, nötr | Tarafsız zemin |
| **Oyuncu** | **En parlak, en doygun** | Gözün ilk gittiği yer |
| Tehlike | Kırmızı/turuncu, yüksek doygunluk | Evrensel tehlike dili |
| Toplanabilir | Sarı/altın, parlak | Gözün ikinci gittiği yer |
| Düşman | Belirgin ama oyuncudan sönük | Fark edilsin, yarışmasın |

- [ ] Palet seçildi ve rolleri yazıldı

### 3. Üç okunabilirlik testi

Çizim yeteneğinden bağımsız, en önemli testler:

**Silüet testi** — tüm sprite'ları tamamen siyah yap. Hangisinin ne olduğunu
anlayabiliyor musun? Anlayamıyorsan şekilleri daha ayırt edici yap.

```
İyi silüetler:        Kötü silüetler:
  ▲   ●   ■             ●   ●   ●
(hepsi farklı)      (hepsi aynı, ayırt edilemez)
```

**Bulanıklaştırma testi** — oyunun ekran görüntüsünü al, bulanıklaştır.
Oyuncu, tehlike ve zemin hâlâ ayırt edilebiliyor mu?

**Gri tonlama testi** — renkleri kaldır. Kontrast yeterli mi? Bu aynı zamanda
renk körü oyuncular için erişilebilirlik testidir (erkeklerin ~%8'i).

- [ ] Üç test de geçildi

### 4. Karakteri çiz

En çok emek vereceğin asset. Oyuncu ona 10 saat bakacak.

| Kural | Neden |
|---|---|
| Silüeti ayırt edici olsun | Şapka, kulak, farklı oran |
| En parlak renk onda olsun | Gözün ilk gittiği yer |
| Yön belli olsun | Hangi tarafa baktığı anlaşılmalı |
| **Animasyon için basit olsun** | Detay çizersen 8 kare çizmek işkence |
| Gözler büyük ve net | Karaktere hayat verir |

**Beceri yoksa:** basit şekiller kullan. Bir yuvarlak + iki göz, kötü
çizilmiş detaylı bir karakterden **çok daha iyi** görünür.

Mevcut `SpriteFactory` karakteri tam bunu yapıyor: yuvarlatılmış dikdörtgen,
iki göz, ağız. Yer tutucu olarak fazlasıyla yeterli.

- [ ] Karakter sprite'ı hazır

### 5. Tile set çiz

Epic 05'teki 9 parçalık set + ekler:

- Tek başına duran platform (4 kenar da açık)
- Yatay uç parçaları (sol uç, sağ uç)
- Dikey uç parçaları
- 2–3 orta varyasyonu (tekrar hissi kırılsın)
- Dekoratif parçalar (ot, taş, çatlak)

**Kritik:** karolar kenarlarından **kusursuz** birleşmeli. Sprite Editor'da
1 piksel kayma bile ızgarada çirkin çizgiler oluşturur.

**Kontrol:** 5×5 alan boya, yakınlaştır, birleşim yerlerine bak.

- [ ] Tile set hazır ve kusursuz birleşiyor

### 6. Arka plan katmanları

`ParallaxLayer.cs` hazır. 3 katman yeterli:

| Katman | Parallax Factor | İçerik | Doygunluk |
|---|---|---|---|
| Uzak | 0.1–0.2 | Gökyüzü, dağ silüeti | Çok düşük |
| Orta | 0.4–0.5 | Tepeler, ağaç silüeti | Düşük |
| Yakın | 0.7–0.8 | Ön plan detayı | Orta |

**Arka plan soluk ve düşük kontrast olsun.** Oynanış katmanıyla yarışmasın.

En sık hata: güzel ama çok belirgin arka plan yüzünden platformların
seçilememesi. Arka plan güzel olmak zorunda değil — **geri çekilmek** zorunda.

Hızlı çözüm: arka plan renklerini gökyüzü rengine doğru %40 karıştır.
Otomatik olarak geri çekilir.

- [ ] 3 parallax katmanı hazır, hepsi geri çekilmiş

### 7. Sprite Atlas oluştur (performans)

Tüm sprite'ları tek atlasta toplamak draw call sayısını ciddi düşürür.

`Assets → Create → 2D → Sprite Atlas`
- `Objects for Packing`: `Assets/Art` klasörünü sürükle
- `Include in Build` ✔
- `Allow Rotation` ✘ (pixel art'ta sorun çıkarır)
- `Tight Packing` ✘

- [ ] Sprite Atlas oluşturuldu

### 8. Sanat kaynakları (çizmiyorsan)

Ücretsiz ve ticari kullanıma uygun:

| Kaynak | Lisans | Not |
|---|---|---|
| [kenney.nl/assets](https://kenney.nl/assets) | **CC0** (atıf bile gerekmez) | En güvenli, çok geniş |
| [itch.io/game-assets](https://itch.io/game-assets) | Değişken | "free" + "2D" filtrele, lisansı oku |
| [OpenGameArt.org](https://opengameart.org) | Değişken | Lisansa **dikkat** |

**Kural: tek bir paketten al.** Farklı sanatçıların işlerini karıştırmak
tutarsızlığın en hızlı yoludur.

Kullandığın her şeyi `docs/LISANSLAR.md` içine yaz:

```markdown
| Asset | Kaynak | Lisans | Atıf gerekli |
|---|---|---|---|
| Karakter sprite | Kenney Platformer Pack | CC0 | Hayır |
| Zıplama sesi | freesound.org/xxxx | CC-BY 3.0 | **Evet** |
```

- [ ] Kaynak seçildi, lisanslar `docs/LISANSLAR.md` içinde

---

## Kabul kriteri

- [ ] Tüm sprite'lar aynı PPU ve aynı piksel yoğunluğunda
- [ ] Tek bir palet kullanılıyor
- [ ] Silüet, bulanıklaştırma ve gri tonlama testleri geçiliyor
- [ ] Arka plan ön planla yarışmıyor
- [ ] Tile set kusursuz birleşiyor
- [ ] Sprite Atlas var
- [ ] Standartlar `docs/SANAT-REHBERI.md` içinde
- [ ] Lisanslar `docs/LISANSLAR.md` içinde

---

## Tuzaklar

**Farklı PPU'lar karıştırmak.** 16 PPU bir sprite ile 32 PPU bir sprite yan
yana geldiğinde piksel boyutları farklı olur, göz hemen yakalar.

**Filter Mode: Bilinear.** Pixel art bulanıklaşır. Her zaman Point.

**Çok belirgin arka plan.** Güzel olsun diye yapılır, oynanışı bozar.

**Farklı kaynaklardan asset karıştırmak.** Her sanatçının stili farklıdır.

**Sanatla başlamak.** Önce oyun iyi hissettirsin. Yer tutucu grafiklerle
oynanan oyun eğlenceli değilse, güzel grafik kurtarmaz.

**Sprite'ı çok detaylı çizmek.** Epic 12'de (animasyon) pişman olursun —
8 kare çizmen gerekecek.

**Lisansı okumadan asset kullanmak.** Yayınladıktan sonra telif talebi gelirse
oyun kaldırılır.

**Işık yönünü karıştırmak.** Bazı sprite'larda gölge solda, bazılarında sağda —
göz bunu yakalar ve "ucuz" hissettirir.

---

## v1'de yapma

- Normal map / dinamik ışıklandırma (2D Lights)
- İskelet animasyon (2D Animation paketi)
- Kıyafet/karakter özelleştirme
- Dinamik hava sistemleri (yağmur, kar)
- Shader efektleri (su yansıması, ısı dalgası)
- Çözünürlük bağımsız vektör sanat
- Mevsim/tema varyasyonları

---

## Sonraki

[Epic 12 — Animasyon](12-animasyon.md)
