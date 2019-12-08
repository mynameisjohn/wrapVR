using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Arc raycaster
    // Shoot a ray that arcs downward
    public class VRArcRaycaster : VRControllerRaycaster
    {
        [Tooltip("How many points make up the curve")]
        [Range(0, 1500)]
        public int NumCurvePoints;

        [Tooltip("How sharply the curve will arc downward")]
        [Range(1, 3)]
        public float Falloff;

        [Tooltip("How far along the curve we start to arc downward")]
        [Range(0.01f, 0.99f)]
        public float FalloffPoint;

        // We'll use the above to construct an animation curve that
        // we can evaluate to determine the shape of the curve
        AnimationCurve m_FalloffCurve;

        public bool ModFalloffWithTouch;

#if !UNITY_ANDROID
        float m_fRiftTouchModY = 0;
#endif

        private void Start()
        {
            m_v3CurvePoints = new Vector3[NumCurvePoints];

            // Construct falloff curve
            // We want a point at (0,0) and a point at (1,1) with zero slope
            // We'll create a middle point with tangents using the Falloff value
            // A high value means a high slope which means a steep curve
            // The X position of the falloff node determines at which point
            // we arc downward - a value of 1 means the curve will be linear
            m_FalloffCurve = new AnimationCurve();
            m_FalloffCurve.AddKey(new Keyframe(0, 0, 0, 0));
            m_FalloffCurve.AddKey(new Keyframe(FalloffPoint, 0.5f, Falloff, Falloff));
            m_FalloffCurve.AddKey(new Keyframe(1, 1, 0, 0));

#if !UNITY_ANDROID
            if (VRCapabilityManager.sdkType == ESDK.Oculus)
                Input.OnTouchUp += (VRInput input) => { m_fRiftTouchModY = 0.5f; };
#endif
        }

        int m_nCurvePointsActive = 0;
        public int NumActivePoints { get { return m_nCurvePointsActive; } }
        Vector3[] m_v3CurvePoints;
        public Vector3[] CurvePoints { get { return m_v3CurvePoints; } }

#if !UNITY_ANDROID
        protected override void Update()
        {
            if (VRCapabilityManager.sdkType != ESDK.Editor)
                m_fRiftTouchModY = Mathf.Clamp01(m_fRiftTouchModY + .005f * touchPos.y);
            base.Update();
        }
#endif

        // Do the arc based raycast out from the controller
        protected override bool castRayFromController(out RaycastHit hit)
        {
            // If we're modifying the curve then recompute its points
            if (ModFalloffWithTouch && isTouchDown)
            {
                // The X position of the falloff point determines where the curve arcs down
#if !UNITY_ANDROID
                if (VRCapabilityManager.sdkType == ESDK.Editor)
                    FalloffPoint = Mathf.Clamp(Util.remap(touchPos.y, -1, 1, 0, 1), 0.01f, 0.99f);
                else
                    FalloffPoint = Mathf.Clamp(m_fRiftTouchModY, 0.005f, 0.99f);
#else                  
                FalloffPoint = Mathf.Clamp(Util.remap(touchPos.y, -1, 1, 0, 1), 0.01f, 0.99f);
#endif
                Keyframe kFalloff = m_FalloffCurve.keys[1];
                kFalloff.time = FalloffPoint;
                kFalloff.inTangent = Falloff;
                kFalloff.outTangent = Falloff;
                m_FalloffCurve.MoveKey(1, kFalloff);
            }
            
            // Recompute curve - start from input in its forward dir
            m_nCurvePointsActive = 0;
            Vector3 v3Curve = FromTransform.position, v3CurveNext;
            Vector3 v3FwdDir = FromTransform.forward.normalized;

            // Travel along the curve using parameterized values
            float fX = 0;
            float fDx = 1f / NumCurvePoints;
            float fDP = _RayLength / NumCurvePoints;

            // Walk along curve points until we hit something - save room for final hit point
            for (int i = 0; i < NumCurvePoints - 1; i++, fX += fDx, m_nCurvePointsActive++, v3Curve = v3CurveNext)
            {
                // For each curve point evaluate our curve direction and advance
                Vector3 v3CurveDir = Vector3.Lerp(v3FwdDir, Vector3.down, m_FalloffCurve.Evaluate(fX));
                m_v3CurvePoints[m_nCurvePointsActive] = v3Curve;
                v3CurveNext = v3Curve + v3CurveDir.normalized * fDP;
                
                // Do a line cast from the previous point to this one to see if we hit anything
                if (Physics.Linecast(v3Curve, v3CurveNext, out hit, ~_ExclusionLayers))
                {
                    // If we hit something then return true and cache hit point
                    m_v3CurvePoints[m_nCurvePointsActive++] = hit.point;
                    return true;
                }
            }
            
            // Nothing hit, get out
            hit = new RaycastHit();
            return false;
        }
    }
}