using System;
using UnityEngine;

namespace wrapVR
{
    // This class should be added to any gameobject in the scene
    // that should react to input based on the user's gaze.
    // It contains events that can be subscribed to by classes that
    // need to know about input specifics to this gameobject.
    public class VRInteractiveItem : MonoBehaviour
    {
        public event Action OnGazeOver;             // Called when the gaze moves over this object
        public event Action OnGazeOut;              // Called when the gaze leaves this object
        public event Action OnPointerOver;             // Called when the gaze moves over this object
        public event Action OnPointerOut;              // Called when the gaze leaves this object
        public event Action OnAutoBeamOver;             // Called when the gaze moves over this object
        public event Action OnAutoBeamOut;              // Called when the gaze leaves this object
        public event Action OnClick;            // Called when click input is detected whilst the gaze is over this object.
        public event Action OnDoubleClick;      // Called when double click input is detected whilst the gaze is over this object.
        public event Action OnUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event Action OnDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event Action OnTriggerUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event Action OnTriggerDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event Action OnTouchpadUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event Action OnTouchpadDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event Action OnTouchUp;               // Called when Fire1 is released whilst the gaze is over this object.
        public event Action OnTouchDown;             // Called when Fire1 is pressed whilst the gaze is over this object.
        public event Action OnAnyTriggerOver;
        public event Action OnAnyTriggerOut;


        protected bool m_IsGazeOver;
        protected bool m_IsPointerOver;
        protected bool m_IsAutoBeamOver;

        Color m_RayColor;

        public bool IsGazeOver
        {
            get { return m_IsGazeOver; }              // Is the gaze currently over this object?
        }
        public bool IsPointerOver
        {
            get { return m_IsPointerOver; }              // Is the gaze currently over this object?
        }
        public bool IsAutoBeamOver
        {
            get { return m_IsAutoBeamOver; }              // Is the gaze currently over this object?
        }

        // The below functions are called by the VREyeRaycaster when the appropriate input is detected.
        // They in turn call the appropriate events should they have subscribers.
        public void GazeOver()
        {
            m_IsGazeOver = true;

            if (OnGazeOver != null)
                OnGazeOver();
        }

        public void PointerOver()
        {
            m_IsPointerOver = true;

            if (OnPointerOver != null)
                OnPointerOver();
        }

        public void AutoBeamOver()
        {
            m_IsAutoBeamOver = true;

            if (OnAutoBeamOver != null)
                OnAutoBeamOver();

            TriggerOver();
        }

        public void GazeOut()
        {
            m_IsGazeOver = false;

            if (OnGazeOut != null)
                OnGazeOut();
        }

        public void PointerOut()
        {
            m_IsPointerOver = false;
            if (OnPointerOut != null)
                OnPointerOut();
        }

        public void AutoBeamOut()
        {
            m_IsAutoBeamOver = false;

            if (OnAutoBeamOut != null)
                OnAutoBeamOut();

            TriggerOut();
        }

        public void Click()
        {
            if (OnClick != null)
                OnClick();
        }


        public void DoubleClick()
        {
            if (OnDoubleClick != null)
                OnDoubleClick();
        }


        public void Up()
        {
            if (OnUp != null)
                OnUp();
        }


        public void Down()
        {
            if (OnDown != null)
                OnDown();
        }

        public void TriggerUp()
        {
            if (OnTriggerUp != null)
                OnTriggerUp();

            TriggerOut();
        }


        public void TriggerDown()
        {
            if (OnTriggerDown != null)
                OnTriggerDown();
        }

        public void TouchpadUp()
        {
            if (OnTouchpadUp != null)
                OnTouchpadUp();
        }


        public void TouchpadDown()
        {
            if (OnTouchpadDown != null)
                OnTouchpadDown();
        }

        public void TouchUp()
        {
            if (OnTouchUp != null)
                OnTouchUp();
        }


        public void TouchDown()
        {
            if (OnTouchDown != null)
                OnTouchDown();
        }
        public void TriggerOver()
        {
            GetComponent<MeshRenderer>().material.color = UnityEngine.Random.ColorHSV();
            if (OnAnyTriggerOver != null)
                OnAnyTriggerOver();
        }

        public void TriggerOut()
        {
            if (OnAnyTriggerOut != null)
                OnAnyTriggerOut();
        }

        public Color GetRayColor()
        {
            return m_RayColor;
        }

        public void SetRayColor(Color raycolor)
        {
            m_RayColor = raycolor;
        }
    }
}