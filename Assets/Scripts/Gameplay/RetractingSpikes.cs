using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Yerden cikip inen diken — sordugu soru: **"Ne zaman gececeksin?"**
    ///
    /// Zemine basiyorsun ama zemin bazen oldurucu. Sabit diken "buradan
    /// gecme" der; bu "simdi gecme" der. Ayni parca, bambaska bir soru.
    ///
    /// Uyari asamasinda diken yarim cikiyor: oyuncu neyin gelecegini
    /// GORUYOR. Aniden cikan diken haksizdir.
    /// </summary>
    public class RetractingSpikes : TimedHazard
    {
        [Header("Gorsel")]
        [Tooltip("Asagi yukari hareket edecek olan gorsel. Prefab bunu " +
                 "acikca bagliyor; bos kalirsa ilk cocuk kullanilir.")]
        [SerializeField] private Transform spikeVisual;

        [Tooltip("Tamamen ciktiginda ne kadar yukarida olsun.")]
        [SerializeField] private float travel = 1f;

        [Tooltip("Uyari asamasinda ne kadari gorunsun (0-1). " +
                 "Sifir olsaydi uyari sadece renkle verilirdi.")]
        [Range(0f, 0.8f)]
        [SerializeField] private float warningPeek = 0.3f;

        [SerializeField] private float moveSpeed = 12f;

        private Vector3 hiddenPosition;
        private Vector3 targetPosition;

        protected override void OnAwakeHazard()
        {
            if (spikeVisual == null && transform.childCount > 0)
            {
                spikeVisual = transform.GetChild(0);
            }

            if (spikeVisual == null)
            {
                Debug.LogError($"{name}: hareket edecek gorsel yok.", this);
                enabled = false;
                return;
            }

            hiddenPosition = spikeVisual.localPosition;
            targetPosition = hiddenPosition;
        }

        protected override void OnPhaseChanged(Phase phase, bool instant)
        {
            if (spikeVisual == null) return;

            float amount = phase switch
            {
                Phase.Active => travel,
                Phase.Warning => travel * warningPeek,
                _ => 0f,
            };

            targetPosition = hiddenPosition + Vector3.up * amount;

            // Sifirlamada animasyon oynatma - checkpoint'ten donen oyuncu
            // dikenin yavasca inisini seyretmesin, hemen dogru halde bulsun.
            if (instant) spikeVisual.localPosition = targetPosition;
        }

        private void LateUpdate()
        {
            if (spikeVisual == null) return;

            spikeVisual.localPosition = Vector3.MoveTowards(
                spikeVisual.localPosition, targetPosition, moveSpeed * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Transform visual = spikeVisual != null ? spikeVisual
                             : (transform.childCount > 0 ? transform.GetChild(0) : null);
            if (visual == null) return;

            Vector3 basePos = Application.isPlaying ? transform.TransformPoint(hiddenPosition)
                                                    : visual.position;

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.7f);
            Gizmos.DrawLine(basePos, basePos + Vector3.up * travel);
            Gizmos.DrawWireCube(basePos + Vector3.up * travel, Vector3.one * 0.3f);
        }
    }
}
