# Epic 04 — Seviye Tasarımı Prensipleri

> ✅ **TAMAMLANDI — 14 Eylül 2026.** Tasarım: [BOLUM-01.md](BOLUM-01.md)
> Bölüm 1 kuruldu ve doğrulayıcıdan temiz geçti (en geniş boşluk 4,0 = maks.ın %75'i).
>
> ⏳ **AÇIK MADDE: başkasına oynatma.** M1 kapısına ertelendi — şu an test
> edilirse ölüm hissi (Epic 09) ve checkpoint (Epic 10) eksik olduğu için
> geri bildirim yanıltıcı olur. Tasarım hatası mı, eksik sistem mi ayırt edilemez.

**Amaç:** Bölüm yapmayı öğrenmek. Bu epic'te az bölüm yapacaksın — asıl iş
nasıl yapıldığını anlamak.

**Ön koşul:** [Epic 02](02-karakter-hissiyati.md) — zıplama ölçüleri hazır olmalı

**Süre:** 3–4 gün

---

## Neden bu epic var

"Bölüm yapmak" sanılanın aksine sanatsal değil, **dilbilgisel** bir iştir.
Oyuncuya bir şey öğretirsin, tekrar ettirirsin, zorlaştırırsın, birleştirirsin.

Bunu bilmeden yapılan bölümler iki kutuptan birine düşer: ya çok kolay ve
sıkıcı, ya da haksız ve sinir bozucu.

---

## Ölçü birimin: zıplama

Epic 02'de **ölçülen** değerler (tahmin değil, oyundan alındı):

| Ölçüm | Değer |
|---|---|
| Maksimum zıplama yüksekliği | **3,03** birim |
| Minimum zıplama (hızlı dokunuş) | **~1,5** birim |
| Koşarak zıplama mesafesi | **5,31** birim |
| Koşarak + dash | **7,62** birim |

Tam zorluk cetveli: **[AYARLAR.md → Zorluk Cetveli](AYARLAR.md#zorluk-cetveli)**

İki kural buradan çıkıyor:

**1. Asla maksimumun üstüne çıkma.** Oyuncu deneyip deneyip başaramazsa
oyunun bozuk olduğunu düşünür — ve haklıdır.

**2. 5,4 birimlik boşluk kullanma.** Dash'siz imkânsız (5,31), dash'li bedava.
Oyuncuya hiçbir şey öğretmez, sadece "Shift'e bas" der.

Bu iki kural `LevelCursor` içinde **kod olarak** uygulanıyor: geçilemez bir
boşluk yazarsan konsol hata verir, belirsiz bölgeye yazarsan uyarır.
Oyunu oynayıp fark etmene gerek kalmıyor.

- [x] Zorluk cetveli çıkarıldı (AYARLAR.md)

---

## Dört adımlı öğretme kalıbı

Her yeni mekanik veya tehlike için bu sırayı uygula. Bu, Nintendo'nun
onlarca yıldır kullandığı kalıptır.

### 1. Tanıtım — risksiz göster
Oyuncu yeni şeyi **ölmeden** görsün. Diken varsa altında zemin olsun;
düşse bile ölmesin.

### 2. Uygulama — güvenli dene
Şimdi kullanması gereksin, ama ceza küçük olsun. Checkpoint hemen yanında.

### 3. Zorlaştırma — baskı ekle
Aynı şey, ama daha dar zamanlama veya daha uzun mesafe.

### 4. Birleştirme — eskiyle kombinle
Yeni mekanik + önceden öğrendiği bir şey aynı anda.

**Örnek — hareketli platform:**

```
1. TANITIM       Geniş, yavaş platform. Altında zemin var.
                 Düşersen ölmezsin, sadece geri çıkarsın.

                 ═════════════   ← platform (yavaş, geniş)
                 ▓▓▓▓▓▓▓▓▓▓▓▓▓   ← altında güvenli zemin

2. UYGULAMA      Platform boşluğun üstünde. Ama geniş ve yavaş.
                 Checkpoint hemen solda.

                 ⚑    ═════════
                 ▓▓▓            ▓▓▓
                      (boşluk)

3. ZORLAŞTIRMA   Platform dar ve hızlı.

                      ═══
                 ▓▓▓        ▓▓▓
                   (geniş boşluk)

4. BİRLEŞTİRME   Dar platform + üstünde devriye düşman.

                      ═══
                       ◆        ← düşman platformda
                 ▓▓▓        ▓▓▓
```

- [ ] Kalıbı bir mekanik için kâğıda uygula

---

## Ritim: bölüm bir dalga olmalı

İyi bölüm düz bir zorluk çizgisi değil, **dalga**dır:

```
zorluk
  │        ╱╲              ╱╲
  │       ╱  ╲            ╱  ╲    ╱╲
  │  ╱╲  ╱    ╲    ╱╲    ╱    ╲  ╱  ╲
  │ ╱  ╲╱      ╲__╱  ╲__╱      ╲╱    ╲___
  └──────────────────────────────────────── zaman
    giriş yoğun  nefes   yoğun   nefes  final
```

**Nefes alanı:** tehlikesiz, belki bir para toplanan düz kısım. 3–5 saniye
yeter. Sürekli baskı yorar ve oyuncu gerilimi hissedemez hale gelir.

Bir bölümün kaba iskeleti:

| Bölüm parçası | Süre | İçerik |
|---|---|---|
| Giriş | 5 sn | Güvenli, oyuncu yönelsin |
| İlk meydan okuma | 10 sn | Bölümün ana fikri |
| Nefes | 4 sn | Para, düz zemin |
| Yükselme | 15 sn | Aynı fikir, daha zor |
| Nefes | 3 sn | Checkpoint burada |
| Final | 15 sn | En zor kombinasyon |
| Bitiş | 3 sn | Bayrak görünür, rahat |

Toplam ≈ 55 saniye. Hedef aralığın içinde.

- [ ] Her bölümde en az 2 nefes alanı var

---

## Okunabilirlik kuralları

Oyuncu ekrana bakıp **anında** anlamalı:

| Kural | Uygulama |
|---|---|
| Tehlike kırmızı/dikenli | Güvenli şeyler asla tehlikeli görünmesin |
| Zemin ≠ arka plan | Basılabilen ile dekoratif net ayrılsın |
| Toplanabilir parlak | Gözün ilk gittiği ikinci şey |
| Oyuncu en parlak | Gözün ilk gittiği şey |
| Arka plan soluk | Düşük kontrast, düşük doygunluk |

**Bulanıklık testi:** ekran görüntüsü al, bulanıklaştır (herhangi bir görsel
düzenleyicide). Oyuncu, tehlike ve zemin hâlâ ayırt edilebiliyor mu?
Edilemiyorsa kontrast yetersiz.

**5 saniye testi:** ekran görüntüsüne 5 saniye bak. Nereye gitmen gerektiğini
anlıyor musun?

- [ ] İki test de geçiliyor

---

## Yönlendirme: oyuncuya yazı yazmadan yol göster

| Teknik | Nasıl çalışır | Gücü |
|---|---|---|
| **Para dizisi** | Toplanabilirler rotayı çizer | ★★★★★ |
| **Hareket** | Göz hareketli şeye gider | ★★★★ |
| **Işık/kontrast** | Parlak alan çekim merkezi | ★★★★ |
| **Mimari çizgi** | Platformların dizilişi yön belirtir | ★★★ |
| **Boşluk** | Geniş boş alan "buradan geç" der | ★★★ |

**Para dizisi en güçlüsüdür.** Zıplama yayını paralarla işaretlersen oyuncu
"şuraya zıplamalıyım" diye düşünmez — sadece paraları takip eder ve doğru
yeri bulur.

[`LevelCursor`](../Assets/Editor/LevelCursor.cs) bunu iki şekilde yapıyor:

```csharp
c.Gap(4f, coinArc: 5);   // boşluğun üstüne zıplama yayı çizer
c.Coins(3);              // son zemin parçasının üstüne yatay dizi
```

`coinArc` parabolü kendisi hesaplıyor ve tepesini zıplama yüksekliğinin
%80'iyle sınırlıyor — ulaşılamayacak yere para koymuyor.

```
Örnek: 4 birimlik boşluğu para yayıyla işaretlemek

              ○ ○ ○
            ○       ○
          ○           ○
    ▓▓▓▓▓                ▓▓▓▓▓
         └── 4 birim ────┘
```

- [ ] Her bölümde en az bir yönlendirme tekniği bilinçli kullanılıyor

---

## Görevler

### 1. Kâğıt üstünde tasarla

Unity'yi açma. Kareli kâğıt al, **1 kare = 1 birim**. Üç bölüm çiz.

Kâğıtta tasarlamak Unity'de tasarlamaktan ~10 kat hızlıdır ve kötü fikirleri
erken eler. Unity'de çizmeye başladığın an, kötü bir fikre bile emek vermiş
olursun ve silmek zorlaşır.

- [ ] 3 bölüm kâğıda çizildi

### 2. Bölüm 1'i Unity'de yap

**Bu bölüm oyunun ilk 60 saniyesi** — en çok emek vereceğin bölüm bu olmalı.
Oyuncuların çoğu buradan sonra devam edip etmeyeceğine karar veriyor.

Kurallar:
- İlk 10 saniyede **ölüm imkânsız** olsun
- Hiçbir yazılı tutorial olmasın — tasarımla öğret
- Bittiğinde oyuncu zıplamayı ve temel tehlikeyi öğrenmiş olsun
- İmza mekaniğini burada tanıtma (Bölüm 2–3'e bırak)

**Tutorial'sız öğretmenin örneği:**

```
Oyuncu başlar, sağa gitmekten başka seçenek yok
  → küçük bir basamak (zıplamayı keşfeder)
    → bir para (toplamayı öğrenir)
      → küçük bir boşluk (zıplama mesafesini öğrenir)
        → altında zemin olan diken (tehlikeyi ölmeden öğrenir)
          → gerçek diken (öğrendiğini uygular)
```

Tek kelime yazı yok, ama her şey öğretilmiş oldu.

- [ ] Bölüm 1 hazır ve oynanabilir

### 3. Bir başkasına oynat

**Bu adımı atlama.** Kendi bölümünü doğru oynarsın çünkü çözümü biliyorsun.

Kurallar:
- **Hiçbir şey söyleme.** Yardım etme, ipucu verme, "şuraya zıpla" deme.
- Ellerini ve yüzünü izle, sözlerini değil.
- Not al:

| Gözlem | Ne demek |
|---|---|
| Bir yerde takıldı | Yönlendirme yetersiz |
| "Ben mi yaptım?" dedi | Geri bildirim yetersiz |
| Aynı yerde 5+ öldü | Tasarım hatası |
| "Değmedim ki" dedi | Collider çok büyük |
| Sessizleşti | Sıkıldı |
| Duraklatıp telefona baktı | Kaybettin |

- [ ] En az 1 kişi oynadı, notlar alındı
- [ ] Notlara göre düzeltme yapıldı

---

## Kabul kriteri

- [ ] Zorluk tabloların (mesafe ve yükseklik) yazılı
- [ ] Bölüm 1 bitmiş ve oynanabilir
- [ ] İlk 10 saniyede ölmek imkânsız
- [ ] Hiç yazılı tutorial yok
- [ ] Bölümde en az 2 nefes alanı var
- [ ] Bulanıklık ve 5 saniye testleri geçiliyor
- [ ] Bir başkası takılmadan bitirdi

---

## Tuzaklar

**Kendi becerine göre tasarlamak.** Sen bölümü 500 kez oynadın. Oyuncu ilk
kez görüyor. Sana "kolay" gelen, ona "imkânsız"dır.

**Ekran dışından gelen tehlike.** Göremediği bir şeyden ölmek haksızlıktır.

**Ölüm sonrası uzun yol.** Oyuncu öldü, checkpoint 40 saniye geride. Bu,
oyunu bıraktıran bir numaralı sebeptir.

**"Sonra düzeltirim" diye kötü bölüm bırakmak.** Düzeltmezsin. 12 bölüm
biriktiğinde hepsini düzeltmeye enerjin kalmaz.

**Bölümü uzun yapmak.** 30–90 saniye yeterli. Uzun bölüm = uzun ölüm cezası
= sinirli oyuncu.

**Maksimum zıplamayı zorunlu kılmak.** Hata payı sıfır olan bir atlayış,
oyuncunun %90'ı için haksızdır.

**Test oyuncusuna yardım etmek.** "Şuraya zıpla" dediğin an veri bozulur.

---

## v1'de yapma

- Prosedürel/rastgele üretilen bölümler
- Dallanan yollar, alternatif rotalar
- Geri dönülebilir açık harita (metroidvania yapısı)
- Bulmaca odaları, anahtar-kapı sistemleri
- Oyun içi bölüm editörü
- Gizli bölümler (sır yeter, ayrı bölüm gerekmez)

---

## Sonraki

[Epic 05 — Tilemap ile bölüm inşası](05-tilemap.md)
