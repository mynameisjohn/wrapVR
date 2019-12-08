using UnityEngine;
using UnityEngine.UI;

namespace wrapVR
{
    // The reticle is a small point at the centre of the screen.
    // It is used as a visual aid for aiming. The position of the
    // reticle is either at a default position in space or on the
    // surface of a VRInteractiveItem as determined by the VREyeRaycaster.
    public abstract class Reticle : MonoBehaviour
    {
        public float _DefaultDistance = 5f;      // The default distance away from the camera the reticle is placed.
        public bool _UseNormal = true;                  // Whether the reticle should be placed parallel to a surface.
        public Transform _ReticleTransform;      // We need to affect the reticle's transform.
        public VRRayCaster _Source;                // The reticle is always placed relative to the camera.
        public wrapVR.EActivation _Activation = EActivation.NONE;
        [Range(0.1f, 10f)]
        public float _Scale = 1f;

        private Vector3 _scaleTounity;                            // Since the scale of the reticle changes, the original scale needs to be stored.
        private Quaternion _originalRotation;                      // Used to store the original rotation of the reticle.

        public Transform SourceTransform { get { return _Source.Input.transform; } }
        
        protected virtual void Start()
        {
            if (_ReticleTransform == null)
                _ReticleTransform = transform;
            _Source = Util.DestroyEnsureComponent(gameObject, _Source);

            // Store the original scale and rotation.
            _originalRotation = _ReticleTransform.localRotation;

            // Create a vector we can use to scale ourselves to be size 1
            _scaleTounity = new Vector3(_Scale / _ReticleTransform.lossyScale.x, _Scale / _ReticleTransform.lossyScale.y, _Scale / _ReticleTransform.lossyScale.z);

            if (_Activation != EActivation.NONE)
                _Source.ActivationUpCallback(_Activation, (VRRayCaster rc) => { ClearPosition(); });
        }

        public void ClearPosition ()
        {
            // Set the position of the reticle to the default distance in front of the camera.
            _ReticleTransform.position = SourceTransform.position + SourceTransform.forward * _DefaultDistance;

            // Set the scale based on the original and the distance from the camera.
            _ReticleTransform.localScale = _scaleTounity * _DefaultDistance;

            // The rotation should just be the default.
            _ReticleTransform.localRotation = _originalRotation;
            Hide();
        }

        public void SetPosition (RaycastHit hit)
        {
            Show();
            _ReticleTransform.position = hit.point;
            _ReticleTransform.localScale = _scaleTounity * hit.distance;
            
            // If the reticle should use the normal of what has been hit...
            if (_UseNormal)
                // ... set it's rotation based on it's forward vector facing along the normal.
                _ReticleTransform.rotation = Quaternion.FromToRotation (Vector3.forward, hit.normal);
            else
                // However if it isn't using the normal then it's local rotation should be as it was originally.
                _ReticleTransform.localRotation = _originalRotation;
        }

        void Update()
        {
            if (_Source.isRayCasting && _Source.IsActivationDown(_Activation))
                SetPosition(_Source.CurrentHit);
            else
                ClearPosition();
        }

        public abstract void Hide();
        public abstract void Show();
    }
}