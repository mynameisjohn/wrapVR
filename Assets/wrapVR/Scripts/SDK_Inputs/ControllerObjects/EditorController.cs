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
        public GameObject Ring;
        public GameObject Trigger;
        public GameObject Grip;
        public GameObject TouchPad;
        public GameObject ControllerBody;

        public override GameObject buttonA() { return ButtonA; }
        public override GameObject buttonB() { return ButtonB; }
        public override GameObject buttonX() { return ButtonX; }
        public override GameObject buttonY() { return ButtonY; }
        public override GameObject ring() { return Ring; }
        public override GameObject trigger() { return Trigger; }
        public override GameObject grip() { return Grip; }
        public override GameObject touchPad() { return TouchPad; }
        public override GameObject controllerBody() { return ControllerBody; }
    }
}