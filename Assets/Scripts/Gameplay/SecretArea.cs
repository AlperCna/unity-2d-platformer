using System.Collections;
using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Gizli alan — bolum basina bir tane.
    ///
    /// NEDEN PARA DEGIL ALAN:
    /// Gizli bir para "bir tane daha topladim" hissi verir. Gizli bir ALAN
    /// "burada bir sey vardi ve ben BULDUM" hissi verir. Ikincisi cok daha
    /// guclu, cunku odul nesne degil KESIF.
    ///
    /// Nasil calisiyor: bir duvar parcasi aslinda gecilebilir. Icine
    /// girince duvar saydamlasiyor ve arkasi gorunuyor.
    ///
    /// IKI KURAL:
    ///
    /// 1. IPUCU ZORUNLU. Ipucusuz sir, sir degil rastlantidir - oyuncuyu
    ///    degil sansi odullendirir. "Tools > Bolumu Denetle" bunu kontrol
    ///    ediyor.
    ///
    /// 2. CIKIS ZORUNLU. Ilk surumde Bolum 1'in sir odasindan cikis yoktu
    ///    ve oyuncu sirri bulunca OLMEK zorunda kaliyordu. Sir bir odul
    ///    degil ceza oluyordu; "iyi ki merak etmemisim" dedirtir, ki
    ///    aradiginizin tam tersi.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SecretArea : MonoBehaviour
    {
        [Header("Odul")]
        [Tooltip("Sir bulununca eklenen puan. Mucevherden de yuksek olmali - " +
                 "bolumun en degerli seyi bu.")]
        [SerializeField] private int scoreReward = 25;

        [Header("Gizleyen Duvar")]
        [Tooltip("Sir bulununca saydamlasacak nesneler. Bos birakirsan " +
                 "cocuk nesnelerdeki tum SpriteRenderer'lar kullanilir.")]
        [SerializeField] private SpriteRenderer[] concealers;

        [Tooltip("Bulununca duvarin kalan saydamligi. 0 = tamamen kaybolur. " +
                 "Biraz birakmak daha iyi: oyuncu neyin acildigini gorur.")]
        [Range(0f, 1f)]
        [SerializeField] private float revealedAlpha = 0.25f;

        [SerializeField] private float revealDuration = 0.35f;

        [Header("Ipucu")]
        [Tooltip("Bu sirrin ipucu ne? Sadece belge amacli - Inspector'da " +
                 "okunur ve denetim araci bos birakilmadigini kontrol eder.")]
        [TextArea(2, 4)]
        [SerializeField] private string hint = "";

        public string Hint => hint;
        public bool Found { get; private set; }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;

            if (concealers == null || concealers.Length == 0)
            {
                concealers = GetComponentsInChildren<SpriteRenderer>();
            }

            // GameManager'a "bu bolumde sir VAR" demek gerekiyor; yoksa
            // bolum sonu ozeti sir satirini hic gostermemeli.
            GameManager.Instance?.RegisterSecret();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (Found || !other.CompareTag("Player")) return;

            Found = true;
            GameManager.Instance?.FindSecret(scoreReward);
            StartCoroutine(Reveal());
        }

        private IEnumerator Reveal()
        {
            var startColors = new Color[concealers.Length];
            for (int i = 0; i < concealers.Length; i++)
            {
                if (concealers[i] != null) startColors[i] = concealers[i].color;
            }

            float t = 0f;
            while (t < revealDuration)
            {
                t += Time.deltaTime;
                float k = t / revealDuration;

                for (int i = 0; i < concealers.Length; i++)
                {
                    if (concealers[i] == null) continue;

                    Color c = startColors[i];
                    c.a = Mathf.Lerp(startColors[i].a, revealedAlpha, k);
                    concealers[i].color = c;
                }

                yield return null;
            }
        }

#if UNITY_EDITOR
        // NOT: burada bir OnValidate uyarisi vardi ("ipucu yazilmamis")
        // ve KALDIRILDI.
        //
        // Sebep: OnValidate, AddComponent aninda calisiyor - yani bolum
        // kurucusu ipucunu ATAMADAN once. Her kurulumda bos yere uyari
        // basiyordu ve dogru olan durumlar bile kirmizi gorunuyordu.
        //
        // Surekli bagiran bir uyari, uyari olmaktan cikip gurultu olur ve
        // insan gercek olani da gormez. Kontrol artik sadece
        // "Tools > 2D Platformer > Bolumu Denetle" icinde - orada her sey
        // yerine oturmus haldeyken bakiliyor.

        private void OnDrawGizmos()
        {
            var col = GetComponent<Collider2D>();
            if (col == null) return;

            Gizmos.color = Found ? new Color(0.3f, 1f, 0.5f, 0.35f)
                                 : new Color(1f, 0.9f, 0.3f, 0.35f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
#endif
    }
}
