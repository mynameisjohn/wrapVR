using UnityEngine;
using UnityEngine.UI;

namespace wrapVR
{
    // The reticle is a small point at the centre of the screen.
    // It is used as a visual aid for aiming. The position of the
    // reticle is either at a default position in space or on the
    // surface of a VRInteractiveItem as determined by the VREyeRaycaster.
    public class ImageReticle : Reticle
    {
        public Image ReticleImage;                     // Reference to the image component that represents the reticle.

        protected override void Start()
        {
            ReticleImage = Util.DestroyEnsureComponent(gameObject, ReticleImage);
            if (ReticleTransform == null)
                ReticleTransform = ReticleImage.transform;
            base.Start();
        }

        public override void Hide()
        {
            ReticleImage.enabled = false;
        }

        public override void Show()
        {
            ReticleImage.enabled = true;
        }
    }
}