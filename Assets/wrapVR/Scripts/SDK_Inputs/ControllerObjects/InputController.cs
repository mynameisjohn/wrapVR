using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public abstract class InputController : MonoBehaviour
    {
        public MeshRenderer buttonA        { get; protected set; }
        public MeshRenderer buttonB        { get; protected set; }
        public MeshRenderer buttonX        { get; protected set; }
        public MeshRenderer buttonY        { get; protected set; }
        public MeshRenderer buttonBack     { get; protected set; }
        public MeshRenderer buttonHome     { get; protected set; }
        public MeshRenderer trigger        { get; protected set; }
        public MeshRenderer grip           { get; protected set; }
        public MeshRenderer touchPad       { get; protected set; }
        public MeshRenderer controllerBody { get; protected set; }

        public VRInput input { get; private set; }
        public void Init(VRInput vrInput)
        {
            input = vrInput;
            init();
        }
        protected abstract void init();
    }
}