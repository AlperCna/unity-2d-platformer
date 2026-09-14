using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Mermi atan dusman — sordugu soru: **"Nereden gececeksin?"**
    ///
    /// Yerinden kimildamaz ama onundeki koridoru tehlikeli yapar. Devriye
    /// "ne zaman" sorusunu soruyordu; bu "nereden" soruyor. Ikisi birlikte
    /// oldugunda oyuncunun iki farkli sey dusunmesi gerekiyor - epic'in
    /// "her dusman farkli bir soru sorsun" kurali bu.
    ///
    /// ATES ONCESI UYARI EN ONEMLI KISMI.
    /// Uyarisiz atan bir dusman "haksiz" hissettirir: oyuncu olur, neden
    /// oldugunu anlamaz, oyunu suclar. Uyari varsa olum oyuncunun hatasi
    /// olur ve tekrar denemek ister. Aradaki fark, oyunu birakmakla devam
    /// etmek arasindaki fark.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShooterEnemy : EnemyBase
    {
        [Header("Ates")]
        [Tooltip("Iki ates arasindaki sure (uyari suresi dahil).")]
        [SerializeField] private float fireInterval = 2.2f;

        [Tooltip("Ates ONCESI uyari suresi. Epic 06: en az 0,3 sn. " +
                 "Bunun altina INMEYIN - dusman haksiz hissettirmeye baslar.")]
        [SerializeField] private float telegraphDuration = 0.45f;

        [Tooltip("Sola mi atsin?")]
        [SerializeField] private bool fireLeft = true;

        [Tooltip("Merminin cikis noktasi, merkeze gore.")]
        [SerializeField] private float muzzleOffset = 0.55f;

        [Header("Menzil")]
        [Tooltip("Oyuncu bu mesafeden uzaksa ates etmez. Ekran disinda " +
                 "bosuna mermi ucmasin diye.")]
        [SerializeField] private float activationRange = 16f;

        [Header("Mermi")]
        [SerializeField] private Projectile projectilePrefab;

        [Tooltip("Havuz boyutu. Ayni anda havada olabilecek mermi sayisindan " +
                 "buyuk olmali: lifetime / fireInterval + pay.")]
        [SerializeField] private int poolSize = 4;

        [Header("Uyari Gorseli")]
        [SerializeField] private Color telegraphColor = new Color(1f, 0.42f, 0.42f);

        [Tooltip("Uyari sirasinda ne kadar buyusun.")]
        [SerializeField] private float telegraphScale = 1.18f;

        private ObjectPool<Projectile> pool;
        private Transform poolParent;
        private Transform player;

        private float timer;
        private bool telegraphing;
        private Color normalColor = Color.white;
        private Vector3 normalScale;

        protected override void OnAwake()
        {
            normalScale = transform.localScale;
            if (spriteRenderer != null)
            {
                normalColor = spriteRenderer.color;
                spriteRenderer.flipX = !fireLeft;
            }

            // Mermiler dusmanin ALTINDA degil, sahne kokunde toplaniyor:
            // dusman oldugunde SetActive(false) oluyor ve cocuklari da
            // kapaniyor - havadaki mermiler aninda kaybolurdu.
            var holder = new GameObject($"{name}_Mermiler");
            poolParent = holder.transform;

            if (projectilePrefab != null)
            {
                pool = new ObjectPool<Projectile>(projectilePrefab, poolSize, poolParent);
            }

            timer = fireInterval;
        }

        private void Start()
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        protected override void OnReset()
        {
            // Havada asili kalan mermileri topla. Yoksa checkpoint'ten
            // doган oyuncu, olmeden once atilmis bir mermiye carpardi.
            pool?.ReturnAll();

            telegraphing = false;
            timer = fireInterval;
            ApplyNormalLook();
        }

        private void Update()
        {
            if (IsDead || pool == null) return;
            if (!PlayerInRange()) { ResetTelegraph(); return; }

            timer -= Time.deltaTime;

            // Uyari penceresine girdik mi?
            if (!telegraphing && timer <= telegraphDuration)
            {
                telegraphing = true;
            }

            if (telegraphing) ApplyTelegraphLook();

            if (timer <= 0f)
            {
                Fire();
                timer = fireInterval;
                telegraphing = false;
                ApplyNormalLook();
            }
        }

        private bool PlayerInRange()
        {
            if (player == null) return false;
            return Mathf.Abs(player.position.x - transform.position.x) <= activationRange;
        }

        private void ResetTelegraph()
        {
            if (!telegraphing) return;

            telegraphing = false;
            timer = fireInterval;
            ApplyNormalLook();
        }

        private void Fire()
        {
            Vector2 direction = fireLeft ? Vector2.left : Vector2.right;
            Vector2 muzzle = (Vector2)transform.position + direction * muzzleOffset;

            Projectile shot = pool.Get(muzzle, Quaternion.identity);
            if (shot == null) return;          // havuz dolu, bu atesi atla

            shot.Expired = ReturnToPool;
            shot.Launch(direction);
        }

        private void ReturnToPool(Projectile p) => pool.Return(p);

        // ---------------------------------------------------------------
        // Uyari gorseli: hem RENK hem BOYUT degisiyor
        //
        // Ikisi birden olmasi bilincli. Sadece renk kullansaydik renk korligi
        // olan oyuncular uyariyi kaciririrdi (Epic 11'in erisilebilirlik
        // kurali: renk TEK BASINA bilgi tasimamali).
        // ---------------------------------------------------------------

        private void ApplyTelegraphLook()
        {
            float k = telegraphDuration <= 0f
                ? 1f
                : 1f - Mathf.Clamp01(timer / telegraphDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(normalColor, telegraphColor, k);
            }

            transform.localScale = Vector3.Lerp(normalScale, normalScale * telegraphScale, k);
        }

        private void ApplyNormalLook()
        {
            if (spriteRenderer != null) spriteRenderer.color = normalColor;
            transform.localScale = normalScale;
        }

        protected override void OnDeath()
        {
            // Olurken havadaki mermiler ucmaya devam etsin - oyuncu dusmani
            // ezdikten sonra hala kacmasi gereken bir sey olmali.
            ApplyNormalLook();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.42f, 0.42f, 0.6f);
            Vector3 dir = (fireLeft ? Vector3.left : Vector3.right);
            Gizmos.DrawLine(transform.position, transform.position + dir * activationRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + dir * muzzleOffset, 0.15f);
        }
    }
}
