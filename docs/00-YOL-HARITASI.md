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

> ⚠️ **Epic numaraları sıra değil, konu numarasıdır.** Kilometre taşları
> onları konuya göre değil **amaca göre** grupluyor, o yüzden sıra
> atlamalı ilerliyor: 01 → 02 → 03 → 04 → **09 → 10** → 05 → 06...
>
> Sebep: M1'in işi "tek bölüm ama bitmiş gibi". Bunun için ölüm (09) ve
> checkpoint (10) şart; tilemap (05) ve düşman çeşitliliği (06) değil.
> Onlar 12 bölüm yaparken lazım olacak.
>
> **Takip edeceğin sıra: kilometre taşları.**

### M1 — Dikey Dilim · tek bölüm, bitmiş gibi

| Sıra | # | Epic | Neden bu taşta | Süre |
|---|---|---|---|---|
| 1 | [01](01-oyun-vizyonu.md) | Oyun vizyonu ve kapsam | Ne yaptığını bilmeden kod yazılmaz | 2–3 saat |
| 2 | [02](02-karakter-hissiyati.md) | Karakter hissiyatı | Platformcunun %70'i budur | 1 hafta |
| 3 | [03](03-kamera.md) | Kamera | Kötü kamera iyi kontrolü mahveder | 2–3 gün |
| 4 | [04](04-seviye-tasarimi-prensipleri.md) | Seviye tasarımı | Bölüm yapmadan önce nasıl yapılır öğren | 3–4 gün |
| 5 | [09](09-can-hasar-olum.md) | Can, hasar, ölüm | Bölüm ölümsüz test edilemez | 3–4 gün |
| 6 | [10](10-checkpoint-ve-kayit.md) | Checkpoint ve kayıt | Checkpoint'siz bölüm bitirilemez | 4–5 gün |

### M2 — İçerik · 10–12 bölüme çıkar

| Sıra | # | Epic | Neden bu taşta | Süre |
|---|---|---|---|---|
| 7 | [05](05-tilemap.md) | Tilemap ile bölüm inşası | 12 bölümü elle dizemezsin | 4–5 gün |
| 8 | [06](06-dusmanlar.md) | Düşmanlar | Çeşitlilik 12 bölümde gerekir | 1 hafta |
| 9 | [07](07-tehlikeler-ve-engeller.md) | Tehlikeler ve engeller | Aynı sebep | 5–6 gün |
| 10 | [08](08-toplanabilirler.md) | Toplanabilirler ve sırlar | Aynı sebep | 3–4 gün |
| 11 | [17](17-zorluk-ve-dengeleme.md) | Zorluk eğrisi | 12 bölüm olmadan eğri çizilemez | 1 hafta |

### M3 — Sunum · iyi görünsün ve duyulsun

| Sıra | # | Epic | Süre |
|---|---|---|---|
| 12 | [11](11-sanat-ve-sprite.md) | Sanat ve sprite üretimi | 1–2 hafta |
| 13 | [12](12-animasyon.md) | Animasyon | 1 hafta |
| 14 | [13](13-ses-ve-muzik.md) | Ses ve müzik | 4–5 gün |
| 15 | [14](14-juice.md) | Juice | 4–5 gün |

### M4 — Paketleme ve Yayın

| Sıra | # | Epic | Süre |
|---|---|---|---|
| 16 | [15](15-ui-ve-menuler.md) | UI ve menüler | 1 hafta |
| 17 | [16](16-sahne-akisi.md) | Sahne akışı ve ilerleme | 4–5 gün |
| 18 | [18](18-test-ve-hata-ayiklama.md) | Test ve hata ayıklama | 5–6 gün |
| 19 | [19](19-performans.md) | Performans | 2–3 gün |
| 20 | [20](20-build-ve-yayinlama.md) | Build ve yayınlama | 3–4 gün |

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

**Yapılış sırasına göre** — epic numarasına göre değil.

| Sıra | # | Epic | Taş | Durum | Tarih |
|---|---|---|---|---|---|
| 1 | 01 | Vizyon | M1 | ✅ | 14.09.2026 |
| 2 | 02 | Hissiyat | M1 | ✅ | 14.09.2026 |
| 3 | 03 | Kamera | M1 | ✅ | 14.09.2026 |
| 4 | 04 | Seviye tasarımı | M1 | ✅ | 14.09.2026 |
| 5 | 09 | Can/Ölüm | M1 | ✅ | 14.09.2026 |
| 6 | 10 | Checkpoint/Kayıt | M1 | ✅ | 14.09.2026 |
| — | — | **M1 KAPISI: Bölüm 1'i birine oynat** | | ✅ | 15.09.2026 — 22,96 sn, 0 ölüm |
| 7 | 05 | Tilemap | M2 | ✅ | 14.09.2026 |
| 8 | 06 | Düşmanlar | M2 | ✅ | 14.09.2026 |
| 9 | 07 | Tehlikeler | M2 | ✅ | 14.09.2026 |
| 10 | 08 | Toplanabilirler | M2 | ✅ | 15.09.2026 |
| 11 | 17 | Zorluk | M2 | 🔧 | ölçüm kuruldu; *3 kişi testi* Epic 16'dan sonra |
| 12 | 11 | Sanat | M3 | ⬜ | **sıradaki** — tek geri bildirim buraya işaret ediyor |
| 13 | 12 | Animasyon | M3 | ⬜ | |
| 14 | 13 | Ses | M3 | ⬜ | |
| 15 | 14 | Juice | M3 | ⬜ | |
| 16 | 15 | UI | M4 | ⬜ | |
| 17 | 16 | Sahne akışı | M4 | ⬜ | |
| 18 | 18 | Test | M4 | ⬜ | |
| 19 | 19 | Performans | M4 | ⬜ | |
| 20 | 20 | Yayın | M4 | ⬜ | |

