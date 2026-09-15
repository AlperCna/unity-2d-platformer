# Lisanslar

> Oyunda kullanılan **her dış kaynak** buraya yazılır. Boş bırakılan bir
> satır, yayınlama gününde bulunması en zor şeydir.
>
> Epic: [11-sanat-ve-sprite.md](11-sanat-ve-sprite.md) görev 8

---

## Görseller

**Dış kaynak yok.** Bütün görseller bu depodaki kodla üretiliyor:

| Ne | Üreten |
|---|---|
| Karakter, düşman, tehlike, toplanabilir | [`SpriteFactory.cs`](../Assets/Editor/SpriteFactory.cs) |
| Karo seti (16 parça + Rule Tile) | [`TilesetFactory.cs`](../Assets/Editor/TilesetFactory.cs) |
| Arka plan katmanları (`bg_*`) | `SpriteFactory.PaintRidge` |

Telif sahibi: **AlperCna**. Atıf gerekmiyor, lisans kısıtı yok.

Bu, tesadüfi bir durum değil — Epic 11'in "tek bir paketten al" kuralının
en uç hâli. Farklı sanatçıların işlerini karıştırmak tutarsızlığın en hızlı
yolu; hiç karıştırmamak en yavaşı.

---

## Sesler

Henüz ses yok. Eklendiğinde **buraya yazılacak** — özellikle
`freesound.org` gibi CC-BY kaynakları kullanılırsa, çünkü CC-BY **atıf
zorunlu** kılıyor ve atıf eksikse lisans ihlali olur.

| Ses | Kaynak | Lisans | Atıf gerekli |
|---|---|---|---|
| *(henüz yok)* | | | |

---

## Yazı tipleri

Şu an Unity'nin yerleşik fontu kullanılıyor (`LegacyRuntime.ttf`).
Dışarıdan font eklenirse buraya yazılacak — ücretsiz fontların çoğu
**oyuna gömmeye** izin verir ama hepsi değil.

---

## Unity paketleri

Aşağıdakiler Unity Editor ile birlikte geliyor,
[Unity Companion License](https://unity.com/legal/licenses/unity-companion-license)
altında. Oyunla dağıtılmaları serbest, ayrı atıf gerekmiyor.

| Paket | Sürüm |
|---|---|
| `com.unity.2d.sprite` | 1.0.0 |
| `com.unity.2d.tilemap` | 1.0.0 |
| `com.unity.2d.tilemap.extras` | 4.1.1 |
| `com.unity.ugui` | 2.0.0 |
| `com.unity.ide.visualstudio` | 2.0.22 |
| `com.unity.test-framework` | 1.4.5 |

Ayrıca `com.unity.modules.*` çekirdek modülleri — bunlar motorun parçası.

> ⚠️ `com.unity.2d.tilemap.extras` **4.1.1'de sabit.** 4.2+ Unity 6000.1
> istiyor, bu proje 6000.0.83f1'de. Sürüm yükseltme Unity yükseltmesi
> demek.

---

## Dışarıdan bir şey eklerken

1. Lisansı **indirmeden önce** oku
2. Buraya satır ekle — sonradan hatırlanmıyor
3. Atıf gerekiyorsa oyun içi künyeye de ekle (Epic 20)
4. "Ücretsiz" ≠ "ticari kullanıma uygun". İkisi ayrı sorular.
