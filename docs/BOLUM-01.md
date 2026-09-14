# Bölüm 1 — Tasarım

> Oyunun ilk 50 saniyesi. Oyuncuların çoğu burada devam edip etmeyeceğine
> karar veriyor.
>
> Cetvel: [AYARLAR.md](AYARLAR.md#zorluk-cetveli)

---

## Bu bölümün işi

| Kural | Neden |
|---|---|
| İlk 10 saniyede **ölüm imkânsız** | Oyuncu önce kontrolü tanısın |
| **Hiç yazılı tutorial yok** | Tasarımla öğret |
| **Dash burada tanıtılmaz** | Bölüm 2–3'e kalsın; bir seferde bir şey |
| Bittiğinde öğrenmiş olacakları | Hareket, zıplama, para, boşluk, diken |

---

## Ritim (beat sheet)

```
zorluk
  │                              ╱╲
  │                    ╱╲       ╱  ╲
  │           ╱╲      ╱  ╲     ╱    ╲   ╱╲
  │  ________╱  ╲____╱    ╲___╱      ╲_╱  ╲___
  └────────────────────────────────────────────
    1   2    3    4    5    6    7    8    9
   giriş    zıpla  boşluk nefes diken  birleş bitiş
```

| # | Süre | İçerik | Öğrettiği | Ölüm? |
|---|---|---|---|---|
| 1 | 0–6 sn | 12 birim düz zemin, 3 para | Hareket, toplama | ✘ |
| 2 | 6–10 sn | **1,2 birim** basamak | Zıplama var | ✘ |
| 3 | 10–16 sn | **2,2 birim** basamak, para yayı | Gerçek zıplama | ✘ |
| 4 | 16–22 sn | **2,5 birim** boşluk, para yayı | Boşluk geçme | ✔ ilk kez |
| 5 | 22–27 sn | 8 birim düz, 3 para | **Nefes** | ✘ |
| 6 | 27–34 sn | Diken, yanından geniş geçit | Diken = ölüm | ✔ kolay kaçınılır |
| 7 | 34–41 sn | **3,5 birim** boşluk | Zorluk artışı | ✔ |
| 8 | 41–48 sn | **4,0 birim** boşluk + karşıda diken | İki şey birden | ✔ |
| 9 | 48–53 sn | Düz zemin, bayrak | Rahatlama | ✘ |

**Toplam ≈ 53 saniye.** Hedef aralık 30–90 sn.

---

## Her sayının gerekçesi

Hiçbiri göz kararı değil — hepsi ölçülen cetvelden:

| Öğe | Değer | Maks.ın %'si | Neden bu |
|---|---|---|---|
| İlk basamak | 1,2 birim | %40 | **Min. zıplama 1,5** — tuşa dokunmak yeter, "bedava" hissettirir |
| İkinci basamak | 2,2 birim | %73 | Gerçek bir zıplama gerekiyor ama rahat |
| İlk boşluk | 2,5 birim | %47 | Cetvelde "çok kolay" — düşünmeden geçilir |
| İkinci boşluk | 3,5 birim | %66 | "Kolay" — biraz dikkat |
| Üçüncü boşluk | 4,0 birim | %75 | "Normal" — bölüm 1'in tavanı |

**Bölüm 1'de %75'in üstüne çıkmıyoruz.** 4,6+ birimlik boşluklar (zor) ve
5,5+ (dash zorunlu) sonraki bölümlere.

---

## Öğretme kalıbı uygulaması

Dikenler için dört adım:

| Adım | Nerede | Nasıl |
|---|---|---|
| **1. Tanıt** | Beat 6 | Diken yerde duruyor, yanından geçmek için 3 birim geniş yol var. Görürsün, dokunmadan geçersin. |
| **2. Uygulat** | Beat 6 sonu | Geçit daralır (1,5 birim) — dikkat gerekir ama hâlâ kolay |
| **3. Zorlaştır** | Beat 8 | Dikenin üstünden zıplaman gerekiyor |
| **4. Birleştir** | Beat 8 | Boşluk + diken aynı anda |

Boşluk için aynı kalıp: 2,5 → 3,5 → 4,0 → 4,0 + diken.

---

## Yönlendirme

Yazı yok, o yüzden paralar konuşuyor:

```
Beat 3 — zıplama yayını paralarla çiz:

              ○ ○ ○
            ○       ○
          ○           ○
    ▓▓▓▓▓▓            ▓▓▓▓▓▓▓
```

Oyuncu "2,2 birim yukarı zıplamalıyım" diye düşünmez — paraları takip eder
ve doğru yeri bulur.

Beat 4'te aynısı boşluk için. Beat 6'da paralar **dikenin uzağından** geçer;
güvenli yolu işaret eder.

---

## Kontrol listesi

- [ ] İlk 10 saniyede ölüm imkânsız
- [ ] Hiç yazılı tutorial yok
- [ ] En az 2 nefes alanı var (beat 1, beat 5)
- [ ] Hiçbir boşluk 4,0'ı geçmiyor
- [ ] Hiçbir basamak 2,5'i geçmiyor
- [ ] Dash gerekmiyor
- [ ] Paralar rota gösteriyor
- [ ] 5 saniye testi: ekrana bakınca nereye gideceğin belli
- [ ] Bir başkası takılmadan bitirdi
