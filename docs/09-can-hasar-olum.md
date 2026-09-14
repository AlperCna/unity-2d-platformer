# Epic 09 — Can, Hasar, Ölüm

> ✅ **TAMAMLANDI — 14 Eylül 2026.**
> Ölüm 0,9 → **0,45 sn**. Hit stop (`TimeController`), kamera sarsıntısı,
> öldüren nesnenin vurgulanması eklendi. Can sistemi kaldırıldı —
> sınırsız deneme, can yerine ölüm sayacı.
>
> Kabul kriterlerinden ikisi bu epic'e ait değildi ve öyle işaretlendi:
> ses (Epic 13) ve `IResettable` ile bölüm sıfırlama (Epic 10).

**Amaç:** Ölümü cezalandırıcı değil, **öğretici** hale getirmek.

**Ön koşul:** [Epic 02](02-karakter-hissiyati.md)

**Süre:** 3–4 gün

---

## Neden bu epic var

Oyuncu oyununu bırakacaksa, büyük ihtimalle ölüm yüzünden bırakır. Ama
*ölmek* yüzünden değil — **ölümden sonra beklemek** yüzünden.

Modern platform oyunlarının çözdüğü problem budur: *Celeste* seni oyun boyunca
ortalama 1500 kez öldürür. İnsanlar yine de saatlerce oynar, çünkü ölüm
1 saniye sürer ve hiçbir şey kaybetmezsin.

> **Ölümün maliyeti zaman değil, bilgi olmalı.**

Oyuncu ne yapması gerektiğini öğrenerek ölsün, ve öğrendiğini hemen denesin.

---

## Elindeki altyapı

`Assets/Scripts/Player/PlayerHealth.cs`:
- `Kill()` — dondurur, collider kapatır, rengi değiştirir
- Küçük yukarı sıçrama efekti
- Gecikmeli respawn, checkpoint'e dönüş
- Yeniden doğunca kısa dokunulmazlık + yanıp sönme

`GameManager` can sayısını tutuyor (`startingLives = 3`).

---

## Görevler

### 1. Can sistemine karar ver

| Sistem | Nasıl | Kime uygun |
|---|---|---|
| **Tek vuruş + sınırsız deneme** | Her hasar öldürür, ama sonsuz denersin | Hızlı, hassas oyunlar ← **önerilen** |
| Kalp sistemi | 3 can, her vuruş 1 azaltır | Daha affedici, keşif odaklı |
| Klasik can | 3 can, bitince Game Over | **Kullanma** — modası geçti ve sinir bozucu |

**Önerim: tek vuruş + sınırsız deneme.** Sebepler:

- Kod en basit (zaten böyle çalışıyor)
- Tasarımı netleştirir — "ya geçtin ya geçemedin"
- Oyuncu asla "3 can bitti, en baştan" cezası yaşamaz
- Checkpoint sistemi tek başına yeterli ceza

`GameManager`'daki Game Over'ı **kaldırmanı** öneririm. Canı sadece
istatistik olarak göster ("bu bölümde 7 kez öldün").

```csharp
// GameManager.ReportPlayerDeath() icinde:
public void ReportPlayerDeath()
{
    DeathCount++;
    OnDeathCountChanged?.Invoke(DeathCount);
    // Game Over yok - sinirsiz deneme
}
```

- [ ] Can sistemine karar verildi

### 2. Ölüm süresini kısalt

Şu an `respawnDelay = 0.9` saniye. **Bu çok uzun.**

Hedef: **0.4–0.6 saniye.**

Hesap: bir bölümde 30 kez öleceksen, her ölümde fazladan 0.4 saniye =
12 saniye boş bekleme. Ve o bekleme, en sinirli olduğun anda gelir.

Süre bütçesi:

| Aşama | Süre | Ne olur |
|---|---|---|
| Ölüm efekti | 0.15 sn | Parçacık, sarsıntı, hit stop |
| Kararma | 0.12 sn | Ekran hızla kararır |
| Konum değişimi | 0 sn | Anında (kamera da anında) |
| Açılma | 0.15 sn | Ekran açılır, kontrol aktif |
| **Toplam** | **0.42 sn** | |

