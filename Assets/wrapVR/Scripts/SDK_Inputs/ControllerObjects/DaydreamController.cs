using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class DaydreamController : InputController
    {
        protected override void init()
        {
            Transform controllerVisual = transform.Find("ControllerVisual");
            buttonHome = controllerVisual.Find("DaydreamButton").gameObject;
            trigger = controllerVisual.Find("AppButton").gameObject;
            touchPad = controllerVisual.Find("Touchpad").gameObject;
            controllerBody = controllerVisual.gameObject;
        }
    }
}