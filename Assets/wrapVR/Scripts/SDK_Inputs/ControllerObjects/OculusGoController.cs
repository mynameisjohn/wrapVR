using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class OculusGoController : InputController
    {
#if WRAPVR_OCULUS
        
        protected void Awake()
        {
            _buttonBack = transform.Find("BackButton").gameObject;
            _buttonHome = transform.Find("HomeButton").gameObject;
            _trigger = transform.Find("Trigger").gameObject;
            _touchPad = transform.Find("Touchpad").gameObject;
            _controllerBody = transform.Find("Controller").gameObject;
        }
#endif
    }
}