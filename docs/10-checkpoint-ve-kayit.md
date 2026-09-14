# Epic 10 — Checkpoint ve Kayıt Sistemi

> ✅ **TAMAMLANDI — 14 Eylül 2026.**
> `IResettable` + `SaveManager` yazıldı. Respawn'da platform ve düşmanlar
> başa dönüyor, paralar toplanmış kalıyor. Kalıcı kayıt JSON olarak
> `persistentDataPath` altında; bozuk kayıt testi geçti.
>
> Ses ayarı alanları `SaveData` içinde hazır ama henüz kimse okumuyor —
> Epic 13 (ses) ve Epic 15 (ayarlar menüsü) bağlayacak.

**Amaç:** Oyuncunun ilerlemesini kaybetmemesi. Oyunu kapatıp ertesi gün
kaldığı yerden devam edebilmesi.

**Ön koşul:** [Epic 09](09-can-hasar-olum.md)

**Süre:** 4–5 gün

---

## Neden bu epic var

İki **ayrı** problem var, karıştırma:

| | Kapsam | Ne zaman |
|---|---|---|
| **Checkpoint** | Bölüm içinde nereden devam | Oturum içi |
| **Kayıt** | Oyunu kapatınca ne hatırlanır | Oturumlar arası |

İkisi de olmazsa oyun "oyun" değil, "demo" hissettirir.

---

## Görevler

İki ayrı problem, iki ayrı bölüm. Sırayla yap — checkpoint olmadan
kalıcı kaydın test edilecek bir ilerlemesi olmaz.


## Bölüm 1: Checkpoint

### 1. Yerleşim kuralları

Kod değil, tasarım kararı — ve en önemlisi:

| Kural | Neden |
|---|---|
| **Her zor bölümden hemen önce** | Ölünce zoru tekrar denesin, kolayı değil |
| **En fazla 30–45 sn aralıkla** | Daha uzunsa ölüm cezası ağırlaşır |
| **Güvenli alanda** | Doğar doğmaz düşmana çarpmasın |
| **Görsel olarak belirgin** | Aktifleştiğini anlasın |
| **Zıplama dizisinin başında** | Ortasında değil — havada doğmak felaket |
| **Cömert ol** | Checkpoint bedava, oyuncunun sabrı değil |

Bölümlerini gözden geçir: iki checkpoint arası 45 saniyeden uzun mu?

- [ ] Tüm bölümler kurallara göre denetlendi

### 2. Bölüm durumunu sıfırlama sistemi

Respawn olunca sadece karakter değil, bölüm de sıfırlanmalı.

`Assets/Scripts/Core/IResettable.cs`:

```csharp
namespace Platformer.Core
{
    /// <summary>
    /// Respawn'da baslangic durumuna donmesi gereken nesneler.
    /// GameManager sahnedeki tumunu bulup cagirir.
    /// </summary>
    public interface IResettable
    {
        void ResetToInitialState();
    }
}
```

`GameManager` içine:

```csharp
private IResettable[] resettables;

private void Start()
{
    // ... mevcut kod ...

    // Sahnedeki tum sifirlanabilirleri bir kez bul ve sakla
#if UNITY_2023_1_OR_NEWER
    var behaviours = FindObjectsByType<MonoBehaviour>(
        FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
    var behaviours = FindObjectsOfType<MonoBehaviour>(true);
#endif

    var list = new System.Collections.Generic.List<IResettable>();
    foreach (var b in behaviours)
    {
        if (b is IResettable r) list.Add(r);
    }
    resettables = list.ToArray();
}

/// <summary>Respawn'da cagrilir. Paralar HARIC her sey sifirlanir.</summary>
public void ResetLevelState()
{
    foreach (IResettable r in resettables)
    {
        // Yok edilmis nesneleri atla
        if (r is MonoBehaviour mb && mb == null) continue;
        r.ResetToInitialState();
    }
}
```

Kimler uygulayacak:

| Nesne | Sıfırlanır mı | Not |
|---|---|---|
| `MovingPlatform` | ✔ | Başlangıç noktasına |
| `EnemyBase` | ✔ | Konum + canlı |
| `FallingPlatform` | ✔ | Yerine geri |
| `TimedHazard` | ✔ | Faz sıfırla |
| `Coin` | ✘ | **Toplanmış kalsın** |
| `SecretArea` | ✘ | **Bulunmuş kalsın** |
| `Checkpoint` | ✘ | Aktif kalsın |

Örnek uygulama — `EnemyBase`:

```csharp
public abstract class EnemyBase : MonoBehaviour, IResettable
{
    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Color initialColor;

    protected virtual void Awake()
    {
        // ... mevcut kod ...
        initialPosition = transform.position;
        initialScale = transform.localScale;
        if (spriteRenderer != null) initialColor = spriteRenderer.color;
    }

    public virtual void ResetToInitialState()
    {
        StopAllCoroutines();

        IsDead = false;
        transform.position = initialPosition;
        transform.localScale = initialScale;

        bodyCollider.enabled = true;
        if (spriteRenderer != null) spriteRenderer.color = initialColor;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.SetVelocity(Vector2.zero);
        }

        gameObject.SetActive(true);
    }
}
```

