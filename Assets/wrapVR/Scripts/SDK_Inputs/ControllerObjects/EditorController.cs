using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class EditorController : InputController
    {
        public GameObject ButtonA;
        public GameObject ButtonB;
        public GameObject ButtonX;
        public GameObject ButtonY;
        public GameObject ButtonHome;
        public GameObject ButtonBack;
        public GameObject Ring;
        public GameObject Trigger;
        public GameObject Grip;
        public GameObject TouchPad;
        public GameObject ControllerBody;

        private void Awake()
        {
            _buttonA = ButtonA;
            _buttonB = buttonB;
            _buttonX = ButtonX;
            _buttonY = ButtonY;
            _buttonHome = ButtonHome;
            _buttonBack = ButtonBack;
            _ring = Ring;
            _trigger = Trigger;
            _grip = Grip;
            _touchPad = TouchPad;
            _controllerBody = ControllerBody;
        }
    }
}