# Epic 14 — Juice (His Veren Küçük Efektler)

**Amaç:** Aynı oynanışı iki kat daha tatmin edici hale getirmek. Tek satır
mekanik değiştirmeden.

**Ön koşul:** [Epic 12](12-animasyon.md), [Epic 13](13-ses-ve-muzik.md)

**Süre:** 4–5 gün — **getirisi en yüksek epic budur**

---

## Neden bu epic var

"Juice", oyuncunun her eylemine oyunun verdiği abartılı tepkidir. Mekanik
değişmez, **his** değişir.

Aynı oyunu juice'suz ve juice'lu oynatırsan insanlar ikincisi için
"daha iyi kontrol ediliyor" der. Kontroller aynıdır. Fark tamamen geri
bildirimdedir.

Bu epic az iş, çok etki. **Atlama.**

---

## Juice katmanları

Her önemli eylem için bu listeyi düşün:

| Katman | Araç |
|---|---|
| 1. Görsel | Parçacık, flaş, ölçek değişimi, iz |
| 2. Hareket | Sarsıntı, sekme, geri tepme |
| 3. Zaman | Hit stop, yavaşlama |
| 4. Ses | Katmanlı, perdesi değişen |
| 5. Kamera | Sarsıntı, kısa kaydırma |

Bir eylem **3 veya daha fazla katmandan** tepki alıyorsa "juicy" hissettirir.

Örnek — düşman ezme:
```
görsel  → ezilme animasyonu + parçacık patlaması
hareket → oyuncu yukarı sekiyor
zaman   → 0.05 sn donma
ses     → "pat" + oyuncunun zıplama sesi
kamera  → 0.1 sn hafif sarsıntı
```
Beş katman. Bu yüzden düşman ezmek tatmin edicidir.

---

## Görevler

### 1. Kamera sarsıntısı

Epic 03'te altyapısını kurmuştun. Şimdi kullan.

```csharp
var cam = UnityEngine.Camera.main?.GetComponent<CameraRig.CameraFollow>();
cam?.Shake(duration, magnitude);
```

| Olay | Süre | Şiddet |
|---|---|---|
| Ölüm | 0.25 sn | 0.40 |
| Düşman ezme | 0.10 sn | 0.18 |
| Sert iniş (vy < −18) | 0.08 sn | 0.10 |
| Zıplama pedi | 0.12 sn | 0.15 |
| Düşen platform | 0.15 sn | 0.12 |

**Az kullan.** Her şey sarsarsa hiçbir şey özel hissettirmez ve oyuncunun
başı ağrır. Normal zıplamada **sarsıntı olmasın**.

- [ ] Sarsıntı 4–5 olaya bağlandı, abartısız

### 2. Hit stop

Epic 09'da `TimeController` yazdın. Şimdi kullan.

```csharp
TimeController.Instance?.HitStop(0.05f);
```

| Olay | Süre |
|---|---|
| Düşman ezme | 0.05 sn |
| Ölüm | 0.08 sn |
| Sır bulma | 0.06 sn |
| Normal para | **yok** |

**Hatırlatma:** `WaitForSecondsRealtime` kullanılmalı. `WaitForSeconds`
`timeScale = 0` iken sonsuza kadar bekler ve oyun kilitlenir.

- [ ] Hit stop bağlandı, oyun kilitlenmiyor

### 3. Parçacık yöneticisi

Her efekt için ayrı prefab yerleştirmek yerine merkezi bir sistem:

`Assets/Scripts/Core/EffectManager.cs`:

