using UnityEngine;
using Platformer.Player;

namespace Platformer.CameraRig
{
    /// <summary>
    /// 2D platform kamerasi.
    ///
    /// Yatay ve dikey eksen AYRI mantiklarla calisir - platform oyunlarinda
    /// bu ikisi ayni sey degildir:
    ///
    /// YATAY  Olu bolge + ileri bakis. Kostugun yonu gosterir.
    ///
    /// DIKEY  Karakterin anlik yuksekligini DEGIL, en son yere degdigi
    ///        yuksekligi takip eder. Boylece normal ziplamada ekran hic
    ///        oynamaz. Istisnalar: hizli duserken (inis noktasini gormen
    ///        icin) ve karakter ekrandan cikmaya yaklasinca.
    ///
    /// LateUpdate'te calisir ki karakter o kare hareketini bitirmis olsun.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Hedef")]
        [Tooltip("Bos birakilirsa 'Player' tag'li nesne otomatik bulunur.")]
        [SerializeField] private Transform target;

        [Tooltip("Kameranin hedefe gore kaymasi. Y pozitif = karakter ekranin altinda durur.")]
        [SerializeField] private Vector2 offset = new Vector2(0f, 1.2f);

        [Header("Yatay Takip")]
        [Tooltip("Takip gecikmesi. Kucuk = siki takip, buyuk = tembel kamera.")]
        [SerializeField] private float horizontalSmoothTime = 0.18f;

        [Tooltip("Bu genislik icinde hedef hareket ederse kamera kimildamaz.")]
        [SerializeField] private float horizontalDeadZone = 1.6f;

        [Header("Ileri Bakis")]
        [Tooltip("Kosarken kameranin one kayma miktari. Ziplama mesafenin ~%40'i iyi bir deger.")]
        [SerializeField] private float lookAheadDistance = 2.2f;

        [Tooltip("Cok dusuk olursa yon degistirirken kamera savrulur.")]
        [SerializeField] private float lookAheadSmoothTime = 0.5f;

        [Header("Dikey Takip")]
        [Tooltip("Acik: kamera en son yere degilen yuksekligi takip eder, ziplamada oynamaz. " +
                 "Kapali: karakteri surekli takip eder (ziplamada ekran zipar).")]
        [SerializeField] private bool followGroundedHeight = true;

        [Tooltip("Normal dikey takip gecikmesi (yeni bir yukseklige inince).")]
        [SerializeField] private float verticalSmoothTime = 0.28f;

        [Tooltip("Dusus hizi bunun altina inerse kamera karakteri takip etmeye baslar " +
                 "- nereye dustugunu gormen icin.")]
        [SerializeField] private float fastFallThreshold = -9f;

        [Tooltip("Hizli duserken daha siki takip.")]
        [SerializeField] private float fastFallSmoothTime = 0.12f;

        [Tooltip("Karakter kayitli yukseklikten bu kadar uzaklasirsa kamera " +
                 "kurali bozup onu takip eder - ekrandan cikmasin.")]
        [SerializeField] private float maxVerticalDrift = 4.5f;

        [Header("Bolum Sinirlari")]
        [SerializeField] private bool useBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-5f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(120f, 30f);

        [Header("Sarsinti")]
        [Tooltip("Sarsintinin sonme hizi. Buyuk = daha cabuk biter.")]
        [SerializeField] private float shakeDecay = 2.5f;

        /// <summary>Erisilebilirlik: ayarlardan kapatilabilir (Epic 15).</summary>
        public static bool ScreenShakeEnabled = true;

        // --- Bilesenler ---
        private UnityEngine.Camera cam;
        private Rigidbody2D targetBody;
        private PlayerController2D targetController;

        // --- Yatay durum ---
        private float horizontalVelocity;
        private float lookAhead;
        private float lookAheadVelocity;

        // --- Dikey durum ---
        private float verticalVelocity;
        private float anchoredY;          // takip edilen yukseklik
        private bool anchorInitialised;

        // --- Sarsinti ---
        private float shakeTimeLeft;
        private float shakeDuration;
        private float shakeMagnitude;

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

            if (target == null) return;

            targetBody = target.GetComponent<Rigidbody2D>();
            targetController = target.GetComponent<PlayerController2D>();

            anchoredY = target.position.y;
            anchorInitialised = true;

