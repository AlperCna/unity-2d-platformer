# Sanat Rehberi

> Bir kere kararlaştırıldı, **değiştirilmiyor.** Her yeni görsel buraya bakar.
>
> Epic: [11-sanat-ve-sprite.md](11-sanat-ve-sprite.md)
> Kod: [`Assets/Editor/SpriteFactory.cs`](../Assets/Editor/SpriteFactory.cs)
> Denetim: `Tools > 2D Platformer > Sanati Denetle`

---

## Neden bu dosya var

> *"Amatör görünen oyunların çoğu kötü çizildiği için değil, **tutarsız**
> olduğu için amatör görünür."*

Bu proje için özellikle önemli: bütün görseller kodla üretiliyor, yani
tutarlılık **zorunlu kılınabilir**. Elle çizilse "unutulur"; kodda
unutulmaz.

---

## Teknik standartlar

| Karar | Değer | Neden |
|---|---|---|
| Pixels Per Unit | **32** | Tüm görseller aynı yoğunlukta |
| Karakter | 32×32 px = 1 birim | Ölçek referansı |
| Karo | 32×32 px | PPU ile aynı |
| Filter Mode | **Point** | Pixel art keskin kalsın |
| Compression | **None** | Dosyalar zaten küçük |
| Mesh Type | **Full Rect** | Tiled çizim için şart |
| Işık yönü | **Sol üst** | Bütün gölgeler aynı yöne |
| Kontur | **Yok** | Seçildi; karışık olmasın |

Bu satırların hepsini `Sanati Denetle` kontrol ediyor. Elle takip
edilmiyor.

---

## Palet ve roller

Rol, rengin **nerede** kullanıldığını değil **ne iş yaptığını** söyler.

| Rol | Renk | Kural |
|---|---|---|
| Arka plan | `#1F2B3E` `#2C3B52` `#3A506B` | Soğuk, koyu, düşük doygunluk — **geri çekilsin** |
| Zemin | `#6B4F3A` `#543D2C` `#38291C` | Orta ton, nötr |
| Çimen | `#5FBF77` `#49A05F` | Yüzeyi işaretler, dikkat çekmez |
| **Oyuncu** | `#F2B544` `#D9952C` | **En parlak, en doygun** — göz ilk buraya gitsin |
| Tehlike | `#E05C5C` `#B33F3F` / alev `#FF9A4D` | Kırmızı-turuncu, evrensel tehlike dili |
| Toplanabilir | `#FFD75E` para, `#5AC8FF` mücevher | Parlak — göz ikinci buraya |
| Düşman | `#9B5DE5` `#7B41C4` | Belirgin ama oyuncudan **sönük** |
| Metal / çizgi | `#8A94A6` `#1A1A22` | Nötr detay |

---

## Üç okunabilirlik testi

Epic üç test sayıyor. İkisi ölçülebilir, biri değil.

### Ölçülen: siluet

Bütün görseller siyaha indirgenip **ortak ölçeğe** getiriliyor, sonra
çiftler karşılaştırılıyor. %75'in üstünde örtüşme "ayırt edilemiyor"
demek.

Boyut kasten yok sayılıyor: küçük bir daire ile büyük bir daire aynı
**şekil**. Soru "aynı büyüklükte mi" değil, **"aynı hat mı"**.

### Ölçülen: gri tonlama

Renkler kaldırılıp parlaklıklar karşılaştırılıyor. Bu aynı zamanda renk
körü oyuncular için erişilebilirlik testi — erkeklerin **~%8'i**.

### Ölçülemeyen: bulanıklık

Oyunu oynat, ekrana **gözünü kısarak** bak. Oyuncu, tehlike ve zemin hâlâ
ayırt ediliyor mu? Bu insan yargısı gerektiriyor.

### En önemli kural: üçü bir takım

**Bir çift siluetten ayırt edilemiyorsa ama parlaklıkları çok farklıysa
sorun yoktur.** Oyuncu onları yine ayırt eder.

Denetim aracı bayrağı ancak **ikisi birden** başarısızsa kaldırıyor. Her
testi ayrı ayrı geçmeye çalışmak, gereksiz yere sanatı bozar.

---

## Ölçülen sorunlar ve düzeltmeleri

İlk denetimde üç gerçek sorun çıktı. Hiçbiri "zevk meselesi" değildi.

