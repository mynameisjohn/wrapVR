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
            buttonHome = controllerVisual.Find("DaydreamButton").GetComponent<MeshRenderer>();
            trigger = controllerVisual.Find("AppButton").GetComponent<MeshRenderer>();
            touchPad = controllerVisual.Find("Touchpad").GetComponent<MeshRenderer>();
            controllerBody = controllerVisual.GetComponent<MeshRenderer>();
        }
    }
}