```csharp
using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Parcacik efektlerini havuzdan cikarip konumda oynatir.
    /// Instantiate/Destroy yerine yeniden kullanim.
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }

        [System.Serializable]
        public class EffectEntry
        {
            public string id;
            public ParticleSystem prefab;
            public int poolSize = 5;
        }

        [SerializeField] private EffectEntry[] effects;

        private System.Collections.Generic.Dictionary<string, ObjectPool<ParticleSystem>> pools;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            pools = new System.Collections.Generic.Dictionary<string, ObjectPool<ParticleSystem>>();
            foreach (EffectEntry e in effects)
            {
                if (e.prefab == null || string.IsNullOrEmpty(e.id)) continue;
                pools[e.id] = new ObjectPool<ParticleSystem>(e.prefab, e.poolSize, transform);
            }
        }

        /// <summary>Efekti konumda oynat. id ornek: "jump_dust"</summary>
        public void Play(string id, Vector3 position, Quaternion rotation = default)
        {
            if (!pools.TryGetValue(id, out var pool))
            {
                Debug.LogWarning($"EffectManager: '{id}' tanimli degil.");
                return;
            }

            ParticleSystem ps = pool.Get(position, rotation);
            ps.Play();
            StartCoroutine(ReturnWhenDone(pool, ps));
        }

        private System.Collections.IEnumerator ReturnWhenDone(
            ObjectPool<ParticleSystem> pool, ParticleSystem ps)
        {
            // Parcacik omru + biraz pay
            float wait = ps.main.duration + ps.main.startLifetime.constantMax;
            yield return new WaitForSeconds(wait);
            pool.Return(ps);
        }
    }
}
```

**Minimum efekt seti:**

| id | Ne zaman | Ayarlar |
|---|---|---|
| `jump_dust` | Zıplarken (ayak altında) | 6–10 parçacık, 0.35 sn, gri |
| `land_dust` | İnerken | 8–14 parçacık, 0.4 sn, yana yayılan |
| `run_dust` | Yön değiştirirken | 4–6 parçacık, 0.3 sn |
| `coin_pop` | Para toplanınca | 8 parçacık, 0.4 sn, sarı |
| `death_burst` | Ölünce | 20 parçacık, 0.7 sn, karakter rengi |
| `enemy_pop` | Düşman ezilince | 12 parçacık, 0.5 sn |

**Zıplama/iniş tozu en etkilisi.** Basit bir gri toz bulutu, karakterin
zeminle ilişkisini somutlaştırır ve ağırlık hissi verir.

**Parçacık ayarları (hepsi için):**
- Start Lifetime: 0.3–0.5 sn (kısa!)
- Max Particles: 15 (az!)
- Simulation Space: **World** (karakter hareket edince toz takip etmesin)
- Color over Lifetime: sona doğru saydamlaş

- [ ] `EffectManager` yazıldı
- [ ] 5–6 efekt tanımlı ve bağlı

### 4. Ekran efektleri

Tam ekran flaş — shader gerekmez, sadece bir `Image`.

`Assets/Scripts/UI/ScreenFlash.cs`:

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Tam ekran renk flasi. Canvas'ta tam ekran bir Image gerekir.
    /// Raycast Target KAPALI olmali - tiklamalari engellemesin.
    /// </summary>
    public class ScreenFlash : MonoBehaviour
    {
        public static ScreenFlash Instance { get; private set; }

        [SerializeField] private Image flashImage;

        private Coroutine current;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (flashImage != null)
            {
                flashImage.raycastTarget = false;
                SetAlpha(0f);
            }
        }

        public void Flash(Color color, float intensity = 0.5f, float duration = 0.25f)
        {
            if (flashImage == null) return;
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(FlashRoutine(color, intensity, duration));
        }

        private IEnumerator FlashRoutine(Color color, float intensity, float duration)
        {
            flashImage.color = new Color(color.r, color.g, color.b, intensity);

            float t = 0f;
            while (t < duration)
            {
                // unscaled: hit stop sirasinda da sonsun
                t += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(intensity, 0f, t / duration));
                yield return null;
            }

            SetAlpha(0f);
            current = null;
        }

        private void SetAlpha(float a)
        {
            Color c = flashImage.color;
            c.a = a;
            flashImage.color = c;
        }
    }
}
```

Kullanım:

| Olay | Renk | Şiddet | Süre |
|---|---|---|---|
| Ölüm | Kırmızı | 0.45 | 0.3 sn |
| Checkpoint | Beyaz | 0.2 | 0.15 sn |
| Sır bulma | Altın | 0.3 | 0.25 sn |

- [ ] Ekran flaşı çalışıyor

### 5. UI hareketlendir

Statik UI ölü görünür.

```csharp
using System.Collections;
using UnityEngine;

namespace Platformer.UI
{
    /// <summary>Bir RectTransform'u kisa sure buyutup geri dondurur.</summary>
    public class UIPunch : MonoBehaviour
    {
        [SerializeField] private float punchScale = 1.35f;
        [SerializeField] private float duration = 0.22f;

