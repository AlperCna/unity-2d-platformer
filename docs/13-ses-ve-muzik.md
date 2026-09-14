# Epic 13 — Ses ve Müzik

**Amaç:** Oyunu, sesi kapatınca eksik hissettirecek hale getirmek.

**Ön koşul:** [Epic 02](02-karakter-hissiyati.md)

**Süre:** 4–5 gün

---

## Neden bu epic var

Ses, **emeğe oranla en çok getirisi olan** yatırımdır. Aynı oyunu sesli ve
sessiz oynat; sesli olan iki kat daha "bitmiş" hissettirir.

Sebebi basit: ses, dokunsal geri bildirim yerine geçer. Ekrandaki bir şeye
dokunamazsın, ama sesi onu somutlaştırır. Zıplama sesi olmadan karakter
havada süzülüyor gibidir; sesle birlikte yerden itiliyormuş gibi olur.

---

## Elindeki altyapı

`PlayerController2D` ses için hazır olay noktaları veriyor:

```csharp
public System.Action OnJumped;   // zıplama sesi
public System.Action OnLanded;   // iniş sesi
```

`PlayerHealth.OnDied`, `OnRespawned` de var.

---

## Görevler

### 1. Ses listesi

| Ses | Öncelik | Uzunluk | Not |
|---|---|---|---|
| **Zıplama** | Zorunlu | 0.1–0.3 sn | En çok duyulacak ses — en çok özen |
| Yere iniş | Zorunlu | 0.1–0.2 sn | Kısa, tok |
| Para toplama | Zorunlu | 0.1–0.2 sn | Combo'da perde yükselir |
| Ölüm | Zorunlu | 0.3–0.6 sn | Net ama tiz/rahatsız değil |
| Düşman ezme | Zorunlu | 0.15–0.3 sn | Tatmin edici "pat" |
| Checkpoint | Önerilen | 0.3–0.5 sn | Olumlu, yükselen |
| Bölüm sonu | Önerilen | 1–2 sn | Zafer jingle'ı |
| Tehlike uyarısı | Önerilen | 0.2 sn | Dikkat çekici |
| Menü tık | Önerilen | 0.05–0.1 sn | Çok kısa |
| Adım | İsteğe bağlı | 0.05 sn | Düşük seviyede |

**Zıplama sesine en çok özen göster.** Oyuncu onu binlerce kez duyacak.
Sinir bozucu olursa oyun bırakılır.

- [ ] Liste çıkarıldı

### 2. Ses kaynağı bul

