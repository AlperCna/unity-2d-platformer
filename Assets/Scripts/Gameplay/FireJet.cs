using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Aralikli ates puskurtucu — sordugu soru: **"Bekleyebilir misin?"**
    ///
    /// Yerden cikan diken "ne zaman GECECEKSIN" diye sorar; bu "ne zaman
    /// BEKLEYECEKSIN" diye sorar. Fark ince ama onemli: diken seni
    /// hizlandirir, ates seni durdurur. Ikisini yan yana koyunca oyuncu
    /// hizlanmayi ve durmayi ayni nefeste yapmak zorunda kalir.
    ///
    /// Alev bir koridoru kapatiyor, yani zemin degil YOL tehlikeli.
    /// </summary>
    public class FireJet : TimedHazard
    {
        [Header("Alev")]
        [Tooltip("Uzayip kisalacak gorsel. Bos birakirsan \"Flame\" adli cocuk aranir.")]
        [SerializeField] private Transform flameVisual;

        [Tooltip("Alevin tam boyu (birim).")]
        [SerializeField] private float length = 3f;

        [Tooltip("Alevin genisligi. Sprite 1 birim genis oldugu icin " +
                 "1 vermek yatay tekrari tamamen onler.")]
        [SerializeField] private float width = 1f;

        [Tooltip("Puskurme yonu.")]
        [SerializeField] private Vector2 direction = Vector2.up;

        [Header("Uyari")]
        [Tooltip("Uyari asamasinda alevin ne kadari cikiyor (0-1).")]
        [Range(0f, 0.6f)]
        [SerializeField] private float warningLength = 0.22f;

        [SerializeField] private Color warningColor = new Color(1f, 0.75f, 0.3f);
        [SerializeField] private Color activeColor = new Color(1f, 0.45f, 0.2f);

        private SpriteRenderer flameRenderer;
        private BoxCollider2D box;
        private float targetLength;
        private float currentLength;

        protected override void OnAwakeHazard()
        {
            // Yedek arama ISME gore. Once indekse gore araniyordu ve
            // prefabin ilk cocugu "Nozzle" oldugu icin yanlis nesneyi
            // buluyordu - alev yerine namluyu uzatmaya calisirdi.
            if (flameVisual == null) flameVisual = transform.Find("Flame");
            if (flameVisual == null && transform.childCount > 0)
            {
                flameVisual = transform.GetChild(transform.childCount - 1);
            }

            flameRenderer = flameVisual != null
                ? flameVisual.GetComponent<SpriteRenderer>()
                : null;

            box = hazardCollider as BoxCollider2D;

            if (flameVisual == null || box == null)
            {
                Debug.LogError($"{name}: alev gorseli veya BoxCollider2D yok.", this);
                enabled = false;
                return;
            }

            direction = direction.sqrMagnitude < 0.01f ? Vector2.up : direction.normalized;

        }

        protected override void OnPhaseChanged(Phase phase, bool instant)
        {
            targetLength = phase switch
            {
                Phase.Active => length,
                Phase.Warning => length * warningLength,
                _ => 0f,
            };

            if (flameRenderer != null)
            {
                flameRenderer.color = phase == Phase.Active ? activeColor : warningColor;
            }

            if (instant)
            {
                currentLength = targetLength;
                Apply();
            }

        }

        private void LateUpdate()
        {
            if (!enabled || flameVisual == null) return;

            // Uzama hizi alevin boyuna gore: kisa alev hizli, uzun alev de
            // ayni surede tamamlansin. Sabit hiz verseydik uzun alevler
            // uyari suresi bittikten sonra bile uzuyor olurdu.
            float speed = Mathf.Max(length, 0.1f) / 0.12f;
            currentLength = Mathf.MoveTowards(currentLength, targetLength,
                                              speed * Time.deltaTime);
            Apply();
        }

        /// <summary>
        /// Gorseli ve collider'i su anki boya gore ayarlar.
        ///
        /// Collider ALEVDEN DAR (%75): oyuncu alevin kenarini siyirip
        /// kurtulabilmeli. Ayni kural dikenlerde de var - gorsel cömert,
        /// collider comert degil.
        /// </summary>
        private void Apply()
        {
            Vector2 dir = direction;
            Vector2 center = dir * (currentLength * 0.5f);

            flameVisual.localPosition = center;
            flameVisual.localRotation = Quaternion.FromToRotation(Vector3.up, dir);

            if (flameRenderer != null)
            {
                flameRenderer.size = new Vector2(width, Mathf.Max(currentLength, 0.01f));
            }

            box.offset = center;
            box.size = new Vector2(
                Mathf.Abs(dir.x) > 0.5f ? Mathf.Max(currentLength - 0.2f, 0.01f) : width * 0.75f,
                Mathf.Abs(dir.x) > 0.5f ? width * 0.75f : Mathf.Max(currentLength - 0.2f, 0.01f));
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 dir = direction.sqrMagnitude < 0.01f ? Vector2.up : direction.normalized;

            Gizmos.color = new Color(1f, 0.45f, 0.2f, 0.7f);
            Gizmos.DrawLine(transform.position,
                            transform.position + (Vector3)(dir * length));
            Gizmos.DrawWireSphere(transform.position + (Vector3)(dir * length), 0.2f);
        }
    }
}
