using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Noktalar arasinda gidip gelen platform. Ustundeki karakteri de tasir.
    /// Yolcu tasima, karakteri platforma "child" yapmak yerine her karede
    /// platformun yer degistirme miktarini karaktere eklemekle yapilir -
    /// boylece karakterin olcegi/rotasyonu bozulmaz.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum LoopMode
        {
            /// <summary>Sona varinca geri doner (A -> B -> A).</summary>
            PingPong,
            /// <summary>Sona varinca basa isinlanir (A -> B -> A -> B).</summary>
            Loop
        }

        [Header("Rota")]
        [Tooltip("Baslangic noktasina GORE yerel offsetler. Ilk nokta (0,0) kabul edilir.")]
        [SerializeField]
        private Vector2[] waypoints = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(4f, 0f)
        };

        [SerializeField] private LoopMode loopMode = LoopMode.PingPong;

        [Header("Hareket")]
        [SerializeField] private float speed = 2.2f;

        [Tooltip("Her noktada bekleme suresi.")]
        [SerializeField] private float waitAtWaypoint = 0.4f;

        [Tooltip("Acik ise uclarda yavaslar (daha yumusak hareket).")]
        [SerializeField] private bool easeAtEnds = true;

        [Header("Yolcu Algilama")]
        [Tooltip("Hangi layer'daki nesneler tasinir (Player layer'ini sec).")]
        [SerializeField] private LayerMask passengerLayers = ~0;

        [Tooltip("Platformun ustundeki algilama kutusunun yuksekligi.")]
        [SerializeField] private float passengerCheckHeight = 0.3f;

        private Rigidbody2D rb;
        private Collider2D platformCollider;
        private Vector2 origin;
        private int currentIndex;
        private int direction = 1;
        private float waitTimer;
        private float segmentProgress;

        // Ayni diziyi tekrar kullaniyoruz - her karede cop uretmemek icin
        private readonly Collider2D[] passengerBuffer = new Collider2D[8];

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            platformCollider = GetComponent<Collider2D>();

            // Hareketli platform kinematik olmali: fizik onu itmemeli,
            // ama karakter carpisma algilayabilmeli.
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            origin = transform.position;

            if (waypoints == null || waypoints.Length < 2)
            {
                Debug.LogWarning($"MovingPlatform ({name}): en az 2 nokta gerekiyor.", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (waitTimer > 0f)
            {
                waitTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector2 from = origin + waypoints[currentIndex];
            Vector2 to = origin + waypoints[NextIndex()];

            float segmentLength = Vector2.Distance(from, to);
            if (segmentLength < 0.001f)
            {
                AdvanceWaypoint();
                return;
            }

            segmentProgress += (speed * Time.fixedDeltaTime) / segmentLength;

            float t = Mathf.Clamp01(segmentProgress);
            if (easeAtEnds) t = Mathf.SmoothStep(0f, 1f, t);

            Vector2 nextPosition = Vector2.Lerp(from, to, t);
            Vector2 delta = nextPosition - (Vector2)transform.position;

            // Once yolculari bul (platform hareket etmeden onceki konumda)
            CarryPassengers(delta);

            rb.MovePosition(nextPosition);

            if (segmentProgress >= 1f)
            {
                AdvanceWaypoint();
            }
        }

        /// <summary>Platformun ustunde duran nesneleri ayni miktarda kaydirir.</summary>
        private void CarryPassengers(Vector2 delta)
        {
            if (delta.sqrMagnitude < 0.0000001f || platformCollider == null) return;

            Bounds bounds = platformCollider.bounds;
            Vector2 checkCenter = new Vector2(bounds.center.x, bounds.max.y + passengerCheckHeight * 0.5f);
            Vector2 checkSize = new Vector2(bounds.size.x, passengerCheckHeight);

            int count = Physics2D.OverlapBoxNonAlloc(checkCenter, checkSize, 0f, passengerBuffer, passengerLayers);

            for (int i = 0; i < count; i++)
            {
                Collider2D passenger = passengerBuffer[i];
                if (passenger == null || passenger.attachedRigidbody == rb) continue;

                passenger.transform.position += (Vector3)delta;
            }
        }

        private int NextIndex()
        {
            int next = currentIndex + direction;

            if (loopMode == LoopMode.PingPong)
            {
                if (next >= waypoints.Length) return waypoints.Length - 2;
                if (next < 0) return 1;
                return next;
            }

            return (next + waypoints.Length) % waypoints.Length;
        }

        private void AdvanceWaypoint()
        {
            segmentProgress = 0f;
            waitTimer = waitAtWaypoint;

            if (loopMode == LoopMode.PingPong)
            {
                int next = currentIndex + direction;
                if (next >= waypoints.Length || next < 0)
                {
                    direction *= -1;
                    next = currentIndex + direction;
                }
                currentIndex = Mathf.Clamp(next, 0, waypoints.Length - 1);
            }
            else
            {
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;

            // Oyun calismiyorken origin transform'un kendisidir
            Vector2 basePosition = Application.isPlaying ? origin : (Vector2)transform.position;

            Gizmos.color = new Color(0.3f, 0.8f, 1f);
            for (int i = 0; i < waypoints.Length; i++)
            {
                Vector2 point = basePosition + waypoints[i];
                Gizmos.DrawWireSphere(point, 0.18f);

                if (i < waypoints.Length - 1)
                {
                    Gizmos.DrawLine(point, basePosition + waypoints[i + 1]);
                }
                else if (loopMode == LoopMode.Loop)
                {
                    Gizmos.DrawLine(point, basePosition + waypoints[0]);
                }
            }
        }
    }
}
