using UnityEngine;
using UnityEngine.UI;

namespace wrapVR
{
    // The reticle is a small point at the centre of the screen.
    // It is used as a visual aid for aiming. The position of the
    // reticle is either at a default position in space or on the
    // surface of a VRInteractiveItem as determined by the VREyeRaycaster.
    public class Reticle : MonoBehaviour
    {
        public float DefaultDistance = 5f;      // The default distance away from the camera the reticle is placed.
        public bool UseNormal;                  // Whether the reticle should be placed parallel to a surface.
        public Image ReticleImage;                     // Reference to the image component that represents the reticle.
        public Transform ReticleTransform;      // We need to affect the reticle's transform.
        public VRControllerRaycaster Source;                // The reticle is always placed relative to the camera.


        private Vector3 m_OriginalScale;                            // Since the scale of the reticle changes, the original scale needs to be stored.
        private Quaternion m_OriginalRotation;                      // Used to store the original rotation of the reticle.

        public Transform SourceTransform { get { return Source.Input.transform; } }


        private void Awake()
        {
            if (ReticleTransform == null)
                ReticleTransform = transform;
            if (ReticleImage == null)
                ReticleImage = transform.Find("Image").GetComponent<Image>();
            if (Source == null)
                Source = transform.parent.GetComponent<VRControllerRaycaster>();

            // Store the original scale and rotation.
            m_OriginalScale = ReticleTransform.localScale;
            m_OriginalRotation = ReticleTransform.localRotation;

        }


        public void Hide()
        {
            ReticleImage.enabled = false;
        }


        public void Show()
        {
            ReticleImage.enabled = true;
        }


        // This overload of SetPosition is used when the the VREyeRaycaster hasn't hit anything.
        public void SetPosition ()
        {
            // Set the position of the reticle to the default distance in front of the camera.
            ReticleTransform.position = SourceTransform.position + SourceTransform.forward * DefaultDistance;

            // Set the scale based on the original and the distance from the camera.
            ReticleTransform.localScale = m_OriginalScale * DefaultDistance;

            // The rotation should just be the default.
            ReticleTransform.localRotation = m_OriginalRotation;
            Hide();
        }


        // This overload of SetPosition is used when the VREyeRaycaster has hit something.
        public void SetPosition (RaycastHit hit)
        {
            Show();
            ReticleTransform.position = hit.point;
            ReticleTransform.localScale = m_OriginalScale * hit.distance;
            
            // If the reticle should use the normal of what has been hit...
            if (UseNormal)
                // ... set it's rotation based on it's forward vector facing along the normal.
                ReticleTransform.rotation = Quaternion.FromToRotation (Vector3.forward, hit.normal);
            else
                // However if it isn't using the normal then it's local rotation should be as it was originally.
                ReticleTransform.localRotation = m_OriginalRotation;
        }
    }
}