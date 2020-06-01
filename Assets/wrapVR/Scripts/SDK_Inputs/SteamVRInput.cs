using System;
using System.Collections;
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
        public Valve.VR.SteamVR_Action_Boolean menuAction;
        public Valve.VR.SteamVR_Action_Boolean triggerAction;
        public Valve.VR.SteamVR_Action_Boolean gripAction;
        public Valve.VR.SteamVR_Action_Boolean touchAction;
        public Valve.VR.SteamVR_Action_Boolean touchPadAction;
        public Valve.VR.SteamVR_Action_Vector2 touchPosAction;

        bool _trigger;
        bool _grip;
        bool _touch;
        bool _touchPad;
        Vector2 _touchPos;

        public Transform model { get; private set; }

        public static string headsetName { get { return Valve.VR.SteamVR.instance.hmd_TrackingSystemName; } }

        public override void Init() 
        {
            if (Type == InputType.GAZE)
                return; // ?
            var src = (Type == InputType.RIGHT) ? Valve.VR.SteamVR_Input_Sources.RightHand : Valve.VR.SteamVR_Input_Sources.LeftHand;

            menuAction[src].onStateDown += menuDown;
            menuAction[src].onStateUp += menuUp;

            triggerAction[src].onStateDown += trigDown;
            triggerAction[src].onStateUp += trigUp;

            gripAction[src].onStateDown += gripDown;
            gripAction[src].onStateUp += gripUp;

            touchAction[src].onStateDown += touchDown;
            touchAction[src].onStateUp += touchUp;

            touchPadAction[src].onStateDown += touchPadDown;
            touchPadAction[src].onStateUp += touchPadUp;

            touchPosAction[src].onUpdate += touchPosUpdate;

            StartCoroutine(GetComponent<SteamControllerRenderers>().CoroFindControllerModels());
            StartCoroutine(coroAttachToTip());
        }

        private void menuDown(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _onMenuDown();
        }
        private void menuUp(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _onMenuUp();
        }

        private void trigDown(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _trigger = true;
            _onTriggerDown();
        }
        private void trigUp(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _trigger = false;
            _onTriggerUp();
        }

        private void gripDown(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _grip = true;
            _onGripDown();
        }
        private void gripUp(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _grip = false;
            _onGripUp();
        }

        private void touchDown(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _touch = true;
            _onTouchDown();
        }
        private void touchUp(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _touch = false;
            _onTouchUp();
        }

        private void touchPadDown(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _touchPad = true;
            _onTouchpadDown();
        }
        private void touchPadUp(Valve.VR.SteamVR_Action_Boolean fromAction, Valve.VR.SteamVR_Input_Sources fromSource)
        {
            _touchPad = false;
            _onTouchpadUp();
        }

        private void touchPosUpdate(Valve.VR.SteamVR_Action_Vector2 fromAction, Valve.VR.SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            _touchPos = axis;
        }

        IEnumerator coroAttachToTip()
        {
            // keep looking for the tip - not sure how long this should take
            while (true)
            {
                // Find the "Model" transform
                model = transform.Find("Model");
                if (model == null || model.childCount == 0)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                var tip = model.Find("tip");
                if (tip)
                {
                    foreach (var rc in GetComponentsInChildren<VRRayCaster>())
                        rc.FromTransform = tip.Find("attach");
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        protected override void CheckInput()
        {
            // Just happens naturally
        }

        public override Vector2 GetTouchPosition()
        {
            return _touchPos;
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
            return _touchPad;
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