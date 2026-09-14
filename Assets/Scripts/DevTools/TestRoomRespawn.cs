using UnityEngine;
using Platformer.Core;

namespace Platformer.DevTools
{
    /// <summary>
    /// Test odasinda bosluga dusunce baslangica dondurur.
    ///
    /// Gercek bolumde bu isi GameManager'in olum cizgisi + PlayerHealth yapiyor.
    /// Test odasinda o sistemler yok, ama bosluga her dustugunde Play'i yeniden
    /// baslatmak istemiyoruz.
    /// </summary>
    public class TestRoomRespawn : MonoBehaviour
    {
        [Tooltip("Bu Y degerinin altina duserse baslangica doner.")]
        [SerializeField] private float resetBelowY = -20f;

        [Tooltip("Donulecek konum. Bos birakilirsa baslangic konumu kullanilir.")]
        [SerializeField] private Vector2 respawnPoint;

        [SerializeField] private bool useStartPosition = true;

        private Rigidbody2D rb;
        private Vector2 startPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            startPosition = transform.position;
        }

        private void Update()
        {
            if (transform.position.y > resetBelowY) return;

            transform.position = useStartPosition ? startPosition : respawnPoint;
            if (rb != null) rb.SetVelocity(Vector2.zero);
        }
    }
}
