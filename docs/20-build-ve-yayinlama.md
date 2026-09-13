# Epic 20 — Build ve Yayınlama

**Amaç:** Oyunu insanların indirip oynayabileceği bir şeye dönüştürmek.

**Ön koşul:** [Epic 18](18-test-ve-hata-ayiklama.md), [Epic 19](19-performans.md)

**Süre:** 3–4 gün

---

## Neden bu epic var

Bitmemiş oyunlarla bitmiş oyunlar arasındaki tek fark budur:
**yayınlanmış olmak.**

Ve bu, ilk oyununun en önemli adımı. Mükemmel olmayacak — olmasın. Yayınla,
geri bildirim al, ikincisini daha iyi yap. Bitirmeden öğrenemezsin.

---

## Görevler

### 1. Player Settings doldur

`Edit → Project Settings → Player`

| Alan | Değer | Not |
|---|---|---|
| **Company Name** | Adın veya takma adın | ⚠️ Kayıt yolunu belirler |
| **Product Name** | Oyunun adı | ⚠️ Kayıt yolunu belirler + pencere başlığı |
| Version | `1.0.0` | Semantic versioning |
| Default Icon | 256×256 PNG | Ve daha küçük boyutlar |
| Default Screen Width/Height | 1920 × 1080 | |
| Fullscreen Mode | Fullscreen Window | Alt+Tab'de sorun çıkarmaz |
| Resizable Window | ✔ | |
| Run In Background | ✘ | Alt+Tab'de duraklasın |

⚠️ **Kritik uyarı:** `Company Name` ve `Product Name`, kayıt dosyasının
yolunu belirler:

```
C:\Users\<kullanıcı>\AppData\LocalLow\<CompanyName>\<ProductName>\save.json
```

Yayınladıktan sonra bunları değiştirirsen **tüm oyuncuların kaydı kaybolur.**
Şimdi doğru gir.

- [ ] Player Settings dolduruldu
- [ ] İkon eklendi
- [ ] Company/Product Name kesinleşti

### 2. Hedef platform

İlk oyun için **Windows Standalone** yeterli.

| Platform | Zorluk | Not |
|---|---|---|
| **Windows** | Kolay | Başla ve bitir |
| WebGL | Orta | Daha çok kişi dener ama build uzun, ses sorunlu, performans düşük |
| Linux | Kolay | Ek maliyet neredeyse yok, isteyene |
| macOS | Orta | İmzalama/notarization sorunları |

WebGL cazip görünür ama:
- Build süresi 10–30 dakika
- Ses sistemi tarayıcıya göre değişir
- Dosya boyutu kritik (sıkıştırma gerekir)
- Performans düşük

Önce Windows'u bitir. İstersen sonra WebGL dene.

- [ ] Platform seçildi

### 3. Build al

`File → Build Settings → Windows → Build`

**Build öncesi kontrol listesi:**

```
☐ Tüm sahneler Build Settings'te ve DOĞRU SIRADA
☐ Development Build KAPALI
☐ Autoconnect Profiler KAPALI
☐ Script Debugging KAPALI
☐ Debug tuşları devre dışı (#if ile sarılı)
☐ Analitik kodu devre dışı
☐ Test sahneleri (TestOdasi) Build Settings'te DEĞİL
☐ Konsol temiz
```

Terminalden de alabilirsin (Unity kapalıyken):

```bash
unity build --project-path . --target Win64 --output Builds/
```

Veya doğrudan:

```bash
"C:\Program Files\Unity\Hub\Editor\6000.0.83f1\Editor\Unity.exe" -batchmode -quit -projectPath . -buildWindows64Player "Builds/Oyun.exe" -logFile build.log
```

- [ ] Build alındı

### 4. Temiz bir bilgisayarda test et

**Kendi bilgisayarın "temiz" değil** — Unity kurulu, kayıt dosyaların var,
gerekli çalışma zamanları yüklü.

| Test | Nasıl |
|---|---|
| Başka bilgisayarda çalıştır | En iyisi |
| Kayıt klasörünü sil | `AppData\LocalLow\<Company>\<Product>` |
| İlk açılış deneyimi | Hiç kaydı olmayan biri ne görüyor? |
| Baştan sona oyna | Tüm bölümler |

- [ ] Temiz makinede test edildi

### 5. itch.io sayfası hazırla

İlk oyun için en doğru yer. Ücretsiz, kolay, indie dostu, keşfedilebilir.

**Kapak görseli (630×500)** — oyunun ruhunu tek karede anlatsın. Tıklanmayı
en çok belirleyen şey budur. Oyun adını üstüne yaz.

**Ekran görüntüleri (3–5)** — oynanışı göstersin, menü değil. Oyunun en
ilginç anlarını seç.

