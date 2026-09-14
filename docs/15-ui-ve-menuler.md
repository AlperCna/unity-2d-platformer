# Epic 15 — UI ve Menüler

**Amaç:** Oyunu "proje" olmaktan çıkarıp "ürün" yapmak. Menüsü olmayan bir
şey oyun değil, demodur.

**Ön koşul:** [Epic 10](10-checkpoint-ve-kayit.md)

**Süre:** 1 hafta

---

## Neden bu epic var

Menüsü olmayan bir şey oyun değil, demodur. Oyuncu oyununu açtığında
karşısına doğrudan oynanış çıkarsa ne olduğunu anlamaz; duraklatamıyorsa
telefonu çaldığında oyunu kapatır; sesi kısamıyorsa gece oynayamaz.

Bunlar "ekstra" değil, oyunun **kullanılabilir** olmasının şartı.

---


## Elindeki altyapı

`Assets/Scripts/UI/HudController.cs` — skor, can, mesaj gösteriyor;
`GameManager` olaylarına abone. Legacy `Text` kullanıyor.

**Karar: TextMeshPro'ya geç.** Legacy `Text` ölçeklendiğinde bulanıklaşır,
TMP keskin kalır ve kontur/gölge desteği çok daha iyi.

`Window → TextMeshPro → Import TMP Essential Resources`

---

## Görevler

### 1. Ekran listesi

| Ekran | İçerik |
|---|---|
| **Ana menü** | Oyna, Bölümler, Ayarlar, Çıkış |
| **Bölüm seçimi** | Bölüm ızgarası, kilit, para/sır göstergesi |
| **Oynanış HUD** | Para sayacı, süre (isteğe bağlı) |
| **Duraklatma** | Devam, Yeniden başla, Ayarlar, Ana menü |
| **Bölüm sonu** | Süre, para, sır, ölüm, Sonraki |
| **Ayarlar** | Ses, tam ekran, sarsıntı, ilerlemeyi sıfırla |

6 ekran. Fazlası v1'de gereksiz.

- [ ] Ekranlar listelendi

### 2. Canvas kurulumu

Her Canvas için:

| Ayar | Değer |
|---|---|
| Render Mode | Screen Space – Overlay |
| Canvas Scaler → UI Scale Mode | **Scale With Screen Size** |
| Reference Resolution | 1920 × 1080 |
| Screen Match Mode | Match Width Or Height |
| Match | **0.5** |

Bu ayarlar olmadan UI farklı çözünürlüklerde dağılır. Unity'nin varsayılanı
"Constant Pixel Size" — **mutlaka değiştir**.

`LevelBuilder` HUD Canvas'ı zaten böyle kuruyor.

- [ ] Tüm Canvas'lar doğru ayarlı

### 3. Duraklatma menüsü

En çok kullanılacak menü ve en çok hata yapılan yer.

`Assets/Scripts/UI/PauseMenu.cs`:

```csharp
using UnityEngine;
using UnityEngine.EventSystems;
using Platformer.Core;

namespace Platformer.UI
{
    /// <summary>
    /// Esc ile acilan duraklatma menusu.
    /// timeScale = 0 iken dikkat: WaitForSeconds calismaz,
    /// animasyonlar unscaledDeltaTime kullanmali.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Odak")]
        [Tooltip("Menu acilinca secili olacak buton (klavye/gamepad icin).")]
        [SerializeField] private GameObject firstSelected;

        public static bool IsPaused { get; private set; }

        private void Awake()
        {
            // Sahne basinda kapali olsun
            if (panel != null) panel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            IsPaused = false;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            // Ayarlar acikken Esc once onu kapatsin
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                SelectFirst();
                return;
            }

            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            if (panel != null) panel.SetActive(true);
            SelectFirst();

            // Istege bagli: sesleri de duraklat
            // AudioListener.pause = true;
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (panel != null) panel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            // AudioListener.pause = false;
        }

        public void RestartLevel()
        {
            // ZORUNLU: sahne yuklemeden once timeScale duzelt
            Time.timeScale = 1f;
            IsPaused = false;
            GameManager.Instance?.RestartLevel();
        }

        public void OpenSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            SaveManager.Instance?.Save();
            SceneLoader.Instance?.LoadScene("MainMenu");   // Epic 16
        }

        private void SelectFirst()
        {
            if (firstSelected == null || EventSystem.current == null) return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        private void OnDisable()
        {
            // Guvenlik agi: menu yok olurken oyun donmus kalmasin
            if (IsPaused)
            {
                Time.timeScale = 1f;
                IsPaused = false;
            }
        }
    }
}
```

