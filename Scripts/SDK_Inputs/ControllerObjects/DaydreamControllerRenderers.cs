using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class DaydreamControllerRenderers : InputControllerRenderers
    {
        protected override void init()
        {
            Transform controllerVisual = transform.Find("ControllerVisual");
            buttonHome = controllerVisual.Find("DaydreamButton").GetComponent<Renderer>();
            trigger = controllerVisual.Find("AppButton").GetComponent<Renderer>();
            touchPad = controllerVisual.Find("Touchpad").GetComponent<Renderer>();
            controllerBody = controllerVisual.GetComponent<Renderer>();
        }
    }
}