using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class Pointer : MonoBehaviour
    {
        public VRRayCaster Source;
        public Reticle Reticle;
        public Transform FromTransform;
        public bool DisableWhileGrabbing = true;

        public EActivation Activation = EActivation.TRIGGER;

        protected bool isPointerActive
        {
            get
            {
                return Source.IsActivationDown(Activation);
            }
        }
    }
}
