using System;
using System.Collections.Generic;
using UnityEngine;

// This class encapsulates all the input required for most VR games.
// It has events that can be subscribed to by classes that need specific input.
// This class must exist in every scene and so can be attached to the main
// camera for ease.
namespace wrapVR
{
    public class OculusVRInput
#if WRAPVR_OCULUS
        : VRInput
    {
        // These are platform specific - 
        // If GearVR they are values for the remote, 
        // and for the rift they vary from left to right
        OVRInput.Controller m_eController;
        OVRInput.Axis2D m_AxisThumb;
        OVRInput.Touch m_TouchThumb;
        OVRInput.Button m_ButtonThumb;
        OVRInput.Button m_IndexTrigger;

        private void Start()
        {
            OVRTouchpad.Create();
#if UNITY_ANDROID
            // Use the GearVR values
            m_eController = new Dictionary<InputType, OVRInput.Controller> {
                { InputType.GAZE, OVRInput.Controller.None },
                { InputType.LEFT, OVRInput.Controller.LTrackedRemote },
                { InputType.RIGHT, OVRInput.Controller.RTrackedRemote },
            }[Type];
            m_AxisThumb = OVRInput.Axis2D.PrimaryTouchpad;
            m_TouchThumb = OVRInput.Touch.PrimaryTouchpad;
            m_ButtonThumb = OVRInput.Button.PrimaryTouchpad;
            m_IndexTrigger = OVRInput.Button.PrimaryIndexTrigger;

            // What does this do?
            OVRTouchpad.TouchHandler += HandleTouchHandler;
#else
            // Get enum values based on which controller we are
            // https://docs.unity3d.com/Manual/OculusControllers.html
            switch (Type)
            {
                case InputType.GAZE:
                    // ?
                    break;
                case InputType.LEFT:
                    m_eController = OVRInput.Controller.LTouch;
                    m_AxisThumb = OVRInput.Axis2D.PrimaryThumbstick;
                    m_TouchThumb = OVRInput.Touch.PrimaryThumbstick;
                    m_ButtonThumb = OVRInput.Button.PrimaryThumbstick;
                    m_IndexTrigger = OVRInput.Button.PrimaryIndexTrigger;
                    break;
                case InputType.RIGHT:
                    m_eController = OVRInput.Controller.RTouch;
                    m_AxisThumb = OVRInput.Axis2D.SecondaryThumbstick;
                    m_TouchThumb = OVRInput.Touch.SecondaryThumbstick;
                    m_ButtonThumb = OVRInput.Button.SecondaryThumbstick;
                    m_IndexTrigger = OVRInput.Button.SecondaryIndexTrigger;
                    break;
            }
#endif
        }

        protected override void CheckInput()
        {
#if UNITY_ANDROID
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
#endif
            // Retreive thumb pos if we're touching
            if (OVRInput.Get(m_TouchThumb))
            {
                Vector2 v2ThumbPos = GetTouchPosition();
                m_MostRecentTouchPosX = v2ThumbPos.x;
                m_MostRecentTouchPosY = v2ThumbPos.y;
            }

            // Detect initial positions if down
            if (OVRInput.GetDown(m_TouchThumb))
            {
                m_TouchTime = Time.time;
                m_InitTouchPosX = 0f;
                m_InitTouchPosY = 0f;

                // Send on down message
                _onTouchpadTouchDown();
                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerDown();
            }
            // Otherwise see if we just got thumb up
            else if (OVRInput.GetUp(m_TouchThumb))
            {
                _onTouchpadTouchUp();
                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerUp();
            }

            if (OVRInput.GetDown(m_ButtonThumb))
            {
                _onTouchpadDown();
            }
            else if (OVRInput.GetUp(m_ButtonThumb))
            {
                _onTouchpadUp();
            }

            // Get back to cancel on Android
#if UNITY_ANDROID
            if (OVRInput.GetDown(OVRInput.Button.Back))
                _onCancel();
#endif

            // I'm not sure why but I'm not getting trigger ups
            // on the Gear - may be bug with my SDK...?
#if UNITY_ANDROID

            // Handle left / right triggers
            if (!VRCapabilityManager.IsGazeFallback)
            {
                if (OVRInput.GetDown(m_IndexTrigger))
                {
                    _onTriggerDown();
                }
                if (OVRInput.GetUp(m_IndexTrigger))
                {
                    _onTriggerUp();
                }
            }
#else
            if (!VRCapabilityManager.IsGazeFallback)
            {
                if (OVRInput.GetDown(m_IndexTrigger))
                {
                    _onTriggerDown();
                }
                if (OVRInput.GetUp(m_IndexTrigger))
                {
                    _onTriggerUp();
                }
            }
#endif
        }
        public override Vector2 GetTouchPosition()
        {
            return OVRInput.Get(m_AxisThumb);
        }

        // I don't think this is really necessary - couldn't it be done in CheckInput?
        protected void HandleTouchHandler(object sender, System.EventArgs e)
        {
            OVRTouchpad.TouchArgs touchArgs = (OVRTouchpad.TouchArgs)e;
            detectAndHandleSwipe();
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

        // I'm not even sure if this is a thing on Rift... I think not
        public SwipeDirection GetHMDTouch()
        {
#if UNITY_ANDROID
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
#endif
            return SwipeDirection.NONE;
        }

        public override bool HardwareExists()
        {
            if (Type == InputType.GAZE)
                return true;
            return OVRInput.IsControllerConnected(m_eController);
        }
#else
        : MonoBehaviour
    { 
#endif
        }
}