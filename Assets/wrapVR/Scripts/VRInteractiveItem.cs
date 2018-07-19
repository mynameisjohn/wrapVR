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

        public event VRAction OnTriggerUp;   
        public event VRAction OnTriggerDown; 
        public event VRAction OnTriggerOver;
        public event VRAction OnTriggerOut;

        public event VRAction OnTouchUp;     
        public event VRAction OnTouchDown;   
        public event VRAction OnTouchOver;   
        public event VRAction OnTouchOut;    

        public event VRAction OnTouchpadUp;  
        public event VRAction OnTouchpadDown;
        public event VRAction OnTouchpadOver;
        public event VRAction OnTouchpadOut;
        
        public event VRAction OnGripUp;
        public event VRAction OnGripDown;
        public event VRAction OnGripOver;
        public event VRAction OnGripOut;

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
        protected int m_nTouchCount;
        protected int m_nTouchpadCount;
        protected int m_nTriggerCount;
        protected int m_nGripCount;

        public bool IsGazeOver
        {
            get { return m_nGazeCount > 0; }              // Is the gaze currently over this object?
        }
        public bool IsPointerOver
        {
            get { return m_nPointerCount > 0; }              // Is the gaze currently over this object?
        }
        public bool IsTouchOver
        {
            get { return m_nTouchCount > 0; }
        }
        public bool IsTouchpadOver
        {
            get { return m_nTouchCount > 0; }
        }
        public bool IsTriggerOver
        {
            get { return m_nTriggerCount > 0; }
        }
        public bool IsGripOver
        {
            get { return m_nGripCount > 0; }
        }


        // The below functions are called by the VREyeRaycaster when the appropriate input is detected.
        // They in turn call the appropriate events should they have subscribers.
        public void GazeOver(VRRayCaster source)
        {
            m_nGazeCount++;
            if (OnGazeOver != null)
                OnGazeOver(source);
        }
        public void GazeOut(VRRayCaster source)
        {
            m_nGazeCount--;
            if (OnGazeOut != null)
                OnGazeOut(source);
        }

        public void PointerOver(VRRayCaster source)
        {
            m_nPointerCount++;
            if (OnPointerOver != null)
                OnPointerOver(source);
        }
        public void PointerOut(VRRayCaster source)
        {
            m_nPointerCount--;
            if (OnPointerOut != null)
                OnPointerOut(source);
        }

        public void TouchUp(VRRayCaster source)
        {
            if (OnTouchUp != null)
                OnTouchUp(source);
            TouchOut(source);
        }
        public void TouchDown(VRRayCaster source)
        {
            if (OnTouchDown != null)
                OnTouchDown(source);
            TouchOver(source);
        }
        public void TouchOver(VRRayCaster source)
        {
            m_nTouchCount++;
            if (OnTouchOver != null)
                OnTouchOver(source);
        }
        public void TouchOut(VRRayCaster source)
        {
            m_nTouchCount--;
            if (OnTouchOut != null)
                OnTouchOut(source);
        }

        public void TouchpadUp(VRRayCaster source)
        {
            if (OnTouchpadUp != null)
                OnTouchpadUp(source);
            TouchpadOut(source);
        }
        public void TouchpadDown(VRRayCaster source)
        {
            if (OnTouchpadDown != null)
                OnTouchpadDown(source);
            TouchpadOver(source);
        }
        public void TouchpadOver(VRRayCaster source)
        {
            m_nTouchpadCount++;
            if (OnTouchpadOver != null)
                OnTouchpadOver(source);
        }
        public void TouchpadOut(VRRayCaster source)
        {
            m_nTouchpadCount--;
            if (OnTouchpadOut != null)
                OnTouchpadOut(source);
        }

        public void TriggerUp(VRRayCaster source)
        {
            if (OnTriggerUp != null)
                OnTriggerUp(source);
            TriggerOut(source);
        }
        public void TriggerDown(VRRayCaster source)
        {
            if (OnTriggerDown != null)
                OnTriggerDown(source);
            TriggerOver(source);
        }
        public void TriggerOver(VRRayCaster source)
        {
            m_nTriggerCount++;
            if (OnTriggerOver != null)
                OnTriggerOver(source);
        }
        public void TriggerOut(VRRayCaster source)
        {
            m_nTriggerCount--;
            if (OnTriggerOut != null)
                OnTriggerOut(source);
        }

        public void GripUp(VRRayCaster source)
        {
            if (OnGripUp != null)
                OnGripUp(source);
            GripOut(source);
        }
        public void GripDown(VRRayCaster source)
        {
            if (OnGripDown != null)
                OnGripDown(source);
            GripOver(source);
        }
        public void GripOut(VRRayCaster source)
        {
            m_nGripCount--;
            if (OnGripOut != null)
                OnGripOut(source);
        }
        public void GripOver(VRRayCaster source)
        {
            m_nGripCount++;
            if (OnGripOver != null)
                OnGripOver(source);
        }

        // TODO touch over?
    }
}