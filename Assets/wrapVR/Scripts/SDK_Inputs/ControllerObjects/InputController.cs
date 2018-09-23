using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class InputController : MonoBehaviour
    {
        protected GameObject _buttonA;
        protected GameObject _buttonB;
        protected GameObject _buttonX;
        protected GameObject _buttonY;
        protected GameObject _ring;
        protected GameObject _trigger;
        protected GameObject _grip;
        protected GameObject _touchPad;
        protected GameObject _controllerBody;

        virtual public GameObject buttonA() { return null; }
        virtual public GameObject buttonB() { return null; }
        virtual public GameObject buttonX() { return null; }
        virtual public GameObject buttonY() { return null; }
        virtual public GameObject ring() { return null; }
        virtual public GameObject trigger() { return null; }
        virtual public GameObject grip() { return null; }
        virtual public GameObject touchPad() { return null; }
        virtual public GameObject controllerBody() { return null; }
    }
}