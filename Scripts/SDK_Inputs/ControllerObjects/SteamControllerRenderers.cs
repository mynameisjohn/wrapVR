using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class SteamControllerRenderers : InputControllerRenderers
    {
        bool _modelsFound = false;

        protected override void init() { }

        void Update()
        {
            if (_modelsFound)
                return;

            // Find the "Model" transform
            var model = input.transform.Find("Model");
            if (model == null)
                return;

            // keep looking for the tip - not sure how long this should take
            var tip = model.Find("tip");
            if (tip)
            {
                trigger = model.transform.Find("trigger").GetComponent<Renderer>();
                controllerBody = model.transform.Find("body").GetComponent<Renderer>();

                var gripTransform = model.transform.Find("grip");
                if (gripTransform)
                {
                    // branch for rift touch controllers
                    grip = gripTransform.GetComponent<Renderer>();
                    touchPad = model.transform.Find("thumbstick").GetComponent<Renderer>();
                    controllerBody = model.transform.Find("body").GetComponent<Renderer>();

                    if (input.Type == InputType.LEFT)
                    {
                        buttonX = model.transform.Find("x_button").GetComponent<Renderer>();
                        buttonY = model.transform.Find("y_button").GetComponent<Renderer>();
                        buttonBack = model.transform.Find("enter_button").GetComponent<Renderer>();
                    }
                    else
                    {
                        buttonA = model.transform.Find("a_button").GetComponent<Renderer>();
                        buttonB = model.transform.Find("b_button").GetComponent<Renderer>();
                        buttonHome = model.transform.Find("home_button").GetComponent<Renderer>();
                    }
                }
                else
                {
                    // vive controllers - switch these so the left shows the right grip
                    if (input.Type == InputType.LEFT)
                        gripTransform = model.transform.Find("rgrip");
                    else
                        gripTransform = model.transform.Find("lgrip");

                    grip = gripTransform.GetComponent<Renderer>();
                    touchPad = model.transform.Find("trackpad").GetComponent<Renderer>();

                    buttonBack = model.transform.Find("button").GetComponent<Renderer>();
                    buttonHome = model.transform.Find("sys_button").GetComponent<Renderer>();
                }
                
                _modelsFound = true;
            }
        }

    }
}