# Epic 16 — Sahne Akışı ve İlerleme

**Amaç:** Bölümleri birbirine bağlayıp tek bir oyun haline getirmek.

**Ön koşul:** [Epic 15](15-ui-ve-menuler.md)

**Süre:** 4–5 gün

---

## Neden bu epic var

12 ayrı bölüm sahnesi bir oyun değil, 12 ayrı demodur. Bu epic onları
"başlayıp biten bir deneyime" dönüştürüyor.

---

## Görevler

### 1. Sahne yapısı

```
0.  MainMenu
1.  LevelSelect
2.  Level_01
3.  Level_02
    ...
13. Level_12
14. Credits
```

`File → Build Settings` içinde bu sırayla ekle.

**İsim ile yükle, index ile değil:** `LoadScene("Level_03")` daha güvenli,
çünkü Build Settings'te sıra değiştiğinde kod bozulmaz.

- [ ] Sahneler oluşturuldu ve Build Settings'e eklendi

### 2. Kalıcı yöneticiler

Hangi yönetici kalıcı, hangisi sahneye özel:

| Yönetici | Kalıcı? | Neden |
|---|---|---|
| `SaveManager` | ✔ | İlerleme sahneler arası |
| `AudioManager` | ✔ | Müzik kesilmesin |
| `TimeController` | ✔ | timeScale güvenliği |
| `EffectManager` | ✔ | Havuzlar korunsun |
| `SceneLoader` | ✔ | Geçişi kendisi yönetiyor |
| **`GameManager`** | ✘ | **Skor/checkpoint sıfırlanmalı** |

Son satır önemli: `GameManager` kalıcı olursa eski bölümün skoru taşınır.
Mevcut kod doğru şekilde sahneye bağlı — öyle kalsın.

**Bootstrap sahnesi (temiz yöntem):**

Sahne 0'a `Bootstrap` diye boş bir sahne koy, kalıcı yöneticileri oraya
yerleştir, `Start()`'ta MainMenu'yü yükle. Böylece hangi sahneden başlarsan
başla yöneticiler hep var olur.

Alternatif (daha basit): yöneticileri bir prefab yap, her sahneye koy,
`Awake`'te singleton kontrolü fazlaları silsin. Mevcut singleton kodların
zaten bunu yapıyor.

- [ ] Kalıcı yöneticiler `DontDestroyOnLoad` ile korunuyor
- [ ] `GameManager` sahneye özel kalıyor

### 3. Sahne yükleyici

`Assets/Scripts/Core/SceneLoader.cs`:

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Platformer.Core
{
    /// <summary>
    /// Fade gecisli sahne yukleme. Kalici - kendi Canvas'ini tasir.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 0.3f;

        public bool IsLoading { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeImage != null)
            {
                fadeImage.raycastTarget = false;
                SetAlpha(0f);
            }
        }

        public void LoadScene(string sceneName)
        {
            if (IsLoading) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;

            // Sahne degisirken zaman normale donsun
            TimeController.Instance?.ResetTimeScale();
            Time.timeScale = 1f;

            yield return Fade(0f, 1f);

            // Async: senkron LoadScene buyuk sahnede oyunu dondurur
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            // 0.9'a kadar yukler, sonra bekler
            while (op.progress < 0.9f) yield return null;

            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;

            // Yeni sahnenin Awake/Start'i calissin
            yield return null;

            yield return Fade(1f, 0f);

            IsLoading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            if (fadeImage == null) yield break;

            fadeImage.raycastTarget = true;   // gecis sirasinda tiklamayi engelle

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;   // timeScale'den bagimsiz
                SetAlpha(Mathf.Lerp(from, to, t / fadeDuration));
                yield return null;
            }

            SetAlpha(to);
            fadeImage.raycastTarget = (to > 0.5f);
        }

        private void SetAlpha(float a)
        {
            Color c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
        }
    }
}
```

**Neden `LoadSceneAsync`:** senkron `LoadScene` sahne yüklenene kadar oyunu
tamamen dondurur. Küçük sahnelerde fark etmez ama alışkanlık edin.

**Neden `unscaledDeltaTime`:** duraklatma menüsünden çıkarken `timeScale = 0`
olabilir; fade donmamalı.

- [ ] Fade geçişi çalışıyor, donma yok

### 4. Bölüm veri tabanı

Bölüm bilgilerini sahnenin içinde tutma — `ScriptableObject` kullan.

`Assets/Scripts/Core/LevelData.cs`:

```csharp
using UnityEngine;

namespace Platformer.Core
{
    [CreateAssetMenu(fileName = "Level_", menuName = "Platformer/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Build Settings'teki sahne adi.")]
        public string sceneName;

        [Tooltip("Menude gosterilecek ad.")]
        public string displayName;

        [Tooltip("1'den baslayan sira.")]
        public int levelNumber;

        [Tooltip("Hedef sure (saniye) - yildiz/rozet icin.")]
        public float targetTime = 60f;

        [Tooltip("Bolumdeki toplam para (elle gir veya GameManager sayar).")]
        public int totalCoins;

        [TextArea(2, 4)]
        public string designNote;   // sadece senin icin
    }
}
```

`Assets/Scripts/Core/LevelDatabase.cs`:

```csharp
using UnityEngine;

