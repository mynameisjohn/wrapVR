using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusTouchController : InputController
    {
#if WRAPVR_OCULUS
        public bool isRightController { get { return transform.parent.GetComponent<OVRTrackedRemote>().m_controller == OVRInput.Controller.RTrackedRemote; } }
        string ctrl { get { return isRightController ? "rctrl:" : "lctrl:"; } }
        string ctrlName(string name)
        {
            return ctrl + name + "_PLY";
        }
        VRInput input { get { return isRightController ? VRCapabilityManager.rightHand : VRCapabilityManager.leftHand; } }

        public override GameObject buttonA() { return _buttonA; } 
        public override GameObject buttonB() { return _buttonB; } 
        public override GameObject buttonX() { return _buttonX; } 
        public override GameObject buttonY() { return _buttonY; } 
        public override GameObject ring() { return _ring; } 
        public override GameObject trigger() { return _trigger; }
        public override GameObject grip() { return _grip; } 
        public override GameObject thumbStick() { return _thumbStick; }
        public override GameObject controllerBody() { return _controllerBody; }

        private void Awake()
        {
            Transform geomContainer = transform.Find(ctrl + "geometry_null");

            if (isRightController)
            {
                _buttonA = geomContainer.Find(ctrlName("a_button")).gameObject;
                _buttonB = geomContainer.Find(ctrlName("b_button")).gameObject;
            }
            else
            {
                _buttonX = geomContainer.Find(ctrlName("x_button")).gameObject;
                _buttonY = geomContainer.Find(ctrlName("y_button")).gameObject;
            }

            _ring = geomContainer.Find(ctrlName("ring")).gameObject;
            _trigger = geomContainer.Find(ctrlName("main_trigger")).gameObject;
            _grip = geomContainer.Find(ctrlName("side_trigger")).gameObject;
            _thumbStick = geomContainer.Find(ctrlName("thumbstick_ball")).gameObject;
            _controllerBody = geomContainer.Find(ctrlName("controller_body")).gameObject;
        }

        void Start()
        {
            input._SetControllerObject(this);
        }
    }
#endif
}