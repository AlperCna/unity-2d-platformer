using UnityEngine;
using Platformer.Core;
using Platformer.Player;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Mermi atan dusmanin mermisi. Duz gider, zemine carpinca veya suresi
    /// dolunca havuza doner.
    ///
    /// NEDEN Hazard BILESENI KULLANMIYOR:
    /// Hazard "dokun = ol" isini zaten yapiyor ve normalde onu kullanmak
    /// dogru olurdu. Ama mermi carpisma aninda kendini de KAPATMAK zorunda.
    /// Iki ayri bilesenin OnTriggerEnter2D'si ayni carpismada calisacak
    /// olsaydi, biri nesneyi kapatinca digerinin calisip calismayacagi
    /// belirsiz kalirdi. Oyuncunun bazen mermiden olmemesi gibi bir hata
    /// cikardi ve tekrar uretmesi neredeyse imkansiz olurdu.
    ///
    /// Tek bilesende tek karar: once oldur, sonra kapan.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Birim/saniye. Oyuncunun kosma hizi 8; bundan yavas olmali " +
                 "ki kacilabilsin, ama cok yavas da olmamali.")]
        [SerializeField] private float speed = 6f;

        [Tooltip("Bu sure sonunda hicbir seye carpmasa da kaybolur.")]
        [SerializeField] private float lifetime = 4f;

        [Tooltip("Mermiyi durduran layer'lar (zemin).")]
        [SerializeField] private LayerMask blockerLayers = ~0;

        /// <summary>Havuza geri vermek icin ates eden dusman baglaniyor.</summary>
        public System.Action<Projectile> Expired;

        private Rigidbody2D rb;
        private float timeLeft;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;   // yercekimi yok
            rb.freezeRotation = true;

            GetComponent<Collider2D>().isTrigger = true;
        }

        /// <summary>Havuzdan alindiktan sonra cagrilir.</summary>
        public void Launch(Vector2 direction)
        {
            timeLeft = lifetime;
            rb.SetVelocity(direction.normalized * speed);
        }

        private void Update()
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f) Expire();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // gameObject'i veriyoruz ki olum aninda mermi parlasin -
                // oyuncu NEDEN oldugunu gorsun
                var health = other.GetComponent<PlayerHealth>();
                if (health != null && !health.IsDead) health.Kill(gameObject);

                Expire();
                return;
            }

            // Zemine carpti mi? Layer maskesi bit kaydirmayla kontrol edilir.
            if ((blockerLayers.value & (1 << other.gameObject.layer)) != 0)
            {
                Expire();
            }
        }

        private void Expire()
        {
            rb.SetVelocity(Vector2.zero);

            // Havuz baglanmissa ona haber ver; baglanmamissa (elle sahneye
            // konmus bir mermi) kendini kapat.
            if (Expired != null) Expired(this);
            else gameObject.SetActive(false);
        }
    }
}
