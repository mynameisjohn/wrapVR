using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public abstract class InputControllerRenderers : MonoBehaviour
    {
        public Renderer buttonA        { get; protected set; }
        public Renderer buttonB        { get; protected set; }
        public Renderer buttonX        { get; protected set; }
        public Renderer buttonY        { get; protected set; }
        public Renderer buttonBack     { get; protected set; }
        public Renderer buttonHome     { get; protected set; }
        public Renderer trigger        { get; protected set; }
        public Renderer grip           { get; protected set; }
        public Renderer touchPad       { get; protected set; }
        public Renderer controllerBody { get; protected set; }

        public VRInput input { get; private set; }
        public void Init(VRInput vrInput)
        {
            input = vrInput;
            init();
        }
        protected abstract void init();
    }
}