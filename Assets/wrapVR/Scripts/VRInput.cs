using System;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // This class encapsulates all the input required for most VR games.
    // It has events that can be subscribed to by classes that need specific input.
    // This class must exist in every scene and so can be attached to the main
    // camera for ease.
    public abstract class VRInput : MonoBehaviour
    {
        // Input type
        public InputType Type;

        public event Action<SwipeDirection> OnSwipe;                // Called when a swipe is detected.
        public event Action OnClick;                                // Called when Fire1 is released and it's not a double click.
        public event Action OnDown;                                 // Called when Fire1 is pressed.
        public event Action OnUp;                                   // Called when Fire1 is released.
        public event Action OnTriggerDown;                          // Called when PrimaryIndexTrigger is pressed.
        public event Action OnTriggerUp;                            // Called when PrimaryIndexTrigger is released.
        public event Action OnTouchpadDown;                         // Called when PrimaryTouchpad is pressed.
        public event Action OnTouchpadUp;                           // Called when PrimaryTouchpad is released.
        public event Action OnTouchpadTouchDown;                    // Called when PrimaryTouchpad is touched.
        public event Action OnTouchpadTouchUp;                      // Called when PrimaryTouchpad is untouched.
        public event Action OnDoubleClick;                          // Called when a double click is detected.
        public event Action OnCancel;                               // Called when Cancel is pressed.

        public System.Action GetActivationUp(EActivation activation)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    return OnTouchpadTouchUp;
                case EActivation.TOUCHPAD:
                    return OnTouchpadUp;
                case EActivation.TRIGGER:
                    return OnTriggerUp;
            }
            return null;
        }

        public System.Action GetActivationDown(EActivation activation)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    return OnTouchpadTouchDown;
                case EActivation.TOUCHPAD:
                    return OnTouchpadDown;
                case EActivation.TRIGGER:
                    return OnTriggerDown;
            }
            return null;
        }

        public bool IsActivationDown(EActivation activation)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    return GetTouchpadTouch();
                case EActivation.TOUCHPAD:
                    return GetTouchpad();
                case EActivation.TRIGGER:
                    return GetTrigger();
            }
            return false;
        }

        [SerializeField] protected float m_DoubleClickTime = 0.3f;    //The max time allowed between double clicks

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

        public float DoubleClickTime{ get { return m_DoubleClickTime; } }

        protected void _onSwipe(SwipeDirection dir)
        {
            if (OnSwipe != null)
                OnSwipe(dir);
        }
        protected void _onTriggerDown()
        {
            if (OnTriggerDown != null)
                OnTriggerDown();
        }
        protected void _onTriggerUp()
        {
            if (OnTriggerUp != null)
                OnTriggerUp();
        }
        protected void _onTouchpadDown()
        {
            if (OnTouchpadDown != null)
                OnTouchpadDown();
        }
        protected void _onTouchpadUp()
        {
            if (OnTouchpadUp != null)
                OnTouchpadUp();
        }

        protected void _onTouchpadTouchDown()
        {
            if (OnTouchpadTouchDown != null)
                OnTouchpadTouchDown();
        }
        protected void _onTouchpadTouchUp()
        {
            if (OnTouchpadTouchUp != null)
                OnTouchpadTouchUp();
        }

        public void _onCancel()
        {
            if (OnCancel != null)
                OnCancel();
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
            OnClick = null;
            OnDoubleClick = null;
            OnDown = null;
            OnUp = null;
            OnTriggerDown = null;
            OnTriggerUp = null;
            OnTouchpadDown = null;
            OnTouchpadUp = null;
            OnTouchpadTouchDown = null;
            OnTouchpadTouchUp = null;
        }

        public float GetTouchAngle()
        {
            Vector2 v2TouchPos = GetTouchPosition();
            return Mathf.Atan2(-v2TouchPos.x, v2TouchPos.y) * Mathf.Rad2Deg;
        }
        protected abstract void CheckInput();
        public abstract Vector2 GetTouchPosition();
        protected abstract void HandleTouchHandler(object sender, System.EventArgs e);
        public abstract bool GetTrigger();
        public abstract bool GetTouchpadTouch();
        public abstract bool GetTouchpad();
        public abstract SwipeDirection GetHMDTouch();

        public virtual bool HardwareExists() { return true; }


        public void ActivationDownCallback(EActivation activation, System.Action action, bool bAdd)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    if (bAdd)
                        OnTouchpadTouchDown += action;
                    else
                        OnTouchpadTouchDown -= action;
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
            }
        }
        public void ActivationUpCallback(EActivation activation, Action action, bool bAdd)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    if (bAdd)
                        OnTouchpadTouchUp += action;
                    else
                        OnTouchpadTouchUp -= action;
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
            }
        }
    }
}