namespace Platformer.Core
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Platformer/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        public LevelData[] levels;

        public LevelData Get(int index)
        {
            if (levels == null || index < 0 || index >= levels.Length) return null;
            return levels[index];
        }

        public int Count => levels != null ? levels.Length : 0;

        public LevelData GetNext(LevelData current)
        {
            for (int i = 0; i < levels.Length - 1; i++)
            {
                if (levels[i] == current) return levels[i + 1];
            }
            return null;   // son bolum
        }
    }
}
```

**Avantajı:** yeni bölüm eklemek için kod değiştirmen gerekmez — yeni bir
`LevelData` asset'i oluşturup listeye eklersin.

- [ ] `LevelData` ve `LevelDatabase` hazır
- [ ] 12 bölüm için asset'ler oluşturuldu

### 5. Bölüm akışını bağla

```
Bölüm sonu bayrağına değ
  → oynanışı dondur
  → süreyi durdur
  → bölüm sonu ekranını göster (sayarak)
  → kaydet (SaveManager.CompleteLevel)
  → sonraki bölümün kilidini aç
  → [Sonraki] → fade → Level_XX
```

`LevelGoal.cs` şu an sadece `GameManager.CompleteLevel()` çağırıyor.
`GameManager` içinde zinciri tamamla:

```csharp
public void CompleteLevel()
{
    if (LevelCompleted) return;
    LevelCompleted = true;

    // Karakteri dondur
    var player = GameObject.FindGameObjectWithTag("Player");
    player?.GetComponent<Player.PlayerController2D>()?.Freeze();

    // Kaydet
    SaveManager.Instance?.CompleteLevel(
        currentLevelData.levelNumber - 1,
        Score, TotalCoins,
        SecretsFound > 0,
        LevelTime,
        DeathCount);

    OnLevelCompleted?.Invoke();   // UI dinliyor
}
```

Son bölümden sonra → Credits → Ana menü.

- [ ] Bölüm zinciri çalışıyor
- [ ] Son bölüm Credits'e gidiyor

### 6. Süre ölçümü

```csharp
public float LevelTime { get; private set; }

private void Update()
{
    if (!LevelCompleted && !UI.PauseMenu.IsPaused)
    {
        LevelTime += Time.deltaTime;
    }

    // ... killPlane kontrolu
}

public static string FormatTime(float seconds)
{
    int m = Mathf.FloorToInt(seconds / 60f);
    int s = Mathf.FloorToInt(seconds % 60f);
    int ms = Mathf.FloorToInt((seconds * 100f) % 100f);
    return $"{m}:{s:00}.{ms:00}";
}
```

En iyi süreyi kaydet — tekrar oynama sebebi olur.

- [ ] Süre ölçülüyor, duraklatmada saymıyor

### 7. Credits

Kısa ve dürüst:

```
        [OYUN ADI]

     Yapım: [adın]

  Kullanılan kaynaklar:
    Sesler — [kaynak], [lisans]
    Grafikler — [kaynak], [lisans]
    Müzik — [kaynak], [lisans]

     Teşekkürler: [...]

   [Herhangi bir tuş: Ana menü]
```

Lisans atıfları **zorunlu** — `docs/LISANSLAR.md` dosyandan taşı.

Atlanabilir olsun (bir tuşa basınca geç).

- [ ] Credits hazır, lisans atıfları eksiksiz

---

## Kabul kriteri

- [ ] Ana menüden başlayıp son bölümü bitirip Credits'e ulaşabiliyorsun
- [ ] Sahne geçişleri yumuşak, donma yok
- [ ] Ayarlar ve müzik sahneler arası korunuyor
- [ ] İlerleme kaydediliyor
- [ ] Oyunu kapatıp açınca kaldığın yerden devam
- [ ] Süre doğru ölçülüyor (duraklatmada durmuş)
- [ ] Credits'te lisans atıfları var
- [ ] Sahne geçişinde `timeScale` normale dönüyor

---

## Tuzaklar

**Her sahneye `AudioManager` koymak (singleton'sız).** Sahne değişince ses
kesilir veya iki tane çalar.

**`DontDestroyOnLoad`'u her şeye uygulamak.** Tersi hata: `GameManager`
kalıcı olursa eski bölümün skoru ve checkpoint'i taşınır.

**Senkron `LoadScene`.** Oyun donuyormuş gibi görünür.

**Sahne index'i ile yüklemek.** Build Settings'te sıra değişince kod bozulur.

**Sahne geçişinde `timeScale` sıfırlamayı unutmak.** Duraklatmadan menüye
dönünce yeni sahne donuk açılır.

**Bölüm verisini sahnenin içine gömmek.** Bölüm seçim ekranı için her sahneyi
yüklemek zorunda kalırsın.

**Fade'de `Time.deltaTime` kullanmak.** `timeScale = 0` iken fade donar.

**Lisans atıflarını unutmak.** Yasal sorun.

---

## v1'de yapma

- Additive sahne yükleme
- Açık dünya / kesintisiz harita
- Bölüm içi ara sahneler
- Dallanan bölüm ağacı
- Yükleme ekranı (bölümler küçük, gerek yok)
- Sahne geçiş shader efektleri

---

## Sonraki

[Epic 17 — Zorluk ve dengeleme](17-zorluk-ve-dengeleme.md)