**`timeScale = 0` iken dikkat edilecekler:**

| Sorun | Çözüm |
|---|---|
| `Time.deltaTime` = 0 | UI animasyonları `unscaledDeltaTime` kullansın |
| `WaitForSeconds` çalışmaz | `WaitForSecondsRealtime` |
| Coroutine'ler donar | Menü coroutine'leri realtime olmalı |
| Ses devam eder | İstersen `AudioListener.pause = true` |

- [ ] Duraklatma çalışıyor
- [ ] Esc ile açılıp kapanıyor, ayarlar açıkken önce onu kapatıyor
- [ ] `timeScale` hiçbir yolla 0'da kalmıyor

### 4. Ayarlar menüsü

```csharp
using UnityEngine;
using UnityEngine.UI;
using Platformer.Core;

namespace Platformer.UI
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Ses")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("Gorsel")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle screenShakeToggle;

        [Header("Tehlikeli")]
        [SerializeField] private GameObject resetConfirmPanel;

        private bool initializing;

        private void OnEnable()
        {
            LoadFromSave();
        }

        private void LoadFromSave()
        {
            SaveData d = SaveManager.Instance?.Data;
            if (d == null) return;

            initializing = true;   // slider degisince kaydetmesin

            masterSlider.value = d.masterVolume;
            musicSlider.value = d.musicVolume;
            sfxSlider.value = d.sfxVolume;
            fullscreenToggle.isOn = d.fullscreen;

            initializing = false;
        }

        // --- Slider olaylarina bagla (Inspector'dan) ---

        public void OnMasterChanged(float v)
        {
            AudioManager.Instance?.SetMasterVolume(v);
            if (initializing) return;
            SaveManager.Instance.Data.masterVolume = v;
            SaveManager.Instance.Save();
        }

        public void OnMusicChanged(float v)
        {
            AudioManager.Instance?.SetMusicVolume(v);
            if (initializing) return;
            SaveManager.Instance.Data.musicVolume = v;
            SaveManager.Instance.Save();
        }

        public void OnSfxChanged(float v)
        {
            AudioManager.Instance?.SetSfxVolume(v);
            if (initializing) return;
            SaveManager.Instance.Data.sfxVolume = v;
            SaveManager.Instance.Save();
        }

        public void OnFullscreenChanged(bool on)
        {
            Screen.fullScreen = on;
            if (initializing) return;
            SaveManager.Instance.Data.fullscreen = on;
            SaveManager.Instance.Save();
        }

        public void OnScreenShakeChanged(bool on)
        {
            CameraRig.CameraFollow.ScreenShakeEnabled = on;
        }

        // --- Ilerlemeyi sifirla: IKI KEZ onay ---

        public void RequestReset() => resetConfirmPanel.SetActive(true);
        public void CancelReset() => resetConfirmPanel.SetActive(false);

        public void ConfirmReset()
        {
            SaveManager.Instance?.ResetProgress();
            resetConfirmPanel.SetActive(false);
            LoadFromSave();
        }
    }
}
```

**`initializing` bayrağı neden gerekli:** slider'a değer atadığında `onValueChanged`
tetiklenir ve kaydetmeye çalışır. Yükleme sırasında bu istenmez.

Değişiklikler **anında** uygulansın ve **anında** kaydedilsin. "Uygula"
butonu koyma.

- [ ] Ayarlar çalışıyor ve kalıcı
- [ ] Sıfırlama iki kez onay soruyor

### 5. Bölüm seçim ekranı

Izgara halinde bölümler. Her kutucukta:

```
┌─────────┐
│    3    │   ← bölüm numarası
│ ★ 18/25 │   ← tamamlandı + para
│   ◆ ✓   │   ← sır bulundu
└─────────┘
```

Veriyi `SaveManager.Instance.Data.GetLevel(index)` ile oku.

**Kilit açma kuralı basit olsun:** önceki bölüm bitince sonraki açılır.
Yıldız/para şartı koyma — oyuncuyu tıkar ve geri dönmeye zorlar.

```csharp
bool unlocked = SaveManager.Instance.IsLevelUnlocked(levelIndex);
button.interactable = unlocked;
lockIcon.SetActive(!unlocked);
```

