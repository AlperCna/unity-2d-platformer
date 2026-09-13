using UnityEngine;

namespace Platformer.CameraRig
{
    /// <summary>
    /// Karakteri yumusak takip eden kamera.
    /// - Olu bolge (dead zone): kucuk hareketlerde kamera titremez
    /// - Ileri bakis (look-ahead): kostugun yone dogru kaydirir, onunu gormeni saglar
    /// - Sinirlar (bounds): kamerayi bolumun disina cikarmaz
    /// LateUpdate'te calisir ki karakter o kare hareketini bitirmis olsun.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Hedef")]
        [Tooltip("Bos birakilirsa 'Player' tag'li nesne otomatik bulunur.")]
        [SerializeField] private Transform target;

        [SerializeField] private Vector2 offset = new Vector2(0f, 1.2f);

        [Header("Yumusatma")]
        [Tooltip("Takip gecikmesi. Kucuk = siki takip, buyuk = tembel kamera.")]
        [SerializeField] private float smoothTime = 0.18f;

        [Tooltip("Bu dikdortgenin icinde hedef hareket ederse kamera kimildamaz.")]
        [SerializeField] private Vector2 deadZone = new Vector2(1.4f, 1.0f);

        [Header("Ileri Bakis")]
        [Tooltip("Hedefin hizina gore kamerayi one kaydirma miktari.")]
        [SerializeField] private float lookAheadDistance = 2.2f;

        [SerializeField] private float lookAheadSmoothTime = 0.5f;

        [Header("Bolum Sinirlari")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-5f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(120f, 30f);

        private Vector3 velocity;
        private float lookAhead;
        private float lookAheadVelocity;
        private UnityEngine.Camera cam;
        private Rigidbody2D targetBody;

        private void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
        }

        private void Start()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }

            if (target != null)
            {
                targetBody = target.GetComponent<Rigidbody2D>();
                // Ilk karede kaymamasi icin dogru yere yerlestir
                Vector3 start = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
                transform.position = ClampToBounds(start);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                transform.position.z);

            // --- Olu bolge: hedef kutunun icindeyse o eksende kamerayi sabit tut ---
            float dx = desired.x - transform.position.x;
            float dy = desired.y - transform.position.y;

            if (Mathf.Abs(dx) < deadZone.x * 0.5f) desired.x = transform.position.x;
            if (Mathf.Abs(dy) < deadZone.y * 0.5f) desired.y = transform.position.y;

            // --- Ileri bakis: hedefin yatay hizina gore kaydir ---
            if (targetBody != null)
            {
                float speedX = Platformer.Core.Rigidbody2DExtensions.GetVelocity(targetBody).x;
                float wanted = Mathf.Clamp(speedX / 8f, -1f, 1f) * lookAheadDistance;
                lookAhead = Mathf.SmoothDamp(lookAhead, wanted, ref lookAheadVelocity, lookAheadSmoothTime);
                desired.x += lookAhead;
            }

            Vector3 smoothed = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.position = ClampToBounds(smoothed);
        }

        /// <summary>Kamerayi bolum sinirlarinin icinde tutar (kenarlarda bosluk gorunmez).</summary>
        private Vector3 ClampToBounds(Vector3 position)
        {
            if (!useBounds || cam == null || !cam.orthographic) return position;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            // Sinir alani kameradan darsa ortala, yoksa kenetle
            float minX = minBounds.x + halfWidth;
            float maxX = maxBounds.x - halfWidth;
            float minY = minBounds.y + halfHeight;
            float maxY = maxBounds.y - halfHeight;

            position.x = minX > maxX ? (minBounds.x + maxBounds.x) * 0.5f : Mathf.Clamp(position.x, minX, maxX);
            position.y = minY > maxY ? (minBounds.y + maxBounds.y) * 0.5f : Mathf.Clamp(position.y, minY, maxY);

            return position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(deadZone.x, deadZone.y, 0.1f));

            if (useBounds)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0.1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
