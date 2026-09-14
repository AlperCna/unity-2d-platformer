# Karakter Ayarları

> Play modunda yapılan değişiklikler **kaybolur**. Beğendiğin değeri
> buraya yaz, sonra Play'den çıkıp Inspector'da kalıcı olarak gir.
>
> Son güncelleme: 14 Eylül 2026 — hedef his: **dengeli ve klasik**

---

## Şu anki değerler

Bunlar `Level01.unity` sahnesindeki `Player` nesnesinde kayıtlı.
Değiştirmek için: Hierarchy → `Player` → Inspector → **Player Controller 2D**

### Yatay hareket

| Değer | Şu an | Önerilen aralık | Ne yapar |
|---|---|---|---|
| Move Speed | **8** | 7–9 | Koşma hızı |
| Acceleration Time | **0.08** | 0.08–0.12 | Hızlanma süresi |
| Deceleration Time | **0.06** | 0.06–0.10 | Durma keskinliği |
| Air Control | **0.75** | 0.70–0.85 | Havada yön değiştirme |

### Zıplama

| Değer | Şu an | Önerilen aralık | Ne yapar |
|---|---|---|---|
| Jump Height | **3.2** | 3.0–3.5 | Yükseklik (birim) |
| Jump Apex Time | **0.38** | 0.38–0.44 | Tepeye çıkma süresi |
| Fall Gravity Multiplier | **1.9** | 1.6–2.0 | Düşüş ağırlığı |
| Jump Cut Multiplier | **0.45** | 0.40–0.55 | Kısa basınca kesme |
| Max Fall Speed | **22** | 20–25 | Terminal hız |
| Extra Jumps | **0** | 0 | Çift zıplama (dash var, gerekmiyor) |

### Toleranslar

| Değer | Şu an | Önerilen aralık | Ne yapar |
|---|---|---|---|
| Coyote Time | **0.10** | 0.10–0.14 | Kenardan sonra zıplama toleransı |
| Jump Buffer Time | **0.12** | 0.12–0.15 | Erken basılan zıpla hafızası |

### Dash *(yeni eklendi)*

| Değer | Varsayılan | Önerilen aralık | Ne yapar |
|---|---|---|---|
| Dash Speed | **18** | 16–22 | Dash hızı |
| Dash Duration | **0.16** | 0.12–0.20 | Ne kadar sürer |
| Dash Cooldown | **0.35** | 0.25–0.50 | İki dash arası bekleme |
| Dash End Speed Multiplier | **0.55** | 0.45–0.70 | Bitince hız ne kadar kesilir |
| Allow Diagonal Dash | **✔** | — | 8 yön mü, sadece yatay mı |

**Klasik his için not:** dash süresini uzun (0.18–0.20), hızını düşük (16–18)
tutmak daha "yumuşak" hissettirir. Kısa+hızlı (0.12 / 22) Celeste'in keskin
hissine yaklaşır — ama sen dengeli/klasik seçtin, uzun tarafta kal.

---

## ÖLÇÜMLER

**Bunları Epic 02'de doldur.** Bölüm tasarımının dilbilgisi bu sayılar
(bkz. [Epic 04](04-seviye-tasarimi-prensipleri.md)).

**Ölçüm tarihi: 14 Eylül 2026** — "Dengeli" ayarıyla, ölçüm pistinde.