        private Coroutine current;
        private Vector3 baseScale;

        private void Awake() => baseScale = transform.localScale;

        public void Punch()
        {
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                // Sin egrisi: hizli buyu, yumusak don
                float k = Mathf.Sin((t / duration) * Mathf.PI);
                transform.localScale = baseScale * (1f + (punchScale - 1f) * k);
                yield return null;
            }

            transform.localScale = baseScale;
            current = null;
        }
    }
}
```

Bağla:
- Para sayacı artınca `Punch()`
- Can göstergesi değişince `Punch()`
- Menü butonu üzerine gelince hafif büyüsün
- Sayılar anında değil, **sayarak** artsın (0→18 arası hızlıca sayılsın)

- [ ] UI animasyonları eklendi

### 6. İnce ayar: abartıyı geri al

Juice ekledikten sonra **fazlasını geri al.** Sık yapılan hata: her şey
sarsılıyor, patlıyor, donuyor — göz yoruluyor.

**Test:** 5 dakika kesintisiz oyna. Başın ağrıyor mu? Gözün yoruldu mu?
O zaman fazla.

**Kural: en önemli 3 eylem juicy olsun, gerisi sade.** Platform oyununda
bunlar genelde:
1. Zıplama / iniş
2. Ölüm
3. Düşman ezme

Normal koşma, para toplama, menü gezinme — bunlar sade kalsın.

**`prefers-reduced-motion` benzeri seçenek:** ayarlarda "Ekran sarsıntısı"
açma/kapama koy. Bazı oyuncular için erişilebilirlik meselesidir.

```csharp
public static bool ScreenShakeEnabled = true;

public void Shake(float duration, float magnitude)
{
    if (!ScreenShakeEnabled) return;
    // ...
}
```

- [ ] Abartı geri alındı, 5 dakika rahat oynanıyor
- [ ] Sarsıntı kapatma seçeneği var

---

## Kabul kriteri

- [ ] Zıplama ve iniş görsel + işitsel geri bildirim veriyor
- [ ] Düşman ezmek en az 4 katmandan tepki alıyor
- [ ] Ölüm net ve okunabilir
- [ ] Para toplamak keyifli
- [ ] UI hareketli
- [ ] 5 dakika oynamak göz yormuyor
- [ ] **`Time.timeScale` hiçbir durumda 0'da takılı kalmıyor**
- [ ] Sarsıntı kapatılabiliyor
- [ ] Parçacıklar havuzdan geliyor

---

## Tuzaklar

**`WaitForSeconds` ile hit stop.** `timeScale = 0` iken sonsuza kadar bekler,
oyun donar. `WaitForSecondsRealtime` kullan.

**`timeScale`'i geri yüklemeyi unutmak.** Coroutine yarıda kesilirse
(sahne değişimi, nesne yok olması) oyun donmuş kalır.

**Juice coroutine'lerini yok olacak nesnede çalıştırmak.** Düşman ezilip
yok olurken başlattığı hit stop yarıda kalır. Kalıcı bir yönetici üzerinden
çalıştır.

**Her şeyi sarsmak.** Sarsıntı enflasyonu — hiçbir şey özel hissettirmez.

**Çok fazla parçacık.** Performans düşer, ekran karışır, oynanış görünmez.

**Parçacıkta Simulation Space: Local.** Karakter hareket edince toz da
onunla hareket eder, tuhaf görünür. World olmalı.

**`Time.deltaTime` kullanmak.** Hit stop sırasında efektler donar.
`unscaledDeltaTime` olmalı.

**Juice'u mekanik sorununu örtmek için kullanmak.** Kontroller kötüyse
parçacık kurtarmaz. Önce Epic 02'yi bitir.

---

## v1'de yapma

- Post-processing yığını (bloom, chromatic aberration, vignette)
- Shader tabanlı ekran distorsiyonu
- Fizik etkileşimli parçacıklar
- Yavaş çekim (slow motion) sistemi
- Gamepad titreşimi (kolay ama zorunlu değil)
- Işık efektleri / 2D Lights
- Ekran geçiş shader'ları

---

## Sonraki

[Epic 15 — UI ve menüler](15-ui-ve-menuler.md)
