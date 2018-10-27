using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(VRInteractiveItem))]
    public class DrawGuideCurve : DrawCurve
    {
        [Tooltip("Over/Out activation to start rendering guide curve")]
        public EActivation Activation;

        [Tooltip("GameObject from which we start the guide curve (most likely a controller)")]
        public VRRayCaster CurveStart;

        [Tooltip("In addition to the Over/Out Activation, allow activating with Gaze")]
        public List<VRRayCaster> Activators;

        // # of activations we have
        // When this becomes nonzero we start drawing the curve
        // When this becomes zero we stop drawing the curve
        // int _activationCount;

        Coroutine particleDestroyingCoroutine;

        void DestroyGuideInstantly()
        {
            if (_guideTarget)
            {
                destroyCurvePoints();
                Destroy(_guideTarget);
                _guideTarget = null;
                
                CurveStart.ActivationDownCallback(GetComponent<Grabbable>().Activation, GetComponent<Grabbable>().Attach, false);
                _currentCasters.Clear();
            }
        }
        
        // Clear the curve instantly when we start grabbing
        void OnGrab(Grabbable gr, VRRayCaster rc)
        {
            DestroyGuideInstantly();
        }

        // Use this for initialization
        protected override void Start()
        {
            if (CurveStart == null)
                Destroy(this);

            base.Start();

            GetComponent<Grabbable>().OnGrab += OnGrab;

            GetComponent<VRInteractiveItem>().ActivationOverCallback(Activation, CreateGuide);
            GetComponent<VRInteractiveItem>().ActivationOutCallback(Activation, DestroyGuide);

            foreach (VRRayCaster rc in Activators)
            {
                if (rc.GetComponent<VREyeRaycaster>())
                {
                    GetComponent<VRInteractiveItem>().ActivationOverCallback(EActivation.GAZE, CreateGuide);
                    GetComponent<VRInteractiveItem>().ActivationOutCallback(EActivation.GAZE, DestroyGuide);
                }
            }
        }

        List<VRRayCaster> _currentCasters = new List<VRRayCaster>();

        // VRRayCaster _currentSource;
        GameObject _guideTarget;
        float computeGuidePosition()
        {
            if (_guideTarget == null)
            {
                _guideTarget = new GameObject();
                _guideTarget.transform.parent = transform;
            }

            float dist = (Vector3.Distance(CurveStart.transform.position, transform.position));
            _guideTarget.transform.position = CurveStart.transform.position + dist * CurveStart.transform.forward.normalized;
            return dist;
        }
        
        private void CreateGuide(VRRayCaster rc)
        {
            if (!CurveStart.isActiveAndEnabled || GetComponent<Grabbable>().isGrabbed)
                return;

            bool notFound = true;
            foreach (VRRayCaster rcActivator in Activators)
                if (rcActivator == rc)
                    notFound = false;
            if (notFound)
                return;

            if (_currentCasters.Count == 0)
            {
                float dist = computeGuidePosition();
                uint numCurvePoints = (uint)(dist / UnitsPerCurvePoint);
                createCurvePoints(numCurvePoints, CurveStart.transform, _guideTarget.transform, transform);

                CurveStart.ActivationDownCallback(GetComponent<Grabbable>().Activation, GetComponent<Grabbable>().Attach, true);
            }

            _currentCasters.Add(rc);
        }

        IEnumerator DestroyGuideParticles()
        {
            ParticleSystem[] particleSystems = m_goCurvePoints.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particleSystems)
            {
                ParticleSystem.EmissionModule em = ps.emission;
                em.rateOverTime = 0;
            }
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystem ps = particleSystems[i];
                if (ps.particleCount > 0)
                {
                    i = 0;
                    yield return true;
                }
            }
            DestroyGuideInstantly();
            particleDestroyingCoroutine = null;
            yield break;
        }

        void DestroyGuide(VRRayCaster rc)
        {
            if (_currentCasters.Remove(rc))
            {
                if (_currentCasters.Count  == 0)
                {
                    if (CurvePointPrefab.GetComponent<ParticleSystem>())
                    {
                        if (particleDestroyingCoroutine != null)
                            StopCoroutine(particleDestroyingCoroutine);
                        particleDestroyingCoroutine = StartCoroutine(DestroyGuideParticles());
                    }
                    else
                        DestroyGuideInstantly();
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_guideTarget)
            {
                if (GetComponent<Grabbable>().isGrabbed)
                {
                    DestroyGuideInstantly();
                }
                else
                {
                    computeGuidePosition();
                }
            }
        }
    }
}