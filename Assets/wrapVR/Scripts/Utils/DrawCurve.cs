using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class DrawCurve : MonoBehaviour
    {
        [Tooltip("The object to place along the curve")]
        public GameObject CurvePointPrefab;
        [Tooltip("How many points make up the curve")]
        [Range(1, 150)]
        public uint UnitsPerCurvePoint;

        [Tooltip("Shape of the curve arc between the controller and the grabbed object")]
        public AnimationCurve ForceCurve;

        // The # of curve points per unit of grabbable pull distance
        // public uint NumCurvePoints { get { return 1 + (uint)(GetComponent<Grabbable>().PullDistance / UnitsPerCurvePoint); } }

        protected GameObject m_goCurvePoints;
        protected CurveBetweenTwo m_CBT;

        protected void createCurvePoints(uint NumCurvePoints, Transform SourceTransform, Transform FollowedTransform, Transform InitialTransform)
        {
            // Create prefab parent and instantiate prefabs (if not already done)
            if (m_goCurvePoints == null)
            {
                m_goCurvePoints = new GameObject("_CurvePoints");
                m_goCurvePoints.transform.parent = transform;
                for (int i = 0; i < NumCurvePoints; i++)
                {
                    GameObject curvePoint = Instantiate(CurvePointPrefab);
                    curvePoint.transform.parent = m_goCurvePoints.transform;
                }
            }

            // Activate curve to align prefabs to curve
            m_CBT.ActivateCurve(SourceTransform, FollowedTransform, InitialTransform);
        }

        protected void destroyCurvePoints()
        {
            // Deactivate curve and destroy prefab parent
            m_CBT.DeactivateCurve();
            Destroy(m_goCurvePoints);
            m_goCurvePoints = null;
        }        

        // Use this for initialization
       protected virtual void Start()
        {
            m_CBT = new CurveBetweenTwo(ForceCurve);

            // Use sphere for curve prefab if we don't have one
            if (CurvePointPrefab == null)
            {
                Destroy(this);
            }
        }

        virtual protected void Update()
        {
            // If we are drawing a curve
            if (m_goCurvePoints)
            {
                // Update prefab positions with curve positions
                int N = m_goCurvePoints.transform.childCount; 
                for (int i = 0; i < N; i++)
                {
                    m_goCurvePoints.transform.GetChild(i).position = m_CBT.Evaluate((float)i / (float)N);
                }
            }
        }
    }
}