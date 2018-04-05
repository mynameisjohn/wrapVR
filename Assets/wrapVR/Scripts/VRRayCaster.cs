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

        public EActivation Activation = EActivation.NONE;
        public bool isRayCasting
        {
            get
            {
                switch (Activation)
                {
                    case EActivation.NONE:
                        return true;
                    case EActivation.TOUCH:
                        return m_VrInput.GetTouchpadTouch();
                    case EActivation.TOUCHPAD:
                        return m_VrInput.GetTouchpad();
                    case EActivation.TRIGGER:
                        return m_VrInput.GetTrigger();
                }
                return false;
            }
        }

        private void Start()
        {
            if (Reticle == null)
                Reticle = GetComponentInChildren<Reticle>();
        }

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
                m_CurrentInteractible.TriggerUp(m_VrInput);
                m_CurrentInteractible.TriggerOut(m_VrInput);
            }
        }

        protected void HandleTriggerDown()
        {
            if (m_CurrentInteractible != null)
            {
                // Do trigger down and over
                m_CurrentInteractible.TriggerDown(m_VrInput);
                m_CurrentInteractible.TriggerOver(m_VrInput);
            }
        }
        protected void HandleTouchUp()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchUp(m_VrInput);
        }

        protected void HandleTouchDown()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchDown(m_VrInput);
        }

        protected void HandleTouchpadUp()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchpadUp(m_VrInput);
        }

        protected void HandleTouchpadDown()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.TouchpadDown(m_VrInput);
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

        protected void HandleUp()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Up(m_VrInput);
        }


        protected void HandleDown()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Down(m_VrInput);
        }


        protected void HandleClick()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.Click(m_VrInput);
        }


        protected void HandleDoubleClick()
        {
            if (m_CurrentInteractible != null)
                m_CurrentInteractible.DoubleClick(m_VrInput);
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
                m_VrInput._SetRayCaster(this);
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
            // Check for activation
            if (isRayCasting)
                doRaycast();
        }

        protected abstract void doRaycast();
    }
}