using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusGoController : InputController
    {
        protected override void init()
        {
            buttonBack = transform.Find("BackButton").gameObject;
            buttonHome = transform.Find("HomeButton").gameObject;
            trigger = transform.Find("Trigger").gameObject;
            touchPad = transform.Find("Touchpad").gameObject;
            controllerBody = transform.Find("Controller").gameObject;
        }
    }
}