            // Ilk karede kaymamasi icin dogru yere yerlestir
            transform.position = ClampToBounds(new Vector3(
                target.position.x + offset.x,
                anchoredY + offset.y,
                transform.position.z));
        }

        private void LateUpdate()
        {
            if (target == null) return;

            float x = UpdateHorizontal();
            float y = UpdateVertical();

            Vector3 desired = new Vector3(x, y, transform.position.z);
            transform.position = ClampToBounds(desired) + UpdateShake();
        }

        // ---------------------------------------------------------------
        // Yatay
        // ---------------------------------------------------------------

        private float UpdateHorizontal()
        {
            float desiredX = target.position.x + offset.x;

            // Ileri bakis: hedefin yatay hizina gore kaydir
            if (targetBody != null)
            {
                float speedX = Platformer.Core.Rigidbody2DExtensions.GetVelocity(targetBody).x;
                float wanted = Mathf.Clamp(speedX / 8f, -1f, 1f) * lookAheadDistance;
                lookAhead = Mathf.SmoothDamp(lookAhead, wanted, ref lookAheadVelocity, lookAheadSmoothTime);
                desiredX += lookAhead;
            }

            // Olu bolge: kucuk hareketlerde kimildama
            float dx = desiredX - transform.position.x;
            if (Mathf.Abs(dx) < horizontalDeadZone * 0.5f) return transform.position.x;

            return Mathf.SmoothDamp(transform.position.x, desiredX,
                                    ref horizontalVelocity, horizontalSmoothTime);
        }

        // ---------------------------------------------------------------
        // Dikey
        // ---------------------------------------------------------------

        private float UpdateVertical()
        {
            float smooth = verticalSmoothTime;

            if (followGroundedHeight && targetController != null)
            {
                UpdateAnchor(ref smooth);
            }
            else
            {
                anchoredY = target.position.y;
            }

            float desiredY = anchoredY + offset.y;
            return Mathf.SmoothDamp(transform.position.y, desiredY,
                                    ref verticalVelocity, smooth);
        }

        /// <summary>
        /// Takip edilen yuksekligi gunceller.
        /// Normalde sadece yere deginca degisir - ziplamada sabit kalir.
        /// </summary>
        private void UpdateAnchor(ref float smooth)
        {
            float playerY = target.position.y;

            if (!anchorInitialised)
            {
                anchoredY = playerY;
                anchorInitialised = true;
                return;
            }

            // 1) Yerdeyiz: yeni yukseklik burasi
            if (targetController.IsGrounded)
            {
                anchoredY = playerY;
                return;
            }

            float vy = targetBody != null
                ? Platformer.Core.Rigidbody2DExtensions.GetVelocity(targetBody).y
                : 0f;

            // 2) Kayitli yuksekligin ALTINA hizli duserken takip et.
            //
            // "playerY < anchoredY" sarti sart: normal bir ziplamanin inisinde de
            // hiz esigi asilir (dusus yercekimi 84 ile 0.1 sn'de -9'u geciyor).
            // O sart olmasaydi her ziplamada kamera inise eslik eder, tam onlemeye
            // calistigimiz zipzip hareketi geri gelirdi.
            if (vy < fastFallThreshold && playerY < anchoredY)
            {
                anchoredY = playerY;
                smooth = fastFallSmoothTime;
                return;
            }

            // 3) Cok uzaklastiysa kurali boz - karakter ekrandan cikmasin
            float drift = playerY - anchoredY;
            if (Mathf.Abs(drift) > maxVerticalDrift)
            {
                anchoredY = playerY - Mathf.Sign(drift) * maxVerticalDrift;
            }

            // Aksi halde anchoredY sabit kalir: normal ziplama ekrani oynatmaz
        }

        // ---------------------------------------------------------------
        // Sinirlar
        // ---------------------------------------------------------------

        /// <summary>Kamerayi bolum sinirlarinin icinde tutar.</summary>
        private Vector3 ClampToBounds(Vector3 position)
        {
            if (!useBounds || cam == null || !cam.orthographic) return position;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float minX = minBounds.x + halfWidth;
            float maxX = maxBounds.x - halfWidth;
            float minY = minBounds.y + halfHeight;
            float maxY = maxBounds.y - halfHeight;

            // Sinir alani kameradan darsa ortala, yoksa kenetle
            position.x = minX > maxX ? (minBounds.x + maxBounds.x) * 0.5f : Mathf.Clamp(position.x, minX, maxX);
            position.y = minY > maxY ? (minBounds.y + maxBounds.y) * 0.5f : Mathf.Clamp(position.y, minY, maxY);

            return position;
        }

        // ---------------------------------------------------------------
        // Sarsinti
        // ---------------------------------------------------------------

        /// <summary>Disaridan cagir: cam.Shake(0.15f, 0.3f)</summary>
        public void Shake(float duration, float magnitude)
        {
            if (!ScreenShakeEnabled) return;

            // Daha siddetli bir sarsinti gelirse onu uygula, zayifi ustune yazma
            if (shakeTimeLeft > 0f && magnitude < shakeMagnitude) return;

            shakeDuration = Mathf.Max(duration, 0.01f);
            shakeTimeLeft = shakeDuration;
            shakeMagnitude = magnitude;
        }

        private Vector3 UpdateShake()
        {
            if (shakeTimeLeft <= 0f)
            {
                shakeMagnitude = 0f;
                return Vector3.zero;
            }

            // unscaledDeltaTime: hit stop sirasinda (timeScale = 0) da sonsun
            shakeTimeLeft -= Time.unscaledDeltaTime;

            float progress = 1f - (shakeTimeLeft / shakeDuration);
            float current = shakeMagnitude * Mathf.Exp(-shakeDecay * progress);

            return new Vector3(
                Random.Range(-1f, 1f) * current,
                Random.Range(-1f, 1f) * current,
                0f);
        }

        // ---------------------------------------------------------------

        private void OnDrawGizmosSelected()
        {
            // Yatay olu bolge
            Gizmos.color = Color.yellow;
            float h = cam != null && cam.orthographic ? cam.orthographicSize * 2f : 10f;
            Gizmos.DrawWireCube(transform.position, new Vector3(horizontalDeadZone, h, 0.1f));

            // Dikey surukleme siniri
            if (Application.isPlaying && followGroundedHeight)
            {
                Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.8f);
                Vector3 a = new Vector3(transform.position.x - 6f, anchoredY, 0f);
                Vector3 b = new Vector3(transform.position.x + 6f, anchoredY, 0f);
                Gizmos.DrawLine(a, b);
            }

            // Bolum sinirlari
            if (useBounds)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f,
                                             (minBounds.y + maxBounds.y) * 0.5f, 0f);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x,
                                           maxBounds.y - minBounds.y, 0.1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
