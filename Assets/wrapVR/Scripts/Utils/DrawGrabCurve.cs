using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(Grabbable))]
    public class DrawGrabCurve : MonoBehaviour
    {
        [Tooltip("The object to place along the curve")]
        public GameObject CurvePointPrefab;
        [Tooltip("How many points make up the curve")]
        [Range(1, 150)]
        public uint UnitsPerCurvePoint;

        [Tooltip("Shape of the curve arc between the controller and the grabbed object")]
        public AnimationCurve ForceCurve;

        // The # of curve points per unit of grabbable pull distance
        public uint NumCurvePoints { get { return 1 + (uint)(GetComponent<Grabbable>().PullDistance / UnitsPerCurvePoint); } }

        GameObject m_goCurvePoints;
        CurveBetweenTwo m_CBT;

        // Use this for initialization
        void Start()
        {
            m_CBT = new CurveBetweenTwo(ForceCurve);

            // Use sphere for curve prefab if we don't have one
            if (CurvePointPrefab == null)
            {
                Destroy(this);
            }

            // Draw and Deactivate curve when we are grabbed / released
            GetComponent<Grabbable>().OnGrab += (Grabbable gr, VRRayCaster rc) =>
            {
                // First activate curve to generate points
                Transform FollowedTransform = gr.Followed;
                Transform SourceTransform = rc.transform;
                m_CBT.ActivateCurve(SourceTransform, FollowedTransform, transform);

                // Create prefab parent and instantiate prefabs
                m_goCurvePoints = new GameObject("_CurvePoints");
                m_goCurvePoints.transform.parent = transform;
                for (int i = 0; i < NumCurvePoints; i++)
                {
                    GameObject curvePoint = Instantiate(CurvePointPrefab);
                    curvePoint.transform.parent = m_goCurvePoints.transform;
                }
            };
            GetComponent<Grabbable>().OnRelease += (Grabbable gr, VRRayCaster rc) =>
            {
                // Deactivate curve and destroy prefab parent
                m_CBT.DeactivateCurve();
                Destroy(m_goCurvePoints);
                m_goCurvePoints = null;
            };
        }
        private void Update()
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