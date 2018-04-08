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
    }
}