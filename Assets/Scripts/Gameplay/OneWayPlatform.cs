using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Player;

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

        [Tooltip("Asagi inme tusu basili sayilma esigi.")]
        [SerializeField] private float downInputThreshold = -0.5f;

        private Collider2D platformCollider;
        private PlatformEffector2D effector;

        /// <summary>Su an ustunde duran oyuncular.</summary>
        private readonly List<Collider2D> riders = new List<Collider2D>();

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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player") && !riders.Contains(collision.collider))
            {
                riders.Add(collision.collider);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            riders.Remove(collision.collider);
        }

        /// <summary>
        /// Girdi LATEUPDATE'te okunuyor. Iki ayri sebep var:
        ///
        /// 1) Carpisma geri cagrisinda OLMAZ. Input.GetButtonDown sadece
        ///    kare basina bir kez dogru; OnCollisionStay2D ise fizik
        ///    adiminda calisiyor ve bir karede hic veya birden fazla kez
        ///    calisabiliyor. Istek bazen kayboluyordu.
        ///
        /// 2) Update de YETMEDI. Iki script ayni kareyi paylasiyor:
        ///    PlayerController2D ziplamayi tamponluyor, bu script iptal
        ///    ediyor. Unity'de iki script arasindaki Update sirasi TANIMSIZ,
        ///    yani bu once calisirsa henuz var olmayan tamponu iptal ediyor
        ///    ve oyuncu yine ziplayarak platforma geri duşuyordu.
        ///
        ///    LateUpdate butun Update'lerden SONRA calisir - tampon kesin
        ///    dolmus olur. Fizik (FixedUpdate) ise bir sonraki karede
        ///    calisacagi icin iptal zamaninda yetisiyor.
        /// </summary>
        private void LateUpdate()
        {
            if (riders.Count == 0) return;

            bool wantsDown = Input.GetAxisRaw("Vertical") <= downInputThreshold;
            if (!wantsDown || !Input.GetButtonDown("Jump")) return;

            for (int i = riders.Count - 1; i >= 0; i--)
            {
                Collider2D rider = riders[i];
                if (rider == null) { riders.RemoveAt(i); continue; }

                var controller = rider.GetComponent<PlayerController2D>();
                if (controller == null || !controller.IsGrounded) continue;

                // Tamponlanmis ziplamayi iptal et. Yoksa oyuncu hem
                // platformdan gecer HEM yukari ziplar - yukari cikip
                // tekrar ustune duser, yani hicbir sey olmamis gibi
                // gorunur. Niyeti inmekti.
                controller.CancelBufferedJump();

                StartCoroutine(DropThrough(rider));
            }
        }

        private IEnumerator DropThrough(Collider2D playerCollider)
        {
            Physics2D.IgnoreCollision(platformCollider, playerCollider, true);
            riders.Remove(playerCollider);

            yield return new WaitForSeconds(dropThroughDuration);

            // Nesne yok edilmis olabilir (sahne degisimi) - null kontrolu sart
            if (platformCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(platformCollider, playerCollider, false);
            }
        }
    }
}