---

## Kod envanteri

> **Okuma kılavuzu:** epic dosyalarındaki kod örneklerinin çoğu **henüz
> yazılmamış** dosyalara aittir — onlar o epic'in işidir. Aşağıdaki tablo
> şu an gerçekten var olanları gösterir.
>
> ✅ oyunda kullanılıyor · 🔧 geliştirme aracı (oyuna girmez)

### Çalışma zamanı — `Assets/Scripts/`

| Dosya | Durum | Not |
|---|---|---|
| `Core/GameManager.cs` | ✅ | Skor, can, checkpoint, ölüm çizgisi |
| `Core/Rigidbody2DExtensions.cs` | ✅ | Unity 2022 / Unity 6 uyumluluğu |
| `Player/PlayerController2D.cs` | ✅ | Coyote time, jump buffer, değişken zıplama, **dash** |
| `Player/PlayerHealth.cs` | ✅ | Ölüm ve respawn *(Epic 09'da geliştirilecek)* |
| `Camera/CameraFollow.cs` | ✅ | Yere değilen yükseklik takibi, sarsıntı, sınırlar |
| `Gameplay/Coin.cs` | ✅ | |
| `Gameplay/Hazard.cs` | ✅ | |
| `Gameplay/Patroller.cs` | ✅ | *(Epic 06'da `EnemyBase`'e taşınacak)* |
| `Gameplay/MovingPlatform.cs` | ✅ | Yolcu taşıyor |
| `Gameplay/Checkpoint.cs` | ✅ | Oturum içi *(Epic 10'da kalıcı kayıt)* |
| `Gameplay/LevelGoal.cs` | ✅ | |
| `Gameplay/ParallaxLayer.cs` | ✅ | |
| `UI/HudController.cs` | ✅ | *(Epic 15'te TMP'ye geçecek)* |
| `DevTools/JumpMeasure.cs` | 🔧 | Zıplama ölçümü (Epic 02) |
| `DevTools/FeelTuner.cs` | 🔧 | Hazır ayar karşılaştırması, sarsıntı testi |
| `DevTools/TestRoomLabel.cs` | 🔧 | Scene view ölçü etiketleri |
| `DevTools/TestRoomRespawn.cs` | 🔧 | Test odasında boşluğa düşünce başa dön |

### Editor — `Assets/Editor/`

| Dosya | Durum | Not |
|---|---|---|
| `SpriteFactory.cs` | ✅ | 8 sprite'ı kod ile üretir, var olanı atlar |
| `LevelCursor.cs` | ✅ | Bölümü adım adım tarif etme + **cetvel doğrulaması** |
| `Level01Builder.cs` | ✅ | Bölüm 1 (bkz. [BOLUM-01.md](BOLUM-01.md)) |
| `LevelBuilder.cs` | ✅ | Eski demo bölüm + proje ayarları + ortak yardımcılar |
| `CameraBoundsTool.cs` | ✅ | Kamera sınırı hesabı (tek uygulama) |
| `TestRoomBuilder.cs` | 🔧 | Ölçülü test odası |

---

## Tools menüsü

| Menü | Ne yapar |
|---|---|
| **Ornek Bolumu Olustur** | Eski demo bölüm (düşmanlı, hareketli platformlu) |
| **Bolum 1'i Kur** | Gerçek Bölüm 1 — `Level01.unity` üzerine yazar |
| **Test Odasi Olustur** | Ölçülü test odası — oyuna girmez |
| **Kamera Sinirlarini Hesapla** | Açık sahnenin sınırlarını ölçüp yazar |
| **Sadece Grafikleri Uret** | 8 PNG'yi **yeniden** üretir (onay sorar) |
| **Sadece Proje Ayarlarini Uygula** | Layer, tag, input ayarları |

---

## Şu an neredesin

**M1 — Dikey Dilim: 4/6 epic bitti.**

| Epic | Durum | Çıktı |
|---|---|---|
| 01 Vizyon | ✅ | [VIZYON.md](VIZYON.md), [SONRA.md](SONRA.md) |
| 02 Hissiyat | ✅ | [AYARLAR.md](AYARLAR.md) + ölçümler + zorluk cetveli |
| 03 Kamera | ✅ | Dikey takip, sarsıntı, otomatik sınır aracı |
| 04 Seviye tasarımı | ✅ | [BOLUM-01.md](BOLUM-01.md) + `LevelCursor` |
| 09 Can/Ölüm | ✅ | 0,45 sn ölüm, hit stop, ölüm sayacı |
| 10 Checkpoint/Kayıt | ✅ | IResettable + JSON kayıt |

**Ölçülen değerler** (bölüm tasarımının cetveli):

| | Değer |
|---|---|
| Maks. zıplama yüksekliği | **3,03** birim |
| Maks. mesafe (dash'siz) | **5,31** birim |
| Maks. mesafe (dash'li) | **7,62** birim |
| Min. zıplama (dokunuş) | ~1,5 birim |

**Açık madde:** Bölüm 1'i başkasına oynatma — M1 kapısına ertelendi
(Epic 09 ve 10 bitmeden geri bildirim yanıltıcı olur).

**M1'in altı epic'i de bitti.** Sıradaki adım kod değil: **Bölüm 1'i birine oynat.**
