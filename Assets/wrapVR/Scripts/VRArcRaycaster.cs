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
            if (VRCapabilityManager.sdkType == VRCapabilityManager.ESDK.Oculus)
                Input.OnTouchpadTouchUp += () => { m_fRiftTouchModY = 0.5f; };
#endif
        }

        int m_nCurvePointsActive = 0;
        public int NumActivePoints { get { return m_nCurvePointsActive; } }
        Vector3[] m_v3CurvePoints;
        public Vector3[] CurvePoints { get { return m_v3CurvePoints; } }

#if !UNITY_ANDROID
        protected override void Update()
        {
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
                FalloffPoint = Mathf.Clamp(m_fRiftTouchModY, 0.005f, 0.99f);
#else                  
                FalloffPoint = Mathf.Clamp(Util.remap(touchPos.y, -1, 1, 0, 1), 0.01f, 0.99f
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
            float fDP = RayLength / NumCurvePoints;

            // Walk along curve points until we hit something - save room for final hit point
            for (int i = 0; i < NumCurvePoints - 1; i++, fX += fDx, m_nCurvePointsActive++, v3Curve = v3CurveNext)
            {
                // For each curve point evaluate our curve direction and advance
                Vector3 v3CurveDir = Vector3.Lerp(v3FwdDir, Vector3.down, m_FalloffCurve.Evaluate(fX));
                m_v3CurvePoints[m_nCurvePointsActive] = v3Curve;
                v3CurveNext = v3Curve + v3CurveDir.normalized * fDP;
                
                // Do a line cast from the previous point to this one to see if we hit anything
                if (Physics.Linecast(v3Curve, v3CurveNext, out hit, ~ExclusionLayers))
                {
                    // If we hit something then return true and cache hit point
                    m_v3CurvePoints[m_nCurvePointsActive++] = hit.point;
                    return true;
                }
            }
            
            // Nothing hit, get out
            hit = new RaycastHit();
            return false;
            /* TODO get rid of this...
            // We'll divide our curve into two sections
            // The first goes from the input out to to a point P0 = inputPos + CurveDrop * RayLength * inputFwd. 
            // The second point is a blend of the point straight below P0 and the point we'd hit we continued
            // passed P0 in the forward direction. This blend can be modulated by the Y touchpad position
            int N0 = Mathf.FloorToInt(CurveDrop * NumCurvePoints); // # of points before curve drops
            int N1 = NumCurvePoints - N0;                          // # of points after curve drops
            float fX = 0;                                          // Current curve position [0:1]
            float fDel = 1f / NumCurvePoints;                      // Change per curve point in X
            float fD0 = CurveDrop * RayLength;                     // Distance of ray before drop
            float fD1 = (1 - CurveDrop) * RayLength;               // Distance of ray after drop

            hit = new RaycastHit();

            // First point - cast out from input pos 
            // In the case that this initial ray hits something 
            // we register that as our hit and get out early
            RaycastHit hit1 = new RaycastHit();
            Vector3 v3P0 = Input.transform.position;
            Vector3 v3Fwd = Input.transform.forward.normalized;
            bool bHit1 = Physics.Raycast(v3P0, v3Fwd, out hit1, fD0, ~ExclusionLayers);
            Vector3 v3P1 = bHit1 ? hit1.point : (v3P0 + fD0 * v3Fwd);
            m_goStraight.transform.position = v3P1;

            // Regardless of hit compute the point that we'd reach
            // if we went all the way - we need it to create curve line
            Vector3 v3P2S = v3P0 + RayLength * v3Fwd;

            // Draw curve to first point regardless of hit
            for (int i = 0; i < N0; i++)
            {
                // This part of the curve mixes the start pos with
                // the point we'd hit if we went straight out 
                Vector3 v3CurvePoint = Vector3.Lerp(v3P0, v3P2S, fX);
                fX += fDel;
            }

            // We can get out now if we hit something
            if (bHit1)
                return true;

            // Otherwise cast a blend of the point below P1 and the point straight ahead

            // Find point below P1 - check for a collision or do second leg length
            RaycastHit hitD;
            bool bHitD = Physics.Raycast(v3P1, Vector3.down, out hitD, fD1, ~ExclusionLayers);
            Vector3 v3P2D = bHitD ? hit.point : v3P0 + fD1 * Vector3.down;

            // See if we'd hit something if we went straight to find the true point straight ahead
            RaycastHit hitS;
            bool bHitS = Physics.Raycast(v3P1, v3Fwd, out hitS, fD1, ExclusionLayers);
            v3P2S = bHitS ? hitS.point : v3P1 + fD1 * v3Fwd;

            // Now blend these two points

            // First hit - cast a ray from the source to the "straight" target and see if we hit anything
            float fd0 = CurveDrop * RayLength;
            bool bFirstHit = Physics.Raycast(Input.transform.position, Input.transform.forward, out hitInfo, fD0, ~ExclusionLayers);
            Vector3 v3FirstHit = bFirstHit ? hitInfo.point : Input.transform.position + fD0 * Input.transform.forward.normalized;
            m_goStraight.transform.position = v3FirstHit;

            // Fill in curve points until straight target
            for (int i = 0; i < NumCurvePoints; i++)
            {
                float fX = (float)i / (float)NumCurvePoints;
                if (fX > CurveDrop)
                    break;
                m_v3CurvePoints[m_nCurvePointsActive++] = Vector3.Lerp(Input.transform.position, v3FirstHit, Util.remap(fX, 0, CurveDrop, 0, 1));
            }

            // If it hit something then register so and get out
            if (bFirstHit)
            {
                hit = hitInfo;
                return true;
            }

            // Otherwise draw the downward target
            // The downward ray starts at the "straight" target position and goes down at an incident angle
            float fd1 = RayLength - fD0;
            Vector3 v3DownRayDir = Input.transform.forward.normalized;

            // If we're modulating with touch sample Y position along the vertical falloff curve and mix that value with straight down
            // This allows control over how hard the curve arcs toward the ground (should there be a parameterized value?)
            if (ModulateWithTouch)
            {
                v3DownRayDir = Vector3.Lerp(v3DownRayDir, Vector3.down, VerticalFalloff.Evaluate(Util.remap(Input.GetTouchPosition().y, -1, 1, 0, 1)));
            }

            bool bSecondHit = Physics.Raycast(v3FirstHit, v3DownRayDir, out hitInfo, fd1, ~ExclusionLayers);
            Vector3 v3SecondHit = bSecondHit ? hitInfo.point : v3FirstHit + fd1 * v3DownRayDir;
            m_goTarget.transform.position = v3SecondHit;
            for (int i = m_nCurvePointsActive; i < NumCurvePoints; i++)
            {
                float fX = (float)i / (float)NumCurvePoints;
                m_v3CurvePoints[m_nCurvePointsActive++] = Vector3.Lerp(v3FirstHit, v3SecondHit, Util.remap(fX, CurveDrop, 1, 0, 1));
            }
            if (bSecondHit)
            {
                 float fDel = 1f / NumCurvePoints;
                 Vector3 P = m_goStraight.transform.position;
                 Vector3 A = Input.transform.position;
                 Vector3 B = m_goTarget.transform.position;
                 Vector3 a = (P - A - CurveDrop * (B - A)) / (CurveDrop * CurveDrop - CurveDrop);
                 Vector3 b = B - A - a;
                 Vector3 c = A;
                 for (m_nCurvePointsActive = 0; m_nCurvePointsActive < NumCurvePoints; m_nCurvePointsActive++)
                 {
                     float fX = (float)m_nCurvePointsActive / NumCurvePoints;
                    if (fX < CurveDrop)
                    {
                        Vector3 v3Straight = m_goStraight.transform.position - Input.transform.position;
                        Vector3 v3Up = v3Straight.magnitude * Vector3.up;
                        m_v3CurvePoints[m_nCurvePointsActive] = Vector3.Lerp()
                    }
                     m_v3CurvePoints[m_nCurvePointsActive] = fX * fX * a + fX * b + c;
                 }

                hit = hitInfo;
                return true;
            }

            hit = new RaycastHit();
            return false;


            */

            /*

            // Compute ray length taking vertical falloff into account
            Debug.Log(fRayLength);

            // Find where the 'straight' transform should go
            Vector3 v3RayDownOfs = fRayLength * Input.transform.forward;
            Vector3 v3RayDownStart = Input.transform.position + v3RayDownOfs;
            m_goStraight.transform.position = v3RayDownStart;

            // We don't need to curve if the straight transform hits something
            if (Physics.Raycast(Input.transform.position, v3RayDownOfs, out hit, fRayLength, ~ExclusionLayers))
            {
                for (int i = 0; i < NumCurvePoints; i++)
                {
                    float fX = (float)i / (float)NumCurvePoints;
                    m_v3CurvePoints[m_nCurvePointsActive++] = Vector3.Lerp(Input.transform.position, hit.point, fX);
                }

                return true;
            }

            // Otherwise curve between point in front and point below
            RaycastHit hitDown;
            if (Physics.Raycast(v3RayDownStart, Vector3.down, out hitDown, fRayLength, ~ExclusionLayers)) 
            {
                if (hitDown.transform.GetComponent<VRInteractiveItem>())
                {
                    m_goTarget.transform.position = hitDown.point;

                    for (int i = 0; i < NumCurvePoints; i++)
                    {
                        float fX = (float)i / (float)NumCurvePoints;

                        if (fX < fAlpha)
                        {

                        }
                        else
                        {

                        }

                        Vector3 v3New = m_CBT.Evaluate(fX);
                        m_v3CurvePoints[m_nCurvePointsActive++] = v3New;

                        if (m_nCurvePointsActive > 1)
                        {
                            // If we actually get a hit return early
                            Vector3 v3Old = m_v3CurvePoints[m_nCurvePointsActive - 2];
                            if (Physics.Linecast(v3Old, v3New, out hit, ~ExclusionLayers))
                            {
                                return true;
                            }
                        }
                    }
                    
                    // Otherwise treat the hit down as the hit point and return
                    hit = hitDown;
                    return true;
                }
            }

            // Nothing hit
            hit = new RaycastHit();
            return false;
            */
        }
    }
}