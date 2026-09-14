using System.Collections;
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
    /// digger yarisi: istekle asagi inme.
    ///
    /// NEDEN COLLIDER'I KAPATMIYORUZ:
    /// Basit yol collider.enabled = false olurdu ama o an platformun
    /// UZERINDEKI baska bir sey de duserdi. Bunun yerine sadece OYUNCU ile
    /// carpismayi gec ici kapatiyoruz - Physics2D.IgnoreCollision.
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

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Player")) return;

            var controller = collision.collider.GetComponent<PlayerController2D>();
            if (controller == null || !controller.IsGrounded) return;

            bool wantsDown = Input.GetAxisRaw("Vertical") <= downInputThreshold;
            bool wantsJump = Input.GetButtonDown("Jump");

            if (wantsDown && wantsJump)
            {
                StartCoroutine(DropThrough(collision.collider));
            }
        }

        private IEnumerator DropThrough(Collider2D playerCollider)
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
