# Epic 17 — Zorluk Eğrisi ve Dengeleme

**Amaç:** Oyuncunun ilk bölümde sıkılmaması, son bölümde bırakmaması.

**Ön koşul:** [Epic 16](16-sahne-akisi.md) — tüm bölümler oynanabilir olmalı

**Süre:** Odaklı 1 hafta, sonra sürekli

---

## Neden bu epic var

Çoğu amatör oyun **çok zordur** ve yapımcısı bunu bilmez. Sebep basit:
sen oyununu 500 kez oynadın, oyuncu ilk kez görüyor.

Sana "kolay" gelen bölüm, oyuncuya "imkânsız"dır. Bu, tahminle çözülebilecek
bir problem değil — **ölçmen** gerekir.

---

## Zorluk eğrisi

```
zorluk
  │                                        ╱─ 12
  │                              ╱─ 10 ─╲╱
  │                    ╱─ 7 ─╲ ╱─ 9
  │          ╱─ 4 ─╲ ╱─ 6      8(nefes)
  │  ╱─ 2 ─╲╱─ 3     5(nefes)
  │ 1
  └──────────────────────────────────────── bölüm
```

Düz artan çizgi değil, **testere dişi**. Zor bölümden sonra kolay bölüm
gelsin — oyuncu nefes alsın, kendini iyi hissetsin, sonra tekrar zorlansın.

Sürekli artan zorluk yorar ve bıraktırır. Dinlenme anları olmadan başarı
hissi de oluşmaz.

---

## M2'de neyi yapabiliriz, neyi yapamayız

Bu epic'in ön koşulu *"tüm bölümler oynanabilir olmalı"* — bizde **bir
bölüm** var. Görevlerin bir kısmı 12 bölüm varsayıyor. Ayrımı baştan
yapalım ki sonra "yapıldı" sanılmasın:

| Görev | M2'de | Neden |
|---|---|---|
| 1. Bölüm rolleri | ⬜ | 12 bölümün rolü, o bölümler var olunca |
| 2. Mekanik tanıtım sırası | 🔧 kısmen | Elimizdeki mekanikler için yazılabilir |
| **3. Veri topla** | ✅ | **Tek bölümle de çalışır — en değerli parça** |
| 4. Başkalarına oynat | ⬜ | Kullanıcının yapması gereken; M1 kapısı da bunu bekliyor |
| 5. Kırmızı bayraklar | 🔧 kısmen | İkisi ölçülebilir, gerisi insan gözlemi |
| 6. Ucuz ayarlama | ✅ | Tablo zaten kullanılabilir |
| 7. Erişilebilirlik | ⬜ | Epic 15 (UI) ile birlikte anlamlı |

---

## Ölçüm altyapısı (görev 3)

```
Tools > 2D Platformer > Olum Isi Haritasi
```

`LevelAnalytics` her ölümü kaydediyor: **konum, bölüme girdikten kaç
saniye sonra, ve ne öldürdü.**

Sebebin kaydedilmesi önemli — çünkü *"burada 12 kez ölündü"* ile
*"burada 12 kez **dikenden** ölündü"* farklı şeyler söyler. Birincisi
"burası zor", ikincisi "bu diken haksız".

Veri **kayıttan ayrı** dosyada (`analytics-level0.json`). Oyuncunun
ilerlemesi ile geliştirme verisi ayrı şeyler: kayıt silinince ölçümler
kaybolmamalı, ölçümler silinince ilerleme bozulmamalı.

### Kümeleme neden gerekli

Ham ölüm noktaları yanıltıcı: bir engelde ölen oyuncu her seferinde biraz
farklı yerde ölüyor. Kümelemeden bakarsan *"her yerde biraz ölüm var"*
görünür; 1,5 birimlik yarıçapla kümeleyince *"şu noktada 14 ölüm"* çıkar.

### Otomatik kontrol edilen iki bayrak

Epic'in kırmızı bayrak tablosundan **ölçülebilir** olanlar:

| Bayrak | Nasıl ölçülüyor |
|---|---|
| Aynı noktada 10+ ölüm | Küme sayısı eşiği aşarsa uyarı + en sık sebep |
| Hiç ölmeden bitirme | `completions > 0 && deaths == 0` |

İkincisinde araç bir uyarı da basıyor: *"bölümü sen yaptın ve ezbere
biliyorsun — bu bayrak ancak **başkası** oynadığında anlamlı."*

Tablodaki diğerleri (*"nereye gideceğim?"*, *"duraklatıp telefona bakma"*)
insan gözlemi gerektiriyor; onlar görev 4'ün işi ve otomatikleştirilemez.

### Ölçüm doğrulandı

