using UnityEngine;
using UnityEngine.UI;

namespace wrapVR
{
    // The reticle is a small point at the centre of the screen.
    // It is used as a visual aid for aiming. The position of the
    // reticle is either at a default position in space or on the
    // surface of a VRInteractiveItem as determined by the VREyeRaycaster.
    public class ParticleReticle : Reticle
    {
        public ParticleSystem _ReticleParticles;

        protected override void Start()
        {
            _ReticleParticles = Util.DestroyEnsureComponent(gameObject, _ReticleParticles);
            if (ReticleTransform == null)
                ReticleTransform = _ReticleParticles.transform;
            base.Start();
        }

        public override void Hide()
        {
            _ReticleParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public override void Show()
        {
            _ReticleParticles.Play();
        }
    }
}