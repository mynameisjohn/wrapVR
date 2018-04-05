using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(CurveBetweenTwo))]
    public class VRArcRaycaster : VRControllerRaycaster
    {
        [Tooltip("How many points make up the curve")]
        [Range(0, 150)]
        public uint NumCurvePoints;

        public AnimationCurve VerticalFalloff;

        GameObject m_goStraight, m_goTarget;
        CurveBetweenTwo m_CBT;
        private void Start()
        {
            if (Reticle == null)
                Reticle = GetComponentInChildren<Reticle>();

            m_CBT = Util.EnsureComponent<CurveBetweenTwo>(gameObject);
            m_goStraight = new GameObject();
            Destroy(m_goStraight.GetComponent<Collider>());
            m_goStraight.transform.SetParent(transform);
            m_goStraight.name = "Straight";
            m_goTarget = new GameObject();
            m_goTarget.transform.SetParent(transform);
            m_goTarget.name = "Curve";
            m_CBT.ActivateCurve(transform, m_goStraight.transform, m_goTarget.transform);

            m_v3CurvePoints = new Vector3[NumCurvePoints];
            Vector3 v3RayDownOfs = RayLength * Input.transform.forward;
            Vector3 v3RayDownStart = Input.transform.position + v3RayDownOfs;
            m_goStraight.transform.position = v3RayDownStart;
        }

        Vector3[] m_v3CurvePoints;
        public event System.Action<List<Vector3>> OnCurveUpdated;

        protected override bool castRayFromController(out RaycastHit hit)
        {
            float fVert = Vector3.Dot(Input.transform.forward, Vector3.up);
            float fRayLength = RayLength * VerticalFalloff.Evaluate(1 -fVert );

            List<Vector3> v3CurvePoints = new List<Vector3>();
            Vector3 v3RayDownOfs = fRayLength * Input.transform.forward;
            Vector3 v3RayDownStart = Input.transform.position + v3RayDownOfs;
            m_goStraight.transform.position = v3RayDownStart;

            // See if we don't need to curve - construct straight line if so
            if (Physics.Raycast(Input.transform.position, v3RayDownOfs, out hit, fRayLength, ~ExclusionLayers))
            {
                for (int i = 0; i < NumCurvePoints; i++)
                {
                    float fX = (float)i / (float)NumCurvePoints;
                    v3CurvePoints.Add(Vector3.Lerp(Input.transform.position, hit.point, fX));
                }
                if (OnCurveUpdated != null)
                    OnCurveUpdated(v3CurvePoints);
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
                        Vector3 v3New = m_CBT.Evaluate(fX);

                        v3CurvePoints.Add(v3New);

                        if (v3CurvePoints.Count > 1)
                        {
                            // If we actually get a hit return early
                            Vector3 v3Old = v3CurvePoints[v3CurvePoints.Count - 2];
                            if (Physics.Linecast(v3Old, v3New, out hit, ~ExclusionLayers))
                            {
                                if (OnCurveUpdated != null)
                                    OnCurveUpdated(v3CurvePoints);
                                return true;
                            }
                        }
                    }
                    
                    // Otherwise treat the hit down as the hit point and return
                    hit = hitDown;
                    if (OnCurveUpdated != null)
                        OnCurveUpdated(v3CurvePoints);
                    return true;
                }
            }

            // Update with empty
            if (OnCurveUpdated != null)
                OnCurveUpdated(v3CurvePoints);

            hit = new RaycastHit();
            return false;
        }
    }
}