- [ ] `respawnDelay` 0.45'e indirildi
- [ ] Ölüm hâlâ okunabilir (çok hızlı da olmamalı)

### 3. Hit stop (zaman donması)

Çarpışma anında oyunu birkaç kare dondurmak, darbeyi "hissettirir".
Az iş, çok etki.

**Uygulandı:** [`Assets/Scripts/Core/TimeController.cs`](../Assets/Scripts/Core/TimeController.cs)

```csharp
TimeController.Instance?.HitStop(0.08f);
```

Üç tuzağı var, üçü de kapatıldı:

**1. `timeScale = 0` iken `WaitForSeconds` sonsuza kadar bekler.**
`WaitForSecondsRealtime` kullanılmalı — yoksa oyun kalıcı donar.

**2. Coroutine yarıda kesilirse oyun donmuş kalır.** Sahne değişimi veya
nesne yok olması bunu yapar. `OnDisable` güvenlik ağı `timeScale = 1`
yazıyor; o satırı silme.

**3. Efekti başlatan nesne yok olabilir.** Ezilen düşman kendi hit stop'unu
başlatıp sonra `Destroy` olursa coroutine ölür ve oyun donuk kalır.
Bu yüzden hit stop **kalıcı bir yöneticide** çalışıyor, çağıran nesnede değil.

**Kullanım süreleri:**

| Olay | Hit stop |
|---|---|
| Düşman ezme | 0.05 sn |
| Ölüm | 0.08 sn |
| Sır bulma | 0.06 sn |
| Normal para | **yok** (25 kez donmak sinir bozucu) |

- [ ] `TimeController` yazıldı ve test edildi
- [ ] Oyun hiçbir durumda donmuş kalmıyor

### 4. Ölüm geri bildirimini güçlendir

**Uygulandı:** [`Assets/Scripts/Player/PlayerHealth.cs`](../Assets/Scripts/Player/PlayerHealth.cs)

Ölüm dizisi (toplam ~0,45 sn):

| Zaman | Ne olur |
|---|---|
| 0,00 | Hit stop (0,08 sn) + kamera sarsıntısı + öldüren nesne parlar |
| 0,08 | Karakter yukarı sıçrar, rengi değişir |
| 0,45 | Checkpoint'te doğar, **anında** kontrol edilebilir |
| +0,50 | Kısa dokunulmazlık (yanıp sönerek) |

**Tüm zamanlamalar unscaled.** `WaitForSeconds` kullansaydık hit stop
süresi respawn'a eklenir (`0,08 + 0,45 = 0,53`) ve hit stop'u her
uzattığımızda ölüm yavaşlardı. Şimdi 0,45 gerçekten 0,45.

- [ ] Ölüm çok katmanlı geri bildirim veriyor

### 5. Ölüm sebebini göster

Oyuncu **neden** öldüğünü anlamalı. Anlaşılmayan ölüm, haksız ölümdür.

Uygulama `PlayerHealth.HighlightKiller()` içinde. İki detay:

- `Time.unscaledDeltaTime` kullanıyor — hit stop sırasında da yanıp sönsün
- Her karede `sr == null` kontrolü var — öldüren nesne bu sırada yok olabilir
  (ezilen düşman gibi)

`Hazard` ve `EnemyBase` içinden `health.Kill(gameObject)` diye çağır.

Ayrıca: **kamera ölüm noktasında kalsın**, hemen checkpoint'e atlamasın.
Respawn anında konum değişsin.

- [ ] Ölüm sebebi görsel olarak belli

### 6. Respawn'ı yumuşat

