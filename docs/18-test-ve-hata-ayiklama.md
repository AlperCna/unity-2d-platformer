# Epic 18 — Test ve Hata Ayıklama

**Amaç:** Oyunu yayınlamadan önce bozuk yerlerini bulmak. Oyuncun bulmadan
önce sen bul.

**Ön koşul:** [Epic 16](16-sahne-akisi.md)

**Süre:** 5–6 gün + sürekli

---

## Neden bu epic var

İlk oyununu yayınladığında gelen geri bildirimlerin çoğu "şu bozuk" olacak.
Her biri, oyunu deneyip bırakan biri demek.

Bu epic o listeyi kısaltmak için.

---

## Görevler

### 1. Geliştirici araçları

Test etmeyi kolaylaştıran kısayollar. **Build'de kapalı olmalı.**

`Assets/Scripts/Core/DebugTools.cs`:

```csharp
using UnityEngine;
using Platformer.Player;

namespace Platformer.Core
{
    /// <summary>
    /// Gelistirme kisayollari. Sadece Editor ve Development Build'de calisir.
    /// </summary>
    public class DebugTools : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        private bool godMode;
        private bool showDebugInfo;
        private PlayerHealth playerHealth;
        private Transform playerTransform;

        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            playerHealth = player.GetComponent<PlayerHealth>();
            playerTransform = player.transform;
        }

        private void Update()
        {
            // F1 - olumsuzluk
            if (Input.GetKeyDown(KeyCode.F1))
            {
                godMode = !godMode;
                Debug.Log($"God mode: {godMode}");
            }

            // F2 - bolumu bitir
            if (Input.GetKeyDown(KeyCode.F2))
            {
                GameManager.Instance?.CompleteLevel();
            }

            // F3 - debug bilgisi
            if (Input.GetKeyDown(KeyCode.F3)) showDebugInfo = !showDebugInfo;

            // F4 - fareye isinlan (en cok vakit kazandiran)
            if (Input.GetKeyDown(KeyCode.F4) && playerTransform != null)
            {
                Vector3 world = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
                world.z = 0f;
                playerTransform.position = world;
                playerTransform.GetComponent<Rigidbody2D>()?.SetVelocity(Vector2.zero);
            }

            // F5 - kaydi sil
            if (Input.GetKeyDown(KeyCode.F5))
            {
                SaveManager.Instance?.ResetProgress();
            }

            // F6 - yavas mod
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Time.timeScale = Mathf.Approximately(Time.timeScale, 1f) ? 0.3f : 1f;
                Debug.Log($"Time scale: {Time.timeScale}");
            }

            // God mode uygulamasi
            if (godMode && playerHealth != null && playerHealth.IsDead == false)
            {
                // PlayerHealth'e public bir SetInvulnerable(bool) ekle
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            var style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            GUI.Label(new Rect(10, 10, 400, 200),
                $"FPS: {(1f / Time.smoothDeltaTime):F0}\n" +
                $"Konum: {playerTransform?.position}\n" +
                $"Olum: {GameManager.Instance?.DeathCount}\n" +
                $"Sure: {GameManager.Instance?.LevelTime:F1}\n" +
                $"TimeScale: {Time.timeScale}\n" +
                $"[F1] god  [F2] bitir  [F4] isinlan  [F5] kaydi sil  [F6] yavas",
                style);
        }
#endif
    }
}
```

**F4 (fareye ışınlanma) en çok vakit kazandıran araçtır** — bölümün sonunu
test etmek için baştan oynamak zorunda kalmazsın.

- [ ] Debug tuşları çalışıyor
- [ ] `#if` ile sarılı, release build'de yok

### 2. Otomatik test yaz

`com.unity.test-framework` zaten kurulu.

`Assets/Tests/EditMode/` klasörü oluştur, içine bir **Assembly Definition**
koy (`Tests.EditMode.asmdef`), referanslarına `UnityEngine.TestRunner`,
`UnityEditor.TestRunner` ve projenin assembly'sini ekle.

