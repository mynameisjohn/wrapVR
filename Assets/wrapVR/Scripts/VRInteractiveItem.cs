using System;
using UnityEngine;

namespace wrapVR
{
    // This class should be added to any gameobject in the scene
    // that should react to input based on the user's gaze.
    // It contains events that can be subscribed to by classes that
    // need to know about input specifics to this gameobject.
    using VRAction = Action<VRRayCaster>;
    public class VRInteractiveItem : MonoBehaviour
    {
        public event VRAction OnGazeOver;             // Called when the gaze moves over this object
        public event VRAction OnGazeOut;              // Called when the gaze leaves this object
        public event VRAction OnPointerOver;             // Called when the gaze moves over this object
        public event VRAction OnPointerOut;              // Called when the gaze leaves this object
        public event VRAction OnClick;            // Called when click input is detected whilst the gaze is over this object.
        public event VRAction OnDoubleClick;      // Called when double click input is detected whilst the gaze is over this object.
        public event VRAction OnUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event VRAction OnDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event VRAction OnTriggerUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event VRAction OnTriggerDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event VRAction OnTouchpadUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event VRAction OnTouchpadDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event VRAction OnTouchUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event VRAction OnTouchDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event VRAction OnTriggerOver;
        public event VRAction OnTriggerOut;

        // Grip buttons - these only work on Rift and Vive
        // For mobile platforms I'll merge them with touchpad or trigger
        public event VRAction OnGripUp;
        public event VRAction OnGripDown;

        public void ActivationDownCallback(EActivation activation, VRAction action, bool bAdd)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    if (bAdd)
                        OnTouchDown += action;
                    else
                        OnTouchDown -= action;
                    break;
                case EActivation.TOUCHPAD:
                    if (bAdd)
                        OnTouchpadDown += action;
                    else
                        OnTouchpadDown -= action;
                    break;
                case EActivation.TRIGGER:
                    if (bAdd)
                        OnTriggerDown += action;
                    else
                        OnTriggerDown -= action;
                    break;
                case EActivation.GRIP:
                    if (bAdd)
                        OnGripDown += action;
                    else
                        OnGripDown -= action;
                    break;
            }
        }
        public void ActivationUpCallback(EActivation activation, VRAction action, bool bAdd)
        {
            switch (activation)
            {
                case EActivation.TOUCH:
                    if (bAdd)
                        OnTouchUp += action;
                    else
                        OnTouchUp -= action;
                    break;
                case EActivation.TOUCHPAD:
                    if (bAdd)
                        OnTouchpadUp += action;
                    else
                        OnTouchpadUp -= action;
                    break;
                case EActivation.TRIGGER:
                    if (bAdd)
                        OnTriggerUp += action;
                    else
                        OnTriggerUp -= action;
                    break;
                case EActivation.GRIP:
                    if (bAdd)
                        OnGripUp += action;
                    else
                        OnGripUp -= action;
                    break;
            }
        }

        // TODO make these counts
        protected int m_nGazeCount;
        protected int m_nPointerCount;
        protected int m_nTriggerCount;
        protected int m_nTouchCount;

        public bool IsGazeOver
        {
            get { return m_nGazeCount > 0; }              // Is the gaze currently over this object?
        }
        public bool IsPointerOver
        {
            get { return m_nPointerCount > 0; }              // Is the gaze currently over this object?
        }
        public bool IsTriggerDown
        {
            get { return m_nTriggerCount > 0; }
        }
        public bool IsTouchDown
        {
            get { return m_nTouchCount > 0; }
        }


        // The below functions are called by the VREyeRaycaster when the appropriate input is detected.
        // They in turn call the appropriate events should they have subscribers.
        public void GazeOver(VRRayCaster source)
        {
            m_nGazeCount++;

            if (OnGazeOver != null)
                OnGazeOver(source);
        }

        public void PointerOver(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;
            if (!VRCapabilityManager.canPointIfTrigger && source.isTriggerDown)
                return;

            m_nPointerCount++;

            if (OnPointerOver != null)
                OnPointerOver(source);
        }

        public void GazeOut(VRRayCaster source)
        {
            m_nGazeCount--;

            if (OnGazeOut != null)
                OnGazeOut(source);
        }

        public void PointerOut(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;
            if (!VRCapabilityManager.canPointIfTrigger && source.isTriggerDown)
                return;

            m_nPointerCount--;
            if (OnPointerOut != null)
                OnPointerOut(source);
        }

        public void Click(VRRayCaster source)
        {
            if (OnClick != null)
                OnClick(source);
        }


        public void DoubleClick(VRRayCaster source)
        {
            if (OnDoubleClick != null)
                OnDoubleClick(source);
        }


        public void Up(VRRayCaster source)
        {
            if (OnUp != null)
                OnUp(source);
        }


        public void Down(VRRayCaster source)
        {
            if (OnDown != null)
                OnDown(source);
        }

        public void GripUp(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;

            if (OnGripUp != null)
                OnGripUp(source);
        }


        public void GripDown(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;

            if (OnGripDown != null)
                OnGripDown(source);
        }

        public void TriggerUp(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;

            if (OnTriggerUp != null)
                OnTriggerUp(source);

            TriggerOut(source);
        }


        public void TriggerDown(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;

            if (OnTriggerDown != null)
                OnTriggerDown(source);
        }

        public void TouchpadUp(VRRayCaster source)
        {
            if (OnTouchpadUp != null)
                OnTouchpadUp(source);
        }


        public void TouchpadDown(VRRayCaster source)
        {
            if (OnTouchpadDown != null)
                OnTouchpadDown(source);
        }

        public void TouchUp(VRRayCaster source)
        {
            if (OnTouchUp != null)
                OnTouchUp(source);
        }


        public void TouchDown(VRRayCaster source)
        {
            if (OnTouchDown != null)
                OnTouchDown(source);
        }
        public void TriggerOver(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;

            m_nTriggerCount++;
            if (OnTriggerOver != null)
                OnTriggerOver(source);
        }

        public void TriggerOut(VRRayCaster source)
        {
            if (!VRCapabilityManager.canPointWhileGrabbing && source.isGrabbing)
                return;

            m_nTriggerCount--;
            if (OnTriggerOut != null)
                OnTriggerOut(source);
        }

        // TODO touch over?
    }
}