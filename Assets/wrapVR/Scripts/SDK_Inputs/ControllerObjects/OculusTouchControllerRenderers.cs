using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusTouchControllerRenderers : InputControllerRenderers
    {
        public bool isRightController { get { return name.Contains("Right"); } }
        string ctrl { get { return isRightController ? "rctrl:" : "lctrl:"; } }
        string ctrlName(string name)
        {
            return ctrl + name + "_PLY";
        }
        
        protected override void init()
        {
            Transform geomContainer = transform.GetChild(0).Find(ctrl + "geometry_null");

            if (isRightController)
            {
                buttonA = geomContainer.Find(ctrlName("a_button")).GetComponent<Renderer>();
                buttonB = geomContainer.Find(ctrlName("b_button")).GetComponent<Renderer>();
                buttonHome = geomContainer.Find(ctrlName("o_button")).GetComponent<Renderer>();
            }
            else
            {
                buttonX = geomContainer.Find(ctrlName("x_button")).GetComponent<Renderer>();
                buttonY = geomContainer.Find(ctrlName("y_button")).GetComponent<Renderer>();
                buttonBack = geomContainer.Find(ctrlName("o_button")).GetComponent<Renderer>();
            }

            trigger = geomContainer.Find(ctrlName("main_trigger")).GetComponent<Renderer>();
            grip = geomContainer.Find(ctrlName("side_trigger")).GetComponent<Renderer>();
            touchPad = geomContainer.Find(ctrlName("thumbstick_ball")).GetComponent<Renderer>();
            controllerBody = geomContainer.Find(ctrlName("controller_body")).GetComponent<Renderer>();
        }
    }
}