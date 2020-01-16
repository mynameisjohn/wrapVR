using System;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // This class encapsulates all the input required for most VR games.
    // It has events that can be subscribed to by classes that need specific input.
    // This class must exist in every scene and so can be attached to the main
    // camera for ease.
    public class VRInput : MonoBehaviour
    {
        // Input type
        public InputType Type;
        
        // Double click time
        public float DoubleClickTime = 0.3f;    //The max time allowed between double clicks
        
        public event Action<SwipeDirection> OnSwipe;        // Called when a swipe is detected.
        public event Action OnTriggerDown;                  // Called when PrimaryIndexTrigger is pressed.
        public event Action OnTriggerUp;                    // Called when PrimaryIndexTrigger is released.
        public event Action OnGripDown;                     // Called when PrimaryHandTrigger is pressed.
        public event Action OnGripUp;                       // Called when PrimaryHandTrigger is released.
        public event Action OnTouchpadDown;                 // Called when PrimaryTouchpad is pressed.
        public event Action OnTouchpadUp;                   // Called when PrimaryTouchpad is released.
        public event Action OnTouchDown;                    // Called when PrimaryTouchpad is touched.
        public event Action OnTouchUp;                      // Called when PrimaryTouchpad is untouched.
        public event Action OnMenuDown;                      // Called when the Menu BUtton is pressed.
        public event Action OnMenuUp;                      // Called when the Menu BUtton is released.

        public System.Action GetActivationUp(EActivation activation)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    return OnTouchUp;
                case EActivation.TOUCHPAD:
                    return OnTouchpadUp;
                case EActivation.TRIGGER:
                    return OnTriggerUp;
                case EActivation.GRIP:
                    return OnGripUp;
            }
            return null;
        }

        public System.Action GetActivationDown(EActivation activation)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    return OnTouchDown;
                case EActivation.TOUCHPAD:
                    return OnTouchpadDown;
                case EActivation.TRIGGER:
                    return OnTriggerDown;
                case EActivation.GRIP:
                    return OnGripDown;
            }
            return null;
        }

        public bool IsActivationDown(EActivation activation)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    return GetTouch();
                case EActivation.TOUCHPAD:
                    return GetTouchpad();
                case EActivation.TRIGGER:
                    return GetTrigger();
                case EActivation.GRIP:
                    return GetGrip();
            }
            return false;
        }


        protected VRRayCaster m_Caster;
        public VRRayCaster Caster { get { return m_Caster; } }
        public void _SetRayCaster(VRRayCaster c)
        {
            m_Caster = c;
        }
        
        protected void _onSwipe(SwipeDirection dir)
        {
            if (OnSwipe != null)
                OnSwipe(dir);
        }

        protected void _onGripDown()
        {
            if (OnGripDown != null)
                OnGripDown();
        }
        protected void _onGripUp()
        {
            if (OnGripUp != null)
                OnGripUp();
        }
        void translateMobileGrip(EActivation sourceActivation, bool bDown)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // If we're on mobile and the grip translation is this activation, grip down
            if (VRCapabilityManager.mobileGrip == sourceActivation)
            {
                if (bDown)
                    _onGripDown();
                else
                    _onGripUp();
            }