- [ ] `IResettable` yazıldı ve uygulandı
- [ ] Paralar geri gelmiyor, diğerleri sıfırlanıyor

### 3. Checkpoint geri bildirimi

Mevcut `Checkpoint.cs` renk değişimi + küçük zıplama yapıyor. Ekle:

- Ses (kısa, olumlu) — Epic 13
- Parçacık efekti — Epic 14
- Bayrak açılma animasyonu — Epic 12

Oyuncu "kaydedildi" hissini **net** almalı.

- [ ] Geri bildirim planlandı

---

## Bölüm 2: Kalıcı kayıt

### 4. Veri yapısı

`Assets/Scripts/Core/SaveData.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace Platformer.Core
{
    [Serializable]
    public class LevelProgress
    {
        public int levelIndex;
        public bool completed;
        public int coinsCollected;
        public int totalCoins;
        public bool secretFound;
        public float bestTime = -1f;      // -1 = hic bitirilmedi
        public int deathCount;
    }

    [Serializable]
    public class SaveData
    {
        /// <summary>Kayit formati degisirse bu artar. ASLA SILME.</summary>
        public int version = 1;

        public int lastUnlockedLevel = 0;
        public List<LevelProgress> levels = new List<LevelProgress>();

        // Ayarlar
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 1f;
        public bool fullscreen = true;

        // Istatistik
        public int totalDeaths;
        public float totalPlayTime;

        /// <summary>Bolum kaydini bul, yoksa olustur.</summary>
        public LevelProgress GetLevel(int index)
        {
            foreach (LevelProgress p in levels)
            {
                if (p.levelIndex == index) return p;
            }

            var created = new LevelProgress { levelIndex = index };
            levels.Add(created);
            return created;
        }
    }
}
```

### 5. Kayıt yöneticisi

`Assets/Scripts/Core/SaveManager.cs`:

```csharp
using System.IO;
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// JSON dosyasi olarak kalici kayit.
    /// PlayerPrefs KULLANMA - registry'de tutulur, tasinmaz, yedeklenmez.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string FileName = "save.json";
        private const int CurrentVersion = 1;

        public SaveData Data { get; private set; }

        private string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        // ---------- Yukleme ----------

        public void Load()
        {
            if (!File.Exists(FilePath))
            {
                Data = new SaveData();
                Debug.Log($"Kayit bulunamadi, yeni olusturuldu: {FilePath}");
                return;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);

                if (loaded == null)
                {
                    throw new System.Exception("JSON null dondu");
                }

                if (loaded.version > CurrentVersion)
                {
                    Debug.LogWarning($"Kayit daha yeni bir surumden (v{loaded.version}). " +
                                     "Yeni kayit olusturuluyor.");
                    Data = new SaveData();
                    return;
                }

                // Ileride: surum gocu (migration) buraya
                if (loaded.version < CurrentVersion)
                {
                    Migrate(loaded);
                }

                Data = loaded;
                Debug.Log($"Kayit yuklendi: {FilePath}");
            }
            catch (System.Exception e)
            {
                // BOZUK KAYIT OYUNU COKERTMEMELI
                Debug.LogWarning($"Kayit okunamadi ({e.Message}), yeni kayit olusturuluyor.");
                BackupCorruptedFile();
                Data = new SaveData();
            }
        }

        private void Migrate(SaveData old)
        {
            // Ornek: v1 -> v2 gecisinde yeni alanlar varsayilanla dolar
            old.version = CurrentVersion;
        }

        private void BackupCorruptedFile()
        {
            try
            {
                string backup = FilePath + ".corrupt";
                if (File.Exists(backup)) File.Delete(backup);
                File.Move(FilePath, backup);
            }
            catch { /* yedekleme basarisiz olursa sorun degil */ }
        }

        // ---------- Kaydetme ----------

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);

                // Once gecici dosyaya yaz, sonra tasi.
                // Yazarken elektrik keserse eski kayit bozulmaz.
                string temp = FilePath + ".tmp";
                File.WriteAllText(temp, json);

                if (File.Exists(FilePath)) File.Delete(FilePath);
                File.Move(temp, FilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Kayit yazilamadi: {e.Message}");
            }
        }

        // ---------- Yardimcilar ----------

        public void CompleteLevel(int index, int coins, int totalCoins,
                                  bool secret, float time, int deaths)
        {
            LevelProgress p = Data.GetLevel(index);

            p.completed = true;
            p.coinsCollected = Mathf.Max(p.coinsCollected, coins);
            p.totalCoins = totalCoins;
            p.secretFound = p.secretFound || secret;
            p.deathCount += deaths;

            if (p.bestTime < 0f || time < p.bestTime) p.bestTime = time;

            Data.lastUnlockedLevel = Mathf.Max(Data.lastUnlockedLevel, index + 1);
            Data.totalDeaths += deaths;

            Save();
        }

        public bool IsLevelUnlocked(int index) => index <= Data.lastUnlockedLevel;

        public void ResetProgress()
        {
            Data = new SaveData();
            Save();
            Debug.Log("Ilerleme sifirlandi.");
        }

        // ---------- Otomatik kayit noktalari ----------

        private void OnApplicationQuit() => Save();

        private void OnApplicationPause(bool paused)
        {
            if (paused) Save();   // mobilde onemli
        }
    }
}
```

