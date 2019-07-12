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
                trigger = model.transform.Find("trigger").gameObject;
                grip = model.transform.Find("grip").gameObject;
                touchPad = model.transform.Find("thumbstick").gameObject;
                controllerBody = model.transform.Find("body").gameObject;

                // only working for rift touch controllers now
                if (input.Type == InputType.LEFT)
                {
                    buttonX = model.transform.Find("x_button").gameObject;
                    buttonY = model.transform.Find("y_button").gameObject;
                    buttonBack = model.transform.Find("enter_button").gameObject;
                }
                else
                {
                    buttonA = model.transform.Find("a_button").gameObject;
                    buttonB = model.transform.Find("b_button").gameObject;
                    buttonHome = model.transform.Find("home_button").gameObject;
                }

                _modelsFound = true;
            }
        }

    }
}