İlk kayıt bilerek üretilen bir ölümle sınandı. Dört alanın dördü de
tasarımla örtüştü:

| Kayıt | Tasarımda | |
|---|---|---|
| `x = 55,87` | Diken x = 56..57 | oyuncunun sağ kenarı 56,26 → dikenin içinde |
| `y = 3,48` | Zemin 3 + yarı boy 0,48 | **tam eşleşme** |
| `cause = Diken_1` | "Diken Tanıtımı"ndaki tek diken | doğru nesne |
| `t = 9,68 sn` | 55,87 ÷ 9,68 = 5,77 birim/sn | yürüme hızı |

Dördü birlikte olayı **yeniden kurabiliyor**: 9,7. saniyede, üçüncü zeminin
üzerinde, yürüyerek dikene girilmiş. `y`'nin 3,48'e tam oturması
zıplanmadığını söylüyor.

`deaths: []` artık "ölüm yok" anlamına geliyor — "kayıt bozuk" değil.
İkisini ayırt edemediğimiz sürece veri işe yaramazdı.

---

## Görevler

### 1. Bölüm rolleri

12 bölümün her birine bir rol ver:

| Bölüm | Rol | Ölüm beklentisi |
|---|---|---|
| 1 | Öğretme — ölmek neredeyse imkânsız | 0–1 |
| 2 | Temel pekiştirme | 1–3 |
| 3 | İlk gerçek meydan okuma | 3–6 |
| 4 | Yeni mekanik | 4–8 |
| 5 | **Nefes** — kolay, keşif odaklı | 1–3 |
| 6 | Mekanik birleştirme | 5–10 |
| 7 | Yeni tehlike | 5–10 |
| 8 | **Nefes** | 2–4 |
| 9 | Gerçek zorluk | 8–15 |
| 10 | Zorluk devam | 8–15 |
| 11 | **Nefes / hazırlık** | 3–6 |
| 12 | Final — her şey bir arada | 15–30 |

Ölüm beklentisi sütunu, Epic 18'deki verilerle karşılaştıracağın hedeftir.

- [ ] Her bölümün rolü ve hedef ölüm sayısı yazıldı

### 2. Mekanik tanıtım sırası

Her bölüm **en fazla 1 yeni şey** tanıtsın. İki yeni mekanik aynı bölümde
olursa oyuncu ikisini de öğrenemez.

| Bölüm | Yeni tanıtılan |
|---|---|
| 1 | Zıplama, para |
| 2 | Boşluk, diken |
| 3 | Devriye düşman |
| 4 | Hareketli platform (yatay) |
| 5 | *(yeni yok — pekiştirme)* |
| 6 | Düşen platform |
| 7 | Mermi atan düşman |
| 8 | *(yeni yok)* |
| 9 | Aralıklı diken |
| 10 | Uçan düşman |
| 11 | *(yeni yok)* |
| 12 | *(yeni yok — hepsi bir arada)* |

Epic 04'teki 4 adımlı kalıbı (tanıt → uygulat → zorlaştır → birleştir)
her mekanik için uygula.

- [ ] Tanıtım sırası tablosu hazır

### 3. Veri topla — tahmin etme, ölç

`Assets/Scripts/Core/LevelAnalytics.cs`:

```csharp
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Platformer.Core
{
    [System.Serializable]
    public class DeathRecord
    {
        public float x, y;
        public float timeIntoLevel;
        public string cause;
    }

    [System.Serializable]
    public class LevelSession
    {
        public string levelName;
        public int deathCount;
        public float completionTime;
        public bool completed;
        public List<DeathRecord> deaths = new List<DeathRecord>();
    }

    [System.Serializable]
    public class AnalyticsFile
    {
        public List<LevelSession> sessions = new List<LevelSession>();
    }

    /// <summary>
    /// Olum konumlarini ve surelerini kaydeder.
    /// SADECE gelistirme icin - build'de kapali.
    /// </summary>
    public class LevelAnalytics : MonoBehaviour
    {
        public static LevelAnalytics Instance { get; private set; }

        private LevelSession current;
        private string FilePath =>
            Path.Combine(Application.persistentDataPath, "analytics.json");

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void BeginLevel(string levelName)
        {
            current = new LevelSession { levelName = levelName };
        }

        public void RecordDeath(Vector2 position, float timeIntoLevel, string cause)
        {
            if (current == null) return;

            current.deathCount++;
            current.deaths.Add(new DeathRecord
            {
                x = position.x,
                y = position.y,
                timeIntoLevel = timeIntoLevel,
                cause = cause
            });
        }

        public void EndLevel(bool completed, float time)
        {
            if (current == null) return;

            current.completed = completed;
            current.completionTime = time;

            AnalyticsFile file = LoadFile();
            file.sessions.Add(current);
            File.WriteAllText(FilePath, JsonUtility.ToJson(file, true));

            Debug.Log($"[Analytics] {current.levelName}: " +
                      $"{current.deathCount} olum, {time:F1} sn");
            current = null;
        }

        private AnalyticsFile LoadFile()
        {
            if (!File.Exists(FilePath)) return new AnalyticsFile();

            try
            {
                return JsonUtility.FromJson<AnalyticsFile>(File.ReadAllText(FilePath))
                       ?? new AnalyticsFile();
            }
            catch { return new AnalyticsFile(); }
        }
    }
}
```

