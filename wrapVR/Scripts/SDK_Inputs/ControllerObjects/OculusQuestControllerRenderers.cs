using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusQuestControllerRenderers : InputControllerRenderers
    {
        public bool isRightController { get { return name.Contains("Right"); } }

        protected override void init()
        {
            if (isRightController)
            {
                buttonA = transform.Find("a_button").GetComponent<Renderer>();
                buttonB = transform.Find("b_button").GetComponent<Renderer>();
                buttonHome = transform.Find("o_button").GetComponent<Renderer>();
            }
            else
            {
                buttonX = transform.Find("x_button").GetComponent<Renderer>();
                buttonY = transform.Find("y_button").GetComponent<Renderer>();
                buttonBack = transform.Find("o_button").GetComponent<Renderer>();
            }

            trigger = transform.Find("main_trigger").GetComponent<Renderer>();
            grip = transform.Find("side_trigger").GetComponent<Renderer>();
            touchPad = transform.Find("thumbstick_ball").GetComponent<Renderer>();
            controllerBody = GetComponent<Renderer>();
        }
    }
}