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

| Ölçüm | Değer | Nasıl ölçülür |
|---|---|---|
| Maks. zıplama yüksekliği | ___ birim | Dur, zıpla, tuşu basılı tut |
| Min. zıplama yüksekliği | ___ birim | Dur, zıpla, tuşu hemen bırak |
| Yerinden zıplama mesafesi | ___ birim | Dur, zıpla, havada yön ver |
| **Koşarak zıplama mesafesi** | ___ birim | Tam hızda koş + zıpla |
| **Koşarak zıplama + dash** | ___ birim | Tam hızda koş + zıpla + dash |
| Havada kalma süresi | ___ sn | Zıpla → yere değene kadar |

Son iki satır en önemlisi: aralarındaki fark, "dash gerektiren boşluk"
ile "dash'siz geçilen boşluk" arasındaki tasarım alanını verir.

Ölçüm script'i [Epic 02](02-karakter-hissiyati.md#4-sayıları-ölç-ve-yaz--bu-adımı-atlama)
içinde hazır.

---

## Zorluk tablosu

Ölçümleri yaptıktan sonra bunu doldur:

| Boşluk | Zorluk | Dash gerekir mi |
|---|---|---|
| ___ – ___ birim | Çok kolay | Hayır |
| ___ – ___ birim | Normal | Hayır |
| ___ – ___ birim | Zor | Hayır (ama kolaylaştırır) |
| ___ – ___ birim | Dash zorunlu | **Evet** |
| ___ + birim | İmkânsız — asla kullanma | — |

---

## Değişiklik günlüğü

| Tarih | Ne değişti | Neden |
|---|---|---|
| 14.09.2026 | Dash eklendi (18 / 0.16 / 0.35) | İmza mekaniği seçildi |
