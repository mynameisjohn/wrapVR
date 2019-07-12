using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusGoControllerRenderers : InputControllerRenderers
    {
        protected override void init()
        {
            buttonBack = transform.Find("BackButton").GetComponent<Renderer>();
            buttonHome = transform.Find("HomeButton").GetComponent<Renderer>();
            trigger = transform.Find("Trigger").GetComponent<Renderer>();
            touchPad = transform.Find("Touchpad").GetComponent<Renderer>();
            controllerBody = transform.Find("Controller").GetComponent<Renderer>();
        }
    }
}