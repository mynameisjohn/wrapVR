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
        OVRPlugin.SystemHeadset _headsetType;
        OVRInput.Controller m_eController;
        OVRInput.Axis2D m_AxisThumb;
        OVRInput.Touch m_TouchThumb;
        OVRInput.Button m_ButtonThumb;
        OVRInput.Button m_IndexTrigger;
        OVRInput.Button m_HandTrigger;  // touch only

        public bool isLikeGearVR
        {
            get
            {
                switch (_headsetType)
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

        private void Start()
        {
            _headsetType = OVRPlugin.GetSystemHeadsetType();

            if (isLikeGearVR)
            {
                m_eController = new Dictionary<InputType, OVRInput.Controller> {
                    { InputType.GAZE, OVRInput.Controller.None },
                    { InputType.LEFT,  OVRInput.Controller.LTrackedRemote  },
                    { InputType.RIGHT, OVRInput.Controller.RTrackedRemote  },
                }[Type];
                m_AxisThumb = OVRInput.Axis2D.PrimaryTouchpad;
                m_TouchThumb = OVRInput.Touch.PrimaryTouchpad;
                m_ButtonThumb = OVRInput.Button.PrimaryTouchpad;
                m_IndexTrigger = OVRInput.Button.PrimaryIndexTrigger;
                m_HandTrigger = OVRInput.Button.None;
            }
            else
            {
                m_eController = new Dictionary<InputType, OVRInput.Controller> {
                    { InputType.GAZE, OVRInput.Controller.None },
                    { InputType.LEFT,  OVRInput.Controller.LTouch },
                    { InputType.RIGHT, OVRInput.Controller.RTouch },
                }[Type];
                m_AxisThumb = OVRInput.Axis2D.PrimaryThumbstick;
                m_TouchThumb = OVRInput.Touch.PrimaryThumbstick;
                m_ButtonThumb = OVRInput.Button.PrimaryThumbstick;
                m_IndexTrigger = OVRInput.Button.PrimaryIndexTrigger;
                m_HandTrigger = OVRInput.Button.PrimaryHandTrigger;
            }
        }

        public override InputController getController()
        {
            if (Type == InputType.GAZE)
                return null;

            var controllerHelper = GetComponentInChildren<OVRControllerHelper>();
            if (controllerHelper == null)
                return null;

            GameObject controller;
            switch (_headsetType)
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

            return controller.GetComponent<InputController>();
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
            if (OVRInput.Get(m_TouchThumb, m_eController))
            {
                Vector2 v2ThumbPos = GetTouchPosition();
                m_MostRecentTouchPosX = v2ThumbPos.x;
                m_MostRecentTouchPosY = v2ThumbPos.y;
            }

            // Detect initial positions if down
            if (OVRInput.GetDown(m_TouchThumb, m_eController))
            {
                m_TouchTime = Time.time;
                m_InitTouchPosX = 0f;
                m_InitTouchPosY = 0f;

                // Send on down message
                _onTouchDown();

                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerDown();
            }
            // Otherwise see if we just got thumb up
            else if (OVRInput.GetUp(m_TouchThumb, m_eController))
            {
                _onTouchUp();
                // Treat as trigger if gaze fallback
                if (VRCapabilityManager.IsGazeFallback)
                    _onTriggerUp();
            }

            if (OVRInput.GetDown(m_ButtonThumb, m_eController))
            {
                _onTouchpadDown();
            }
            else if (OVRInput.GetUp(m_ButtonThumb, m_eController))
            {
                _onTouchpadUp();
            }

            // Get back to cancel on Android
            if (isLikeGearVR)
            {
                if (OVRInput.GetDown(OVRInput.Button.Back))
                    _onCancel();
            }
            else if (Type == InputType.LEFT && OVRInput.GetDown(OVRInput.Button.Start))
                _onCancel();

            // I'm not sure why but I'm not getting trigger ups
            // on the Gear - may be bug with my SDK...?

            // Handle left / right triggers
            if (isLikeGearVR && VRCapabilityManager.IsGazeFallback)
                return;

            if (OVRInput.GetDown(m_IndexTrigger, m_eController))
            {
                _onTriggerDown();
            }
            if (OVRInput.GetUp(m_IndexTrigger, m_eController))
            {
                _onTriggerUp();
            }
            if (OVRInput.GetDown(m_HandTrigger, m_eController))
            {
                _onGripDown();
            }
            if (OVRInput.GetUp(m_HandTrigger, m_eController))
            {
                _onGripUp();
            }
        }

        public override Vector2 GetTouchPosition()
        {
            return OVRInput.Get(m_AxisThumb, m_eController);
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

            return OVRInput.Get(m_HandTrigger, m_eController);
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
                return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, m_eController);
            }
        }

        public override bool GetTouch()
        {
            return OVRInput.Get(m_TouchThumb, m_eController);
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
                return OVRInput.Get(m_ButtonThumb, m_eController);
            }
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
            return OVRInput.IsControllerConnected(m_eController);
        }
#endif
    }
}