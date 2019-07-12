using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusGoController : InputController
    {
        protected override void init()
        {
            buttonBack = transform.Find("BackButton").GetComponent<MeshRenderer>();
            buttonHome = transform.Find("HomeButton").GetComponent<MeshRenderer>();
            trigger = transform.Find("Trigger").GetComponent<MeshRenderer>();
            touchPad = transform.Find("Touchpad").GetComponent<MeshRenderer>();
            controllerBody = transform.Find("Controller").GetComponent<MeshRenderer>();
        }
    }
}