```csharp
private void Respawn()
{
    Vector3 target = GameManager.Instance != null
        ? GameManager.Instance.CheckpointPosition
        : transform.position;

    transform.position = target;
    if (rb != null) rb.SetVelocity(Vector2.zero);

    if (bodyCollider != null) bodyCollider.enabled = true;
    if (spriteRenderer != null) spriteRenderer.color = originalColor;

    IsDead = false;
    controller.Unfreeze();          // aninda kontrol - bekleme yok

    // Bolumu sifirla (Epic 10)
    GameManager.Instance?.ResetLevelState();

    OnRespawned?.Invoke();
    StartCoroutine(InvulnerabilityWindow());
}
```

Kontrol listesi:
- [ ] Karakter **anında** kontrol edilebilir (bekleme yok)
- [ ] Kısa dokunulmazlık var (0.5–0.8 sn — 2 sn **çok uzun**)
- [ ] Kamera checkpoint'e anında geçiyor (yumuşak değil)
- [ ] Düşmanlar ve hareketli platformlar sıfırlanıyor

Son madde önemli: hareketli platform yanlış yerdeyken doğarsan geçemezsin
ve sonsuz ölüm döngüsüne girersin.

### 7. Ölüm istatistiği

Bölüm sonunda kaç kez öldüğünü göster. İki faydası var:

- **Oyuncu için:** kendi gelişimini görür
- **Senin için:** tasarım verisi

Ölüm konumlarını da kaydet (Epic 17'de ısı haritası için):

```csharp
// GameManager icinde
public List<Vector2> DeathPositions { get; } = new List<Vector2>();

public void ReportPlayerDeath(Vector2 position)
{
    DeathCount++;
    DeathPositions.Add(position);
    OnDeathCountChanged?.Invoke(DeathCount);
}
```

- [ ] Ölüm sayacı ve konumları kaydediliyor

---

## Kabul kriteri

- [x] Ölümden yeniden oynamaya geçiş **0.6 saniyeden kısa**
- [x] Ölüm görsel ve zamansal olarak net *(işitsel kısım → Epic 13)*
- [x] Neden öldüğün anlaşılıyor — öldüren nesne parlıyor
- [x] Respawn sonrası anında kontrol var
- [x] Hareketli parçalar respawn'da sıfırlanıyor *(→ Epic 10, `IResettable`)*
- [x] 20 kez üst üste ölmek sinir bozucu değil
- [x] `Time.timeScale` hiçbir durumda 0'da takılı kalmıyor
- [x] Game Over ekranı yok

---

## Tuzaklar

**Uzun ölüm animasyonu.** 2 saniyelik güzel bir ölüm animasyonu, 50. ölümde
nefret edilen bir şeye dönüşür.

**Game Over ekranı.** Oyuncuyu menüye atmak, oyunu bıraktırmanın en hızlı yolu.

**`WaitForSeconds` ile hit stop.** `timeScale = 0` iken sonsuza kadar bekler,
oyun kilitlenir. **`WaitForSecondsRealtime` kullan.**

**`timeScale`'i geri yüklememek.** Coroutine yarıda kesilirse (sahne değişimi,
nesne yok olması) oyun donmuş kalır. `TimeController` bunu `OnDisable` ile
koruyor — o güvenlik ağını silme.

**Çok uzun dokunulmazlık.** 2 saniye dokunulmazlık, oyuncunun tehlikeden
"geçip gitmesine" izin verir ve tasarımı bozar.

**Ölüm sebebini gizlemek.** Ekran anında kararırsa oyuncu ne olduğunu anlamaz.

**Ölünce toplanan paraları geri almak.** Oyuncu aynı parayı 20 kez toplar.

---

## v1'de yapma

- Zırh/kalkan sistemi
- İyileşme eşyaları
- Açlık, oksijen, enerji gibi ek barlar
- Kalıcı ölüm (permadeath)
- Ölüm cezası olarak para/ilerleme kaybı
- Diriltme / ekstra can satın alma
- Ölüm animasyonu çeşitleri (tehlikeye göre farklı ölüm)

---

## Sonraki

[Epic 10 — Checkpoint ve kayıt](10-checkpoint-ve-kayit.md)