**Isı haritası görüntüleyici** — `Assets/Editor/DeathHeatmap.cs`:

```csharp
using System.IO;
using UnityEditor;
using UnityEngine;
using Platformer.Core;

namespace Platformer.EditorTools
{
    /// <summary>
    /// analytics.json icindeki olum konumlarini Scene view'da gosterir.
    /// Menu: Tools > 2D Platformer > Olum Isi Haritasi
    /// </summary>
    public class DeathHeatmap : EditorWindow
    {
        private AnalyticsFile data;
        private string levelFilter = "";

        [MenuItem("Tools/2D Platformer/Olum Isi Haritasi", false, 31)]
        public static void Open() => GetWindow<DeathHeatmap>("Olum Haritasi");

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Reload();
        }

        private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

        private void Reload()
        {
            string path = Path.Combine(Application.persistentDataPath, "analytics.json");
            if (!File.Exists(path)) { data = null; return; }

            try { data = JsonUtility.FromJson<AnalyticsFile>(File.ReadAllText(path)); }
            catch { data = null; }
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Yenile")) Reload();

            levelFilter = EditorGUILayout.TextField("Bolum filtresi", levelFilter);

            if (data == null || data.sessions.Count == 0)
            {
                EditorGUILayout.HelpBox("Veri yok. Once oyunu oyna.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"{data.sessions.Count} oturum kayitli");

            foreach (LevelSession s in data.sessions)
            {
                EditorGUILayout.LabelField(
                    $"{s.levelName}: {s.deathCount} olum, {s.completionTime:F1} sn" +
                    (s.completed ? "" : "  [BITIRILMEDI]"));
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (data == null) return;

            foreach (LevelSession s in data.sessions)
            {
                if (!string.IsNullOrEmpty(levelFilter) && !s.levelName.Contains(levelFilter))
                    continue;

                foreach (DeathRecord d in s.deaths)
                {
                    Handles.color = new Color(1f, 0.2f, 0.2f, 0.35f);
                    Handles.DrawSolidDisc(new Vector3(d.x, d.y, 0f), Vector3.forward, 0.35f);
                }
            }
        }
    }
}
```

Bir noktada kırmızı yığılma görüyorsan **orası bozuk**.

- [x] Analitik toplanıyor — `LevelAnalytics`, konum + süre + sebep
- [x] Isı haritası çalışıyor — `Tools > Olum Isi Haritasi`, kümeleme ve iki otomatik bayrak

### 4. Başkalarına oynat — en önemli görev

**En az 3 kişi.** Farklı beceri seviyelerinden:

| Kişi | Ne öğrenirsin |
|---|---|
| Platform oyunu oynamayan | İlk bölümün gerçekten öğretip öğretmediği |
| Ara seviye | Genel zorluk dengesi |
| İyi oyuncu | Son bölümlerin yeterince zorlayıcı olup olmadığı |

**Kurallar:**
- **Hiçbir şey söyleme.** Yardım etme, ipucu verme.
- Ellerini ve yüzünü izle, sözlerini değil. "Güzelmiş" diyen biri sıkılmış olabilir.
- Ekranı kaydet (OBS, ücretsiz) — sonra tekrar izleyebilirsin.
- Not al: nerede takıldı, nerede sinirlendi, nerede güldü, **nerede bıraktı**

Bıraktığı nokta en değerli veridir.

- [ ] En az 3 kişi oynadı, notlar alındı

### 5. Kırmızı bayraklar

Bu işaretlerden biri varsa o bölüm bozuk:

| Bayrak | Anlamı | Çözüm |
|---|---|---|
| Aynı noktada 10+ ölüm | Engel haksız veya okunmuyor | Kontrast artır / mesafe azalt |
| "Nereye gideceğim?" | Yönlendirme yetersiz | Para dizisi ekle (Epic 08) |
| "Değmedim ama öldüm" | Collider çok büyük | Collider'ı küçült |
| Hiç ölmeden bitirme | Çok kolay veya çok kısa | Zorluk ekle veya uzat |
| Oynarken sessizleşme | Sıkılmış | Ritim ekle, nefes alanını kısalt |
| Duraklatıp telefona bakma | Kaybettin | Bölümü yeniden tasarla |
| Aynı yeri 3+ kez tekrar deneyip bırakma | Çok zor | Checkpoint ekle |

