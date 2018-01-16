#if WRAPVR_OCULUS
using System;
using System.Collections.Generic;
using UnityEngine;

// This class encapsulates all the input required for most VR games.
// It has events that can be subscribed to by classes that need specific input.
// This class must exist in every scene and so can be attached to the main
// camera for ease.
namespace wrapVR
{
    public class OculusVRInput : VRInput
    {
        private void Start()
        {
            OVRTouchpad.Create();
            OVRTouchpad.TouchHandler += HandleTouchHandler;
        }
        protected override void CheckInput()
        {
            // For Gaze Fallback we use the touchpad on the HMD. 
            if (VRCapabilityManager.IsGazeFallback)
            {
                if (OVRInput.GetDown(OVRInput.Button.DpadRight))
                {
                    _onSwipe(SwipeDirection.RIGHT);
                }
                if (OVRInput.GetDown(OVRInput.Button.DpadLeft))
                {
                    _onSwipe(SwipeDirection.LEFT);
                }
                if (OVRInput.GetDown(OVRInput.Button.DpadUp))
                {
                    _onSwipe(SwipeDirection.UP);
                }
                if (OVRInput.GetDown(OVRInput.Button.DpadDown))
                {
                    _onSwipe(SwipeDirection.DOWN);
                }
            }
            else
            {
                if (OVRInput.Get(OVRInput.Touch.PrimaryTouchpad))
                {
                    m_MostRecentTouchPosX = Util.remap(OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad).x, -1f, 1f, 0f, 1f);
                    m_MostRecentTouchPosY = Util.remap(OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad).y, -1f, 1f, 0f, 1f);
                }
                if (OVRInput.GetDown(OVRInput.Touch.PrimaryTouchpad))
                {
                    m_TouchTime = Time.time;
                    m_InitTouchPosX = Util.remap(OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad).x, -1f, 1f, 0f, 1f);
                    m_InitTouchPosY = Util.remap(OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad).y, -1f, 1f, 0f, 1f);
                }
            }

            // If not gaze fallback handle mouse or trigger messages
            if (!VRCapabilityManager.IsGazeFallback)
            {
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
                {
                    _onTriggerDown();
                }
                if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger))
                {
                    _onTriggerUp();
                }
            }
            if (OVRInput.GetDown(OVRInput.Touch.PrimaryTouchpad))
            {
                _onTouchpadTouchDown();
            }
            if (OVRInput.GetUp(OVRInput.Touch.PrimaryTouchpad))
            {
                _onTouchpadTouchUp();
            }
            if (OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad))
            {
                _onTouchpadDown();
                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerDown();
            }
            if (OVRInput.GetUp(OVRInput.Button.PrimaryTouchpad))
            {
                _onTouchpadUp();
                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerUp();
            }
        }
        public override Vector2 GetTouchPosition()
        {
            return OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
        }

        protected override void HandleTouchHandler(object sender, System.EventArgs e)
        {
            OVRTouchpad.TouchArgs touchArgs = (OVRTouchpad.TouchArgs)e;
            if (Time.time - m_TouchTime < m_SwipeTimeOut && Mathf.Abs(m_InitTouchPosX - m_MostRecentTouchPosX) > .5f)
            {
                if (touchArgs.TouchType == OVRTouchpad.TouchEvent.Right)
                {
                    _onSwipe(SwipeDirection.RIGHT);
                }
                else if (touchArgs.TouchType == OVRTouchpad.TouchEvent.Left)
                {
                    _onSwipe(SwipeDirection.LEFT);
                }
            }
            if (Time.time - m_TouchTime < m_SwipeTimeOut && Mathf.Abs(m_InitTouchPosY - m_MostRecentTouchPosY) > .5f)
            {
                if (touchArgs.TouchType == OVRTouchpad.TouchEvent.Up)
                {
                    _onSwipe(SwipeDirection.UP);
                }
                else if (touchArgs.TouchType == OVRTouchpad.TouchEvent.Down)
                {
                    _onSwipe(SwipeDirection.DOWN);
                }
            }
        }
        public override bool GetTrigger()
        {
            // If gaze fallback count touch as trigger
            if (VRCapabilityManager.IsGazeFallback)
            {
                return GetTouchpadTouch();
            }
            else
            {
                return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger);
            }
        }
        public override bool GetTouchpadTouch()
        {
            return OVRInput.Get(OVRInput.Touch.PrimaryTouchpad);
        }
        public override bool GetTouchpad()
        {
            return OVRInput.Get(OVRInput.Button.PrimaryTouchpad);
        }
        public override SwipeDirection GetHMDTouch()
        {
            if (OVRInput.Get(OVRInput.Button.DpadRight))
            {
                return SwipeDirection.RIGHT;
            }
            else if (OVRInput.Get(OVRInput.Button.DpadLeft))
            {
                return SwipeDirection.LEFT;
            }
            else if (OVRInput.Get(OVRInput.Button.DpadUp))
            {
                return SwipeDirection.UP;
            }
            else if (OVRInput.Get(OVRInput.Button.DpadDown))
            {
                return SwipeDirection.DOWN;
            }
            else
            {
                return SwipeDirection.NONE;
            }
        }

        public override bool HardwareExists()
        {
            switch (Type)
            {
                case InputType.GAZE:
                    return true;
                case InputType.RIGHT:
                    return OVRInput.IsControllerConnected(OVRInput.Controller.RTrackedRemote);
                case InputType.LEFT:
                    return OVRInput.IsControllerConnected(OVRInput.Controller.LTrackedRemote);
                default:
                    return false;
            }
        }
    }
}
#endif