- [ ] Bölüm seçimi çalışıyor
- [ ] Kilit durumu kayıttan okunuyor

### 6. Bölüm sonu ekranı

Oyuncunun kendini iyi hissetmesi gereken an:

```
        BÖLÜM TAMAMLANDI

   Süre        1:24      (en iyi: 1:19)
   Para        18 / 25
   Sır         ✓ bulundu
   Ölüm        7

   [Sonraki]  [Tekrar]  [Menü]
```

**Sayılar sırayla ve sayarak belirsin** (Epic 14). Hepsi aynı anda
görünürse tatmin etmez.

```csharp
private IEnumerator RevealStats()
{
    yield return CountUp(coinText, 0, coinsCollected, 0.5f);
    yield return new WaitForSecondsRealtime(0.15f);
    secretText.gameObject.SetActive(true);
    punchSecret.Punch();
    yield return new WaitForSecondsRealtime(0.15f);
    // ...
}
```

- [ ] Bölüm sonu ekranı var, sayılar sayarak beliriyor

### 7. Klavye/gamepad navigasyonu

Fare olmadan da gezilebilmeli:

- `EventSystem` → **First Selected** ayarla
- Butonlarda `Navigation` → **Automatic** (veya elle Explicit)
- Seçili butonun görünümü **net** olsun (renk + ölçek + kontur)

Menüsü sadece fareyle kullanılan oyun amatör hissettirir.

**Test:** fareyi çıkar, sadece ok tuşları + Enter ile tüm menüleri gez.

- [ ] Klavyeyle tüm menüler gezilebiliyor

### 8. Çözünürlük testi

| Çözünürlük | Oran | Kontrol |
|---|---|---|
| 1920×1080 | 16:9 | Referans |
| 1280×720 | 16:9 | Yazılar okunuyor mu |
| 2560×1080 | 21:9 | Kenardaki öğeler kaybolmuyor mu |
| 1024×768 | 4:3 | Üst/alt kesilmiyor mu |

Anchor'ları doğru kullanırsan hepsi çalışır:
- Sol üstteki öğe → sol üste anchor
- Ortadaki öğe → ortaya anchor
- Alt bardaki öğe → alt kenara stretch

Game view'da `+` ile bu çözünürlükleri ekleyip test et.

- [ ] 4 oranda da UI düzgün

---

## Kabul kriteri

- [ ] 6 ekranın hepsi çalışıyor
- [ ] Duraklatma `timeScale` hatası yapmıyor
- [ ] Ayarlar kaydediliyor ve geri yükleniyor
- [ ] Bölüm seçimi ilerlemeyi doğru gösteriyor
- [ ] Klavyeyle tüm menüler gezilebiliyor
- [ ] 4 farklı en-boy oranında UI bozulmuyor
- [ ] Yazılar keskin (TMP)
- [ ] İlerleme sıfırlama iki onay istiyor

---

## Tuzaklar

**`timeScale = 0` iken `WaitForSeconds`.** Sonsuza kadar bekler, menü kilitlenir.

**Duraklatmadan sonra `timeScale`'i geri yüklememek.** Ana menüye dönüp
tekrar oyuna girince her şey donuk. `OnDisable` güvenlik ağını koy.

**Canvas Scaler'ı "Constant Pixel Size" bırakmak.** Unity'nin varsayılanı
budur ve farklı çözünürlüklerde UI dağılır.

**Anchor'ları ayarlamamak.** Öğeler yanlış yere kayar.

**Legacy Text ile büyük yazı.** Bulanıklaşır. TMP kullan.

**Klavye navigasyonunu atlamak.** Gamepad'li oyuncu menüde sıkışır.

**Slider yüklerken kaydetmek.** `initializing` bayrağı olmadan sonsuz döngü
veya yanlış kayıt olur.

**Sıfırlamayı tek onayla yapmak.** Yanlışlıkla basan oyuncu her şeyini kaybeder.

---

## v1'de yapma

- Animasyonlu sahne geçiş efektleri (basit fade yeter)
- Başarım/istatistik ekranları
- Skor tablosu (leaderboard)
- Tuş atama (rebinding) arayüzü
- Birden fazla dil
- Ekran içi yardım/ipucu sistemi
- Ayarlarda grafik kalitesi seçenekleri (2D oyunda gereksiz)

---

## Sonraki

[Epic 16 — Sahne akışı](16-sahne-akisi.md)
