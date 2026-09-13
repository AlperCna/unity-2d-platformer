using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Arka plan derinlik efekti. Kamera hareket ettikce bu katman daha yavas
    /// kayar, boylece uzakta duruyormus gibi gorunur.
    /// factor = 0  -> kamerayla birlikte hareket eder (sonsuz uzak)
    /// factor = 1  -> sahneyle birlikte durur (normal nesne gibi)
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("0 = cok uzak (kamerayi takip eder), 1 = sabit (normal nesne).")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxFactor = 0.4f;

        [Tooltip("Dikey eksende de paralaks uygulansin mi?")]
        [SerializeField] private bool applyVertical = true;

        private Transform cameraTransform;
        private Vector3 previousCameraPosition;

        private void Start()
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning($"ParallaxLayer ({name}): MainCamera bulunamadi.", this);
                enabled = false;
                return;
            }

            cameraTransform = mainCamera.transform;
            previousCameraPosition = cameraTransform.position;
        }

        private void LateUpdate()
        {
            Vector3 cameraDelta = cameraTransform.position - previousCameraPosition;

            // Kameranin hareketinin (1 - factor) kadarini geri alarak yavaslatiyoruz
            float shiftX = cameraDelta.x * (1f - parallaxFactor);
            float shiftY = applyVertical ? cameraDelta.y * (1f - parallaxFactor) : 0f;

            transform.position += new Vector3(shiftX, shiftY, 0f);

            previousCameraPosition = cameraTransform.position;
        }
    }
}
