using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class GearVRControllerRenderer : InputControllerRenderers
    {
        protected override void init()
        {
            buttonBack = transform.Find("back_button_PLY").GetComponent<Renderer>();
            buttonHome = transform.Find("home_button_PLY").GetComponent<Renderer>();
            trigger = transform.Find("trigger_PLY").GetComponent<Renderer>();
            touchPad = transform.Find("disc_button_PLY").GetComponent<Renderer>();
            controllerBody = transform.Find("chassis_PLY").GetComponent<Renderer>();
        }
    }
}
