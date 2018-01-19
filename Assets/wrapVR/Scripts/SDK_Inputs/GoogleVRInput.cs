using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class GoogleVRInput
#if WRAPVR_GOOGLE
        : VRInput
    {
        protected override void CheckInput()
        {
            if (GvrControllerInput.IsTouching)
            {
                m_MostRecentTouchPosX = Util.remap(GvrControllerInput.TouchPos.x, -1f, 1f, 0f, 1f);
                m_MostRecentTouchPosY = Util.remap(GvrControllerInput.TouchPos.y, -1f, 1f, 0f, 1f);
            }
            if (GvrControllerInput.TouchDown)
            {
                m_TouchTime = Time.time;
                m_InitTouchPosX = Util.remap(GvrControllerInput.TouchPos.x, -1f, 1f, 0f, 1f);
                m_InitTouchPosY = Util.remap(GvrControllerInput.TouchPos.y, -1f, 1f, 0f, 1f);

                _onTouchpadTouchDown();
            }
            if (GvrControllerInput.TouchUp)
            {
                _onTouchpadTouchUp();
            }
            // We treat... one of those buttons as the trigger
            if (GvrControllerInput.AppButtonDown)
            {
                _onTriggerDown();
            }
            if (GvrControllerInput.AppButtonUp)
            {
                _onTriggerUp();
            }
            if (GvrControllerInput.ClickButtonDown)
            {
                _onTouchpadDown();
            }
            if (GvrControllerInput.ClickButtonUp)
            {
                _onTouchpadUp();
            }
        }

        public override Vector2 GetTouchPosition()
        {
            return GvrControllerInput.TouchPos;
        }

        protected override void HandleTouchHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException(); // ?
        }

        public override bool GetTrigger()
        {
            return GvrControllerInput.AppButton;
        }
        public override bool GetTouchpadTouch()
        {
            return GvrControllerInput.IsTouching;
        }
        public override bool GetTouchpad()
        {
            return GvrControllerInput.ClickButton;
        }
        public override SwipeDirection GetHMDTouch()
        {
            throw new NotImplementedException(); // ?
        }
        public override bool HardwareExists()
        {
            switch (Type)
            {
                case InputType.GAZE:
                    return false; // No gaze on Daydream?
                case InputType.RIGHT:
                    return (GvrControllerInput.State == GvrConnectionState.Connected);
                case InputType.LEFT:
                    return false; // always the right hand
                default:
                    return false;
            }
        }
#else
        : MonoBehaviour
    {
#endif
    }
}