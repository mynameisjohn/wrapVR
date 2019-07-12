using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusTouchController : InputController
    {
        public bool isRightController { get { return transform.parent.parent.name.StartsWith("Right"); } }
        string ctrl { get { return isRightController ? "rctrl:" : "lctrl:"; } }
        string ctrlName(string name)
        {
            return ctrl + name + "_PLY";
        }
        
        protected override void init()
        {
            Transform geomContainer = transform.Find(ctrl + "geometry_null");

            if (isRightController)
            {
                buttonA = geomContainer.Find(ctrlName("a_button")).gameObject;
                buttonB = geomContainer.Find(ctrlName("b_button")).gameObject;
                buttonHome = geomContainer.Find(ctrlName("o_button")).gameObject;
            }
            else
            {
                buttonX = geomContainer.Find(ctrlName("x_button")).gameObject;
                buttonY = geomContainer.Find(ctrlName("y_button")).gameObject;
                buttonBack = geomContainer.Find(ctrlName("o_button")).gameObject;
            }

            trigger = geomContainer.Find(ctrlName("main_trigger")).gameObject;
            grip = geomContainer.Find(ctrlName("side_trigger")).gameObject;
            touchPad = geomContainer.Find(ctrlName("thumbstick_ball")).gameObject;
            controllerBody = geomContainer.Find(ctrlName("controller_body")).gameObject;
        }
    }
}