using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class GoogleVRInput : VRInput
    {
#if WRAPVR_GOOGLE
        GvrControllerHand _gvrHand;
        GvrControllerInputDevice gvrDevice { get { return GvrControllerInput.GetDevice(_gvrHand); } }
        public override void Init()
        {
            // Gaze?
            _gvrHand = (Type == InputType.GAZE ? GvrControllerHand.NonDominant : GvrControllerHand.Dominant);
            // gvrDevice = GvrControllerInput.GetDevice(_gvrHand);
        }

        protected override void CheckInput()
        {
            // Note that we negate the y pos - up is down with Google
            if (gvrDevice.GetButton(GvrControllerButton.TouchPadTouch))
            {
                _lastTouchPosX = gvrDevice.TouchPos.x;
                _lastTouchPosY = gvrDevice.TouchPos.y;

                // Swipe
                detectAndHandleSwipe();
            }
            if (gvrDevice.GetButtonDown(GvrControllerButton.TouchPadTouch))
            {
                _touchTime = Time.time;
                _initTouchPosX = _lastTouchPosX;
                _initTouchPosY = _lastTouchPosY;

                _onTouchDown();
            }
            if (gvrDevice.GetButtonUp(GvrControllerButton.TouchPadTouch))
            {
                _onTouchUp();
            }
            // We treat the app button (the one that isn't home) as the trigger
            if (gvrDevice.GetButtonDown(GvrControllerButton.App))
            {
                _onTriggerDown();
            }
            if (gvrDevice.GetButtonUp(GvrControllerButton.App))
            {
                _onTriggerUp();
            }
            if (gvrDevice.GetButtonDown(GvrControllerButton.TouchPadButton))
            {
                _onTouchpadDown();
            }
            if (gvrDevice.GetButtonUp(GvrControllerButton.TouchPadButton))
            {
                _onTouchpadUp();
            }
        }

        public override Vector2 GetTouchPosition()
        {
            return new Vector2(_lastTouchPosX, _lastTouchPosY);
        }

        // If the CapMgr translates grip then do so, otherwise return false
        public override bool GetGrip()
        {
            switch(VRCapabilityManager.mobileGrip)
            {
                case EActivation.TOUCHPAD:
                    return GetTouchpad();
                case EActivation.TRIGGER:
                    return GetTrigger();
            };
            return false;
        }
        public override bool GetTrigger()
        {
            return gvrDevice.GetButton(GvrControllerButton.App);
        }
        public override bool GetTouch()
        {
            return gvrDevice.GetButton(GvrControllerButton.TouchPadTouch);
        }
        public override bool GetTouchpad()
        {
            return gvrDevice.GetButton(GvrControllerButton.TouchPadButton);
        }
        public override bool HardwareExists()
        {
            switch (Type)
            {
                case InputType.GAZE:
                    return false; // No gaze on Daydream?
                case InputType.RIGHT:
                    return gvrDevice != null && gvrDevice.State == GvrConnectionState.Connected;
                case InputType.LEFT:
                    return false; // always the right hand
                default:
                    return false;
            }
        }

        public override InputControllerRenderers getController()
        {
            return GetComponent<DaydreamControllerRenderers>();
        }
#endif
    }
}