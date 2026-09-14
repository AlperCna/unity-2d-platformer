using UnityEngine;

namespace Platformer.DevTools
{
    /// <summary>
    /// Test odasindaki olcu etiketleri. Sadece Scene view'da gorunur,
    /// oyunda hicbir sey cizmez ve hicbir maliyeti yoktur.
    /// </summary>
    public class TestRoomLabel : MonoBehaviour
    {
        public string text = "";

        [SerializeField] private Color color = new Color(1f, 0.85f, 0.35f);

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (string.IsNullOrEmpty(text)) return;

            var style = new GUIStyle
            {
                normal = { textColor = color },
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            UnityEditor.Handles.Label(transform.position, text, style);

            // Olcunun neyi isaretledigini gosteren kucuk dikey cizgi
            Gizmos.color = color;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 0.5f);
        }
#endif
    }
}
