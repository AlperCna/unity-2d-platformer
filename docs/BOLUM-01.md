# Bölüm 1 — Tasarım

> Oyunun ilk 30–40 saniyesi. Oyuncuların çoğu burada devam edip
> etmeyeceğine karar veriyor.
>
> Cetvel: [AYARLAR.md](AYARLAR.md#zorluk-cetveli)
> Kod: [`Assets/Editor/Level01Builder.cs`](../Assets/Editor/Level01Builder.cs)

---

## v1 neden sıkıcıydı

İlk sürüm kuruldu, oynandı ve **sıkıcı** bulundu. Tahmin etmek yerine kayıt
dosyasındaki gerçek sayılara bakıldı (`bestTime 19,58 sn`, `0 ölüm`):

| Ölçüt | v1 | Hedef | Sorun |
|---|---|---|---|
| Süre | **20 sn** | 30–90 sn | Yarı yarıya kısa |
| Engel sayısı | 6 | — | 83 birimde seyrek |
| Boş zemin | **%54** | — | Yarısı "sağa yürü" |
| En zor an | **%75** | ~%85 | Bölüm kendi tavanına hiç yaklaşmıyor |
| Dikey aralık | 3,4 birim | — | Neredeyse tamamen düz |
| Zorluk şekli | 2,5 → 3,5 → 4,0 | dalga | **Rampa** |

Ama en derin sorun tabloda değildi: **her engel aynı fiildi.** Boşluğa
zıpla, dikene zıpla. Zihinsel işlem hep aynı. Zamanlama yok, ritim yok.

Ve zorluk şekli, bu projenin kendi kuralını çiğniyordu —
[04-seviye-tasarimi-prensipleri.md](04-seviye-tasarimi-prensipleri.md)
"bölüm bir dalga olmalı" diyor, v1 düz bir rampaydı.

---

## Bu bölümün işi

| Kural | Neden |
|---|---|
| İlk 10 saniyede **ölüm imkânsız** | Oyuncu önce kontrolü tanısın |
| **Hiç yazılı tutorial yok** | Tasarımla öğret |
| **Dash burada tanıtılmaz** | Bölüm 2–3'e kalsın; bir seferde bir şey |
| **Yeni mekanik yok** | Çeşitlilik dizilişten gelmeli, envanterden değil |
| Bittiğinde öğrenmiş olacakları | Hareket, zıplama, para, boşluk, diken, ritim |

Dördüncü kural v2'nin bütün tasarımını belirledi. Düşman veya hareketli
platform eklemek kolay olurdu ama onlar Epic 06–07'nin işi. Buradaki soru
şuydu: **aynı üç parçayla (zemin, boşluk, diken) nasıl daha zengin bir
bölüm kurulur?**

Üç cevap çıktı: **ritim**, **dikey**, **süre**.

---

## Ritim (beat sheet)

```
zorluk
 %83 |                                                   /\
     |                                                  /  \
 %68 |                             /\      /\     /\   /    \
     |                            /  \    /  \   /  \ /      \
 %50 |       /\  /\/\                \   /    \ /    X        \
     |  ____/  \/    \_____________   \_/      \     |         \__
     +-------------------------------------------------------------
      1-3   4  ritim   nefes  diken  tırm.  kori. birleş  final bitiş
                5        6      7      8      9     11     12    13
```

| # | x | İçerik | Öğrettiği / işlevi | Ölüm? |
|---|---|---|---|---|
| 1 | 0–9 | 9 birim düz zemin, 3 para | Hareket, toplama | ✘ |
| 2 | 9–13 | **1,2 birim** basamak | Zıplama var | ✘ |
| 3 | 13–18 | **2,2 birim** basamak | Gerçek zıplama | ✘ |
| 4 | 18–20,5 | **2,5 birim** boşluk (%47) | Boşluk geçme | ✔ ilk kez |
| 5 | 20,5–42,9 | **2,8 / 3,0 / 2,6** ardışık boşluk | **Ritim** | ✔ |
| 6 | 42,9–50,9 | 8 birim düz, 3 para, **checkpoint** | Nefes | ✘ |
| 7 | 50,9–61,9 | Tek diken, geniş zemin | Diken = ölüm | ✔ kolay kaçınılır |
| 8 | 61,9–73,1 | **3,2 boşluk + 2×1,4 basamak** (%68) | **Dikey** | ✔ |
| 9 | 73,1–82,1 | **2 birimlik diken koridoru** | Mesafeli tehlike | ✔ |
| 10 | 82,1–89,1 | 2,8 iniş, 3 para, **checkpoint** | Nefes | ✘ |
| 11 | 89,1–100,7 | Diken koridoru + **3,6 boşluk** (%68) | Birleştirme | ✔ |
| 12 | 100,7–113,1 | Diken + **4,4 boşluk** (%83) | **Zirve** | ✔ |
| 13 | 113,1–124,1 | Düz zemin, bayrak | Rahatlama | ✘ |

**Toplam 124,1 birim** — kurulum çıktısıyla birebir doğrulandı, sorun yok.
Tahmin 29,5 sn, ilk oynanış 28,86 sn — tahmin %2 tutturdu.

### Ölçüm

| | v1 | v2 — 1. koşu | v2 — 2. koşu |
|---|---|---|---|
| Süre | 19,58 sn | 28,86 sn | **22,24 sn** |
| Ölüm | 0 | 0 | **2** |
| Para | 30/31 | 50/50 | 44/50 |
| Hız | 4,24 birim/sn | 4,30 birim/sn | **5,58 birim/sn** |

**Hedefe göre durum:** İlk oynanış 28,86 sn — hedefin 1,1 saniye altında.
Bölümü sırf sayıyı tutturmak için uzatmak ölçüyü kandırmak olurdu; bölüm
olduğu gibi bırakıldı.

**İkinci koşu neden daha hızlı?** Aynı oyuncu, aynı bölüm — sadece artık
biliyor. %30 hızlanma. Bu yüzden "30–90 sn" hedefi **ilk oynanış** için
anlamlı; tekrar oynayan biri her bölümü hedefin altında bitirir.
`LevelCursor`'ın süre tahmini de bu yüzden ilk-oynanış hızını kullanıyor.

**Asıl kazanç: 0 → 2 ölüm.** v1'de ve v2'nin ilk koşusunda sıfır ölüm
vardı — yani bölüm hiç direnç göstermiyordu. Artık gösteriyor. Para da
50/50'den 44/50'ye düştü: geri dönüp toplanacak bir şey var.

**Not:** İlk koşunun süresi `bestTime` alanından okunamadı — v1'in rekoru
(19,58) orada duruyordu ve v2 daha uzun olduğu için kırılamazdı.
`totalPlayTime` farkından hesaplandı. Bu bir hataydı, düzeltildi:
[10-checkpoint-ve-kayit.md](10-checkpoint-ve-kayit.md#9-tasarım-sürümü).
İkinci koşuda düzeltme çalıştı, rekor sıfırlandı ve süre doğrudan okundu.

---

## Üç cevap

### 1. Ritim (beat 5)

Tek bir boşluk **engeldir**. Üç ardışık boşluk **ritimdir.**

```
    ▓▓▓▓▓▓▓   ▓▓▓▓▓▓   ▓▓▓▓▓▓▓   ▓▓▓▓▓▓▓▓
           2,8      3,0       2,6
```

Fiil değişmiyor — hâlâ sadece zıplıyorsun. Değişen şey **arka arkaya
gelmesi**: oyuncu tek tek düşünmeyi bırakıp akışa giriyor. v1'de her engel
tek başına duruyordu, aralarında 8–10 birim boş zemin vardı.

Son boşluk kasıtlı olarak daha küçük (**%49**). Oyuncu ritimden kazanarak
çıkıyor, tokatlanarak değil.

### 2. Dikey (beat 8)

v1'in dikey aralığı 3,4 birimdi — neredeyse düz bir çizgi. v2'de 6,2.

Beat 8 boşluk ve tırmanışı birleştiriyor: **3,2 birimlik boşluğun karşı
tarafı 1,4 birim yukarıda.** Bu ikisi ayrı ayrı kolay, birlikte değil —
aşağıdaki [birleşik erişim](#birleşik-erişim-32-neden-68) bölümüne bak.

### 3. Süre

v1: 83 birim, %54 boş.
v2: 124 birim, %48 boş.

Bölüm **1,5 kat uzadı ama boşluk oranı düştü** — yani eklenen 41 birimin
çoğu içerik. Giriş 12→9'a, bitiş 17→11'e kısaldı.

Uzunluk arttığı için **ikinci bir checkpoint** eklendi (beat 10). Tek
checkpoint'le son boşlukta ölmek 70 birim geri göndermek demekti.

---

## Her sayının gerekçesi

Hiçbiri göz kararı değil — hepsi ölçülen cetvelden:

| Öğe | Değer | Maks.ın %'si | Neden bu |
|---|---|---|---|
| İlk basamak | 1,2 birim | %40 | **Min. zıplama 1,5** — tuşa dokunmak yeter, "bedava" hissettirir |
| İkinci basamak | 2,2 birim | %73 | Gerçek bir zıplama gerekiyor ama rahat |
| İlk boşluk | 2,5 birim | %47 | Cetvelde "çok kolay" — düşünmeden geçilir |
| Ritim boşlukları | 2,8 / 3,0 / 2,6 | %53 / %56 / %49 | Birbirine yakın olmalı — ritim **tutarlılıktır** |
| Tırmanış boşluğu | 3,2 birim | **%68** | Düz zeminde %60 olurdu; 1,4 yukarı çıktığı için limit daralıyor |
| Birleştirme boşluğu | 3,6 birim | %68 | Tırmanışla aynı zorlukta ama düz — aynı şeyin başka hâli |
| Final boşluğu | 4,4 birim | **%83** | Bölümün zirvesi. v1'in tavanı %75'ti |
| Tırmanış basamakları | 1,4 birim | %46 | Kolay olmalı — zorluk boşlukta, tırmanışta değil |

**Bölüm 1'de %85'in üstüne çıkmıyoruz.** 4,9+ birimlik boşluklar ve 5,5+
(dash zorunlu) sonraki bölümlere.

### Birleşik erişim: 3,2 neden %68

Düz zeminde 5,31 birim atlarsın. Ama **yukarıdaki** bir platforma
ineceksen, iniş noktasına varmadan önce o yüksekliğin altına düşmüş
olmamalısın — yani kullanılabilir mesafe kısalır:

```
x(h) = xTepe + (D - xTepe) * karekök(1 - h/H)
```

`D = 5,31` (düz mesafe), `H = 3,03` (zıplama yüksekliği), `xTepe = 0,58·D`
(yerçekimi inişte 1,9 kat güçlü olduğu için tepe ortada değil, biraz ileride).

`h = 1,4` için: **4,72 birim.** Yani 3,2 birimlik boşluk aslında %68.

Bu hesap artık `LevelCursor.MaxDistanceAtHeight()` içinde ve boşluktan
hemen sonra basamak gelirse **otomatik doğrulanıyor.**

---

## Öğretme kalıbı uygulaması

Dikenler için dört adım:

| Adım | Nerede | Nasıl |
|---|---|---|
| **1. Tanıt** | Beat 7 | Tek diken, 11 birimlik geniş zeminde. Görürsün, rahatça üstünden zıplarsın. |
| **2. Zorlaştır** | Beat 9 | **İki birimlik koridor** — artık mesafe ölçtürüyor |
| **3. Birleştir** | Beat 11 | Koridor + hemen ardından 3,6 boşluk |
| **4. Zirve** | Beat 12 | Diken + 4,4 boşluk |

Boşluk için: 2,5 → ritim → 3,2 (dikey) → 3,6 → 4,4.

### Diken tuzağı

v2 kurulurken **üç tane** şu hatadan çıktı: oyuncu dikeni görür, çekinir,
tam güç zıplar — ve dikenin arkasındaki boşluğa düşer. Yanlış yaptığı için
değil, **fazla dikkatli** davrandığı için ölür.

Kural: her dikenden sonra en az **5,8 birim** zemin olmalı (tam zıplama
mesafesi + pay). Bu yüzden beat 7'nin zemini 9 değil 11 birim.

Artık `LevelCursor.ValidateSpikeLandings()` bunu her kurulumda kontrol
ediyor — bir daha elle aranmayacak.

---

## Yönlendirme

Yazı yok, o yüzden paralar konuşuyor:

```
Beat 4 — zıplama yayını paralarla çiz:

              o o o
            o       o
          o           o
    ▓▓▓▓▓▓            ▓▓▓▓▓▓▓
```

Oyuncu "2,5 birim atlamalıyım" diye düşünmez — paraları takip eder ve doğru
yeri bulur.

Beat 9'da paralar **diken koridorunun üstünde ve yüksekte** duruyor:
zıplamanın nereden başlaması gerektiğini söylüyorlar. Alçak zıplama yetmez.

---

## Kontrol listesi

- [x] İlk 10 saniyede ölüm imkânsız
- [x] Hiç yazılı tutorial yok
- [x] En az 2 nefes alanı var (beat 6, beat 10)
- [x] Hiçbir boşluk %85'i geçmiyor
- [x] Hiçbir basamak 2,5'i geçmiyor
- [x] Dash gerekmiyor
- [x] Paralar rota gösteriyor
- [x] Zorluk eğrisi dalga (47 → 56 → **49** → 68 → 68 → **83**)
- [x] Diken tuzağı yok (otomatik doğrulandı)
- [~] Süre 30–90 sn arasında — ilk oynanış **28,86 sn** (1,1 sn altında, kabul edildi)
- [ ] 5 saniye testi: ekrana bakınca nereye gideceğin belli
- [ ] Bulanıklık testi
- [ ] Bir başkası takılmadan bitirdi
