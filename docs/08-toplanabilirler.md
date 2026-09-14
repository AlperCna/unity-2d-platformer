# Epic 08 — Toplanabilirler ve Sırlar

**Amaç:** Oyuncuya keşfetme sebebi vermek ve bölümde sessizce yol göstermek.

**Ön koşul:** [Epic 04](04-seviye-tasarimi-prensipleri.md)

**Süre:** 3–4 gün

---

## Neden bu epic var

Toplanabilirler iki iş yapar, ve **ikincisi daha önemlidir**:

1. **Ödül** — toplamak tatmin edici
2. **Yönlendirme** — oyuncuya nereye gideceğini söyler, tek kelime yazmadan

Para dizisi bir zıplama yayı çizdiğinde, oyuncu farkında olmadan doğru rotayı
izler. Bu, oyun tasarımının en zarif araçlarından biridir ve bedavadır.

---

## Elindeki altyapı

`Assets/Scripts/Gameplay/Coin.cs`:
- Salınım + dönme animasyonu, her para farklı fazda (hepsi senkron değil)
- Toplanınca büyüyüp saydamlaşma efekti
- `GameManager` üzerinden skor

[`LevelCursor`](../Assets/Editor/LevelCursor.cs) (Epic 04) para yerleştirmeyi
hallediyor:

| Çağrı | Ne yapar |
|---|---|
| `c.Coins(3)` | Son zemin parçasının üstüne yatay dizi |
| `c.Gap(4f, coinArc: 5)` | Boşluğun üstüne zıplama yayı — parabol otomatik |

Yayın tepesi zıplama yüksekliğinin %80'iyle sınırlı; ulaşılamayacak yere
para konmuyor.

---

## Görevler

### 1. Toplanabilir hiyerarşisi

Üç kademe yeterli:

| Kademe | Bölüm başına | İşlev | Görsel |
|---|---|---|---|
| **Para** | 20–30 | Yönlendirme, küçük ödül | Küçük, sarı |
| **Mücevher** | 1–3 | Risk ödülü, zor yerlerde | Büyük, parlak, farklı renk |
| **Sır** | 1 | Gerçek keşif | Belirgin, özel |

- [ ] Üç kademe tanımlandı

### 2. Combo'lu toplama sesi

Art arda toplamada ses perdesinin yükselmesi, küçük bir detay ama toplamayı
bağımlılık yapar. Mario'nun 8 para sesi bu yüzden vardır.

`Coin.cs` içine ekle:

```csharp
// --- Combo takibi (tum paralar icin ortak) ---
private static int comboCount;
private static float lastCollectTime;

[Header("Combo")]
[Tooltip("Bu sure icinde toplanan paralar combo sayilir.")]
[SerializeField] private float comboWindow = 1.2f;

[Tooltip("Her combo adiminda perde bu kadar artar.")]
[SerializeField] private float pitchStep = 0.06f;

[Tooltip("Maksimum perde artisi.")]
[SerializeField] private float maxPitchIncrease = 0.6f;

private float CalculatePitch()
{
    // Combo penceresi kapandiysa sifirla
    if (Time.time - lastCollectTime > comboWindow) comboCount = 0;

    comboCount++;
    lastCollectTime = Time.time;

    return 1f + Mathf.Min(comboCount * pitchStep, maxPitchIncrease);
}
```

`OnTriggerEnter2D` içinde:

```csharp
float pitch = CalculatePitch();
// Epic 13 sonrasi:
// AudioManager.Instance.PlaySfx(collectClip, pitch);
```

**Dikkat:** `static` alan sahne değişiminde sıfırlanmaz. `GameManager.Start()`
içinde `Coin.ResetCombo()` çağıran bir static metot ekle:

```csharp
public static void ResetCombo()
{
    comboCount = 0;
    lastCollectTime = 0f;
}
```

- [ ] Combo perdesi çalışıyor
- [ ] Sahne değişiminde sıfırlanıyor

### 3. Sır sistemi

`Assets/Scripts/Gameplay/SecretArea.cs`:

```csharp
using System.Collections;
using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Gizli alan. Oyuncu girince ortuyu saydamlastirir ve sirri kaydeder.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SecretArea : MonoBehaviour
    {
        [Header("Ortu")]
        [Tooltip("Girisi gizleyen sahte duvar / ortu.")]
        [SerializeField] private SpriteRenderer cover;

        [Tooltip("Ortunun sonundaki saydamlik.")]
        [Range(0f, 1f)]
        [SerializeField] private float revealedAlpha = 0.15f;

        [SerializeField] private float fadeDuration = 0.35f;

        [Header("Odul")]
        [SerializeField] private int scoreReward = 10;

        public bool Found { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Found || !other.CompareTag("Player")) return;

            Found = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreReward);
                GameManager.Instance.ReportSecretFound();   // Epic 10'da eklenecek
            }

            if (cover != null) StartCoroutine(FadeCover());
            // Epic 13: "sir bulundu" sesi
        }

        private IEnumerator FadeCover()
        {
            Color c = cover.color;
            float startAlpha = c.a;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(startAlpha, revealedAlpha, t / fadeDuration);
                cover.color = c;
                yield return null;
            }

            c.a = revealedAlpha;
            cover.color = c;
        }
    }
}
```

**Gizleme teknikleri:**

| Teknik | Nasıl | İpucu bırakma yolu |
|---|---|---|
| Sahte duvar | Örtü sprite'ı, girince saydamlaşır | Duvara giden bir para dizisi |
| Kamera dışı yükseklik | Yukarıda görünmeyen platform | Yukarı giden tek bir para |
| Tehlikeli geçit | Dikenler arası dar yol | Arkada parlayan bir ışık |
| Yalancı zemin | Altından geçilebilen platform | Farklı renkte tek bir karo |