- [ ] Her bölüm bayraklara göre denetlendi

### 6. Ucuz ayarlama teknikleri

Bölüm zor geliyorsa, **yeniden tasarlamadan önce** bunları dene:

| Sorun | Ucuz çözüm | Maliyeti |
|---|---|---|
| Ölüm çok | Checkpoint ekle | 1 dakika |
| Zamanlama zor | Tehlike döngüsünü yavaşlat | 30 saniye |
| Boşluk geçilmiyor | 0.5 birim daralt | 30 saniye |
| Düşman haksız | Devriye alanını daralt | 1 dakika |
| Okunmuyor | Renk kontrastını artır | 5 dakika |
| Ölüm sinir bozucu | Respawn süresini kısalt | 10 saniye |
| Platform kaçıyor | Bekleme süresini uzat | 30 saniye |

**Çoğu zorluk sorunu tasarım değil, ayar sorunudur.** Önce bunları dene.

- [ ] Ayarlamalar yapıldı ve tekrar test edildi

### 7. Erişilebilirlik seçenekleri (isteğe bağlı, çok değerli)

Zorluk seviyesi yerine **yardım seçenekleri** sun:

| Seçenek | Etkisi |
|---|---|
| Sonsuz can | Zaten önerildi |
| Oyun hızı %70 | `Time.timeScale = 0.7f` |
| Dokunulmazlık | Tehlikeler öldürmez |
| Tehlike vurgulama | Tehlikeler daha parlak |
| Sonsuz dash | İmza mekaniği kısıtsız |

*Celeste*'in "Assist Mode"u bunu çok iyi yapar: oyunu kolaylaştırmaz,
**oyuncuya kontrol verir** ve utandırmaz. Menüde şöyle yazar:
*"Bu oyun zor olacak şekilde tasarlandı. Ama deneyimin senin."*

**Neden değerli:** engelli oyuncular, yeni başlayanlar ve az vakti olan
insanlar oyununu bitirebilir. Hiçbir şey kaybetmezsin.

- [ ] (İsteğe bağlı) Yardım seçenekleri eklendi

---

## Kabul kriteri

- [ ] İlk bölüm ölmeden bitirilebiliyor
- [ ] Zorluk testere dişi, düz artan değil
- [ ] Her bölüm en fazla 1 yeni şey tanıtıyor
- [ ] 3 farklı kişi oyunu bitirebildi
- [ ] Tek bir noktada ölüm yığılması yok (ısı haritasıyla doğrulandı)
- [ ] Hiç kimse "bu haksız" demedi
- [ ] Gerçek ölüm sayıları hedeflerle uyumlu

---

## Tuzaklar

**Kendi becerine göre dengelemek.** En büyük ve en yaygın hata.

**Test oyuncusuna yardım etmek.** "Şuraya zıpla" dediğin an veri bozulur.

**"Zor olsun, öğrenirler" demek.** Öğrenmezler, bırakırlar. Oyununu
bitirmeyen bir oyuncu hiçbir şey öğrenmemiştir.

**İlk bölümü zor yapmak.** Oyuncuların büyük kısmı ilk 2 dakikada karar verir.

**Tek bir kişinin görüşüne göre değiştirmek.** 3 kişiden 1'i takıldıysa
o kişi olabilir; 3'ü de takıldıysa tasarım hatasıdır.

**Zorluğu can sayısıyla ayarlamak.** Can azaltmak oyunu zorlaştırmaz,
sadece tekrar ettirir — ve sıkar.

**Veri toplamayı atlamak.** Tahminle dengeleme yapamazsın.

**Analitik kodu build'de bırakmak.** Oyuncunun diskine gereksiz dosya yazar.
`#if UNITY_EDITOR || DEVELOPMENT_BUILD` ile sar.

---

## v1'de yapma

- Kolay/Normal/Zor seviye seçenekleri (üç kat dengeleme işi)
- Dinamik zorluk ayarı (oyuncuya göre otomatik)
- Sunucuya analitik gönderme
- New Game+
- Zaman yarışı / speedrun modu (en iyi süre yeterli)
- Zorluk rozetleri / madalyalar

---

## Sonraki

[Epic 18 — Test ve hata ayıklama](18-test-ve-hata-ayiklama.md)