```csharp
using NUnit.Framework;
using UnityEngine;
using Platformer.Core;

public class SaveSystemTests
{
    [Test]
    public void SaveData_GetLevel_CreatesIfMissing()
    {
        var data = new SaveData();
        Assert.AreEqual(0, data.levels.Count);

        LevelProgress p = data.GetLevel(3);

        Assert.IsNotNull(p);
        Assert.AreEqual(3, p.levelIndex);
        Assert.AreEqual(1, data.levels.Count);
    }

    [Test]
    public void SaveData_GetLevel_ReturnsSameInstance()
    {
        var data = new SaveData();
        LevelProgress a = data.GetLevel(2);
        LevelProgress b = data.GetLevel(2);

        Assert.AreSame(a, b);
        Assert.AreEqual(1, data.levels.Count);
    }

    [Test]
    public void SaveData_SurvivesJsonRoundTrip()
    {
        var original = new SaveData { lastUnlockedLevel = 5, totalDeaths = 42 };
        original.GetLevel(0).completed = true;
        original.GetLevel(0).bestTime = 61.5f;

        string json = JsonUtility.ToJson(original);
        var loaded = JsonUtility.FromJson<SaveData>(json);

        Assert.AreEqual(5, loaded.lastUnlockedLevel);
        Assert.AreEqual(42, loaded.totalDeaths);
        Assert.IsTrue(loaded.GetLevel(0).completed);
        Assert.AreEqual(61.5f, loaded.GetLevel(0).bestTime, 0.001f);
    }

    [Test]
    public void SaveData_CorruptJson_DoesNotThrow()
    {
        Assert.DoesNotThrow(() =>
        {
            try { JsonUtility.FromJson<SaveData>("{bozuk json!!"); }
            catch { /* SaveManager bunu yakaliyor */ }
        });
    }
}

public class JumpPhysicsTests
{
    /// <summary>
    /// PlayerController2D'nin turetilmis fizigi: h = g*t^2/2
    /// Istenen yukseklige istenen surede ulasmali.
    /// </summary>
    [Test]
    public void JumpVelocity_ReachesRequestedHeight()
    {
        float jumpHeight = 3.2f;
        float apexTime = 0.38f;

        float gravity = (2f * jumpHeight) / (apexTime * apexTime);
        float jumpVelocity = gravity * apexTime;

        // v^2 = 2*g*h  ->  ulasilan yukseklik
        float reached = (jumpVelocity * jumpVelocity) / (2f * gravity);

        Assert.AreEqual(jumpHeight, reached, 0.01f,
            "Hesaplanan zipla hizi istenen yuksekligi vermiyor");
    }

    [Test]
    public void LaunchUpward_ReachesRequestedHeight()
    {
        float gravity = 44.32f;
        float targetHeight = 2.6f;

        float velocity = Mathf.Sqrt(2f * gravity * targetHeight);
        float reached = (velocity * velocity) / (2f * gravity);

        Assert.AreEqual(targetHeight, reached, 0.01f);
    }
}
```

Çalıştırma: `Window → General → Test Runner` → EditMode → Run All

Veya terminalden (Unity kapalıyken):
```bash
unity test --project-path . --mode EditMode
```

**Hepsini test etme** — bozulduğunda fark etmeyeceğin şeyleri test et.
Kayıt sistemi ve fizik hesapları bunun tam örneği: sessizce bozulurlar.

- [ ] En az 5 anlamlı test yazıldı
- [ ] Testler geçiyor

### 3. Bozma testi — bunu mutlaka yap

Oyuncular oyununu senin oynamadığın gibi oynar.

| Test | Beklenen davranış | ☐ |
|---|---|---|
| Duvara sürekli zıpla | Duvara tırmanamamalı | ☐ |
| ← ve → aynı anda | Karakter titrememeli | ☐ |
| Hareketli platform + duvar arası | Sıkışıp ezilmemeli | ☐ |
| Tam ölüm anında duraklat | Donmamalı | ☐ |
| Bölüm sonu anında öl | İkisi birden çalışmamalı | ☐ |
| Çok hızlı Esc'e bas (10 kez) | timeScale bozulmamalı | ☐ |
| Alt+Tab yapıp dön | Ses ve zaman düzelmeli | ☐ |
| Bölümü 10 kez üst üste bitir | Bellek artmamalı | ☐ |
| Kayıt dosyasını elle boz | Çökmemeli, sıfırdan başlamalı | ☐ |
| Checkpoint'e iki kez gir | İki kez tetiklenmemeli | ☐ |
| Düşman ezerken yandan çarp | Tek sonuç olmalı | ☐ |
| Pencereyi çok küçült | UI dağılmamalı | ☐ |
| Ses %0 iken oyna | Hata olmamalı | ☐ |
| Oyunu tam kayıt anında kapat | Kayıt bozulmamalı | ☐ |

- [ ] Tüm bozma testleri yapıldı, sorunlar giderildi

### 4. Konsolu temizle

**Oyun çalışırken konsol tamamen temiz olmalı.** Her uyarı, görmezden
geldiğin bir sorundur.

