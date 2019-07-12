using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class SteamController : InputController
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
                trigger = model.transform.Find("trigger").GetComponent<MeshRenderer>();
                grip = model.transform.Find("grip").GetComponent<MeshRenderer>();
                touchPad = model.transform.Find("thumbstick").GetComponent<MeshRenderer>();
                controllerBody = model.transform.Find("body").GetComponent<MeshRenderer>();

                // only working for rift touch controllers now
                if (input.Type == InputType.LEFT)
                {
                    buttonX = model.transform.Find("x_button").GetComponent<MeshRenderer>();
                    buttonY = model.transform.Find("y_button").GetComponent<MeshRenderer>();
                    buttonBack = model.transform.Find("enter_button").GetComponent<MeshRenderer>();
                }
                else
                {
                    buttonA = model.transform.Find("a_button").GetComponent<MeshRenderer>();
                    buttonB = model.transform.Find("b_button").GetComponent<MeshRenderer>();
                    buttonHome = model.transform.Find("home_button").GetComponent<MeshRenderer>();
                }

                _modelsFound = true;
            }
        }

    }
}