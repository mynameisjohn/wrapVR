using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusTouchController : InputController
    {
#if WRAPVR_OCULUS
        public bool isRightController { get { return transform.parent.parent.name.StartsWith("Right"); } }// VRCapabilityManager.rightHand.getController() == this; } }
        string ctrl { get { return isRightController ? "rctrl:" : "lctrl:"; } }
        string ctrlName(string name)
        {
            return ctrl + name + "_PLY";
        }
        
        protected void Awake()
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

            if (isRightController)
                _buttonHome = geomContainer.Find(ctrlName("o_button")).gameObject;
            else
                _buttonBack = geomContainer.Find(ctrlName("o_button")).gameObject;

            _ring = geomContainer.Find(ctrlName("ring")).gameObject;
            _trigger = geomContainer.Find(ctrlName("main_trigger")).gameObject;
            _grip = geomContainer.Find(ctrlName("side_trigger")).gameObject;
            _touchPad = geomContainer.Find(ctrlName("thumbstick_ball")).gameObject;
            _controllerBody = geomContainer.Find(ctrlName("controller_body")).gameObject;
        }
#endif
    }
}