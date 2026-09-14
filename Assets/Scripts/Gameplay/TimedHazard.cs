using UnityEngine;
using Platformer.Core;
using Platformer.Player;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Donguyle acilip kapanan tehlikelerin temeli.
    ///
    /// Dusman ile tehlike arasindaki fark: dusman TEPKI VERIR, tehlike
    /// vermez. Tehlikenin degeri tam da bu - dongusu sabit oldugu icin
    /// OGRENILEBILIR. Oyuncu deseni cozdugunde akis hissi olusur; platform
    /// oyununun en tatmin edici ani budur.
    ///
    /// Dongu:  [bekleme] -> [UYARI] -> [aktif] -> [bekleme] -> ...
    ///
    /// UYARI ASAMASI ZORUNLU. Uyarisiz tehlike "haksiz" hissettirir:
    /// oyuncu olur, nedenini anlamaz, oyunu suclar. Ayni sebeple mermi atan
    /// dusmanda da ates oncesi uyari var (Epic 06).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class TimedHazard : MonoBehaviour, IResettable
    {
        public enum Phase
        {
            /// <summary>Guvenli. Tehlike gizli/kapali.</summary>
            Idle,

            /// <summary>Hala guvenli ama "geliyorum" diyor.</summary>
            Warning,

            /// <summary>Oldurur.</summary>
            Active,
        }

        /// <summary>Uyari suresinin inilemeyecek alt siniri.</summary>
        public const float MinWarningDuration = 0.3f;

        [Header("Dongu")]
        [Tooltip("Tam dongu suresi (saniye).")]
        [SerializeField] protected float cycleDuration = 2.5f;

        [Tooltip("Tehlikenin OLDURUCU oldugu sure.")]
        [SerializeField] protected float activeDuration = 1f;

        [Tooltip("Aktif olmadan onceki uyari suresi. 0,3'un altina inme - " +
                 "daha kisasi insan tepki suresinin altinda kalir.")]
        [SerializeField] protected float warningDuration = 0.4f;

        [Tooltip("Dongunun neresinden baslasin (0-1). Yan yana duran " +
                 "tehlikeleri senkrondan cikarmak icin: 0 / 0,33 / 0,66 " +
                 "verirsen dalga gibi calisirlar.")]
        [Range(0f, 1f)]
        [SerializeField] protected float phaseOffset = 0f;

        public Phase CurrentPhase { get; private set; } = Phase.Idle;

        protected Collider2D hazardCollider;
        private float timer;

        protected virtual void Awake()
        {
            hazardCollider = GetComponent<Collider2D>();
            hazardCollider.isTrigger = true;

            ResetCycle();
            OnAwakeHazard();
        }

        protected virtual void OnAwakeHazard() { }

        /// <summary>Respawn'da dongu basa saridir - oyuncu dogar dogmaz olmesin.</summary>
        public void ResetToInitialState()
        {
            ResetCycle();
        }

        private void ResetCycle()
        {
            timer = phaseOffset * Mathf.Max(cycleDuration, 0.01f);

            Phase phase = PhaseAt(timer);
            CurrentPhase = phase;
            OnPhaseChanged(phase, instant: true);
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= cycleDuration) timer -= cycleDuration;

            Phase phase = PhaseAt(timer);

            if (phase != CurrentPhase)
            {
                CurrentPhase = phase;
                OnPhaseChanged(phase, instant: false);
            }

            OnTick(phase, PhaseProgress(timer, phase));
        }

        /// <summary>
        /// Dongunun neresindeyiz?
        ///
        /// Aktif bolum dongunun SONUNDA. Boylece uyari hemen oncesine
        /// dusuyor ve bekleme suresi bastan hesaplaniyor:
        ///   [--------- bekleme ---------][uyari][--- aktif ---]
        /// </summary>
        private Phase PhaseAt(float t)
        {
            float activeStart = cycleDuration - activeDuration;
            float warningStart = activeStart - Mathf.Max(warningDuration, MinWarningDuration);

            if (t >= activeStart) return Phase.Active;
            if (t >= warningStart) return Phase.Warning;
            return Phase.Idle;
        }

        private float PhaseProgress(float t, Phase phase)
        {
            float activeStart = cycleDuration - activeDuration;
            float warning = Mathf.Max(warningDuration, MinWarningDuration);
            float warningStart = activeStart - warning;

            switch (phase)
            {
                case Phase.Active:
                    return activeDuration <= 0f ? 1f : (t - activeStart) / activeDuration;
                case Phase.Warning:
                    return warning <= 0f ? 1f : (t - warningStart) / warning;
                default:
                    return warningStart <= 0f ? 1f : t / warningStart;
            }
        }

        /// <summary>
        /// Asama degisti. instant = true ise sifirlamadan geliyor,
        /// gecis animasyonu oynatma.
        /// </summary>
        protected abstract void OnPhaseChanged(Phase phase, bool instant);

        /// <summary>Her karede. progress = bu asamanin 0-1 arasi ilerlemesi.</summary>
        protected virtual void OnTick(Phase phase, float progress) { }

        // ---------------------------------------------------------------
        // Oldurme
        //
        // Sadece Active asamasinda. Collider'i acip kapatmak yerine asamaya
        // bakiyoruz: collider surekli acik kalinca Unity'nin temas listesi
        // bozulmuyor ve "icindeyken aktiflesen" tehlike de dogru calisiyor.
        // ---------------------------------------------------------------

        private void OnTriggerEnter2D(Collider2D other) => TryKill(other);
        private void OnTriggerStay2D(Collider2D other) => TryKill(other);

        private void TryKill(Collider2D other)
        {
            if (CurrentPhase != Phase.Active) return;
            if (!other.CompareTag("Player")) return;

            var health = other.GetComponent<PlayerHealth>();
            if (health != null && !health.IsDead) health.Kill(gameObject);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (warningDuration < MinWarningDuration)
            {
                Debug.LogWarning(
                    $"{name}: uyari suresi {warningDuration:0.00} sn - " +
                    $"en az {MinWarningDuration} olmali. Daha kisasi insan tepki " +
                    "suresinin altinda kalir ve tehlike haksiz hissettirir.", this);
            }

            if (activeDuration + MinWarningDuration > cycleDuration)
            {
                Debug.LogWarning(
                    $"{name}: aktif sure + uyari, dongu suresini asiyor. " +
                    "Bekleme asamasi kalmiyor, yani tehlike hic kapanmiyor.", this);
            }
        }
#endif
    }
}
