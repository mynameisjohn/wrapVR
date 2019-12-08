using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class Pointer : MonoBehaviour
    {
        public VRRayCaster _Source;
        public Reticle _Reticle;
        public Transform _FromTransform;
        public bool _DisableWhileGrabbing = true;

        public EActivation _Activation = EActivation.TRIGGER;

        protected bool isPointerActive
        {
            get
            {
                return _Source.IsActivationDown(_Activation);
            }
        }
    }
}
