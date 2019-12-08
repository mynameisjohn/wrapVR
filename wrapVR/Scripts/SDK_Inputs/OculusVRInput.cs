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
#if WRAPVR_OCULUS
        // These are platform specific - 
        // If GearVR they are values for the remote, 
        // and for the rift they vary from left to right
        public static OVRPlugin.SystemHeadset headsetType { get; private set; }
        OVRInput.Controller _controllerType;
        OVRInput.Axis2D _axisThumb;
        OVRInput.Touch _touchThumb;
        OVRInput.Button _buttonThumb;
        OVRInput.Button _indexTrigger;
        OVRInput.Button _menuButton;
        OVRInput.Button _handTrigger;  // touch only

        public static bool isLikeGearVR
        {
            get
            {
                switch (headsetType)
                {
                    case OVRPlugin.SystemHeadset.GearVR_R320:
                    case OVRPlugin.SystemHeadset.GearVR_R321:
                    case OVRPlugin.SystemHeadset.GearVR_R322:
                    case OVRPlugin.SystemHeadset.GearVR_R323:
                    case OVRPlugin.SystemHeadset.GearVR_R324:
                    case OVRPlugin.SystemHeadset.GearVR_R325:
                    case OVRPlugin.SystemHeadset.Oculus_Go:
                        return true;
                    case OVRPlugin.SystemHeadset.Oculus_Quest:
                    case OVRPlugin.SystemHeadset.Rift_CV1:
                    case OVRPlugin.SystemHeadset.Rift_S:
                        return false;
                    default:
#if !UNITY_EDITOR && UNITY_ANDROID
                    return true;
#else
                        return false;
#endif
                }
            }
        }

        public static bool isOculusQuest { get { return headsetType == OVRPlugin.SystemHeadset.Oculus_Quest; } }
        public static bool isOculusGO { get { return headsetType == OVRPlugin.SystemHeadset.Oculus_Go; } }

        public override void Init()
        {
            headsetType = OVRPlugin.GetSystemHeadsetType();

            if (isLikeGearVR)
            {
                _controllerType = new Dictionary<InputType, OVRInput.Controller> {
                    { InputType.GAZE, OVRInput.Controller.None },
                    { InputType.LEFT,  OVRInput.Controller.LTrackedRemote  },
                    { InputType.RIGHT, OVRInput.Controller.RTrackedRemote  },
                }[Type];
                _axisThumb = OVRInput.Axis2D.PrimaryTouchpad;
                _touchThumb = OVRInput.Touch.PrimaryTouchpad;
                _buttonThumb = OVRInput.Button.PrimaryTouchpad;
                _indexTrigger = OVRInput.Button.PrimaryIndexTrigger;
                _menuButton = OVRInput.Button.Two;
                _handTrigger = OVRInput.Button.None;
            }
            else
            {
                _controllerType = new Dictionary<InputType, OVRInput.Controller> {
                    { InputType.GAZE, OVRInput.Controller.None },
                    { InputType.LEFT,  OVRInput.Controller.LTouch },
                    { InputType.RIGHT, OVRInput.Controller.RTouch },
                }[Type];
                _axisThumb = OVRInput.Axis2D.PrimaryThumbstick;
                _touchThumb = OVRInput.Touch.PrimaryThumbstick;
                _buttonThumb = OVRInput.Button.PrimaryThumbstick;
                _indexTrigger = OVRInput.Button.PrimaryIndexTrigger;
                _menuButton = OVRInput.Button.Start;
                _handTrigger = OVRInput.Button.PrimaryHandTrigger;
            }
        }

        public override InputControllerRenderers getController()
        {
            if (Type == InputType.GAZE)
                return null;

            var controllerHelper = GetComponentInChildren<OVRControllerHelper>();
            if (controllerHelper == null)
                return null;

            GameObject controller;
            switch (headsetType)
            {
                case OVRPlugin.SystemHeadset.Oculus_Go:
                    controller = controllerHelper.m_modelOculusGoController;
                    break;
                case OVRPlugin.SystemHeadset.Oculus_Quest:
                case OVRPlugin.SystemHeadset.Rift_S:
                    if (Type == InputType.LEFT)
                        controller = controllerHelper.m_modelOculusTouchQuestAndRiftSLeftController;
                    else
                        controller = controllerHelper.m_modelOculusTouchQuestAndRiftSRightController;
                    break;
                case OVRPlugin.SystemHeadset.Rift_CV1:
                    if (Type == InputType.LEFT)
                        controller = controllerHelper.m_modelOculusTouchRiftLeftController;
                    else
                        controller = controllerHelper.m_modelOculusTouchRiftRightController;
                    break;
                case OVRPlugin.SystemHeadset.GearVR_R320:
                case OVRPlugin.SystemHeadset.GearVR_R321:
                case OVRPlugin.SystemHeadset.GearVR_R322:
                case OVRPlugin.SystemHeadset.GearVR_R323:
                case OVRPlugin.SystemHeadset.GearVR_R324:
                case OVRPlugin.SystemHeadset.GearVR_R325:
                    controller = controllerHelper.m_modelGearVrController;
                    break;
                default:
#if UNITY_EDITOR || !UNITY_ANDROID
                    if (Type == InputType.LEFT)
                        controller = controllerHelper.m_modelOculusTouchRiftLeftController;
                    else
                        controller = controllerHelper.m_modelOculusTouchRiftRightController;
#else
                    controller = controllerHelper.m_modelGearVrController;
#endif
                    break;
            }

            return controller.GetComponent<InputControllerRenderers>();
        }

        protected override void CheckInput()
        {
            if (isLikeGearVR && VRCapabilityManager.IsGazeFallback)
            {
                // For Gaze Fallback we use the touchpad on the HMD. 
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

            // Retreive thumb pos if we're touching
            if (OVRInput.Get(_touchThumb, _controllerType))
            {
                Vector2 v2ThumbPos = GetTouchPosition();
                _lastTouchPosX = v2ThumbPos.x;
                _lastTouchPosY = v2ThumbPos.y;
            }

            // Detect initial positions if down
            if (OVRInput.GetDown(_touchThumb, _controllerType))
            {
                _touchTime = Time.time;
                _initTouchPosX = 0f;
                _initTouchPosY = 0f;

                // Send on down message
                _onTouchDown();

                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerDown();
            }
            // Otherwise see if we just got thumb up
            else if (OVRInput.GetUp(_touchThumb, _controllerType))
            {
                _onTouchUp();
                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerUp();
            }

            if (OVRInput.GetDown(_buttonThumb, _controllerType))
            {
                _onTouchpadDown();
            }
            else if (OVRInput.GetUp(_buttonThumb, _controllerType))
            {
                _onTouchpadUp();
            }

            if (OVRInput.GetDown(_menuButton, _controllerType))
                _onMenuDown();
            if (OVRInput.GetUp(_menuButton, _controllerType))
                _onMenuUp();

            // I'm not sure why but I'm not getting trigger ups
            // on the Gear - may be bug with my SDK...?

            // Handle left / right triggers
            if (isLikeGearVR && VRCapabilityManager.IsGazeFallback)
                return;

            if (OVRInput.GetDown(_indexTrigger, _controllerType))
            {
                _onTriggerDown();
            }
            if (OVRInput.GetUp(_indexTrigger, _controllerType))
            {
                _onTriggerUp();
            }
            if (OVRInput.GetDown(_handTrigger, _controllerType))
            {
                _onGripDown();
            }
            if (OVRInput.GetUp(_handTrigger, _controllerType))
            {
                _onGripUp();
            }
        }

        public override Vector2 GetTouchPosition()
        {
            return OVRInput.Get(_axisThumb, _controllerType);
        }

        // I don't think this is really necessary - couldn't it be done in CheckInput?
        protected void HandleTouchHandler(object sender, System.EventArgs e)
        {
            detectAndHandleSwipe();
        }

        public override bool GetGrip()
        {
            if (isLikeGearVR)
            {
                switch (VRCapabilityManager.mobileGrip)
                {
                    case EActivation.TOUCH:
                        return GetTouch();
                    case EActivation.TOUCHPAD:
                        return GetTouchpad();
                    case EActivation.TRIGGER:
                        return GetTrigger();
                };
                return false;
            }

            return OVRInput.Get(_handTrigger, _controllerType);
        }

        public override bool GetTrigger()
        {
            // If gaze fallback count touch as trigger
            if (VRCapabilityManager.IsGazeFallback)
            {
                return GetTouch();
            }
            else
            {
                return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, _controllerType);
            }
        }

        public override bool GetTouch()
        {
            return OVRInput.Get(_touchThumb, _controllerType);
        }
        public override bool GetTouchpad()
        {
            // If gaze fallback count touch as trigger
            if (VRCapabilityManager.IsGazeFallback)
            {
                return GetTouch();
            }
            else
            {
                return OVRInput.Get(_buttonThumb, _controllerType);
            }
        }

        public override bool GetMenu()
        {
            return OVRInput.Get(_menuButton, _controllerType);
        }

        // I'm not even sure if this is a thing on Rift... I think not
        public SwipeDirection GetHMDTouch()
        {
            if (isLikeGearVR)
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
            }

            return SwipeDirection.NONE;
        }

        public override bool HardwareExists()
        {
            if (Type == InputType.GAZE)
                return true;
            return OVRInput.IsControllerConnected(_controllerType);
        }
#endif
    }
}