| Sorun | Ölçüm | Düzeltme |
|---|---|---|
| **Oyuncu ↔ düşman** | siluet **%78** örtüşme | İkisi de yuvarlatılmış kutuydu. Oyuncuya **bacak arası + tepe tüyü**, düşmana **sırt dikenleri + ayak arası** verildi → **%47** |
| Düşman ↔ para | siluet %84 | %73'e indi; parlaklık farkı 100 olduğu için bayrak kalkmıyor |
| **Alev ↔ oyuncu** | parlaklık farkı **6,5** | Alev bir **tehlike** ve renk körü oyuncu onu karakterinden ayıramıyordu. Çekirdek büyütülüp beyaza yaklaştırıldı → **17,3** |

### Siluet neden dışbükey olmamalı

Yuvarlak bir leke, başka bir yuvarlak lekeden ayırt edilemez. Siluet
ancak **dışbükeyliği kırılırsa** kimlik kazanır:

```
dışbükey (kötü)        kırılmış (iyi)
                            ╷
     ▄▄▄▄▄                 ▄▄▄▄▄
    ███████               ███████
    ███████               ███████
    ███████               ██   ██     ← bacak arası
```

Bacak arası, tepe tüyü, sırt dikeni — hepsi aynı işi yapıyor: hattı
başka hiçbir şeye benzemez hale getirmek.

---

## Arka plan

> *"Arka plan güzel olmak zorunda değil — **geri çekilmek** zorunda."*

Üç katman, uzaktan yakına. `ParallaxLayer`'daki faktör alışılmışın
**tersi**: 0 = kamerayla birlikte hareket eder (sonsuz uzak), 1 = sahneyle
birlikte durur.

| Katman | Faktör | Genişlik | Şekil | Parlaklık |
|---|---|---|---|---|
| Uzak | 0,15 | 16 birim | sivri dağ | 47,9 |
| Orta | 0,45 | 24 birim | yumuşak tepe | 58,1 |
| Yakın | 0,75 | 32 birim | geniş tepe | 65,0 |

### Geri çekilme elle yapılmıyor

`SpriteFactory.PullBack(renk, oran)` rengi **gökyüzüne doğru** karıştırıyor.
Gözle "biraz soluklaştırayım" demek unutulur; `Lerp` unutulmaz.

Ham palet bunu neden gerektiriyor: `HillNear` **77,3** parlaklıkta, zemin
karosu **82,4**. Arada 5 birim — arka plan ön planla aynı banttaydı ve ilk
oynayan kişi bunu "ortamı beğenmedim" diye bildirdi.

### İki ayrı yarışma biçimi

`Sanati Denetle` ikisini de ölçüyor, çünkü arka plan iki farklı yoldan
yarışabilir:

| Test | Kural | Neden |
|---|---|---|
| **Ton** | en sönük oynanış görselinden 15 birim daha sönük | aynı bantta olursa zemin seçilmez |
| **Kalabalık** | iç kontrast < 60 | sönük ama **detaylı** arka plan da platformları yutar |

İkincisi sinsi olan. Karşılaştırma için: karo setinin iç kontrastı 93,6.
Arka plan ondan belirgin şekilde **düz** olmalı.

### Arka planlar "ayırt edilemiyor" testine girmez

Siluet karşılaştırması `bg_` ile başlayan görselleri atlıyor. Arka plan
katmanlarının birbirine benzemesi **istenen** şey; teste sokulsalardı araç
her çift için yanlış bayrak kaldırırdı.

### Tekrar

Katmanlar yatayda tekrar ediyor (`SpriteDrawMode.Tiled`). Ek yeri yok,
çünkü siluet harmoniklerinin hepsi genişliğe tam bölünüyor:

```
sin(2πk(x+W)/W) = sin(2πkx/W + 2πk) = sin(2πkx/W)
```

Karo setinden farkı bu: orada dolgu gerekiyordu, burada desen matematiksel
olarak devam ediyor.

> ⚠️ `SpriteRenderer.size.y` sprite yüksekliğiyle **birebir aynı** olmalı.
> Aksi halde dikeyde de tekrar eder — alev sprite'ı tam bu yüzden "iki ince
> çubuk" olarak görünmüştü.

---

## Görsel eklerken

1. Paletten çık — yeni renk **ekleme**
2. Silueti mevcutlardan farklı kur
3. `Tools > 2D Platformer > Sanati Denetle` çalıştır
4. Bayrak varsa şekli **veya** tonu değiştir, ikisini birden değil

> ⚠️ `Sadece Grafikleri Uret` menüsü `Assets/Art/` altındaki PNG'lerin
> **üzerine yazar.** Elle çizim koyduysan o menüyü bir daha çalıştırma.
