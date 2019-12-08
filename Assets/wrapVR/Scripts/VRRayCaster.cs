using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace wrapVR
{
    public abstract class VRRayCaster : MonoBehaviour
    {
        public LayerMask ExclusionLayers;           // Layers to exclude from the raycast.
        protected VRInput m_VrInput;                // Used to call input based events on the current VRInteractiveItem.
        public bool ShowDebugRay;                   // Optionally show the debug ray.
        public float DebugRayLength = 5f;           // Debug ray length.
        public float DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        public float RayLength = 500f;              // How far into the scene the ray is cast.
        public bool _Log = false;

        [Tooltip("Where we starting casting from (self if null)")]
        public Transform FromTransform;

        public event Action<RaycastHit> OnRaycasthit;                   // This event is called every frame that the user's gaze is over a collider.
        protected VRInteractiveItem m_CurrentInteractible;                //The current interactive item
        protected VRInteractiveItem m_LastInteractible;                   //The last interactive item
        protected bool m_Enabled = true; // not sure about this
        // protected Vector3 m_HitPosition;
        // protected Quaternion m_HitAngle;
        protected RaycastHit m_CurrentHit;

        // Turn on if the raycaster should only hit navmeshes
        public bool ForNavMesh = false;
        public float NavMeshSampleDistance;
        public int NavMeshAreaFilter = UnityEngine.AI.NavMesh.AllAreas;

        public Transform InputTransform { get { return m_VrInput.transform; } }

        public EActivation Activation = EActivation.NONE;
        public bool isRayCasting
        {
            get
            {
#if UNITY_EDITOR
                if (VRCapabilityManager.sdkType == VRCapabilityManager.ESDK.Editor && !((EditorVRInput)Input).IsHandActive)
                    return false;
#endif
                return IsActivationDown(Activation);
            }
        }

        public bool isTouchDown { get { return m_VrInput.GetTouch(); } }
        public bool isTouchpadDown { get { return m_VrInput.GetTouchpad(); } }
        public bool isTriggerDown { get { return m_VrInput.GetTrigger(); } }
        public bool isGripDown { get { return m_VrInput.GetGrip(); } }

        public Vector2 touchPos { get { return m_VrInput.GetTouchPosition(); } }

        public bool IsActivationDown(EActivation eActivation)
        {
            switch (eActivation)
            {
                case EActivation.NONE:
                    return true;
                case EActivation.TOUCH:
                    return isTouchDown;
                case EActivation.TOUCHPAD:
                    return isTouchpadDown;
                case EActivation.TRIGGER:
                    return isTriggerDown;
                case EActivation.GRIP:
                    return isGripDown;
            }
            return false;
        }
        public bool IsAnyActivationDown()
        {
            return
                IsActivationDown(EActivation.TOUCH) ||
                IsActivationDown(EActivation.TOUCHPAD) ||
                IsActivationDown(EActivation.TRIGGER) ||
                IsActivationDown(EActivation.GRIP);
        }

        public void ActivationDownCallback(EActivation activation, Action<VRRayCaster> action, bool bAdd = true)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    if (bAdd)
                        OnTouchDown += action;
                    else
                        OnTouchDown -= action;
                    break;
                case EActivation.TOUCHPAD:
                    if (bAdd)
                        OnTouchpadDown += action;
                    else
                        OnTouchpadDown -= action;
                    break;
                case EActivation.TRIGGER:
                    if (bAdd)
                        OnTriggerDown += action;
                    else
                        OnTriggerDown -= action;
                    break;
                case EActivation.GRIP:
                    if (bAdd)
                        OnGripDown += action;
                    else
                        OnGripDown -= action;
                    break;
            }
        }
        public void ActivationUpCallback(EActivation activation, Action<VRRayCaster> action, bool bAdd = true)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    if (bAdd)
                        OnTouchUp += action;
                    else
                        OnTouchUp -= action;
                    break;
                case EActivation.TOUCHPAD:
                    if (bAdd)
                        OnTouchpadUp += action;
                    else
                        OnTouchpadUp -= action;
                    break;
                case EActivation.TRIGGER:
                    if (bAdd)
                        OnTriggerUp += action;
                    else
                        OnTriggerUp -= action;
                    break;
                case EActivation.GRIP:
                    if (bAdd)
                        OnGripUp += action;
                    else
                        OnGripUp -= action;
                    break;
            }
        }

        // Utility for other classes to get the current interactive item
        public VRInteractiveItem CurrentInteractible
        {
            get { return m_CurrentInteractible; }
        }

        // We subscribe to our inputs callbacks in order
        // to let our interactibles know what's up.
        protected abstract void setCallbacks();
        protected abstract void clearCallbacks();

        protected void _onRaycastHit(RaycastHit hit)
        {
            if (OnRaycasthit != null)
                OnRaycasthit(hit);
        }

        protected abstract void deactiveLastInteractible();

        // We might not want to interact with items while grabbing
        protected bool shouldInteract { get { return VRCapabilityManager.canInteractWhileGrabbing || !isGrabbing; } }

        // Touch up
        public Action<VRRayCaster> OnTouchUp;
        protected void HandleTouchUp(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.TouchUp(this);

            if (OnTouchUp != null)
                OnTouchUp(this);
        }

        // Touch down
        public Action<VRRayCaster> OnTouchDown;
        protected void HandleTouchDown(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.TouchDown(this);

            if (OnTouchDown != null)
                OnTouchDown(this);
        }

        // Touchpad up
        public Action<VRRayCaster> OnTouchpadUp;
        protected void HandleTouchpadUp(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.TouchpadUp(this);

            if (OnTouchpadUp != null)
                OnTouchpadUp(this);
        }

        // Touchpad down
        public Action<VRRayCaster> OnTouchpadDown;
        protected void HandleTouchpadDown(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.TouchpadDown(this);

            if (OnTouchpadDown != null)
                OnTouchpadDown(this);
        }

        // Trigger up
        public Action<VRRayCaster> OnTriggerUp;
        protected void HandleTriggerUp(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.TriggerUp(this);

            if (OnTriggerUp != null)
                OnTriggerUp(this);
        }

        // Trigger down
        public Action<VRRayCaster> OnTriggerDown;
        protected void HandleTriggerDown(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.TriggerDown(this);

            if (OnTriggerDown != null)
                OnTriggerDown(this);
        }

        // Grip Up
        public Action<VRRayCaster> OnGripUp;
        protected void HandleGripUp(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.GripUp(this);

            if (OnGripUp != null)
                OnGripUp(this);
        }
        // Grip down
        public Action<VRRayCaster> OnGripDown;
        protected void HandleGripDown(VRInput input)
        {
            if (shouldInteract && m_CurrentInteractible != null)
                m_CurrentInteractible.GripDown(this);

            if (OnGripDown != null)
                OnGripDown(this);
        }
        
        public RaycastHit CurrentHit { get { return m_CurrentHit; } }
        public GameObject CurrentHitObject { get { return m_CurrentHit.collider ? m_CurrentHit.collider.gameObject : null; } }
        public Vector3 CurrentHitPosition { get { return m_CurrentHit.point; } }
        public Vector3 GetLastHitPosition()
        {
            return m_CurrentHit.point;
        }
        public Quaternion GetLastHitAngle()
        {
            return Quaternion.FromToRotation(Vector3.forward, m_CurrentHit.normal);
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
                m_VrInput._SetRayCaster(this);

            if (isActiveAndEnabled)
            {
                setCallbacks();
            }

            if (FromTransform == null)
                FromTransform = transform;
        }

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

        protected virtual void Update()
        {
            // Check for activation
            if (isRayCasting)
            {
                doRaycast();
            }
            // If we aren't raycasting then clear our interaction state
            else
            {
                deactiveLastInteractible();
                m_CurrentInteractible = null;
                m_CurrentHit = new RaycastHit();
            }
        }

        // Grabbable stuff
        public event Action<Grabbable> OnGrab;                      // Called when the input grabs a wrapVR.Grabbable
        public event Action<Grabbable> OnRelease;                   // Called when the input releases a wrapVR.Grabbable

        public Grabbable currentGrabbable { get; protected set; }
        public bool isGrabbing { get { return currentGrabbable != null; } }
        public void _onGrab(Grabbable g)
        {
            currentGrabbable = g;
            if (OnGrab != null)
                OnGrab(g);
        }
        public void _onRelease(Grabbable g)
        {
            currentGrabbable = null;
            if (OnRelease != null)
                OnRelease(g);
        }

        protected abstract void doRaycast();
    }
}