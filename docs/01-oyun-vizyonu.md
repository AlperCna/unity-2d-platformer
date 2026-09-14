# Epic 01 — Oyun Vizyonu ve Kapsam

> ✅ **TAMAMLANDI — 14 Eylül 2026.** Çıktı: [VIZYON.md](VIZYON.md), [SONRA.md](SONRA.md).
> İmza mekaniği dash seçildi ve yatayla sınırlandı. Vizyon onaylandı.

**Amaç:** Ne yaptığını bir sayfada yazılı hale getirmek. Proje bitene kadar
başvuracağın referans bu olacak.

**Ön koşul:** Yok. Buradan başlanır.

**Süre:** 2–3 saat. Hiç kod yok.

---

## Neden bu epic var

Kod yazmak somut ve ödüllendirici; plan yazmak soyut ve sıkıcı. O yüzden
herkes atlar. Sonra iki ay sonra "bu oyunda ne olacaktı?" diye düşünüp bırakır.

Bir sayfalık vizyon belgesi sana üç şey verir:

**1. Karar verirken başvuracağın ölçüt.** "Duvara tırmanma ekleyeyim mi?"
sorusunun cevabı vizyonunda yazıyorsa tartışma 5 saniyede biter. Yazmıyorsa
her karar yeni bir tartışmadır ve her tartışma enerji yer.

**2. Hayır deme gücü.** Proje boyunca onlarca iyi fikir gelecek. İyi fikirlerin
çoğu **senin oyununa ait değil**. Vizyon belgesi, "bu iyi bir fikir ama bu
oyunun fikri değil" demeni sağlar.

**3. Bitiş çizgisi.** Nerede duracağını bilmiyorsan hiç durmazsın. Kapsam
tablosu bitiş çizgisidir.

---

## Görevler

### 1. Tek cümleyi yaz

Oyununu **bir cümlede** anlat. Cümle mekanik + duygu içersin.

Kötü örnekler ve sorunları:

| Cümle | Sorun |
|---|---|
| "Bir platform oyunu, kahraman maceraya çıkıyor." | Her platform oyunu bu. Ayırt edici hiçbir şey yok. |
| "Zorlu bölümlerde hayatta kalmaya çalıştığın atmosferik bir deneyim." | Pazarlama dili. Ne yaptığını söylemiyor. |
| "Metroidvania tarzı, açık dünyalı, çok mekanikli bir platformcu." | Kapsam patlaması. Bu 3 yıllık iş. |

İyi örnekler:

| Cümle | Neden iyi |
|---|---|
| "Zıplayamayan ama duvarlardan sekebilen bir robotun, çöken bir kuleden kaçışı." | Mekanik net, kısıt ilginç, durum acil |
| "Her ölümünde bir öncekinin hayaleti seninle yarışır." | Tek mekanik, tek duygu, hemen anlaşılıyor |
| "Yerçekimini ters çevirerek tavanla zemin arasında koşturduğun 60 saniyelik bölümler." | Mekanik + ölçek aynı cümlede |

**Yöntem:** 10 tane yaz, hepsini oku, en iyisini seç. İlk yazdığın neredeyse
kesinlikle en sıradan olanıdır.

Cümlen sıradansa oyunun da sıradan olacak.

- [ ] 10 cümle yazıldı, biri seçildi

### 2. Üç sütunu belirle (design pillars)

Oyununun üzerinde durduğu 3 şey. Her kararı bunlara sorarsın.

Sütun, **oyunun ne olduğunu değil, neyi öncelediğini** söyler. "Güzel grafikler"
sütun değildir; "her zaman okunabilirlik" sütundur.

Örnek set:

> **Hassasiyet** — ölüm her zaman oyuncunun hatasıdır, asla oyunun.
> Şüpheye düştüğümüzde oyuncu lehine karar veririz.
>
> **Akıcılık** — durmadan ilerleyen bir ritim. Bekleme, yükleme, kesinti yok.
>
> **Kısalık** — her bölüm 30–60 saniye. Ölüm cezası neredeyse sıfır.

Bu sütunlar sana şunları söyler: collider'lar cömert olacak (hassasiyet),
uzun ölüm animasyonu olmayacak (akıcılık), 5 dakikalık bölüm olmayacak (kısalık).

