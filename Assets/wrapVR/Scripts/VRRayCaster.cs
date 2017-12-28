using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public abstract class VRRayCaster : MonoBehaviour
    {
        public LayerMask ExclusionLayers;           // Layers to exclude from the raycast.
        public Reticle Reticle;                     // The reticle, if applicable.
        protected VRInput m_VrInput;                // Used to call input based events on the current VRInteractiveItem.
        public bool ShowDebugRay;                   // Optionally show the debug ray.
        public float DebugRayLength = 5f;           // Debug ray length.
        public float DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        public float RayLength = 500f;              // How far into the scene the ray is cast.

        public event System.Action<RaycastHit> OnRaycasthit;                   // This event is called every frame that the user's gaze is over a collider.
        protected VRInteractiveItem m_CurrentInteractible;                //The current interactive item
        protected VRInteractiveItem m_LastInteractible;                   //The last interactive item
        protected bool m_Enabled = true; // not sure about this
        protected Vector3 m_HitPosition;
        protected Quaternion m_HitAngle;

        public Transform InputTransform { get { return m_VrInput.transform; } }

        // Utility for other classes to get the current interactive item
        public VRInteractiveItem CurrentInteractible
        {
            get { return m_CurrentInteractible; }
        }

        protected abstract void setCallbacks();
        protected abstract void clearCallbacks();

        protected void _onRaycastHit(RaycastHit hit)
        {
            if (OnRaycasthit != null)
                OnRaycasthit(hit);
        }

        protected abstract void deactiveLastInteractible();

        protected void HandleTriggerUp()
        {
            if (m_CurrentInteractible != null)
            {
                // Do trigger up and out
                m_CurrentInteractible.TriggerUp();
                m_CurrentInteractible.TriggerOut();
            }
        }

        protected void HandleTriggerDown()
        {
            if (m_CurrentInteractible != null)
            {
                // Do trigger down and over
                m_CurrentInteractible.TriggerDown();
                m_CurrentInteractible.TriggerOver();
            }
        }
        protected void HandleTouchUp()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchUp();
        }

        protected void HandleTouchDown()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchDown();
        }

        protected void HandleTouchpadUp()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchpadUp();
        }

        protected void HandleTouchpadDown()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchpadDown();
        }

        public void EmulateTrigger(bool triggerDown)
        {
            if (triggerDown)
            {
                HandleTriggerDown();
            }
            else
            {
                HandleTriggerUp();
            }
        }
        public Vector3 GetLastHitPosition()
        {
            return m_HitPosition;
        }
        public Quaternion GetLastHitAngle()
        {
            return m_HitAngle;
        }

        protected void RefreshInteractibleColor(VRInteractiveItem interactible)
        {
            interactible.SetRayColor(Color.red);
        }

        protected void HandleUp()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Up();
        }


        protected void HandleDown()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Down();
        }


        protected void HandleClick()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Click();
        }


        protected void HandleDoubleClick()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.DoubleClick();
        }


        public void SetInput(VRInput input)
        {
            // Unlikely, but if we weren't null and are enabled 
            // then unsubscribe and resubscribe later
            if (isActiveAndEnabled)
            {
                clearCallbacks();
            }

            m_VrInput = input;
            if(m_VrInput != null)
            {
                transform.SetParent(m_VrInput.transform);
            }

            if (isActiveAndEnabled)
            {
                setCallbacks();
            }
        }

        protected Transform ctrlT { get { return m_VrInput.transform; } }

        private void OnEnable()
        {
            setCallbacks();
        }

        private void OnDisable()
        {
            clearCallbacks();

            m_LastInteractible = m_CurrentInteractible;
            m_CurrentInteractible = null;
        }

        public VRInput Input { get { return m_VrInput; } }
        public bool HasInput()
        {
            return m_VrInput != null && m_VrInput.HardwareExists();
        }

        private void Update()
        {
            doRaycast();
        }

        protected abstract void doRaycast();
    }
}