using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class EditorController : InputController
    {
        public GameObject _ButtonA;
        public GameObject _ButtonB;
        public GameObject _ButtonX;
        public GameObject _ButtonY;
        public GameObject _ButtonHome;
        public GameObject _ButtonBack;
        public GameObject _Trigger;
        public GameObject _Grip;
        public GameObject _TouchPad;
        public GameObject _ControllerBody;

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