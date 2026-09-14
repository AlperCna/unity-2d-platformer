# Yol Haritası — İlk Unity Oyunun

Bu klasör, elindeki 2D platform iskeletini **baştan sona oynanabilen, yayınlanmış
bir oyuna** dönüştürmenin planı. 20 epic'e bölünmüş; her biri kendi dosyasında,
çalışır kod örnekleriyle.

---

## İçindekiler

1. [Önce bunu oku: kapsam](#önce-bunu-oku-kapsam)
2. [Dört kilometre taşı](#dört-kilometre-taşı)
3. [Bağımlılık haritası](#bağımlılık-haritası)
4. [Epic listesi](#epic-listesi)
5. [Haftalık çalışma planı](#haftalık-çalışma-planı)
6. [Risk listesi](#risk-listesi)
7. [Her epic dosyasının yapısı](#her-epic-dosyasının-yapısı)
8. [Çalışma disiplini](#çalışma-disiplini)
9. [İlerleme takibi](#i̇lerleme-takibi)
10. [Şu an neredesin](#şu-an-neredesin)

---

## Önce bunu oku: kapsam

İlk oyununu bitirenlerle bitiremeyenler arasındaki fark yetenek değil, **kapsam**.

Bitmeyen ilk oyunların neredeyse tamamı şu kalıba uyar:

```
Ay 1  → Karakter kontrolü yazılır. Eğlencelidir, iyi gider.
Ay 2  → Envanter sistemi, diyalog sistemi, beceri ağacı yazılır.
        Hâlâ tek bölüm yok.
Ay 3  → "Önce sanat tarzına karar vereyim" — 3 hafta sprite denemesi.
Ay 4  → Motivasyon biter. Proje klasörü bir daha açılmaz.
```

Fark ettiysen: **hiç bölüm yapılmadı.** Oyun hiç oynanmadı. Sistem yazmak
somut ve ödüllendirici hissettirir, bölüm yapmak ise belirsiz ve zordur —
o yüzden herkes sistem yazmaya kaçar.

Bu planın hedefi:

> **10–15 dakikada bitirilen, 10–12 bölümlük, tek imza mekanikli bir platform oyunu.**

Küçük geliyorsa iyi. Karşılaştırma için:

| Oyun | İlk prototip | Kaç kişi |
|---|---|---|
| *Celeste* | 4 gün (Pico-8 jam) | 2 |
| *Super Meat Boy* | 3 hafta (Flash) | 2 |
| *Downwell* | ~1 ay | 1 |

Hepsi küçük başladı ve **bitirildi**. Büyük başlayanların çoğunu hiç duymadın,
çünkü çıkmadılar.

Her epic'te **"v1'de yapma"** başlığı var. Oraya yazdıklarımı gerçekten yapma.
Hepsi iyi fikir — ama ikinci oyunun fikri.

---

## Dört kilometre taşı

Epic'leri tek tek değil, kilometre taşı olarak düşün. Her taşın sonunda
**oynanabilir bir şey** olmalı; yarım bırakılmış bir sistem değil.

### M1 — Dikey Dilim · 2–3 hafta

Tek bir bölüm, ama **bitmiş gibi**. Hissiyat oturmuş, sesi var, ölünce doğru
yerde doğuyorsun, checkpoint çalışıyor.

Bu bölüm oyunun geri kalanının **kalite çıtasını** belirler. Buradaki her
kararı 11 kez daha tekrarlayacaksın; ucuz atlatırsan 11 kez ucuz olur.

→ Epic **01, 02, 03, 04, 09, 10**

**Bitti sayılır:** bir arkadaşına oynatıp, sen hiçbir şey söylemeden
"bu iyiymiş" dedirtiyorsan.

**Bitmedi sayılır:** "şurası daha bitmedi ama..." diye açıklama yapman gerekiyorsa.

### M2 — İçerik · 4–6 hafta

Bölüm sayısını 10–12'ye çıkar. **Yeni sistem yazma** — var olanlarla bölüm yap.

Bu en uzun ve en sıkıcı kilometre taşı. Yeni sistem yazmak isteyeceksin çünkü
bölüm yapmak zor ve belirsiz. Direnç göster. Oyununu oyun yapan şey burası.

→ Epic **05, 06, 07, 08, 17**

**Bitti sayılır:** baştan sona oynanabiliyor, zorluk gerçekten artıyor ve
hiçbir bölüm "burayı sonra düzeltirim" durumunda değil.

### M3 — Sunum · 3–4 hafta

Oyun iyi *hissettiriyordu*; şimdi iyi *görünsün ve duyulsun*.

Bu taş, emeğe oranla en görünür farkı yaratan taştır. Aynı oyunu M2 sonu ve
M3 sonu hâliyle gösterirsen insanlar ikincisini "çok daha profesyonel" bulur —
oynanış hiç değişmemiş olsa bile.

→ Epic **11, 12, 13, 14**

**Bitti sayılır:** sesi kapatınca oyun eksik hissettiriyor.

### M4 — Paketleme ve Yayın · 2–3 hafta

Menüsü, kaydı, ayarları olan; indirilip oynanabilen bir dosya.

→ Epic **15, 16, 18, 19, 20**

**Bitti sayılır:** itch.io'da paylaşabileceğin bir linki var.

---

**Toplam: 13,5 hafta ≈ 3–4 ay**, haftada 8–10 saat çalışmayla. Daha hızlı
giderse sevin; yavaş giderse normal — tahminler her zaman iyimserdir.

---

## Bağımlılık haritası

Hangi epic'in hangisinden önce gelmesi gerektiği. Ok yönü "önce bu bitmeli":

```
01 Vizyon
 └─► 02 Hissiyat ─┬─► 03 Kamera
                  ├─► 04 Seviye tasarımı ─┬─► 05 Tilemap ─┬─► 06 Düşmanlar
                  │                       │               ├─► 07 Tehlikeler
                  │                       │               └─► 11 Sanat
                  │                       └─► 08 Toplanabilirler
                  ├─► 09 Can/Ölüm ─► 10 Checkpoint/Kayıt ─► 15 UI ─► 16 Sahne akışı
                  ├─► 12 Animasyon ──┐
                  └─► 13 Ses ────────┴─► 14 Juice
                                          │
    16 Sahne akışı ─► 17 Zorluk ─► 18 Test ─► 19 Performans ─► 20 Yayın
```

**Paralel yapılabilenler:** 03 ve 04 aynı anda; 12 ve 13 aynı anda.

**Kilit noktalar** (bunlar gecikirse her şey gecikir): 02 Hissiyat,
05 Tilemap, 16 Sahne akışı.

---

## Epic listesi

### Faz 1 — Temel
| # | Epic | Neden önce | Süre |
|---|---|---|---|
| [01](01-oyun-vizyonu.md) | Oyun vizyonu ve kapsam | Ne yaptığını bilmeden kod yazılmaz | 2–3 saat |
| [02](02-karakter-hissiyati.md) | Karakter hissiyatı | Platformcunun %70'i budur | 1 hafta |
| [03](03-kamera.md) | Kamera | Kötü kamera iyi kontrolü mahveder | 2–3 gün |
| [04](04-seviye-tasarimi-prensipleri.md) | Seviye tasarımı prensipleri | Bölüm yapmadan önce nasıl yapılır öğren | 3–4 gün |

### Faz 2 — İçerik sistemleri
| # | Epic | Süre |
|---|---|---|
| [05](05-tilemap.md) | Tilemap ile bölüm inşası | 4–5 gün |
| [06](06-dusmanlar.md) | Düşmanlar ve davranışlar | 1 hafta |
| [07](07-tehlikeler-ve-engeller.md) | Tehlikeler, tuzaklar, hareketli parçalar | 5–6 gün |
| [08](08-toplanabilirler.md) | Toplanabilirler ve sırlar | 3–4 gün |
| [09](09-can-hasar-olum.md) | Can, hasar, ölüm | 3–4 gün |
| [10](10-checkpoint-ve-kayit.md) | Checkpoint ve kayıt sistemi | 4–5 gün |

### Faz 3 — Sunum
| # | Epic | Süre |
|---|---|---|
| [11](11-sanat-ve-sprite.md) | Sanat ve sprite üretimi | 1–2 hafta |
| [12](12-animasyon.md) | Animasyon | 1 hafta |
| [13](13-ses-ve-muzik.md) | Ses ve müzik | 4–5 gün |
| [14](14-juice.md) | Juice — his veren küçük efektler | 4–5 gün |

### Faz 4 — Oyun haline getirme
| # | Epic | Süre |
|---|---|---|
| [15](15-ui-ve-menuler.md) | UI ve menüler | 1 hafta |
| [16](16-sahne-akisi.md) | Sahne akışı ve ilerleme | 4–5 gün |
| [17](17-zorluk-ve-dengeleme.md) | Zorluk eğrisi ve dengeleme | 1 hafta |
| [18](18-test-ve-hata-ayiklama.md) | Test ve hata ayıklama | 5–6 gün |
| [19](19-performans.md) | Performans | 2–3 gün |
| [20](20-build-ve-yayinlama.md) | Build ve yayınlama | 3–4 gün |

---

## Haftalık çalışma planı

Haftada ~9 saat varsayımıyla kaba bir takvim. Sapma normal — bunu takvim
değil, **ritim** olarak kullan.

| Hafta | Odak | Hafta sonunda elinde ne var |
|---|---|---|
| 1 | Epic 01, 02 başla | Vizyon belgesi + test odası, ayarlanmış zıplama |
| 2 | Epic 02 bitir, 03, 04 | Hissiyat oturmuş, kamera iyi, kâğıtta 3 bölüm |
| 3 | Epic 09, 10 | **M1 bitti** — tek bölüm, tam döngü |
| 4 | Epic 05 | Tilemap akışı, prefab'ler, bölüm şablonu |
| 5–6 | Epic 06, 07 | Düşmanlar ve tehlikeler çalışıyor |
| 7 | Epic 08 + bölüm yapımı | 4–5 bölüm |
| 8–9 | Bölüm yapımı + Epic 17 | **M2 bitti** — 10–12 bölüm |
| 10 | Epic 11 | Sanat tutarlı |
| 11 | Epic 12, 13 | Animasyon + ses |
| 12 | Epic 14 | **M3 bitti** — oyun "canlı" |
| 13 | Epic 15, 16 | Menüler, sahne akışı |
| 14 | Epic 18, 19, 20 | **M4 bitti** — yayında |

**Hafta 8–9 en tehlikeli dönem.** Yeni bir şey yapmıyorsun, sadece bölüm
üretiyorsun. Sıkılacaksın. Buradan kaçmanın yolu: her bölümü bitirdiğinde
oyna ve küçük bir şey ekle (yeni bir tehlike kombinasyonu, bir sır).

---

## Risk listesi

Gerçekleşmesi muhtemel sorunlar ve şimdiden hazırlığı:

| Risk | Olasılık | Erken uyarı işareti | Ne yapmalı |
|---|---|---|---|
| Kapsam büyümesi | **Çok yüksek** | "Şunu da eklesem..." | Vizyon belgesini aç, oku. Yoksa ekleme. |
| M2'de motivasyon kaybı | **Yüksek** | Proje 1 hafta açılmadı | Bölüm sayısını 12'den 8'e indir. Bitmek > büyük olmak. |
| Sanat tarzında takılma | Orta | 3 gün sprite denemesi | Hazır asset paketi al (Kenney.nl). Kendi sanatın ikinci oyunda. |
| Hissiyat bir türlü oturmuyor | Orta | 1 haftadan uzun ayar | Referans oyunu aç, yan yana oyna, farkı yaz. |
| "Bu yeterince iyi değil" | **Yüksek** | Yayını erteliyorsun | Yayınla. İlk oyun bir öğrenme aracıdır, başyapıt değil. |
| Teknik bir şey çözülemiyor | Düşük | 2 saat takıldın | Dur ve sor. Tek başına 6 saat harcama. |

---

## Her epic dosyasının yapısı

```
Amaç             → bu epic bittiğinde ne değişmiş olacak
Ön koşul         → önce hangi epic'ler bitmeli
Neden bu epic var→ bu konunun oyuna gerçek etkisi
Elindeki altyapı → projede zaten hazır olan kısım
Görevler         → somut, işaretlenebilir adımlar + çalışır kod
Kabul kriteri    → "bitti" ne demek
Tuzaklar         → burada takılan insanların takıldığı yerler
v1'de yapma      → cazip ama erteleyeceğin şeyler
```

Kod blokları **kopyala-yapıştır çalışır** durumda yazıldı. Değiştirmen gereken
yerler `// DEĞİŞTİR:` yorumuyla işaretli.

---

## Çalışma disiplini

**Her oturumu oynanabilir bitir.** Yarım kalmış bir sistemle günü kapatma;
bozuksa `git checkout .` ile geri al. Ertesi gün bozuk projeye dönmek
motivasyon katilidir.

**Haftada bir kez baştan sona oyna.** Sadece kendi eklediğin kısmı değil,
tamamını. Bozulan şeyleri erken yakalarsın — geç yakalanan bozukluk 10 kat
pahalıdır.

**Bir şey 2 saatten fazla sürüyorsa dur.** Muhtemelen yanlış yoldasın.
Sor, ara, veya basitleştir.

**Her kilometre taşında commit at.** `git init` yapıldıysa:

```bash
git add -A
git commit -m "M1: dikey dilim tamam"
git tag m1
```

**Bir "sonra" listesi tut.** `docs/SONRA.md` diye bir dosya aç. Aklına gelen
her iyi fikri oraya yaz ve **yapma**. İkinci oyunun tasarım belgesi
kendiliğinden oluşacak, sen de odağını kaybetmeyeceksin.

---

## İlerleme takibi

Bu tabloyu doldur, her epic bitince işaretle:

| # | Epic | Durum | Bitiş tarihi | Not |
|---|---|---|---|---|
| 01 | Vizyon | ✅ | 14.09.2026 | Dash + dengeli/klasik. [VIZYON.md](VIZYON.md) |
| 02 | Hissiyat | ✅ | 14.09.2026 | Ölçüldü: 3,03 / 5,31 / 7,62. [AYARLAR.md](AYARLAR.md) |
| 03 | Kamera | ✅ | 14.09.2026 | Dikey takip + sarsıntı + otomatik sınır aracı |
| 04 | Seviye tasarımı | ☐ | | |
| 05 | Tilemap | ☐ | | |
| 06 | Düşmanlar | ☐ | | |
| 07 | Tehlikeler | ☐ | | |
| 08 | Toplanabilirler | ☐ | | |
| 09 | Can/Ölüm | ☐ | | |
| 10 | Checkpoint/Kayıt | ☐ | | |
| 11 | Sanat | ☐ | | |
| 12 | Animasyon | ☐ | | |
| 13 | Ses | ☐ | | |
| 14 | Juice | ☐ | | |
| 15 | UI | ☐ | | |
| 16 | Sahne akışı | ☐ | | |
| 17 | Zorluk | ☐ | | |
| 18 | Test | ☐ | | |
| 19 | Performans | ☐ | | |
| 20 | Yayın | ☐ | | |

---

## Şu an neredesin

İskelet hazır ve doğrulandı:

| Sistem | Dosya | Durum |
|---|---|---|
| Karakter kontrolü | `Scripts/Player/PlayerController2D.cs` | ✅ Coyote time, jump buffer, değişken zıplama |
| Ölüm/respawn | `Scripts/Player/PlayerHealth.cs` | ✅ Temel çalışıyor |
| Kamera | `Scripts/Camera/CameraFollow.cs` | ✅ Ölü bölge, ileri bakış, sınırlar |
| Oyun durumu | `Scripts/Core/GameManager.cs` | ✅ Skor, can, checkpoint |
| Düşman | `Scripts/Gameplay/Patroller.cs` | ✅ Devriye + ezilebilir |
| Hareketli platform | `Scripts/Gameplay/MovingPlatform.cs` | ✅ Yolcu taşıyor |
| Tehlike | `Scripts/Gameplay/Hazard.cs` | ✅ Yönlü olabilir |
| Toplanabilir | `Scripts/Gameplay/Coin.cs` | ✅ Animasyonlu |
| Checkpoint | `Scripts/Gameplay/Checkpoint.cs` | ✅ |
| HUD | `Scripts/UI/HudController.cs` | ✅ Skor/can |
| Bölüm üreteci | `Editor/LevelBuilder.cs` | ✅ Tek tıkla örnek bölüm |
| Grafik üreteci | `Editor/SpriteFactory.cs` | ✅ 8 sprite kod ile |

Bu, **M1'in yaklaşık yarısı** demek. Eksik olan: hissiyat ayarı, sanat,
ses ve bölüm tasarımı.

Sıradaki adım: **[Epic 01 — Oyun vizyonu](01-oyun-vizyonu.md)**. Kod
yazmayacaksın, bir sayfa yazı yazacaksın. Atlamak isteyeceksin — atlama.