| Kaynak | Ne için | Lisans |
|---|---|---|
| [jsfxr](https://sfxr.me) | Retro efekt üretimi | Ürettiğin senin |
| [freesound.org](https://freesound.org) | Gerçek kayıtlar | Değişken — **CC0 filtrele** |
| [kenney.nl/assets](https://kenney.nl/assets) | Hazır ses paketleri | CC0 |
| [incompetech.com](https://incompetech.com) | Müzik | CC-BY (atıf gerekli) |
| [freepd.com](https://freepd.com) | Müzik | Public domain |

**jsfxr özellikle iyi:** tarayıcıda açarsın, "Jump" butonuna basarsın,
rastgele bir zıplama sesi üretir. Beğenene kadar tıklarsın, WAV indirirsin.
Retro platformcu için mükemmel ve 5 dakikada tüm efektlerini üretirsin.

- [ ] Tüm efektler toplandı
- [ ] Lisanslar `docs/LISANSLAR.md` içine yazıldı

### 3. Audio Mixer kur

`Window → Audio → Audio Mixer` → `+` ile yeni mixer

Gruplar:

```
Master
├── Music
└── SFX
```

Her grup için **Expose Parameter** yap (ayarlardan kontrol edilebilsin):
- Grubu seç → Inspector'da `Volume` → sağ tık → **Expose 'Volume' to script**
- Mixer penceresinde sağ üstteki `Exposed Parameters` → adlandır:
  `MasterVolume`, `MusicVolume`, `SFXVolume`

- [ ] Mixer kuruldu, 3 parametre expose edildi

### 4. AudioManager yaz

Her nesneye `AudioSource` koyma. Tek bir yönetici:

`Assets/Scripts/Core/AudioManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.Audio;

namespace Platformer.Core
{
    /// <summary>
    /// Tum ses calma tek yerden. Sahne degisiminde korunur.
    /// Rastgele perde varyasyonu ile tekduzelik kirilir.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer mixer;

        [Header("Kaynaklar")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Varsayilan Varyasyon")]
        [Tooltip("Her caliste perdeye eklenen rastgelelik. 0.1 = +-%10")]
        [Range(0f, 0.3f)]
        [SerializeField] private float defaultPitchVariation = 0.08f;

        // Ayni ses ust uste calinca kulak tirmalamasin
        private const float MinRepeatInterval = 0.04f;
        private AudioClip lastClip;
        private float lastPlayTime;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ---------- Efektler ----------

        /// <summary>Tek seferlik efekt cal.</summary>
        public void PlaySfx(AudioClip clip, float pitch = 1f, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;

            // Ayni klip cok kisa arayla tekrar calinmasin
            if (clip == lastClip && Time.unscaledTime - lastPlayTime < MinRepeatInterval) return;

            lastClip = clip;
            lastPlayTime = Time.unscaledTime;

            sfxSource.pitch = pitch + Random.Range(-defaultPitchVariation, defaultPitchVariation);
            sfxSource.PlayOneShot(clip, volume);
        }

        /// <summary>Diziden rastgele birini cal - cesitlilik icin.</summary>
        public void PlaySfxRandom(AudioClip[] clips, float pitch = 1f, float volume = 1f)
        {
            if (clips == null || clips.Length == 0) return;
            PlaySfx(clips[Random.Range(0, clips.Length)], pitch, volume);
        }

        // ---------- Muzik ----------

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;   // zaten caliyor

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }

        public void StopMusic() => musicSource?.Stop();

        // ---------- Seviye ----------

        /// <summary>
        /// Slider degeri (0-1) -> desibel.
        /// Ses logaritmik algilanir; dogrudan atarsan yarisi "cok sessiz" olur.
        /// </summary>
        public void SetMasterVolume(float value) => SetMixerVolume("MasterVolume", value);
        public void SetMusicVolume(float value)  => SetMixerVolume("MusicVolume", value);
        public void SetSfxVolume(float value)    => SetMixerVolume("SFXVolume", value);

        private void SetMixerVolume(string parameter, float value)
        {
            if (mixer == null) return;

            // Mathf.Max ZORUNLU: Log10(0) = -Infinity, ses sistemi bozulur
            float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
            mixer.SetFloat(parameter, db);
        }
    }
}
```

**En kritik satır:**

```csharp
float db = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
```

`Mathf.Max` olmadan slider 0'a gelince `Log10(0) = -Infinity` olur ve
ses sistemi tamamen bozulur. Bu klasik bir hatadır ve bulması zordur.

- [ ] `AudioManager` yazıldı
- [ ] Perde varyasyonu çalışıyor
- [ ] Slider 0'a gelince ses bozulmuyor

### 5. Sesleri bağla

`Assets/Scripts/Player/PlayerAudio.cs`:

```csharp
using UnityEngine;
using Platformer.Core;

namespace Platformer.Player
{
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerAudio : MonoBehaviour
    {
        [Header("Klipler")]
        [SerializeField] private AudioClip[] jumpClips;
        [SerializeField] private AudioClip[] landClips;
        [SerializeField] private AudioClip deathClip;

        [Header("Seviye")]
        [Range(0f, 1f)] [SerializeField] private float jumpVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float landVolume = 0.5f;

        private PlayerController2D controller;
        private PlayerHealth health;

        private void Awake()
        {
            controller = GetComponent<PlayerController2D>();
            health = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            controller.OnJumped += PlayJump;
            controller.OnLanded += PlayLand;
            if (health != null) health.OnDied += PlayDeath;
        }

        private void OnDisable()
        {
            controller.OnJumped -= PlayJump;
            controller.OnLanded -= PlayLand;
            if (health != null) health.OnDied -= PlayDeath;
        }

        private void PlayJump() => AudioManager.Instance?.PlaySfxRandom(jumpClips, 1f, jumpVolume);
        private void PlayLand() => AudioManager.Instance?.PlaySfxRandom(landClips, 1f, landVolume);
        private void PlayDeath() => AudioManager.Instance?.PlaySfx(deathClip);
    }
}
```

**Dizi kullanmak:** 2–3 farklı zıplama sesi koyup rastgele seçmek,
tek sesi perde değiştirerek çalmaktan daha iyi sonuç verir.

- [ ] Tüm sesler bağlı ve çalışıyor
- [ ] `OnDisable`'da abonelikler bırakılıyor

### 6. Müzik

Bölüm başına ayrı müzik gerekmez. 2–3 parça yeterli:
- Menü
- Oynanış (1–2 varyasyon)
- Bölüm sonu (kısa jingle, 2–3 sn)

**Döngü kusursuz olmalı.** Kesik duyulan döngü dikkat dağıtır ve amatör
hissettirir.

Audacity (ücretsiz) ile döngü noktası ayarlama:
1. Parçayı aç
2. Sonundaki sessizliği kes
3. Başlangıçtaki sessizliği kes
4. Export → OGG

**Seviye dengesi:** müzik %55–70, efektler %100. Müzik fon olmalı,
efektlerle yarışmamalı.

- [ ] Müzik eklendi, döngü temiz
- [ ] Seviye dengesi ayarlandı

### 7. Import ayarları

| Ses tipi | Load Type | Compression | Quality |
|---|---|---|---|
| Kısa efekt (<1 sn) | **Decompress On Load** | ADPCM veya PCM | — |
| Orta efekt (1–5 sn) | Compressed In Memory | Vorbis | 70% |
| **Müzik** | **Streaming** | Vorbis | 70% |

Yanlış ayarın sonuçları:
- Müzik "Decompress On Load" → RAM'de onlarca MB yer kaplar
- Efekt "Streaming" → çalarken gecikir, tepki geç gelir

Ayrıca: efektler için **Force To Mono** ✔ — dosya boyutu yarıya iner ve
2D oyunda stereo efekt gereksiz.

- [ ] Import ayarları doğru

---

## Kabul kriteri

- [ ] Zıplama, iniş, toplama, ölüm, ezme sesleri var
- [ ] Aynı ses art arda çalınca rahatsız etmiyor
- [ ] Müzik kusursuz döngüde
- [ ] Müzik ve efekt seviyeleri ayrı ayarlanabiliyor
- [ ] Slider 0'a gelince ses bozulmuyor
- [ ] **Sesi kapatınca oyun eksik hissettiriyor**
- [ ] Import ayarları doğru (müzik streaming, efekt decompress)
- [ ] Lisanslar `docs/LISANSLAR.md` içinde

---

## Tuzaklar

**Ses modülü `manifest.json`'da yoksa hiçbir şey çalışmaz.** Bu projede
bir kez yaşandı: `Packages/manifest.json` minimal tutulmuştu ve
`com.unity.modules.audio` listede yoktu. Belirti şuydu:

```
AudioListener component deleted: Component belongs to a disabled built-in package.
```

`AudioSource` eklenemez, ses çalmaz, ama **derleme hatası da vermez** — sadece
sessizce çalışmaz. Kontrol: `Packages/manifest.json` içinde şunlar olmalı:

```
com.unity.modules.audio            ← ses
com.unity.modules.animation        ← Animator (Epic 12)
com.unity.modules.particlesystem   ← parçacık (Epic 14)
com.unity.modules.imageconversion  ← EncodeToPNG (SpriteFactory)
```

`imageconversion` özellikle sinsi: Editor'de çalışır, **build'de patlar**.

**`Log10(0) = -Infinity`.** Slider'ı sıfıra çekince ses sistemi bozulur.
`Mathf.Max(value, 0.0001f)` kullan. **En sık yapılan ses hatası budur.**

**Perde varyasyonu yapmamak.** Tekdüze ses tekrarı en çok rahatsız eden şey.

**Çok uzun ses efekti.** 1 saniyelik zıplama sesi üst üste çalınca çorbaya
döner. 0.1–0.3 sn olmalı.

**Müziği çok yüksek yapmak.** Efektler duyulmaz olur.

**`OnDisable`'da abonelik bırakmamak.** Sahne değişince
`MissingReferenceException`.

**Her nesneye `AudioSource` koymak.** 50 düşman = 50 AudioSource = performans
ve karmaşa. Tek yönetici yeterli.

**Lisans kontrol etmemek.** freesound.org'da CC-BY sesler var; atıf vermezsen
telif talebi gelebilir.

**Müziği "Decompress On Load" yapmak.** RAM patlar.

---

## v1'de yapma

- Dinamik/katmanlı müzik (tehlikede yoğunlaşan)
- 3D konumsal ses
- Reverb bölgeleri, ortam akustiği
- FMOD / Wwise entegrasyonu
- Seslendirme (voice over)
- Prosedürel ses üretimi
- Sesle senkron oynanış (rhythm mekaniği)

---

## Sonraki

[Epic 14 — Juice](14-juice.md)
