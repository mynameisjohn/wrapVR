using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class EditorController : InputController
    {
        public MeshRenderer _ButtonA;
        public MeshRenderer _ButtonB;
        public MeshRenderer _ButtonX;
        public MeshRenderer _ButtonY;
        public MeshRenderer _ButtonHome;
        public MeshRenderer _ButtonBack;
        public MeshRenderer _Trigger;
        public MeshRenderer _Grip;
        public MeshRenderer _TouchPad;
        public MeshRenderer _ControllerBody;

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