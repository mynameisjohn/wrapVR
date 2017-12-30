using System;
using UnityEngine;

namespace wrapVR
{
    // This class should be added to any gameobject in the scene
    // that should react to input based on the user's gaze.
    // It contains events that can be subscribed to by classes that
    // need to know about input specifics to this gameobject.
    using VRAction = Action<wrapVR.VRInput>;
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
        public event VRAction OnAnyTriggerOver;
        public event VRAction OnAnyTriggerOut;

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
        public void GazeOver(VRInput source)
        {
            m_nGazeCount++;

            if (OnGazeOver != null)
                OnGazeOver(source);
        }

        public void PointerOver(VRInput source)
        {
            m_nPointerCount++;

            if (OnPointerOver != null)
                OnPointerOver(source);
        }

        public void GazeOut(VRInput source)
        {
            m_nGazeCount--;

            if (OnGazeOut != null)
                OnGazeOut(source);
        }

        public void PointerOut(VRInput source)
        {
            m_nPointerCount--;
            if (OnPointerOut != null)
                OnPointerOut(source);
        }

        public void Click(VRInput source)
        {
            if (OnClick != null)
                OnClick(source);
        }


        public void DoubleClick(VRInput source)
        {
            if (OnDoubleClick != null)
                OnDoubleClick(source);
        }


        public void Up(VRInput source)
        {
            if (OnUp != null)
                OnUp(source);
        }


        public void Down(VRInput source)
        {
            if (OnDown != null)
                OnDown(source);
        }

        public void TriggerUp(VRInput source)
        {
            if (OnTriggerUp != null)
                OnTriggerUp(source);

            TriggerOut(source);
        }


        public void TriggerDown(VRInput source)
        {
            if (OnTriggerDown != null)
                OnTriggerDown(source);
        }

        public void TouchpadUp(VRInput source)
        {
            if (OnTouchpadUp != null)
                OnTouchpadUp(source);
        }


        public void TouchpadDown(VRInput source)
        {
            if (OnTouchpadDown != null)
                OnTouchpadDown(source);
        }

        public void TouchUp(VRInput source)
        {
            if (OnTouchUp != null)
                OnTouchUp(source);
        }


        public void TouchDown(VRInput source)
        {
            if (OnTouchDown != null)
                OnTouchDown(source);
        }
        public void TriggerOver(VRInput source)
        {
            m_nTriggerCount++;
            if (OnAnyTriggerOver != null)
                OnAnyTriggerOver(source);
        }

        public void TriggerOut(VRInput source)
        {
            m_nTriggerCount--;
            if (OnAnyTriggerOut != null)
                OnAnyTriggerOut(source);
        }

        // TODO touch over?
    }
}