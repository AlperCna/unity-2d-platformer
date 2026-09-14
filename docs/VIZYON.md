# VİZYON

> Bu belge projenin pusulası. Yeni bir fikir geldiğinde buraya bak:
> vizyona uymuyorsa `SONRA.md`'ye yaz ve **yapma**.
>
> Son güncelleme: 14 Eylül 2026

---

## Tek cümle

> **Sıçradıkça sönen, yere değdikçe yeniden dolan bir kıvılcımın,
> rüzgârlı bir vadiyi geçme hikâyesi.**

**Bu cümleyi değiştirebilirsin — tema senin.** Mekanik ve his zaten
kararlaştırıldı; aşağıdaki her şey onlardan türüyor ve tema değişse de geçerli.

İki alternatif, aynı mekaniğe oturuyor:

| Tema | Cümle | Neden işe yarar |
|---|---|---|
| **Kıvılcım** *(seçili)* | Yukarıdaki | Dash = enerji patlaması; yere değince dolması doğal. Mevcut turuncu sprite'a birebir uyuyor. |
| Kurye robot | "Paketi teslim etmek için fabrikayı geçmeye çalışan küçük bir kurye robot." | Dash = itki motoru. Endüstriyel tema, tilemap çizmesi kolay. |
| Kâğıt uçak | "Rüzgârla taşınan, hamle hakkı sınırlı bir kâğıt uçak." | Dash = rüzgâr hamlesi. Sade sanat, az çizim. |

**Neden "kıvılcım" önerildi:** dash mekaniğini tema açıklıyor. Oyuncu
"neden yere değince dash yenileniyor?" diye sormaz — kıvılcım yere
değince yeniden tutuşur. Mekanik ve tema birbirini destekliyor.

---

## Üç sütun

**1. Okunabilirlik** — Oyuncu ölmeden önce ölümü görebilmeli. Şüpheye
düştüğümüzde oyuncu lehine karar veririz: collider'lar görselden küçük,
tehlikelerin uyarısı var, hiçbir şey ekran dışından gelmez.

**2. Akıcılık** — Dash, zıplamayı kesmez; onu uzatır. İyi oynayan biri
bölümü hiç durmadan geçebilmeli. Bekleme, yükleme, gereksiz duraklama yok.

**3. Adalet** — Ölüm cezası neredeyse sıfır (0.45 sn), checkpoint'ler cömert.
Zorluk "tekrar ettirmek"ten değil, "öğretmek"ten gelir.

---

## Çekirdek döngü

```
Bölüme gir
  → önündeki boşluğu/tehlikeyi oku
    → zıpla, gerekiyorsa havada dash'le
      → yere değ, dash hakkın yenilensin
        → tekrar
          → başar veya öl (anında yeniden)
```

**30 saniyelik ritim:** zıpla–dash–in, zıpla–dash–in. Dash'in yerde
yenilenmesi bu ritmi kuruyor: havada bir hamle hakkın var, onu nerede
harcayacağına karar veriyorsun.

---

## İmza mekaniği: Dash

Havada ileri fırlama. Yerçekimi devre dışı, sabit hız, sabit süre.

| Kural | Değer | Neden |
|---|---|---|
| Yön | 8 yön (giriş yoksa baktığın yön) | Esneklik |
| Dash sırasında yerçekimi | Kapalı | Düz çizgi, tahmin edilebilir |
| Hak yenilenmesi | **Yere değince** | Havada sonsuz dash olmasın |
| Bekleme | 0.35 sn | Yerde spam edilmesin |
| Bitince | Hız %55'e düşer | "Fırlamış" hissi olmasın |

**Bölüm tasarımına etkisi:** dash, zıplama mesafesini yaklaşık **%55
artırıyor**. Yani iki tür boşluk tasarlayabilirsin:
- Dash'siz geçilebilen (kolay, ritim)
- Dash gerektiren (karar noktası)

Bu ikilik, tek mekanikle bölüm çeşitliliği üretmenin yolu.

---

## Kapsam

| | Hedef |
|---|---|
| Bölüm sayısı | **10–12** |
| Bölüm başına süre | 30–90 saniye |
| Toplam oynanış | 10–15 dakika |
| Düşman çeşidi | 3 (devriye, mermi atan, uçan) |
| Tehlike çeşidi | 5 (diken, boşluk, hareketli platform, düşen platform, aralıklı diken) |
| Mekanik | 1 imza (dash) + temel zıplama |
| Boss | **Yok** |
| Hikâye | En fazla 3 ekran yazı |
| Müzik | 2–3 parça |
| Platform | Windows |

Bu tablo **bitiş çizgisi**. Hepsi tamamlandığında oyun bitmiştir.

---

## Referanslar

**Super Mario Bros. / Celeste karışımı bir his.**

| Oyun | Alıyorum | Almıyorum |
|---|---|---|
| **Super Mario Bros.** | Dengeli zıplama hissi, affedici toleranslar, keşif için nefes alanları | Güç-yükseltmeleri, dünya haritası, 8 dünyalık yapı |
| **Celeste** | Dash mekaniği, sıfıra yakın ölüm cezası, bölüm başına tek fikir | Hikâye, diyalog, B-side'lar, aşırı zorluk |
| **Hollow Knight** | **Hiçbir şey.** Üç kişinin üç yılı — benim ölçeğimde değil. | |

Son satır kasıtlı: referansın senin ölçeğinde olmazsa kendi oyunun
hep yetersiz görünür.

---

## Bu oyunda OLMAYACAK şeyler

Buraya ekleme yaptıkça bitmeye yaklaşırsın.

- Hikâye, diyalog, ara sahne
- Envanter, para ekonomisi, mağaza
- Beceri ağacı, yükseltme
- İkinci bir mekanik (duvar sekmesi, kanca, yerçekimi çevirme)
- Boss savaşı
- Açık dünya / geri dönülebilir harita
- Çok oyunculu
- Karakter özelleştirme
- Birden fazla dil
- Kolay/Normal/Zor seviyeleri

---

## Bağlantılar

- [Yol haritası](00-YOL-HARITASI.md)
- [Karakter ayarları](AYARLAR.md)
- [Sonra listesi](SONRA.md)
