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
        protected GameObject _buttonBack;
        protected GameObject _buttonHome;
        protected GameObject _ring;
        protected GameObject _trigger;
        protected GameObject _grip;
        protected GameObject _touchPad;
        protected GameObject _controllerBody;

        public GameObject buttonA        { get { return _buttonA; } }
        public GameObject buttonB        { get { return _buttonB; } }
        public GameObject buttonX        { get { return _buttonX; } }
        public GameObject buttonY        { get { return _buttonY; } }
        public GameObject buttonBack     { get { return _buttonBack; } }
        public GameObject buttonHome     { get { return _buttonHome; } }
        public GameObject ring           { get { return _ring; } }
        public GameObject trigger        { get { return _trigger; } }
        public GameObject grip           { get { return _grip; } }
        public GameObject touchPad       { get { return _touchPad; } }
        public GameObject controllerBody { get { return _controllerBody; } }
    }
}