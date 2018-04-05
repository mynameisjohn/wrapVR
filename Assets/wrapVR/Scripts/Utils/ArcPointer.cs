using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class ArcPointer : Pointer
    {
        public GameObject OffCurvePoint;
        public GameObject OnCurvePoint;

        public bool hasOff { get { return OffCurvePoint; } }
        public bool hasOn { get { return OnCurvePoint; } }
        
        // Use this for initialization
        void Start()
        {
            VRArcRaycaster arcCaster = (VRArcRaycaster)Source;
            arcCaster = Util.DestroyEnsureComponent(gameObject, arcCaster);
            arcCaster.OnCurveUpdated += Source_OnCurveUpdated;
            Source = arcCaster;
            if (Reticle == null)
                Reticle = GetComponent<Reticle>();
            if (FromTransform == null)
                FromTransform = Source.transform;
        }

        List<GameObject> m_liCurvePoints = new List<GameObject>();
        void clear()
        {
            foreach (GameObject go in m_liCurvePoints)
                Destroy(go);
            m_liCurvePoints.Clear();
        }

        bool bLastOn = false;
        void drawCurve(List<Vector3> liCurvePoints, bool bOn)
        {
            if (bLastOn != bOn)
            {
                clear();
                bLastOn = bOn;
            }
            GameObject goPoint = bOn ? OnCurvePoint : OffCurvePoint;
            if (liCurvePoints.Count > m_liCurvePoints.Count)
            {
                int nDiff = liCurvePoints.Count - m_liCurvePoints.Count;
                for (int i = 0; i < nDiff; i++)
                {
                    m_liCurvePoints.Add(Instantiate(goPoint));
                }
            }
            else if (liCurvePoints.Count < m_liCurvePoints.Count)
            {
                int nDiff = m_liCurvePoints.Count - liCurvePoints.Count;
                for (int i = 0; i < nDiff; i++)
                {
                    Destroy(m_liCurvePoints[i + liCurvePoints.Count]);
                }
                m_liCurvePoints.RemoveRange(liCurvePoints.Count, nDiff);
            }

            for (int i = 0; i < m_liCurvePoints.Count; i++)
            {
                m_liCurvePoints[i].transform.position = liCurvePoints[i];
            }
        }
        
        private void Source_OnCurveUpdated(List<Vector3> liCurvePoints)
        {
            // Don't do anything if laser is disabled
            if (VRCapabilityManager.isLaserDisabled)
                return;
            
            if (!(DisableWhileGrabbing && Source.Input.isGrabbing) && isPointerActive)
            {
                if (hasOn)
                {
                    drawCurve(liCurvePoints, true);
                }
                else
                {
                    clear();
                }
            }
            else
            {
                if (hasOff)
                {
                    drawCurve(liCurvePoints, false);
                }
                else
                {
                    clear();
                }
            }
        }

        private void Update()
        {
            if (!isPointerActive && !hasOff)
                clear();
        }
    }
}