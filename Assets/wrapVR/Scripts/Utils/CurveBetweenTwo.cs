using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class CurveBetweenTwo : MonoBehaviour
    {
        // This should go between 0 and 1 in both axes
        [Tooltip("Weight of curve between first and second target")]
        public AnimationCurve CurveWeight;
        public VRRayCaster RayCaster;

        // Curve goes from start to end1 / end2 based on curve shape
        Transform m_tStart, m_tEnd1, m_tEnd2;

        // Start drawing the curve
        public void ActivateCurve(Transform tStart, Transform tEnd1, Transform tEnd2)
        {
            DeactivateCurve();
            if (tStart && tEnd1 && tEnd2)
            {
                m_tStart = tStart;
                m_tEnd1 = tEnd1;
                m_tEnd2 = tEnd2;
            }
        }

        // Turn the curve off
        public void DeactivateCurve()
        {
            m_tStart = null;
            m_tEnd1 = null;
            m_tEnd2 = null;
        }

        public Vector3 Evaluate(float fX)
        {
            float fCurve = CurveWeight.Evaluate(fX);
            Vector3 v3Start = m_tStart.transform.position;
            Vector3 v3End1 = m_tEnd1.transform.position;
            Vector3 v3End2 = m_tEnd2.transform.position;

            // The lines from start to p1/2 are straight
            Vector3 p1 = Vector3.Lerp(m_tStart.transform.position, v3End1, fX);
            Vector3 p2 = Vector3.Lerp(v3Start, v3End2, fX);

            // But we mix them for the final point
            return Vector3.Lerp(p1, p2, fCurve);
        }

        //RaycastHit m_CurveHitPoint;
        //public RaycastHit CurveHitPoint { get { return m_CurveHitPoint; } }
        //bool m_bCurveHit = false;
        //bool hasCurveHit { get { return m_bCurveHit; } }

        //bool createCurvePoints()
        //{
        //    // Get out if we don't have a curve
        //    if (m_liCurvePoints.Count == 0)
        //        return false;

        //    // Cache start and end1 / end2 points
        //    Vector3 v3Start = m_tStart.transform.position;
        //    Vector3 v3End1 = m_tEnd1.transform.position;
        //    Vector3 v3End2 = m_tEnd2.transform.position;

        //    // Draw curve points
        //    m_nSoftLimit = 0;
        //    for (int i = 0; i < m_liCurvePoints.Count; i++, m_nSoftLimit++)
        //    {
        //        // Sample animation curve for smooth points
        //        float fi = i / (float)NumCurvePoints;
        //        float fCurve = CurveWeight.Evaluate(fi);

        //        // The lines from start to p1/2 are straight
        //        Vector3 p1 = Vector3.Lerp(v3Start, v3End1, fi);
        //        Vector3 p2 = Vector3.Lerp(v3Start, v3End2, fi);

        //        // But we mix them for the final point
        //        m_liCurvePoints[i] = Vector3.Lerp(p1, p2, fCurve);
                
        //        // If we have a raycaster then use its exclusion layer to detect collisions
        //        if (i > 0 && RayCaster)
        //        {
        //            if (Physics.Linecast(m_liCurvePoints[i - 1], m_liCurvePoints[i], out m_CurveHitPoint, ~RayCaster.ExclusionLayers))
        //            {
        //                m_bCurveHit = true;
        //                return true;
        //            }
        //        }
        //    }

        //    m_bCurveHit = false;
        //    return false;
        //}
            
        //// Update is called once per frame
        //void Update()
        //{
        //    createCurvePoints();
        //}
    }
}