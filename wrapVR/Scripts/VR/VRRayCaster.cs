using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace wrapVR
{
    public abstract class VRRayCaster : MonoBehaviour
    {
        public LayerMask _ExclusionLayers;           // Layers to exclude from the raycast.
        public bool _ShowDebugRay;                   // Optionally show the debug ray.
        public float _DebugRayLength = 5f;           // Debug ray length.
        public float _DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        public float _RayLength = 500f;              // How far into the scene the ray is cast.
        public bool _Log = false;

        [Tooltip("Where we starting casting from (self if null)")]
        public Transform FromTransform;

        protected VRInput _vrInput;                // Used to call input based events on the current VRInteractiveItem.


        public event Action<RaycastHit> OnRaycasthit;                   // This event is called every frame that the user's gaze is over a collider.
        protected VRInteractiveItem _currentInteractible;                //The current interactive item
        protected VRInteractiveItem _lastInteractible;                   //The last interactive item
        protected RaycastHit _currentHit;

        // Turn on if the raycaster should only hit navmeshes
        public bool _ForNavMesh = false;
        public float _NavMeshSampleDistance;
        public int _NavMeshAreaFilter = UnityEngine.AI.NavMesh.AllAreas;

        public Transform inputTransform { get { return _vrInput.transform; } }

        public EActivation activation { get; set; }
        public bool isRayCasting
        {
            get
            {
#if UNITY_EDITOR
                if (VRCapabilityManager.sdkType == ESDK.Editor && !((EditorVRInput)Input).IsHandActive)
                    return false;
#endif
                return IsActivationDown(activation);
            }
        }

        public bool isTouchDown { get { return _vrInput.GetTouch(); } }
        public bool isTouchpadDown { get { return _vrInput.GetTouchpad(); } }
        public bool isTriggerDown { get { return _vrInput.GetTrigger(); } }
        public bool isGripDown { get { return _vrInput.GetGrip(); } }

        public Vector2 touchPos { get { return _vrInput.GetTouchPosition(); } }

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
            get { return _currentInteractible; }
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
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.TouchUp(this);

            if (OnTouchUp != null)
                OnTouchUp(this);
        }

        // Touch down
        public Action<VRRayCaster> OnTouchDown;
        protected void HandleTouchDown(VRInput input)
        {
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.TouchDown(this);

            if (OnTouchDown != null)
                OnTouchDown(this);
        }

        // Touchpad up
        public Action<VRRayCaster> OnTouchpadUp;
        protected void HandleTouchpadUp(VRInput input)
        {
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.TouchpadUp(this);

            if (OnTouchpadUp != null)
                OnTouchpadUp(this);
        }

        // Touchpad down
        public Action<VRRayCaster> OnTouchpadDown;
        protected void HandleTouchpadDown(VRInput input)
        {
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.TouchpadDown(this);

            if (OnTouchpadDown != null)
                OnTouchpadDown(this);
        }

        // Trigger up
        public Action<VRRayCaster> OnTriggerUp;
        protected void HandleTriggerUp(VRInput input)
        {
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.TriggerUp(this);

            if (OnTriggerUp != null)
                OnTriggerUp(this);
        }

        // Trigger down
        public Action<VRRayCaster> OnTriggerDown;
        protected void HandleTriggerDown(VRInput input)
        {
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.TriggerDown(this);

            if (OnTriggerDown != null)
                OnTriggerDown(this);
        }

        // Grip Up
        public Action<VRRayCaster> OnGripUp;
        protected void HandleGripUp(VRInput input)
        {
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.GripUp(this);

            if (OnGripUp != null)
                OnGripUp(this);
        }
        // Grip down
        public Action<VRRayCaster> OnGripDown;
        protected void HandleGripDown(VRInput input)
        {
            if (shouldInteract && _currentInteractible != null)
                _currentInteractible.GripDown(this);

            if (OnGripDown != null)
                OnGripDown(this);
        }
        
        public RaycastHit CurrentHit { get { return _currentHit; } }
        public GameObject CurrentHitObject { get { return _currentHit.collider ? _currentHit.collider.gameObject : null; } }
        public Vector3 CurrentHitPosition { get { return _currentHit.point; } }
        public Vector3 GetLastHitPosition()
        {
            return _currentHit.point;
        }
        public Quaternion GetLastHitAngle()
        {
            return Quaternion.FromToRotation(Vector3.forward, _currentHit.normal);
        }

        public void SetInput(VRInput input)
        {
            // Unlikely, but if we weren't null and are enabled 
            // then unsubscribe and resubscribe later
            if (isActiveAndEnabled)
            {
                clearCallbacks();
            }

            _vrInput = input;
            if(_vrInput != null)
                _vrInput._SetRayCaster(this);

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

            _lastInteractible = _currentInteractible;
            _currentInteractible = null;
        }

        public VRInput Input { get { return _vrInput; } }
        public bool HasInput()
        {
            return _vrInput != null && _vrInput.HardwareExists();
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
                _currentInteractible = null;
                _currentHit = new RaycastHit();
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