**Kural: sır bulunabilir olmalı.** Hiçbir ipucu olmayan sır, sır değil
kazadır. Her sırrın en az bir ipucu olsun.

- [ ] Sır sistemi çalışıyor
- [ ] Her sırrın ipucu var

### 4. Yönlendirme için para yerleştir

**Bu epic'in en önemli görevi.** Paraları rastgele serpme.

**Zıplama yayı çiz:**

```
              ○ ○ ○
            ○       ○
          ○           ○
    ▓▓▓▓▓                ▓▓▓▓▓
```

Oyuncu "şuraya zıplamalıyım" diye düşünmez, sadece paraları takip eder.

**Riski işaretle:**

```
    ○ ○ ○ ○ ○      ← tehlikeli ama kısa yol, ödüllü
    ▲▲▲▲▲▲▲▲▲
  ▓▓▓▓▓▓▓▓▓▓▓▓▓   ← güvenli ama uzun yol
```

**Gizli yolu ima et:**

```
    ▓▓▓▓▓▓▓
    ○ ○ →         ← duvara doğru giden dizi: orada bir şey var
    ▓▓▓▓▓▓▓
```

**Yapma:**
- Ulaşılamayacak yere para koyma
- Tehlikenin *içine* para koyma (oyuncu ölür ve senin hatan olur)
- Zorunlu yolun dışına, ama ulaşılabilir görünen yere koyma

- [ ] Bölüm 1'deki paralar bilinçli olarak yol gösteriyor

### 5. İlerleme verisi

`GameManager`'a sır takibi ekle:

```csharp
public int SecretsFound { get; private set; }
public int TotalSecrets { get; private set; }
public System.Action<int, int> OnSecretsChanged;

// Start() icinde:
#if UNITY_2023_1_OR_NEWER
TotalSecrets = FindObjectsByType<Gameplay.SecretArea>(
    FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
#else
TotalSecrets = FindObjectsOfType<Gameplay.SecretArea>(true).Length;
#endif

public void ReportSecretFound()
{
    SecretsFound++;
    OnSecretsChanged?.Invoke(SecretsFound, TotalSecrets);
}
```

Bölümler arası kalıcılık için (Epic 10):

```csharp
[System.Serializable]
public class LevelProgress
{
    public int levelIndex;
    public bool completed;
    public int coinsCollected;
    public int totalCoins;
    public bool secretFound;
    public float bestTime;
    public int deathCount;
}
```

- [ ] Sır sayacı çalışıyor
- [ ] `LevelProgress` yapısı tanımlandı

### 6. Toplama geri bildirimi

Toplamak **tatmin edici** olmalı. Katmanlar birleşince olur:

| Katman | Ne | Epic |
|---|---|---|
| Görsel | Büyüme + saydamlaşma (var) + parçacık | 14 |
| Ses | Kısa "ting", combo'da perde yükselir | 13 |
| UI | Sayaç zıplar/parlar | 14 |
| Zaman | 0.05 sn donma — **sadece mücevher/sırda** | 14 |

Hit stop'u normal paraya koyma — 25 kez donan oyun sinir bozucudur.

- [ ] Geri bildirim planı yapıldı (uygulama Epic 13/14'te)

### 7. Tamamlama hedefi

Bölüm sonunda:

```
Para      18 / 25
Sır       ✓ bulundu
```

Bölüm seçiminde tamamlanan bölümlerde tik/yıldız.

**%100 tamamlama zorunlu olmasın.** Oyunu bitirmek için hepsini toplamak
gerekmesin. `LevelGoal.requireAllCoins` seçeneği var ama **kapalı tut**.

- [ ] Tamamlama isteğe bağlı

---

## Kabul kriteri

- [ ] Paralar yol gösteriyor, rastgele serpilmemiş
- [ ] Combo perdesi çalışıyor ve sahne değişiminde sıfırlanıyor
- [ ] Her bölümde 1 bulunabilir sır var, ipucuyla
- [ ] Sır bulununca görsel + skor tepkisi var
- [ ] Bölüm sonu özeti hazırlanabilir durumda
- [ ] Hiçbir para ulaşılamaz yerde değil (kendin test ettin)

---

## Tuzaklar

**Rastgele serpmek.** En büyük kayıp fırsat. Her para bir karar olmalı.

**Ulaşılamaz para.** Oyuncu 10 dakika deneyip alamazsa oyunu suçlar.
**Her parayı kendin topla** ve test et.

**Çok fazla para.** 200 para toplamak iş gibi hissettirir. 20–30 yeterli.

**Zorunlu tamamlama.** "Tüm paraları topla yoksa kapı açılmaz" — sinir bozucu.

**İpuçsuz sır.** Bulunamayan sır, olmayan sırdır.

**`static` combo alanını sıfırlamamak.** Sahne değişince perde tavanda kalır.

**Toplama sesi tekdüze.** Aynı ses 25 kez = gürültü.

---

## v1'de yapma

- Para ile alışveriş / mağaza sistemi
- Toplanabilir kostüm, karakter, boya
- Koleksiyon galerisi / müze ekranı
- Başarım (achievement) sistemi
- Günlük görevler
- Para ile can satın alma
- Farklı para türleri (bakır/gümüş/altın)

---

## Sonraki

[Epic 09 — Can, hasar, ölüm](09-can-hasar-olum.md)
