using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class EditorControllerRenderers : InputControllerRenderers
    {
        public Renderer _ButtonA;
        public Renderer _ButtonB;
        public Renderer _ButtonX;
        public Renderer _ButtonY;
        public Renderer _ButtonHome;
        public Renderer _ButtonBack;
        public Renderer _Trigger;
        public Renderer _Grip;
        public Renderer _TouchPad;
        public Renderer _ControllerBody;

        protected override void init()
        {
            buttonA = _ButtonA;
            buttonB = _ButtonB;
            buttonX = _ButtonX;
            buttonY = _ButtonY;
            buttonHome = _ButtonHome;
            buttonBack = _ButtonBack;
            trigger = _Trigger;
            grip = _Grip;
            touchPad = _TouchPad;
            controllerBody = _ControllerBody;
        }
    }
}