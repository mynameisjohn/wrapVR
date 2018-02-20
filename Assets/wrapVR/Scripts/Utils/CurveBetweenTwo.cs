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
        [Tooltip("The object to place along the curve")]
        public GameObject CurvePointPrefab;
        [Tooltip("How many objects make up the curve")]
        [Range(0, 150)]
        public uint CurvePoints;

        // Curve goes from start to end1 / end2 based on curve shape
        Transform m_tStart, m_tEnd1, m_tEnd2;

        // List of curve points
        List<GameObject> m_liCurvePoints = new List<GameObject>();
        GameObject m_goCurvePoints;

        private void Start()
        {
            if (CurvePointPrefab == null)
            {
                Debug.LogError("No curve point prefab, using spheres");
                CurvePointPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(CurvePointPrefab.GetComponent<Collider>());
                CurvePointPrefab.transform.localScale *= 0.075f;
            }
        }

        // Start drawing the curve
        public void ActivateCurve(Transform tStart, Transform tEnd1, Transform tEnd2)
        {
            DeactivateCurve();
            if (tStart && tEnd1 && tEnd2)
            {
                m_tStart = tStart;
                m_tEnd1 = tEnd1;
                m_tEnd2 = tEnd2;

                m_goCurvePoints = new GameObject("_CurvePoints");
                m_goCurvePoints.transform.parent = transform;
                for (int i = 0; i < CurvePoints; i++)
                {
                    GameObject curvePoint = Instantiate(CurvePointPrefab);
                    curvePoint.transform.parent = m_goCurvePoints.transform;
                    m_liCurvePoints.Add(curvePoint);
                }
            }
        }

        // Turn the curve off
        public void DeactivateCurve()
        {
            m_liCurvePoints.Clear();
            Destroy(m_goCurvePoints);
        }
    
        // Update is called once per frame
        void Update()
        {
            // Get out if we don't have a curve
            if (m_liCurvePoints.Count == 0)
                return;

            // Cache start and end1 / end2 points
            Vector3 v3Start = m_tStart.transform.position;
            Vector3 v3End1 = m_tEnd1.transform.position;
            Vector3 v3End2 = m_tEnd2.transform.position;

            // Draw curve points
            for (int i = 0; i < m_liCurvePoints.Count; i++)
            {
                // Sample animation curve for smooth points
                float fi = i / (float)CurvePoints;
                float fCurve = CurveWeight.Evaluate(fi);

                // The lines from start to p1/2 are straight
                Vector3 p1 = Vector3.Lerp(v3Start, v3End1, fi);
                Vector3 p2 = Vector3.Lerp(v3Start, v3End2, fi);

                // But we mix them for the final point
                Vector3 p = Vector3.Lerp(p1, p2, fCurve);
                m_liCurvePoints[i].transform.position = p;
            }
        }
    }
}