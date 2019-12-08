using System;
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
        SteamVR_TrackedController _controller;
        SteamVR_TrackedObject _trackedObj;
        private bool _trigger;
        private bool _grip;
        private bool _touch;
        private bool _touchPadClick;

        public Transform model { get; private set; }

        public static string headsetName { get { return SteamVR.instance.hmd_TrackingSystemName; } }

        public override void Init() 
        {
            // Make sure we have TrackedController components on the controllers
            _controller = Util.EnsureComponent<SteamVR_TrackedController>(gameObject);
            _trackedObj = Util.EnsureComponent<SteamVR_TrackedObject>(gameObject);

            // Subscribe to the controller button events selected in the inspector.
            _controller.Gripped += M_Controller_Gripped;
            _controller.Ungripped += M_Controller_Ungripped;
            _controller.TriggerClicked += M_Controller_TriggerClicked;
            _controller.TriggerUnclicked += M_Controller_TriggerUnclicked;
            _controller.PadTouched += M_Controller_PadTouched;
            _controller.PadUntouched += M_Controller_PadUntouched;
            _controller.PadClicked += M_Controller_PadClicked;
            _controller.PadUnclicked += M_Controller_PadUnclicked;
            _controller.MenuButtonClicked += M_Controller_MenuButtonClicked;
            _controller.MenuButtonUnclicked += M_Controller_MenuButtonUnclicked;
        }

        private void SetDeviceIndex(int index)
        {
            StartCoroutine(coroAttachToTip());
        }

        private void M_Controller_MenuButtonUnclicked(object sender, ClickedEventArgs e)
        {
            _onMenuUp();
        }

        private void M_Controller_MenuButtonClicked(object sender, ClickedEventArgs e)
        {
            _onMenuDown();
        }

        System.Collections.IEnumerator coroAttachToTip()
        {
            if (Type == InputType.GAZE)
                yield break;

            // Find the "Model" transform
            model = transform.Find("Model");
            if (model == null)
                yield break;

            // keep looking for the tip - not sure how long this should take
            while (true)
            {
                var tip = model.Find("tip");
                if (tip)
                {
                    foreach (var rc in GetComponentsInChildren<VRRayCaster>())
                        rc.FromTransform = tip.Find("attach");
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }

            yield break;
        }

        private void M_Controller_PadUnclicked(object sender, ClickedEventArgs e)
        {
            _touchPadClick = false;
            _onTouchpadUp();
        }

        private void M_Controller_PadClicked(object sender, ClickedEventArgs e)
        {
            _touchPadClick = true;
            _onTouchpadDown();
        }

        private void M_Controller_PadUntouched(object sender, ClickedEventArgs e)
        {
            _touch = false;
            _onTouchUp();
        }

        private void M_Controller_PadTouched(object sender, ClickedEventArgs e)
        {
            _touch = true;
            _onTouchDown();
        }

        private void M_Controller_TriggerUnclicked(object sender, ClickedEventArgs e)
        {
            _trigger = false;
            _onTriggerUp();
        }

        private void M_Controller_TriggerClicked(object sender, ClickedEventArgs e)
        {
            _trigger = true;
            _onTriggerDown();
        }

        private void M_Controller_Ungripped(object sender, ClickedEventArgs e)
        {
            _grip = false;
            _onGripUp();
        }

        private void M_Controller_Gripped(object sender, ClickedEventArgs e)
        {
            _grip = true;
            _onGripDown();
        }

        protected override void CheckInput()
        {
            // Just happens naturally
        }

        public override Vector2 GetTouchPosition()
        {
            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)_trackedObj.index);
            return device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0);
        }

        public override bool GetGrip()
        {
            return _grip;
        }
        public override bool GetTrigger()
        {
            return _trigger;
        }
        public override bool GetTouch()
        {
            return _touch;
        }
        public override bool GetTouchpad()
        {
            return _touchPadClick;
        }

        public override bool HardwareExists()
        {
            return true;
        }

        public override InputControllerRenderers getController()
        {
            return GetComponent<SteamControllerRenderers>();
        }
#endif
    }
}