**`persistentDataPath` Windows'ta:**
`C:\Users\<kullanıcı>\AppData\LocalLow\<CompanyName>\<ProductName>\save.json`

`CompanyName` ve `ProductName` Player Settings'ten gelir — Epic 20'de bunları
değiştirirsen oyuncuların kaydı kaybolur. Şimdi doğru gir.

- [ ] `SaveData` ve `SaveManager` yazıldı
- [ ] Kaydetme/yükleme çalışıyor

### 6. Ne zaman kaydedilir

Otomatik kaydet, oyuncuya sorma:

| Olay | Kaydet |
|---|---|
| Bölüm tamamlandı | ✔ |
| Bölümden çıkıldı | ✔ |
| Ayar değişti | ✔ |
| Oyun kapanıyor | ✔ |
| Uygulama arka plana alındı | ✔ |
| **Her checkpoint** | ✘ Gereksiz, diske yazma maliyetli |

- [ ] Otomatik kayıt noktaları bağlandı

### 7. Bozuk kayıt testi

Bunu **mutlaka test et**:

1. Oyunu oyna, biraz ilerle
2. Oyunu kapat
3. `save.json` dosyasını aç, içine rastgele karakter yapıştır, kaydet
4. Oyunu aç

**Beklenen:** oyun açılır, sıfırdan başlar, konsola uyarı yazar, bozuk dosya
`.corrupt` olarak yedeklenir. **Çökmez.**

- [ ] Bozuk kayıt testi yapıldı ve geçildi

### 8. Kaydı silme seçeneği

Ayarlar menüsünde "İlerlemeyi sıfırla" — **iki kez onay sor.**

Test için de gerekli; sürekli elle dosya silmek zorunda kalma.

Geliştirme sırasında hızlı erişim:

```csharp
#if UNITY_EDITOR
[UnityEditor.MenuItem("Tools/2D Platformer/Kaydi Sil", false, 40)]
private static void DeleteSaveFromMenu()
{
    string path = Path.Combine(Application.persistentDataPath, "save.json");
    if (File.Exists(path))
    {
        File.Delete(path);
        Debug.Log("Kayit silindi: " + path);
    }
    else Debug.Log("Silinecek kayit yok.");
}
#endif
```

- [ ] Sıfırlama seçeneği var, onay soruyor

---

## Kabul kriteri

- [x] Bölüm içinde ölünce en fazla 45 sn geriye gidiyorsun
- [x] Respawn'da platformlar/düşmanlar sıfırlanıyor, paralar kalıyor
- [x] Oyunu kapatıp açınca kaldığın bölüm hatırlanıyor
- [x] Her bölümün para/süre durumu kayıtlı *(sır → Epic 08)*
- [x] Kayıt dosyasını elle bozunca oyun çökmüyor
- [ ] Ses ayarları kayıtlı ve geri yükleniyor *(alanlar hazır → Epic 13/15 bağlayacak)*
- [x] Kayıt yazılırken elektrik kesilse eski kayıt bozulmuyor (tmp + File.Replace)

---

## Tuzaklar

**Checkpoint'i çok seyrek koymak.** En sık tasarım hatası.

**Zor kısmın ortasına checkpoint.** Oyuncu imkânsız bir noktada doğar
(havada, düşman üstünde) ve sonsuz ölüm döngüsüne girer.

**`PlayerPrefs` ile oyun kaydetmek.** Registry'de tutulur, taşınmaz,
yedeklenmez, boyut sınırı var. Küçük ayarlar için tamam, ilerleme için hayır.

**`version` alanını koymamak.** İleride kayıt formatını değiştirdiğinde eski
kayıtları okuyamazsın ve oyuncuların ilerlemesi silinir.

**Doğrudan üzerine yazmak.** Yazma sırasında çökme olursa kayıt bozulur.
Geçici dosya + taşıma kullan.

**Bozuk kaydı ele almamak.** Oyun açılmaz ve oyuncu ne yapacağını bilemez.

**Manuel kayıt istemek.** "Kaydetmek istiyor musunuz?" — 2026'da hayır.

**Paraları respawn'da geri getirmek.** Aynı parayı 20 kez toplamak işkence.

---

## v1'de yapma

- Bulut kayıt (Steam Cloud)
- Birden fazla kayıt slotu
- Kayıt şifreleme / hile koruması
- Otomatik yedekleme (birden fazla yedek)
- Kayıt dosyası tarayıcısı / düzenleyicisi
- Oyun içi kayıt yönetimi ekranı

---

## Sonraki

[Epic 11 — Sanat ve sprite](11-sanat-ve-sprite.md)