Alternatif set örneği (tamamen farklı bir oyun için):

> **Keşif** — her bölümde görünmeyen bir şey var.
> **Yavaşlık** — acele yok, oyuncu düşünsün.
> **Sessizlik** — müzik yok, sadece ortam sesi.

- [ ] 3 sütun yazıldı, her biri bir cümlelik açıklamayla

### 3. Çekirdek döngüyü tanımla (core loop)

Oyuncu 30 saniyede ne yapıyor, tekrar tekrar?

```
Bölüme gir
  → engelleri gözle oku
    → zıpla / kaç / zamanla
      → başar veya öl
        → anında tekrar
```

Bu döngü eğlenceli değilse oyun eğlenceli olmaz. Grafik, hikâye, müzik,
efekt — hiçbiri bunu kurtarmaz.

**Test:** döngüyü yazdıktan sonra kendine sor — "bunu 200 kez yapmak
sıkıcı olur mu?" Olursa döngü yanlış.

- [ ] Çekirdek döngü yazıldı

### 4. Tek bir "imza mekaniği" seç

Oyununu diğerlerinden ayıran **bir** şey. Sadece bir tane.

Elindeki iskelette temel zıplama zaten var. İmza mekaniği onun üstüne gelen
tek ek yetenek olacak.

| Mekanik | Ne katar | Zorluğu | Bölüm tasarımına etkisi |
|---|---|---|---|
| **Dash** (havada atılma) | Mesafe + hız hissi | Kolay | Boşluklar genişler, zamanlama önem kazanır |
| **Duvar sekmesi** | Dikey hareket | Orta | Bölümler dikey olur, dar koridorlar anlam kazanır |
| **Yerçekimi çevirme** | Uzamsal bulmaca | Orta | Her bölüm iki yüzlü tasarlanır |
| **Zaman yavaşlatma** | Rahatlama + hassasiyet | Kolay | Zor zamanlamalar mümkün olur |
| **Hayalet tekrar** | Yarış hissi | Zor | Kayıt/oynatma sistemi gerekir |
| **Zincirleme sekme** | Akış ustalığı | Kolay | Düşman yerleşimi kritik olur |

**Kolay olanla başla.** Dash veya zincirleme sekme en az riskli. İkisi de
mevcut kodla iyi çalışır — `PlayerController2D.LaunchUpward()` zaten zincirleme
sekme için hazır.

**Bir tane seç.** İki mekanik iki kat iş değil, dört kat iştir: her ikisi de
diğeriyle ve her tehlikeyle etkileşmek zorundadır.

- [ ] İmza mekaniği seçildi ve neden seçildiği yazıldı

### 5. Kapsam sınırını yaz

Şu tabloyu doldur ve **buna sadık kal**:

| | Hedef | Senin kararın |
|---|---|---|
| Bölüm sayısı | 10–12 | |
| Bölüm başına süre | 30–90 saniye | |
| Toplam oynanış | 10–15 dakika | |
| Düşman çeşidi | 2–3 | |
| Tehlike çeşidi | 4–6 | |
| Mekanik sayısı | 1 imza + temel zıplama | |
| Boss | Yok (veya 1, en sonda) | |
| Hikâye | En fazla 3 ekran yazı | |
| Müzik parçası | 2–3 | |
| Hedef platform | Windows | |

Bu tablo **bitiş çizgisi**. Hepsi tamamlandığında oyun bitmiştir — daha iyi
olabilirdi, ama bitmiştir.

- [ ] Kapsam tablosu dolduruldu

### 6. Referans oyunları seç

2–3 oyun seç, her birinden **ne aldığını ve ne almadığını** yaz.

Örnek:

> **Celeste'ten alıyorum:** ölüm cezasının sıfıra yakın olması, checkpoint
> cömertliği, bölüm başına tek fikir.
> **Almıyorum:** hikâye, diyalog, 8 bölgelik yapı, B-side'lar.
>
> **Super Meat Boy'dan alıyorum:** bölüm uzunluğu (30 sn), ölüm sonrası
> anında yeniden başlama.
> **Almıyorum:** aşırı zorluk, 300 bölüm.
>
> **Hollow Knight'tan almıyorum: hiçbir şey.** Üç kişinin üç yılı. Benim
> ölçeğimde değil, referans alırsam kendimi kötü hissederim.