**GIF (1 adet)** — en etkili tanıtım aracı. Oyunun en iyi 3–5 saniyesi.
[ScreenToGif](https://www.screentogif.com) ile ücretsiz alırsın.
Dosya boyutunu 3 MB altında tut.

**Açıklama şablonu:**

```
[OYUN ADI]

[Tek cümlelik tanım — Epic 01'deki cümlen]

━━━━━━━━━━━━━━━━━━━━━

NASIL OYNANIR
  A / D  veya  ← →    Hareket
  Space               Zıpla (basılı tut = daha yüksek)
  Shift               [imza mekaniğin]
  R                   Yeniden başla
  Esc                 Duraklat

━━━━━━━━━━━━━━━━━━━━━

12 bölüm · yaklaşık 15 dakika

━━━━━━━━━━━━━━━━━━━━━

Yapım: [adın]
Sesler: [kaynak] ([lisans])
Grafikler: [kaynak] ([lisans])
Müzik: [kaynak] ([lisans])
```

**Etiketler:** `platformer`, `2d`, `pixel-art`, `precision-platformer`,
`singleplayer`, `short`

**Fiyat:** ücretsiz (veya "name your own price"). İlk oyundan para bekleme.

- [ ] itch.io sayfası hazır

### 6. Lisans ve atıflar

`docs/LISANSLAR.md` dosyandaki her satırı kontrol et:

| Lisans | Atıf gerekli? | Ticari kullanım |
|---|---|---|
| CC0 / Public Domain | Hayır | ✔ |
| CC-BY | **Evet** | ✔ |
| CC-BY-SA | **Evet** + aynı lisans | ✔ |
| CC-BY-NC | Evet | ✘ **Ticari kullanılamaz** |
| MIT / Apache | Evet (kod için) | ✔ |

Atıfları iki yere koy:
1. Oyun içi **Credits** ekranı
2. itch.io sayfası açıklaması

Bu yasal bir zorunluluk, süs değil. CC-BY lisanslı bir sesi atıfsız
kullanırsan oyunun kaldırılabilir.

- [ ] Tüm lisanslar kontrol edildi
- [ ] Atıflar Credits ve itch.io'da

### 7. Yayınla

1. itch.io'da hesap aç → `Upload new project`
2. Build klasörünü **zip'le** (`.exe` tek başına değil — `_Data` klasörü şart)
3. Yükle, `Kind of project: Downloadable` seç
4. Platform: Windows ✔
5. **Önce "Draft"** olarak bırak
6. Linki 2–3 arkadaşına gönder, indirip oynasınlar
7. Sorun çıkmazsa **"Public"** yap

**Zip içeriği doğru mu:**
```
Oyun.zip
├── Oyun.exe
├── UnityPlayer.dll
├── Oyun_Data/
│   └── ...
└── MonoBleedingEdge/
```

Sadece `.exe` yüklersen çalışmaz — bu klasik bir hatadır.

**Bu son adımı erteleme.** "Biraz daha düzelteyim" diye aylarca bekleyen
oyunlar hiç çıkmaz.

- [ ] Oyun yayında, linki var

### 8. Yayın sonrası

**İlk 48 saat:**
- Geri bildirimleri topla (itch.io yorumları, arkadaşlar)
- **Kritik hataları** düzelt (çökme, ilerlemeyi engelleyen bug)
- `1.0.1` yayınla

**Sonra: dur.**

İlk oyunu sonsuza kadar cilalamak, ikinci oyunu yapmamanın en zarif yoludur.
İlk oyun bir **öğrenme aracıdır**, başyapıt değil.

**Öğrendiklerini yaz** — `docs/OGRENILENLER.md`:

```markdown
# Bu projeden öğrendiklerim

## Neyi yanlış tahmin ettim
- [süre tahminleri, zorluk, kapsam...]

## Ne beklediğimden uzun sürdü
- [...]

## Ne beklediğimden kolay oldu
- [...]

## İkinci oyunda farklı yapacağım
- [...]

## İkinci oyunda aynı yapacağım
- [...]
```

İkinci oyunda bu dosya altın değerinde olacak.

- [ ] Kritik hatalar düzeltildi
- [ ] `docs/OGRENILENLER.md` yazıldı

---

## Kabul kriteri

- [ ] Build çalışıyor, temiz makinede test edildi
- [ ] Kayıt sistemi build'de çalışıyor
- [ ] Debug tuşları kapalı
- [ ] itch.io sayfası hazır (kapak, GIF, ekran görüntüleri, açıklama)
- [ ] Tüm lisanslar atıflı
- [ ] **Oyun yayında ve paylaşabileceğin bir linki var**
- [ ] Öğrenilenler yazıldı

---

## Tuzaklar

**Sürekli erteleme.** "Biraz daha" en tehlikeli cümle. Yayınlanmamış mükemmel
oyun, yayınlanmış vasat oyundan **daha az** değerlidir — çünkü yayınlanmış
olan sana bir şey öğretir.

**Product Name'i sonradan değiştirmek.** Oyuncuların kayıtları kaybolur.

**Sadece `.exe` yüklemek.** `_Data` klasörü olmadan çalışmaz.

**Sadece kendi makinende test etmek.** Eksik DLL, farklı çözünürlük,
kayıt izni sorunları ancak başka makinede çıkar.

**Lisans atıflarını unutmak.** Ciddi sonuçları olabilir.

**Development Build ile yayınlamak.** Yavaş çalışır ve debug bilgisi sızdırır.

**Geri bildirimi kişisel almak.** "Kontroller kötü" demek "sen kötüsün"
demek değil. Ölçülebilir bilgidir, kullan.

**İlk oyundan para beklemek.** Beklenti sıfır olsun. Kazanç: bitirmiş olmak
ve bir sonrakini daha iyi yapabilecek olmak.

**Yayınladıktan sonra 6 ay daha cilalamak.** İkinci oyuna geç.

---

## Bu plan bittiğinde

Yayınlanmış bir oyunun olacak.

Bu, oyun yapmaya başlayan insanların **çok büyük çoğunluğunun** ulaşamadığı
bir nokta. Oyun yapmaya başlayanların tahminen %5'inden azı ilk oyununu bitirir.

İkinci oyun her zaman daha iyi olur, çünkü artık nerelerin zor olduğunu
biliyorsun — ve daha önemlisi, bir oyunu bitirebileceğini biliyorsun.

---

## Geri dön

[Yol haritası](00-YOL-HARITASI.md)
