using System.Collections;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Alttan gecilen, ustune basilan platform.
    ///
    /// Yukari ziplarken icinden gecersin, tepesine basarsin. Asagi inmek
    /// icin ASAGI + ZIPLA.
    ///
    /// PlatformEffector2D isin yarisini yapiyor (alttan gecirme). Bu script
    /// diger yarisi: istekle asagi inme.
    ///
    /// NEDEN COLLIDER'I KAPATMIYORUZ:
    /// Basit yol collider.enabled = false olurdu ama o an platformun
    /// UZERINDEKI baska bir sey de duserdi. Bunun yerine sadece OYUNCU ile
    /// carpismayi gecici kapatiyoruz - Physics2D.IgnoreCollision.
    /// </summary>
    [RequireComponent(typeof(PlatformEffector2D))]
    [RequireComponent(typeof(Collider2D))]
    public class OneWayPlatform : MonoBehaviour
    {
        [Tooltip("Asagi inerken carpismanin kapali kalacagi sure. " +
                 "Platform kalinligindan gecmeye yetecek kadar olmali.")]
        [SerializeField] private float dropThroughDuration = 0.35f;

        private Collider2D platformCollider;
        private PlatformEffector2D effector;

        private void Awake()
        {
            platformCollider = GetComponent<Collider2D>();
            effector = GetComponent<PlatformEffector2D>();

            // Effector'un dogru calismasi icin sart olan ayarlar. Elle
            // yapilmasina birakilsaydi bir sahnede unutulur ve platform
            // "bazen" calisirdi.
            platformCollider.usedByEffector = true;
            platformCollider.isTrigger = false;

            effector.useOneWay = true;
            effector.surfaceArc = 170f;      // ustten gelis kabul araligi
            effector.rotationalOffset = 0f;
        }

        /// <summary>
        /// Oyuncunun bu platformdan asagi inmesini saglar.
        ///
        /// KARARI BU SINIF VERMIYOR - PlayerController2D veriyor ve burayi
        /// cagiriyor. Iki kez yanlis yaptim, ikisi de ayni koktendi:
        ///
        /// 1. Girdiyi OnCollisionStay2D'de okumak. O fizik adiminda calisir,
        ///    bir karede hic veya birden fazla kez; GetButtonDown kayboluyordu.
        ///
        /// 2. Girdiyi Update/LateUpdate'te okuyup "ustumde kim var" diye
        ///    carpisma olaylariyla takip etmek. PlatformEffector2D carpismayi
        ///    surekli acip kapattigi icin o liste guvenilir degil.
        ///
        /// Dogrusu: karakter neyin ustunde durdugunu ZATEN biliyor (zemin
        /// kontrolu yapiyor). Karar orada verilmeli, burasi sadece
        /// "tamam, gec" demeli.
        /// </summary>
        public void DropThrough(Collider2D playerCollider)
        {
            if (playerCollider == null) return;
            StartCoroutine(DropThroughRoutine(playerCollider));
        }

        private IEnumerator DropThroughRoutine(Collider2D playerCollider)
        {
            Physics2D.IgnoreCollision(platformCollider, playerCollider, true);
            yield return new WaitForSeconds(dropThroughDuration);

            // Nesne yok edilmis olabilir (sahne degisimi) - null kontrolu sart
            if (platformCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(platformCollider, playerCollider, false);
            }
        }
    }
}