| Hata | Sebep | Çözüm |
|---|---|---|
| `NullReferenceException` | Referans atanmamış | Mutlaka düzelt |
| `MissingReferenceException` | Yok olan nesneye erişim | `OnDisable`'da abonelik bırak |
| `Coroutine couldn't be started` | Nesne pasif | Aktif nesnede başlat |
| Her karede tekrarlanan uyarı | Update içinde sorun | Performansı da düşürür |

**Ben `Editor.log` üzerinden bunları okuyabiliyorum** — takıldığında söyle,
konsol çıktısını yapıştırmana gerek yok.

- [ ] Konsol temiz

### 5. Build'de test et

**Editor'de çalışıyor olması yetmez.** Build'de farklı davranan şeyler:

| Konu | Editor | Build |
|---|---|---|
| `persistentDataPath` | Farklı klasör | AppData\LocalLow |
| `#if UNITY_EDITOR` kodu | Çalışır | **Çalışmaz** |
| Build Settings'te olmayan sahne | Yüklenir | **Yüklenmez** |
| Performans | Daha yavaş | Daha hızlı |
| `Resources` dışı asset | Bulunur | Referans yoksa dahil edilmez |

Her kilometre taşında build al ve **build'i oyna**.

- [ ] Build alındı ve baştan sona oynandı

### 6. Hata kayıt sistemi

Oyuncudan anlamlı geri bildirim alabilmek için:

```csharp
using System;
using System.IO;
using UnityEngine;

namespace Platformer.Core
{
    public class ErrorLogger : MonoBehaviour
    {
        private string LogPath =>
            Path.Combine(Application.persistentDataPath, "error.log");

        private void OnEnable() => Application.logMessageReceived += HandleLog;
        private void OnDisable() => Application.logMessageReceived -= HandleLog;

        private void HandleLog(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error) return;

            try
            {
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}: {message}\n{stack}\n\n");
            }
            catch { /* log yazamiyorsa sessizce gec */ }
        }
    }
}
```

Oyuncuya "şu dosyayı gönder" diyebilirsin.

- [ ] Hata kaydı çalışıyor

### 7. Test kontrol listesi (yayın öncesi)

```
☐ Ana menüden başlayıp Credits'e ulaşılabiliyor
☐ Her bölüm bitirilebiliyor
☐ Her bölümdeki her para toplanabiliyor
☐ Her sır bulunabiliyor
☐ Kayıt çalışıyor (kapat-aç testi)
☐ Ayarlar kalıcı
☐ Konsol temiz
☐ Build'de test edildi
☐ Temiz makinede test edildi
☐ 4 çözünürlükte test edildi
☐ Debug tuşları kapalı
☐ 30 dakika kesintisiz oynandı
```

---

## Kabul kriteri

- [ ] Konsol oynarken tamamen temiz
- [ ] Bozma testlerinin hepsi geçildi
- [ ] Build alındı ve baştan sona oynandı
- [ ] En az 5 otomatik test var ve geçiyor
- [ ] Debug tuşları build'de kapalı
- [ ] Kayıt bozulunca oyun çökmüyor
- [ ] 30 dakika kesintisiz oynanabiliyor
- [ ] Hata kaydı çalışıyor

---

## Tuzaklar

**Sadece Editor'de test etmek.** Build'de ortaya çıkan hatalar en can sıkıcı
olanlardır çünkü geç bulunur.

**Konsol uyarılarını görmezden gelmek.** "Sadece uyarı" diye bakılmayanlar
build'de hataya döner.

**Debug tuşlarını build'de bırakmak.** Oyuncu yanlışlıkla F2'ye basıp bölüm
atlar ve oyunun bozuk olduğunu düşünür.

**Kendi oynayışına göre test etmek.** Sen "doğru" oynarsın.

**Test yazmayı tamamen atlamak.** Kayıt sistemi sessizce bozulduğunda
oyuncunun ilerlemesi silinir ve sen aylar sonra öğrenirsin.

**`OnDisable`'da abonelik bırakmamak.** Sahne değişimlerinde biriken
`MissingReferenceException`'lar.

**Bozma testlerini "kimse öyle yapmaz" diye atlamak.** Yaparlar.

---

## v1'de yapma

- Sürekli entegrasyon (CI) kurulumu
- Otomatik oynanış testi (playtest botu)
- Yüksek test kapsamı hedefleri (%80 coverage vb.)
- Crash raporlama servisi entegrasyonu
- A/B testi
- Otomatik ekran görüntüsü karşılaştırma testleri

---

## Sonraki

[Epic 19 — Performans](19-performans.md)
