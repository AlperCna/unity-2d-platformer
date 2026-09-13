using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Unity 6 ile birlikte Rigidbody2D.velocity -> linearVelocity olarak degisti.
    /// Bu uzantilar sayesinde proje hem Unity 2022 LTS'te hem Unity 6'da derlenir.
    /// Kodun geri kalaninda her zaman GetVelocity/SetVelocity kullan.
    /// </summary>
    public static class Rigidbody2DExtensions
    {
        public static Vector2 GetVelocity(this Rigidbody2D rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        public static void SetVelocity(this Rigidbody2D rb, Vector2 value)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = value;
#else
            rb.velocity = value;
#endif
        }

        public static void SetVelocityX(this Rigidbody2D rb, float x)
        {
            Vector2 v = rb.GetVelocity();
            v.x = x;
            rb.SetVelocity(v);
        }

        public static void SetVelocityY(this Rigidbody2D rb, float y)
        {
            Vector2 v = rb.GetVelocity();
            v.y = y;
            rb.SetVelocity(v);
        }
    }
}
