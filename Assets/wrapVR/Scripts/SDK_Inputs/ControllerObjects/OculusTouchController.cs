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
                buttonA = geomContainer.Find(ctrlName("a_button")).GetComponent<MeshRenderer>();
                buttonB = geomContainer.Find(ctrlName("b_button")).GetComponent<MeshRenderer>();
                buttonHome = geomContainer.Find(ctrlName("o_button")).GetComponent<MeshRenderer>();
            }
            else
            {
                buttonX = geomContainer.Find(ctrlName("x_button")).GetComponent<MeshRenderer>();
                buttonY = geomContainer.Find(ctrlName("y_button")).GetComponent<MeshRenderer>();
                buttonBack = geomContainer.Find(ctrlName("o_button")).GetComponent<MeshRenderer>();
            }

            trigger = geomContainer.Find(ctrlName("main_trigger")).GetComponent<MeshRenderer>();
            grip = geomContainer.Find(ctrlName("side_trigger")).GetComponent<MeshRenderer>();
            touchPad = geomContainer.Find(ctrlName("thumbstick_ball")).GetComponent<MeshRenderer>();
            controllerBody = geomContainer.Find(ctrlName("controller_body")).GetComponent<MeshRenderer>();
        }
    }
}