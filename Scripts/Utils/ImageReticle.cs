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
        public Image _ReticleImg;                     // Reference to the image component that represents the reticle.

        protected override void Start()
        {
            _ReticleImg = Util.DestroyEnsureComponent(gameObject, _ReticleImg);
            if (_ReticleTransform == null)
                _ReticleTransform = _ReticleImg.transform;
            base.Start();
        }

        public override void Hide()
        {
            _ReticleImg.enabled = false;
        }

        public override void Show()
        {
            _ReticleImg.enabled = true;
        }
    }
}