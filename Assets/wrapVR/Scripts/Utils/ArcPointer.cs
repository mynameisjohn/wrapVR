using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Arc pointer class that places some prefab along a curve
    // The prefab will represent points along the curve
    public class ArcPointer : MonoBehaviour
    {
        [Tooltip("The arc raycaster that we follow")]
        public VRArcRaycaster Source;
        [Tooltip("Don't draw curve if grabbing")]
        public bool DisableWhileGrabbing = true;

        [Tooltip("Prefab to place at curve target (end of curve)")]
        public GameObject TargetPrefab;
        public bool AlignTarget = false;

        // These are exclusive - only one will be drawn at a time
        [Tooltip("Prefab to place along curve if off")]
        public GameObject OffCurvePrefab;
        [Tooltip("Prefab to place along curve if touch is down")]
        public GameObject TouchCurvePrefab;
        [Tooltip("Prefab to place along curve if touchpad is down")]
        public GameObject TouchPadCurvePrefab;
        [Tooltip("Prefab to place along curve if trigger is down")]
        public GameObject TriggerCurvePrefab;

        // Cache caster input's last activation
        // We clear our curve if it changes
        protected EActivation m_eLastActivation = EActivation.NONE;

        // The constructed target object, if we have a prefab
        protected GameObject m_goTarget;

        // Determines if we'll be drawing a curve of this type
        public bool hasTarget { get { return TargetPrefab; } }
        public bool hasOff { get { return OffCurvePrefab; } }
        public bool hasTouch { get { return TouchCurvePrefab; } }
        public bool hasTouchPad { get { return TouchCurvePrefab; } }
        public bool hasTrigger { get { return TriggerCurvePrefab; } }

        // Use this for initialization
        protected virtual void Start()
        {
            // Use the on curve updated callback to draw curve
            Source = Util.DestroyEnsureComponent(gameObject, Source);

            // If the source's activation is not none then 
            // we'll never get a proper "off" raycast
            if (OffCurvePrefab && Source.Activation != EActivation.NONE)
            {
                Debug.LogWarning("Warning: arc raycaster " + name + " will never display off raycast curve");
                OffCurvePrefab = null;
            }
        }

        // Cached list of curve points
        protected List<GameObject> m_liCurvePoints = new List<GameObject>();

        // Destroy any prefabs we've constructed
        // This gets called pretty regularly but hopefully
        // it's not too expensive if the call is redundant
        protected virtual void clear()
        {
            foreach (GameObject go in m_liCurvePoints)
                Destroy(go);
            m_liCurvePoints.Clear();
            if (m_goTarget)
            {
                Destroy(m_goTarget);
                m_goTarget = null;
            }
        }

        // Idea here is to create enough curve points or remove 
        // unnecessary ones and place them along our source's curve
        protected virtual void drawCurve(GameObject curvePrefab)
        {
            // If we have to create new points do so now
            if (Source.NumActivePoints > m_liCurvePoints.Count)
            {
                int nDiff = Source.NumActivePoints - m_liCurvePoints.Count;
                for (int i = 0; i < nDiff; i++)
                {
                    // Parent curve points to us (?)
                    m_liCurvePoints.Add(Instantiate(curvePrefab));
                    m_liCurvePoints[m_liCurvePoints.Count - 1].transform.SetParent(transform);
                }
            }
            // Or if we have too many points then destroy the extra
            else if (Source.NumActivePoints < m_liCurvePoints.Count)
            {
                int nDiff = m_liCurvePoints.Count - Source.NumActivePoints;
                for (int i = 0; i < nDiff; i++)
                {
                    Destroy(m_liCurvePoints[i + Source.NumActivePoints]);
                }
                m_liCurvePoints.RemoveRange(Source.NumActivePoints, nDiff);
            }

            // Our size should now match the curve point size
            // Match all curve point positions
            for (int i = 0; i < m_liCurvePoints.Count; i++)
            {
                m_liCurvePoints[i].transform.position = Source.CurvePoints[i];
            }

            // Create target prefab if necessary
            if (hasTarget)
            {
                if (m_goTarget == null)
                    m_goTarget = Instantiate(TargetPrefab);

                // Place target prefab
                m_goTarget.SetActive(true);
                m_goTarget.transform.position = Source.CurvePoints[Source.NumActivePoints - 1];
            }
        }
        
        // Use update to verify what we're drawing
        private void Update()
        {
            // Don't do anything if laser is disabled
            if (VRCapabilityManager.isLaserDisabled)
            {
                clear();
                return;
            }

            // Or we're grabbing and DisableWhileGrabbing is true
            if (DisableWhileGrabbing && Source.isGrabbing)
            {
                clear();
                return;
            }

            // Or we aren't raycasting / the curve is empty
            if (!Source.isRayCasting || Source.NumActivePoints == 0)
            {
                clear();
                return;
            }

            // Instead of drawing multiple curves each activation
            // has a precedence (so no touchpad and trigger)
            // trigger > touchpad > touch > off
            EActivation ePrevActivation = m_eLastActivation;
            if (hasTrigger && Source.Input.GetTrigger())
            {
                m_eLastActivation = EActivation.TRIGGER;
            }
            else if (hasTouchPad && Source.Input.GetTouchpad())
            {
                m_eLastActivation = EActivation.TOUCHPAD;
            }
            else if (hasTouch && Source.Input.GetTouchpadTouch())
            {
                m_eLastActivation = EActivation.TOUCH;
            }
            else if (hasOff)
            {
                m_eLastActivation = EActivation.NONE;
            }

            // If this changed clear now
            if (ePrevActivation != m_eLastActivation)
                clear();

            // See if we have a prefab to draw
            GameObject curvePrefab = null;
            switch (m_eLastActivation)
            {
                case EActivation.NONE:
                    curvePrefab = OffCurvePrefab;
                    break;
                case EActivation.TOUCH:
                    curvePrefab = TouchCurvePrefab;
                    break;
                case EActivation.TOUCHPAD:
                    curvePrefab = TouchPadCurvePrefab;
                    break;
                case EActivation.TRIGGER:
                    curvePrefab = TriggerCurvePrefab;
                    break;
            }

            // Maybe get out if we have nothing to draw
            if (curvePrefab == null)
            {
                clear();
                return;
            }

            drawCurve(curvePrefab);
        }
    }
}