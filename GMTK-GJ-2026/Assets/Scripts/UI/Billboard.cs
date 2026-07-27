using UnityEngine;

namespace UI
{
    public class Billboard : MonoBehaviour
    {
        public enum BillboardType { LookAtCamera, CameraForward }

        [SerializeField] private BillboardType billboardType = BillboardType.CameraForward;
        [SerializeField] private bool lockYAxis = false;

        private Camera mainCamera;

        private void Start()
        {
            // Cache the main camera for performance optimization
            mainCamera = Camera.main;
        }

        // LateUpdate runs after regular Update, ensuring smooth camera movement tracking
        private void LateUpdate()
        {
            if (mainCamera == null) return;

            switch (billboardType)
            {
                case BillboardType.LookAtCamera:
                    LookAtCameraMethod();
                    break;
                case BillboardType.CameraForward:
                    CameraForwardMethod();
                    break;
            }
        }

        private void LookAtCameraMethod()
        {
            Vector3 targetPosition = mainCamera.transform.position;

            if (lockYAxis)
            {
                // Keep the target at the same height as this object to restrict vertical tilt
                targetPosition.y = transform.position.y;
            }

            // Rotate the object to point its forward vector directly at the target
            transform.LookAt(targetPosition);
        }

        private void CameraForwardMethod()
        {
            Vector3 targetForward = mainCamera.transform.forward;

            if (lockYAxis)
            {
                // Zero out the Y axis component to prevent tilting upward/downward
                targetForward.y = 0;
                targetForward.Normalize();
            }

            // Force the object's forward direction to match the camera's angle
            transform.forward = targetForward;
        }
    }
}