#endif
        }

        protected void _onTriggerDown()
        {
            if (OnTriggerDown != null)
                OnTriggerDown();

            translateMobileGrip(EActivation.TRIGGER, true);
        }
        protected void _onTriggerUp()
        {
            if (OnTriggerUp != null)
                OnTriggerUp();

            translateMobileGrip(EActivation.TRIGGER, false);
        }

        protected void _onTouchpadDown()
        {
            if (OnTouchpadDown != null)
                OnTouchpadDown();

            translateMobileGrip(EActivation.TOUCHPAD, true);
        }
        protected void _onTouchpadUp()
        {
            if (OnTouchpadUp != null)
                OnTouchpadUp();

            translateMobileGrip(EActivation.TOUCHPAD, false);
        }

        // For swipe we only allow one per touch down
        // Therefore cache if we allow swipe 
        protected void _onTouchDown()
        {
            _lastFrameTouchPos = GetTouchPosition();

            if (OnTouchDown != null)
                OnTouchDown();

            translateMobileGrip(EActivation.TOUCH, true);
        }
        protected void _onTouchUp()
        {
            if (OnTouchUp != null)
                OnTouchUp();

            _lastHorizontalSwipe = SwipeDirection.NONE;
            _lastVerticalSwipe = SwipeDirection.NONE;
            _swipeSampleCount = 0;

            translateMobileGrip(EActivation.TOUCH, false);
        }

        public void _onMenuDown()
        {
            if (OnMenuDown != null)
                OnMenuDown();
        }

        public void _onMenuUp()
        {
            if (OnMenuUp != null)
                OnMenuUp();
        }

        private void Update()
        {
            if (!HardwareExists())
                return;

            // Don't check input if we're gaze and not gaze fallback ?
            // This is a bit confusing...
            if (Type == InputType.GAZE && VRCapabilityManager.IsGazeFallback)
                CheckInput();
            else if (Type != InputType.GAZE && !VRCapabilityManager.IsGazeFallback)
                CheckInput();

            if (_swipeSamples == null)
                _swipeSamples = new Vector2[VRCapabilityManager.swipeSampleCount];
            
            detectAndHandleSwipe();
        }
        private void OnDestroy()
        {
            // Ensure that all events are unsubscribed when this is destroyed.
            OnSwipe = null;
            OnTriggerDown = null;
            OnTriggerUp = null;
            OnTouchpadDown = null;
            OnTouchpadUp = null;
            OnTouchDown = null;
            OnTouchUp = null;
        }

        public float GetTouchAngle()
        {
            Vector2 v2TouchPos = GetTouchPosition();
            return Mathf.Atan2(-v2TouchPos.x, v2TouchPos.y) * Mathf.Rad2Deg;
        }

        public virtual void Init() { }
        protected virtual void CheckInput() { }
        public virtual Vector2 GetTouchPosition() { return Vector2.zero; }
        public virtual bool GetTrigger() { return false; }
        public virtual bool GetTouch() { return false; }
        public virtual bool GetTouchpad() { return false; }
        public virtual bool GetGrip() { return false; }
        public virtual bool GetMenu() { return false; }
        public virtual InputControllerRenderers getController() { return null; }

        int _swipeSampleCount;
        Vector2[] _swipeSamples;
        Vector2 _lastFrameTouchPos;
        SwipeDirection _lastHorizontalSwipe;
        SwipeDirection _lastVerticalSwipe;

        protected bool detectAndHandleSwipe()
        {
            int s = _swipeSampleCount++ % VRCapabilityManager.swipeSampleCount;
            var currentTouch = GetTouchPosition();
            _swipeSamples[s] = (currentTouch - _lastFrameTouchPos) / Time.deltaTime;
            _lastFrameTouchPos = currentTouch;

            if (_swipeSampleCount < VRCapabilityManager.swipeSampleCount)
                return false;

            // average swipe velocity
            Vector2 swipeVel = Vector2.zero;
            foreach (var sample in _swipeSamples)
                swipeVel += sample;
            swipeVel /= VRCapabilityManager.swipeSampleCount;

            // reset these if the touch position is changing direction from the last swipe
            if (_lastHorizontalSwipe == SwipeDirection.LEFT && swipeVel.x > 0)
                _lastHorizontalSwipe = SwipeDirection.NONE;
            else if (_lastHorizontalSwipe == SwipeDirection.RIGHT && swipeVel.x < 0)
                _lastHorizontalSwipe = SwipeDirection.NONE;

            if (_lastVerticalSwipe == SwipeDirection.DOWN && swipeVel.y > 0)
                _lastVerticalSwipe = SwipeDirection.NONE;
            else if (_lastVerticalSwipe == SwipeDirection.UP && swipeVel.y < 0)
                _lastVerticalSwipe = SwipeDirection.NONE;

            // we consider something a swipe if 
            // a) we aren't already swiping in that direction
            // b) the velocity is moving in that direction
            // c) the touch position reflects the swipe direction
            bool bSwipe = false;

            if (Mathf.Abs(swipeVel.x) > VRCapabilityManager.swipeThreshold)
            {
                SwipeDirection horizontalSwipeDir = SwipeDirection.NONE;
                if (_lastHorizontalSwipe != SwipeDirection.RIGHT && swipeVel.x > 0 && currentTouch.x > 0)
                    horizontalSwipeDir = SwipeDirection.RIGHT;
                else if (_lastHorizontalSwipe != SwipeDirection.LEFT && swipeVel.x < 0 && currentTouch.x < 0)
                    horizontalSwipeDir = SwipeDirection.LEFT;

                if (horizontalSwipeDir != SwipeDirection.NONE)
                {
                    _onSwipe(horizontalSwipeDir);
                    _lastHorizontalSwipe = horizontalSwipeDir;
                    bSwipe = true;
                }
            }
            else
                _lastHorizontalSwipe = SwipeDirection.NONE;
            
            if (Mathf.Abs(swipeVel.y) > VRCapabilityManager.swipeThreshold)
            {
                SwipeDirection verticalSwipeDir = SwipeDirection.NONE;
                if (_lastVerticalSwipe != SwipeDirection.UP && swipeVel.y > 0 && currentTouch.y > 0)
                    verticalSwipeDir = SwipeDirection.UP;
                else if (_lastVerticalSwipe != SwipeDirection.DOWN && swipeVel.x < 0 && currentTouch.y < 0)
                    verticalSwipeDir = SwipeDirection.DOWN;

                if (verticalSwipeDir != SwipeDirection.NONE)
                {
                    _onSwipe(verticalSwipeDir);
                    _lastVerticalSwipe = verticalSwipeDir;
                    bSwipe = true;
                }
            }
            else
                _lastVerticalSwipe = SwipeDirection.NONE;

            return bSwipe;
        }

        public virtual bool HardwareExists() { return true; }
        
        public void ActivationDownCallback(EActivation activation, System.Action action, bool bAdd = true)
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
        public void ActivationUpCallback(EActivation activation, Action action, bool bAdd = true)
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
    }
}