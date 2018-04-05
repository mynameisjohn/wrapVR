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
                switch (Activation)
                {
                    case EActivation.NONE:
                        return true;
                    case EActivation.TOUCH:
                        return Source.Input.GetTouchpadTouch();
                    case EActivation.TOUCHPAD:
                        return Source.Input.GetTouchpad();
                    case EActivation.TRIGGER:
                        return Source.Input.GetTrigger();
                }
                return false;
            }
        }
    }
}
