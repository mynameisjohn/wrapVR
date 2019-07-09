﻿using System;
using System.Collections.Generic;
using UnityEngine;

// This class encapsulates all the input required for most VR games.
// It has events that can be subscribed to by classes that need specific input.
// This class must exist in every scene and so can be attached to the main
// camera for ease.
namespace wrapVR
{
    public class SteamVRInput : VRInput
    {
#if WRAPVR_STEAM
        SteamVR_TrackedController m_Controller;
        SteamVR_TrackedObject m_TrackedObj;
        private bool m_bTrigger;
        private bool m_bGrip;
        private bool m_bTouch;
        private bool m_bTouchpadClick;

        private void Start()
        {
            // Make sure we have TrackedController components on the controllers
            m_Controller = Util.EnsureComponent<SteamVR_TrackedController>(gameObject);
            m_TrackedObj = Util.EnsureComponent<SteamVR_TrackedObject>(gameObject);

            // Subscribe to the controller button events selected in the inspector.
            m_Controller.Gripped += M_Controller_Gripped;
            m_Controller.Ungripped += M_Controller_Ungripped;
            m_Controller.TriggerClicked += M_Controller_TriggerClicked;
            m_Controller.TriggerUnclicked += M_Controller_TriggerUnclicked;
            m_Controller.PadTouched += M_Controller_PadTouched;
            m_Controller.PadUntouched += M_Controller_PadUntouched;
            m_Controller.PadClicked += M_Controller_PadClicked;
            m_Controller.PadUnclicked += M_Controller_PadUnclicked;

            StartCoroutine(coroAttachToTip());
        }

        System.Collections.IEnumerator coroAttachToTip()
        {
            if (Type == InputType.GAZE)
                yield break;

            // Find the "Model" transform
            var modelTransform = transform.Find("Model");
            if (modelTransform == null)
                yield break;

            // It always seems to be 2 frames after the model is found
            const int numFramesToWait = 5;
            for (int i = 0; i < numFramesToWait; i++)
            {
                var tip = modelTransform.Find("tip");
                if (tip)
                {
                    foreach (var rc in GetComponentsInChildren<VRRayCaster>())
                        rc.FromTransform = tip.Find("attach");
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private void M_Controller_PadUnclicked(object sender, ClickedEventArgs e)
        {
            m_bTouchpadClick = false;
            _onTouchpadUp();
        }

        private void M_Controller_PadClicked(object sender, ClickedEventArgs e)
        {
            m_bTouchpadClick = true;
            _onTouchpadDown();
        }

        private void M_Controller_PadUntouched(object sender, ClickedEventArgs e)
        {
            m_bTouch = false;
            _onTouchUp();
        }

        private void M_Controller_PadTouched(object sender, ClickedEventArgs e)
        {
            m_bTouch = true;
            _onTouchDown();
        }

        private void M_Controller_TriggerUnclicked(object sender, ClickedEventArgs e)
        {
            m_bTrigger = false;
            _onTriggerUp();
        }

        private void M_Controller_TriggerClicked(object sender, ClickedEventArgs e)
        {
            m_bTrigger = true;
            _onTriggerDown();
        }

        private void M_Controller_Ungripped(object sender, ClickedEventArgs e)
        {
            m_bGrip = false;
            _onGripUp();
        }

        private void M_Controller_Gripped(object sender, ClickedEventArgs e)
        {
            m_bGrip = true;
            _onGripDown();
        }

        protected override void CheckInput()
        {
            // Just happens naturally
        }

        public override Vector2 GetTouchPosition()
        {
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)m_TrackedObj.index);
            return device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
        }

        public override bool GetGrip()
        {
            return m_bGrip;
        }
        public override bool GetTrigger()
        {
            return m_bTrigger;
        }
        public override bool GetTouch()
        {
            return m_bTouch;
        }
        public override bool GetTouchpad()
        {
            return m_bTouchpadClick;
        }

        public override bool HardwareExists()
        {
            return true;
        }

        public override InputController getController()
        {
            return null;
        }
#endif
    }
}