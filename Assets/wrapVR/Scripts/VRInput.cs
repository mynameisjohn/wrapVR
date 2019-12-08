using System;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // This class encapsulates all the input required for most VR games.
    // It has events that can be subscribed to by classes that need specific input.
    // This class must exist in every scene and so can be attached to the main
    // camera for ease.
    using VRInputAction = Action<VRInput>;
    public class VRInput : MonoBehaviour
    {
        // Input type
        public InputType Type;
        
        // Double click time
        public float DoubleClickTime = 0.3f;    //The max time allowed between double clicks
        
        public event Action<VRInput, SwipeDirection> OnSwipe;      // Called when a swipe is detected.
        public event VRInputAction OnTriggerDown;                  // Called when PrimaryIndexTrigger is pressed.
        public event VRInputAction OnTriggerUp;                    // Called when PrimaryIndexTrigger is released.
        public event VRInputAction OnGripDown;                     // Called when PrimaryHandTrigger is pressed.
        public event VRInputAction OnGripUp;                       // Called when PrimaryHandTrigger is released.
        public event VRInputAction OnTouchpadDown;                 // Called when PrimaryTouchpad is pressed.
        public event VRInputAction OnTouchpadUp;                   // Called when PrimaryTouchpad is released.
        public event VRInputAction OnTouchDown;                    // Called when PrimaryTouchpad is touched.
        public event VRInputAction OnTouchUp;                      // Called when PrimaryTouchpad is untouched.
        public event VRInputAction OnMenuDown;                     // Called when the Menu BUtton is pressed.
        public event VRInputAction OnMenuUp;                       // Called when the Menu BUtton is released.

        public VRInputAction GetActivationUp(EActivation activation)
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

        public VRInputAction GetActivationDown(EActivation activation)
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

        public void ActivationDownCallback(EActivation activation, VRInputAction action, bool bAdd = true)
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

        public void ActivationUpCallback(EActivation activation, VRInputAction action, bool bAdd = true)
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

        protected float m_TouchTime;
        protected float m_InitTouchPosX;
        protected float m_MostRecentTouchPosX;
        protected float m_InitTouchPosY;
        protected float m_MostRecentTouchPosY;
        protected float m_SwipeTimeOut = 1f;

        protected VRRayCaster m_Caster;
        public VRRayCaster Caster { get { return m_Caster; } }
        public void _SetRayCaster(VRRayCaster c)
        {
            m_Caster = c;
        }
        
        protected void _onSwipe(SwipeDirection dir)
        {
            if (OnSwipe != null)
                OnSwipe(this, dir);
        }

        protected void _onGripDown()
        {
            if (OnGripDown != null)
                OnGripDown(this);
        }
        protected void _onGripUp()
        {
            if (OnGripUp != null)
                OnGripUp(this);
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
                OnTriggerDown(this);

            translateMobileGrip(EActivation.TRIGGER, true);
        }
        protected void _onTriggerUp()
        {
            if (OnTriggerUp != null)
                OnTriggerUp(this);

            translateMobileGrip(EActivation.TRIGGER, false);
        }

        protected void _onTouchpadDown()
        {
            if (OnTouchpadDown != null)
                OnTouchpadDown(this);

            translateMobileGrip(EActivation.TOUCHPAD, true);
        }
        protected void _onTouchpadUp()
        {
            if (OnTouchpadUp != null)
                OnTouchpadUp(this);

            translateMobileGrip(EActivation.TOUCHPAD, false);
        }

        // For swipe we only allow one per touch down
        // Therefore cache if we allow swipe 
        bool m_bSwipedX, m_bSwipedY;
        protected void _onTouchDown()
        {
            m_bSwipedX = false;
            m_bSwipedY = false;

            if (OnTouchDown != null)
                OnTouchDown(this);

            translateMobileGrip(EActivation.TOUCH, true);
        }
        protected void _onTouchUp()
        {
            if (OnTouchUp != null)
                OnTouchUp(this);

            translateMobileGrip(EActivation.TOUCH, false);
        }

        public void _onMenuDown()
        {
            if (OnMenuDown != null)
                OnMenuDown(this);
        }

        public void _onMenuUp()
        {
            if (OnMenuUp != null)
                OnMenuUp(this);
        }

        private void Update()
        {
            // Don't check input if we're gaze and not gaze fallback ?
            // This is a bit confusing...
            if (!HardwareExists())
                return;

            if (Type == InputType.GAZE && VRCapabilityManager.IsGazeFallback)
                CheckInput();
            else if (Type != InputType.GAZE && !VRCapabilityManager.IsGazeFallback)
                CheckInput();
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

        // Swipe detection logic
        // Use the touch time and X/Y delta to check for swipes in that direction
        protected SwipeDirection detectSwipeX()
        {
            if (Time.time - m_TouchTime > m_SwipeTimeOut)
                return SwipeDirection.NONE;
            float fDX = m_MostRecentTouchPosX - m_InitTouchPosX;
            if (fDX > .5f)
                return SwipeDirection.RIGHT;
            else if (fDX < -0.5f)
                return SwipeDirection.LEFT;
            return SwipeDirection.NONE;
        }
        protected SwipeDirection detectSwipeY()
        {
            if (Time.time - m_TouchTime > m_SwipeTimeOut)
                return SwipeDirection.NONE;
            float fDY = m_MostRecentTouchPosY - m_InitTouchPosY;
            if (fDY > .5f)
                return SwipeDirection.UP;
            else if (fDY < -0.5f)
                return SwipeDirection.DOWN;
            return SwipeDirection.NONE;
        }

        // Detect swipe in either direction and cache
        // that we've swiped - we clear that each time
        // a touch down is detected and allow one swipe per touch down
        // (this prevents multiple swipes being detected within the interval)
        protected bool detectAndHandleSwipe()
        {
            bool bSwipe = false;
            if (!m_bSwipedX)
            {
                SwipeDirection dirX = detectSwipeX();
                if (dirX != SwipeDirection.NONE)
                {
                    _onSwipe(dirX);
                    bSwipe = true;
                    m_bSwipedX = true;
                }
            }
            if (!m_bSwipedY)
            {
                SwipeDirection dirY = detectSwipeY();
                if (dirY != SwipeDirection.NONE)
                {
                    _onSwipe(dirY);
                    bSwipe = true;
                    m_bSwipedY = true;
                }
            }

            return bSwipe;
        }

        public virtual bool HardwareExists() { return true; }
    }
}