Son madde önemli — neyi **almadığını** yazmak, kapsam kontrolünün yarısıdır.

- [ ] 3 referans, her biri için "alıyorum/almıyorum"

### 7. Bir sayfaya sığdır

Yukarıdakileri `docs/VIZYON.md` dosyasına yaz. **Bir sayfayı geçmesin.**
Geçiyorsa kapsamın çok büyük — kes.

---

## Şablon: docs/VIZYON.md

```markdown
# [OYUN ADI]

## Tek cümle
[Mekanik + duygu içeren tek cümle]

## Sütunlar
1. **[Sütun]** — [bir cümle açıklama]
2. **[Sütun]** — [bir cümle açıklama]
3. **[Sütun]** — [bir cümle açıklama]

## Çekirdek döngü
[30 saniyede oyuncunun yaptığı şey, adım adım]

## İmza mekaniği
**[Mekanik adı]** — [ne yapar, neden bu seçildi]

## Kapsam
| | Hedef |
|---|---|
| Bölüm | |
| Toplam süre | |
| Düşman çeşidi | |
| Tehlike çeşidi | |
| Boss | |
| Hikâye | |
| Platform | |

## Referanslar
- **[Oyun]** — alıyorum: [...] / almıyorum: [...]
- **[Oyun]** — alıyorum: [...] / almıyorum: [...]

## Bu oyunda OLMAYACAK şeyler
- [...]
- [...]
```

Son bölüm ("olmayacak şeyler") en değerli kısım. Proje boyunca oraya ekleme
yapacaksın ve her ekleme seni bitmeye yaklaştıracak.

- [ ] `docs/VIZYON.md` yazıldı

---

## Kabul kriteri

- [x] Tek cümlelik tanım var ve sıradan değil
- [x] 3 sütun yazılı, her biri açıklamalı
- [x] Çekirdek döngü yazılı ve "200 kez yapılabilir" testini geçiyor
- [x] Tek imza mekaniği seçilmiş ve gerekçelendirilmiş
- [x] Kapsam tablosu dolu
- [x] 3 referans, "alıyorum/almıyorum" ayrımıyla
- [x] Hepsi `docs/VIZYON.md` içinde, bir sayfayı geçmiyor
- [x] "Olmayacak şeyler" listesi başlatıldı

---

## Tuzaklar

**"Sonra karar veririm."** Vermezsin. Karar vermeden yazılan kodun yarısı atılır.

**Çok mekanik.** "Hem dash, hem duvar sekmesi, hem kanca olsun" — her mekanik
diğerleriyle ve her tehlikeyle etkileşmek zorunda. 3 mekanik 3 kat değil,
yaklaşık 9 kat iş demek.

**Hikâyeyle başlamak.** Platform oyununda hikâye en son gelir, hatta hiç
gelmeyebilir. Önce zıplama iyi hissettirsin.

**Referans olarak devasa oyun seçmek.** Hollow Knight'ı referans alırsan
kendi oyunun hep yetersiz görünür. Referansın senin ölçeğinde olsun.

**Vizyonu yazıp bir daha açmamak.** Her hafta bir kez oku. Özellikle yeni
bir fikir geldiğinde.

**Mükemmel cümleyi arayıp takılmak.** 2 saat yeter. Yeterince iyi olan
cümleyi seç, devam et. Proje ilerledikçe zaten netleşecek.

---

## v1'de yapma

- Hikâye, diyalog, ara sahne, anlatıcı
- Karakter özelleştirme
- Birden fazla oynanabilir karakter
- Açık dünya / harita ekranı / merkez üs (hub)
- Çok oyunculu, online, skor tablosu
- Beceri ağacı, envanter, para ekonomisi, eşya
- Yan görevler
- Birden fazla son

Hepsi `docs/SONRA.md` dosyasına gitsin. Silme — ertele.

---

## Sonraki

[Epic 02 — Karakter hissiyatı](02-karakter-hissiyati.md)
