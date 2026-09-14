# VİZYON

> Projenin pusulası. Yeni bir fikir geldiğinde buraya bak — vizyona uymuyorsa
> [SONRA.md](SONRA.md)'ye yaz ve **yapma**.
>
> **Onaylandı: 14 Eylül 2026**

---

## Tek cümle

> **Sıçradıkça sönen, yere değdikçe yeniden dolan bir kıvılcımın,
> rüzgârlı bir vadiyi geçme hikâyesi.**

Tema mekaniği açıklıyor: kıvılcım yere değince yeniden tutuşur, dash hakkı da
yerde yenilenir. Oyuncu "neden böyle?" diye sormaz.

## Üç sütun

**Okunabilirlik** — Oyuncu ölümü gelmeden görebilmeli. Şüphede oyuncu lehine
karar veririz: collider'lar görselden küçük, tehlikelerin uyarısı var, hiçbir
şey ekran dışından gelmez.

**Akıcılık** — Dash zıplamayı kesmez, uzatır. İyi oynayan biri bölümü hiç
durmadan geçebilmeli.

**Adalet** — Ölüm cezası 0,45 saniye, checkpoint'ler cömert. Zorluk tekrar
ettirmekten değil, öğretmekten gelir.

## Çekirdek döngü

```
boşluğu oku → zıpla → gerekiyorsa dash → yere değ, dash yenilensin → tekrar
```

Havada tek hamle hakkın var; onu nerede harcayacağına karar veriyorsun.

---

## İmza mekaniği: Dash

Yatay, havada tek kullanım, yere değince yenilenir.

| Kural | Değer |
|---|---|
| Yön | **Sadece yatay** (giriş yoksa baktığın yön) |
| Dash sırasında yerçekimi | Kapalı |
| Hak yenilenmesi | Yere değince |
| Bekleme | 0,35 sn |
| Bitince | Hız %55'e düşer |

**Neden yatay:** 8 yönlü denendi, yukarı dash erişilebilir yüksekliği 3,03'ten
6,47'ye çıkardı. Her platform yüksekliğini iki kez hesaplamak gerekirdi.
Yatay dash dikey ekseni tamamen zıplamaya bırakıyor.

**Tasarıma etkisi:** ölçüldü — mesafeyi 5,31'den 7,62'ye çıkarıyor. İki tür
boşluk demek:

- **5,3 altı** → dash'siz geçilir, ritim kurar
- **5,5 üstü** → dash zorunlu, karar noktası yaratır

Tam cetvel: [AYARLAR.md](AYARLAR.md#zorluk-cetveli)

---

## Kapsam

| | Hedef |
|---|---|
| Bölüm | **10–12** |
| Bölüm süresi | 30–90 sn |
| Toplam oynanış | 10–15 dk |
| Düşman | 3 çeşit |
| Tehlike | 5 çeşit |
| Mekanik | 1 imza + zıplama |
| Boss | Yok |
| Hikâye | En fazla 3 ekran |
| Platform | Windows |

Bu tablo **bitiş çizgisi**. Hepsi tamamlanınca oyun bitmiştir.

## Referanslar

| Oyun | Alıyorum | Almıyorum |
|---|---|---|
| **Mario** | Dengeli zıplama, affedici toleranslar, nefes alanları | Güç-yükseltmeleri, dünya haritası |
| **Celeste** | Dash, sıfıra yakın ölüm cezası, bölüm başına tek fikir | Hikâye, B-side'lar, aşırı zorluk |
| **Hollow Knight** | **Hiçbir şey** — üç kişinin üç yılı, benim ölçeğimde değil | |

---

## OLMAYACAK şeyler

Hikâye · diyalog · envanter · beceri ağacı · ikinci mekanik · boss ·
açık dünya · çok oyunculu · karakter özelleştirme · çoklu dil ·
zorluk seviyeleri

Buraya ekleme yaptıkça bitmeye yaklaşırsın.
