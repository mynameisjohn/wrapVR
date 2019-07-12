using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public abstract class InputController : MonoBehaviour
    {
        public GameObject buttonA        { get; protected set; }
        public GameObject buttonB        { get; protected set; }
        public GameObject buttonX        { get; protected set; }
        public GameObject buttonY        { get; protected set; }
        public GameObject buttonBack     { get; protected set; }
        public GameObject buttonHome     { get; protected set; }
        public GameObject trigger        { get; protected set; }
        public GameObject grip           { get; protected set; }
        public GameObject touchPad       { get; protected set; }
        public GameObject controllerBody { get; protected set; }

        public VRInput input { get; private set; }
        public void Init(VRInput vrInput)
        {
            input = vrInput;
            init();
        }
        protected abstract void init();
    }
}