using UnityEngine;
using Platformer.Player;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Zipla pedi — sordugu soru: **"Nereye gidecegini gorebiliyor musun?"**
    ///
    /// Tehlike degil, ARAC. Epic 07'nin diger parcalari oyuncuyu
    /// yavaslatiyor; bu hizlandiriyor. Bolum tasariminda ikisi birlikte
    /// kullanildiginda ritim cikiyor.
    ///
    /// Ziplama yuksekligi BIRIM cinsinden veriliyor, hiz cinsinden degil.
    /// "6 birim yukari firlatir" diye dusunebilirsin; gereken hizi
    /// PlayerController2D kendi yercekiminden hesapliyor. Yercekimi ayari
    /// degisirse pedler kendiliginden dogru kaliyor.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class JumpPad : MonoBehaviour
    {
        [Tooltip("Firlatma yuksekligi (birim). Normal zipla 3,03 birim; " +
                 "bundan belirgin yuksek olmali yoksa pedin anlami kalmaz.")]
        [SerializeField] private float launchHeight = 6f;

        [Tooltip("Ayni oyuncuyu tekrar firlatmadan once beklenecek sure. " +
                 "Ped uzerinde zipladiginda iki kez tetiklenmesin diye.")]
        [SerializeField] private float cooldown = 0.15f;

        [Header("Geri Bildirim")]
        [SerializeField] private Transform squashVisual;
        [SerializeField] private float squashAmount = 0.55f;
        [SerializeField] private float squashRecoverSpeed = 6f;

        private float cooldownLeft;
        private Vector3 baseScale = Vector3.one;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;

            if (squashVisual == null && transform.childCount > 0)
            {
                squashVisual = transform.GetChild(0);
            }

            if (squashVisual != null) baseScale = squashVisual.localScale;
        }

        private void Update()
        {
            if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;

            if (squashVisual != null)
            {
                squashVisual.localScale = Vector3.Lerp(
                    squashVisual.localScale, baseScale,
                    squashRecoverSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryLaunch(other);
        private void OnTriggerStay2D(Collider2D other) => TryLaunch(other);

        private void TryLaunch(Collider2D other)
        {
            if (cooldownLeft > 0f || !other.CompareTag("Player")) return;

            var controller = other.GetComponent<PlayerController2D>();
            if (controller == null) return;

            // Sadece ASAGI inen oyuncuyu firlat. Yukari cikarken tetiklenseydi
            // ped, oyuncunun kendi ziplamasini keser ve kontrol elinden
            // alinmis gibi hissettirirdi.
            if (controller.Velocity.y > 0.5f) return;

            controller.LaunchUpward(launchHeight);
            cooldownLeft = cooldown;

            if (squashVisual != null)
            {
                squashVisual.localScale = new Vector3(
                    baseScale.x * (2f - squashAmount),
                    baseScale.y * squashAmount,
                    baseScale.z);
            }
        }
    }
}
