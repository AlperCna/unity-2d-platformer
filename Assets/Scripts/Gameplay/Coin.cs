using UnityEngine;
using Platformer.Core;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Kademe 1 — para. Bolum basina 20-30 tane.
    ///
    /// Asil isi YONLENDIRME: dizilisi oyuncuya nereye gidecegini soyler.
    /// Odul kismi ikincil. Bu yuzden degeri de dusuk tutuluyor - paranin
    /// kiymeti topladigin sayida degil, seni goturdugu yerde.
    /// </summary>
    public class Coin : Collectible
    {
        [Header("Deger")]
        [SerializeField] private int scoreValue = 1;

        protected override void OnCollected()
        {
            GameManager.Instance?.CollectCoin(scoreValue);
        }
    }
}
