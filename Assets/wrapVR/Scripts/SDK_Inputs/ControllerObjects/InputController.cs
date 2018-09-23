using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public abstract class InputController : MonoBehaviour
    {
        protected GameObject _buttonA;
        protected GameObject _buttonB;
        protected GameObject _buttonX;
        protected GameObject _buttonY;
        protected GameObject _ring;
        protected GameObject _trigger;
        protected GameObject _grip;
        protected GameObject _thumbStick;
        protected GameObject _controllerBody;

        abstract public GameObject buttonA();
        abstract public GameObject buttonB();
        abstract public GameObject buttonX();
        abstract public GameObject buttonY();
        abstract public GameObject ring();
        abstract public GameObject trigger();
        abstract public GameObject grip();
        abstract public GameObject thumbStick();
        abstract public GameObject controllerBody();
    }
}