| Ölçüm | Teorik | **Ölçülen** | Fark |
|---|---|---|---|
| **Maks. zıplama yüksekliği** | 3,20 | **3,03** | −5% (ayrık fizik) |
| **Maks. mesafe (dash'siz)** | 5,19 | **5,31** | +2% |
| **Maks. mesafe (dash'li)** | 8,10 | **7,62** | −6% (tepe zamanlaması) |
| Dash kazancı | +2,91 | **+2,31** | |

### Dash neden teoriden az kazandırıyor

Dash yatay hızı 18'e sabitlerken **dikey hızı da sıfırlıyor**. Tepe noktasından
önce dash atarsan kalan yükselme hızını kaybedersin — zıplaman alçalır, havada
daha az kalırsın, mesafe düşer.

Ölçümde görüldü: dash'li zıplamanın yüksekliği 2,85 çıktı (maksimum 3,03 iken).
Teorik 8,10 ancak tam tepe noktasında dash atınca elde edilir ve bunu insan eli
her seferinde tutturamaz.

**Tasarımda 7,62 kullan.** Oyuncudan mükemmel zamanlama beklemek haksızlıktır.

### Neden teorik ≠ gerçek

Unity fiziği saniyede 50 ayrık adımda çalışıyor (yarı-örtük Euler). Her adımda
önce hız azalıyor, sonra konum değişiyor — bu, tepe noktasını yaklaşık
`(v₀ × dt) / 2` kadar düşürüyor:

```
v₀ = g × t = 44,32 × 0,38 = 16,84 birim/sn
kayıp = (16,84 × 0,02) / 2 = 0,17 birim
3,20 − 0,17 = 3,03      ← ölçülen değerle birebir
```

**Bölüm tasarımında cetvel olarak 3,03 kullan, 3,20'yi değil.** 0,17 birimlik
farka güvenip platform koyarsan, oyuncu oraya "az kalanla" ulaşamaz ve bunu
senin hatan olarak değil, oyunun bozukluğu olarak algılar.

`PlayerController2D.MovementSettings.RealJumpHeight` bu düzeltmeyi yapıyor.

Son iki satır en önemlisi: aralarındaki fark, "dash gerektiren boşluk"
ile "dash'siz geçilen boşluk" arasındaki tasarım alanını verir.

Ölçüm script'i [Epic 02](02-karakter-hissiyati.md#4-sayıları-ölç-ve-yaz--bu-adımı-atlama)
içinde hazır.

---

## ZORLUK CETVELİ

Bölüm tasarımının dilbilgisi. Her boşluk ve her platform yüksekliği buradan seçilir.

### Yatay boşluklar

| Boşluk | Maks.ın % | Zorluk | Dash | Nerede kullan |
|---|---|---|---|---|
| 2,0 – 2,6 | %38–49 | Çok kolay | — | Bölüm 1–2, ritim |
| 3,0 – 3,6 | %56–68 | Kolay | — | Her yerde |
| 4,0 – 4,4 | %75–83 | Normal | — | Orta bölümler |
| 4,6 – 4,9 | %87–92 | Zor | — | Geç bölümler |
| 5,0 – 5,3 | %94–100 | Maksimum | — | Çok seyrek, final |
| **5,5 – 6,5** | — | Kolay | **Zorunlu** | Dash öğretimi |
| **6,6 – 7,0** | — | Normal | **Zorunlu** | Orta-geç bölümler |
| **7,1 – 7,4** | — | Zor | **Zorunlu** | Final |
| 5,4 | — | **Kullanma** | — | Belirsiz bölge: dash'siz imkânsız, dash'li çok kolay |
| 7,5+ | — | **İmkânsız** | — | Asla |

### Platform yükseklikleri

| Yükseklik | Zorluk |
|---|---|
| 1,0 – 1,8 | Kolay basamak |
| 2,0 – 2,5 | Normal |
| 2,6 – 2,9 | Zor, tam zamanlama |
| 3,0 | Maksimum |
| 3,1+ | **İmkânsız** |

Dash yatay olduğu için yüksekliği **hiç etkilemiyor** — bu yüzden tek bir
yükseklik tablosu yeterli. 8 yönlü dash seçseydik her satırı iki kez
hesaplamamız gerekirdi.

### İki tür boşluk = tasarım aracı

Tek mekanikle çeşitlilik üretmenin yolu:

- **5,3'ün altı** → dash'siz geçilir. Ritim kurar, akış sağlar.
- **5,5'in üstü** → dash zorunlu. Karar noktası yaratır, oyuncuyu durdurup düşündürür.

5,4 birimlik boşluktan kaçın: dash'siz imkânsız, dash'li bedava. Hiçbir şey öğretmez.

---

## Değişiklik günlüğü

| Tarih | Ne değişti | Neden |
|---|---|---|
| 14.09.2026 | Dash eklendi (18 / 0.16 / 0.35) | İmza mekaniği seçildi |
