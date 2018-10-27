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

        public bool AlignTarget = false;

        // These are exclusive - only one will be drawn at a time
        [Tooltip("Prefab to place along curve if off")]
        public GameObject OffCurvePrefab;
        [Tooltip("Prefab to place at target if off")]
        public GameObject OffTargetPrefab;
        [Tooltip("Prefab to place along curve if touch is down")]
        public GameObject TouchCurvePrefab;
        [Tooltip("Prefab to place at target for touch")]
        public GameObject TouchTargetPrefab;
        [Tooltip("Prefab to place along curve if touchpad is down")]
        public GameObject TouchPadCurvePrefab;
        [Tooltip("Prefab to place at target for touchpad")]
        public GameObject TouchpadTargetPrefab;
        [Tooltip("Prefab to place along curve if trigger is down")]
        public GameObject TriggerCurvePrefab;
        [Tooltip("Prefab to place at target for trigger")]
        public GameObject TriggerTargetPrefab;
        [Tooltip("Prefab to place along curve if Grip is down")]
        public GameObject GripCurvePrefab;
        [Tooltip("Prefab to place at target for grip")]
        public GameObject GripTargetPrefab;

        // Cache caster input's last activation
        // We clear our curve if it changes
        protected EActivation m_eLastActivation = EActivation.NONE;

        // The constructed target object, if we have a prefab
        protected GameObject m_goTarget;

        // Determines if we'll be drawing a curve of this type
        public bool hasOff { get { return OffCurvePrefab; } }
        public bool hasOffTarget { get { return OffTargetPrefab; } }
        public bool hasTouch { get { return TouchCurvePrefab; } }
        public bool hasTouchTarget { get { return TouchTargetPrefab; } }
        public bool hasTouchPad { get { return TouchCurvePrefab; } }
        public bool hasTouchpadTarget { get { return TouchpadTargetPrefab; } }
        public bool hasTrigger { get { return TriggerCurvePrefab; } }
        public bool hasTriggerTarget { get { return TriggerTargetPrefab; } }
        public bool hasGrip { get { return GripCurvePrefab; } }
        public bool hasGripTarget { get { return GripTargetPrefab; } }

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
        protected virtual void drawCurve(GameObject curvePrefab, GameObject targetPrefab)
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
            if (targetPrefab)
            {
                if (m_goTarget == null)
                    m_goTarget = Instantiate(targetPrefab);

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
            if (hasGrip && Source.Input.GetGrip())
            {
                m_eLastActivation = EActivation.GRIP;
            }
            else if (hasTrigger && Source.Input.GetTrigger())
            {
                m_eLastActivation = EActivation.TRIGGER;
            }
            else if (hasTouchPad && Source.Input.GetTouchpad())
            {
                m_eLastActivation = EActivation.TOUCHPAD;
            }
            else if (hasTouch && Source.Input.GetTouch())
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
            GameObject targetPrefab = null;
            switch (m_eLastActivation)
            {
                case EActivation.NONE:
                    curvePrefab = OffCurvePrefab;
                    targetPrefab = OffTargetPrefab;
                    break;
                case EActivation.TOUCH:
                    curvePrefab = TouchCurvePrefab;
                    targetPrefab = TouchTargetPrefab;
                    break;
                case EActivation.TOUCHPAD:
                    curvePrefab = TouchPadCurvePrefab;
                    targetPrefab = TouchpadTargetPrefab;
                    break;
                case EActivation.TRIGGER:
                    curvePrefab = TriggerCurvePrefab;
                    targetPrefab = TriggerTargetPrefab;
                    break;
                case EActivation.GRIP:
                    curvePrefab = GripCurvePrefab;
                    targetPrefab = GripTargetPrefab;
                    break;
            }

            // Maybe get out if we have nothing to draw
            if (curvePrefab == null)
            {
                clear();
                return;
            }

            drawCurve(curvePrefab, targetPrefab);
        }
    }
}