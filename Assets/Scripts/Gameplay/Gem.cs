using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Kademe 2 — mucevher. Bolum basina 1-3 tane.
    ///
    /// Paradan farki SAYIDA degil YERDE: mucevher hep bir riskin ardinda
    /// durur. Dikenin ustunde, zor bir ziplamanin sonunda, ucan dusmanin
    /// otesinde.
    ///
    /// Bu yuzden "toplama" degil KARAR nesnesidir: oyuncu her gordugunde
    /// "riske girer miyim" diye sorar. Rastgele serpilirse o soru kaybolur
    /// ve mucevher sadece buyuk bir para olur.
    /// </summary>
    public class Gem : Collectible
    {
        [Header("Deger")]
        [Tooltip("Paradan belirgin sekilde yuksek olmali - riski karsilamali.")]
        [SerializeField] private int scoreValue = 10;

        protected override void Awake()
        {
            base.Awake();

            // Paradan AYIRT EDILEBILIR olmali. Sadece renk yetmez (renk
            // korlugu), o yuzden daha yavas salinip daha hizli donuyor -
            // hareket de farkli.
            bobSpeed *= 0.6f;
            spinSpeed *= 1.6f;
        }

        protected override void OnCollected()
        {
            GameManager.Instance?.CollectGem(scoreValue);
        }
    }
}
