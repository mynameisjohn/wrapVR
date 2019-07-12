using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class DaydreamController : InputController
    {
#if WRAPVR_GOOGLE
        protected override void init()
        {
            Transform controllerVisual = transform.Find("ControllerVisual");
            _buttonHome = controllerVisual.Find("DaydreamButton").gameObject;
            _trigger = controllerVisual.Find("AppButton").gameObject;
            _touchPad = controllerVisual.Find("Touchpad").gameObject;
            _controllerBody = controllerVisual.gameObject;
        }
#endif
    }
}