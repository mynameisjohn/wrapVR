using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class EditorVRInput : VRInput
    {
        protected Vector2 m_MouseDownPosition;                        // The screen position of the mouse when Fire1 is pressed.
        protected Vector2 m_MouseUpPosition;                          // The screen position of the mouse when Fire1 is released.

        public bool isTouching { get { return m_nTouchCount > 0; } }
        protected int m_nTouchCount;

        protected bool m_bIsTouchPressed;
        public bool isTouchPressed { get { return m_bIsTouchPressed; } }

        protected Vector2 m_CurrentTouchPosition;
        protected Vector2 m_TouchDownPosition;                        // The touch position when the touchpad is touched.
        protected Vector2 m_TouchUpPosition;                          // The touch position when the touchpad is untouched.
        protected Vector2 m_TouchpadDownPosition;                     // The touch position when the touchpad is pressed.
        protected Vector2 m_TouchpadUpPosition;                       // The touch position when the touchpad is released.
        protected float m_LastMouseUpTime;                            // The time when Fire1 was last released.

        protected float m_LastHorizontalValue;                        // The previous value of the horizontal axis used to detect keyboard swipes.
        protected float m_LastVerticalValue;                          // The previous value of the vertical axis used to detect keyboard swipes.

        int ixMouse { get { return Type == InputType.LEFT ? 1 : 0; } }

        protected override void CheckInput()
        {
            if (Input.GetKeyDown(KeyCode.Keypad6) && Input.GetKey(KeyCode.Keypad4))
            {
                _onSwipe(SwipeDirection.RIGHT);
            }
            if (Input.GetKeyDown(KeyCode.Keypad4) && Input.GetKey(KeyCode.Keypad6))
            {
                _onSwipe(SwipeDirection.LEFT);
            }
            if (Input.GetKeyDown(KeyCode.Keypad8) && Input.GetKey(KeyCode.Keypad2))
            {
                _onSwipe(SwipeDirection.UP);
            }
            if (Input.GetKeyDown(KeyCode.Keypad2) && Input.GetKey(KeyCode.Keypad8))
            {
                _onSwipe(SwipeDirection.DOWN);
            }

            if (!VRCapabilityManager.IsGazeFallback)
            {
                // If not gaze fallback use mouse button 0 as trigger
                if (Input.GetMouseButtonDown(ixMouse))
                {
                    _onTriggerDown();
                }
                if (Input.GetMouseButtonUp(ixMouse))
                {
                    _onTriggerUp();
                }
            }

            // Treat escape as cancel
            if (Input.GetKeyDown(KeyCode.Escape))
                _onCancel();

            /////////////////////////////
            // Touchpad stuff on PC
            // Numpad maps to touchpad touch with 5 at center
            // (pressing 5 sets position to zero, using numpad
            // enter to "click")
            for (KeyCode k = KeyCode.Keypad1; k <= KeyCode.Keypad9; k++)
            {
                if (Input.GetKeyDown(k) || Input.GetKeyUp(k))
                {
                    if (Input.GetKeyDown(k))
                    {
                        if (k == KeyCode.Keypad5)
                        {
                            m_CurrentTouchPosition = new Vector2();
                        }
                        else
                        {
                            int ixSlice = new Dictionary<KeyCode, int>{
                            { KeyCode.Keypad6, 0},
                            { KeyCode.Keypad9, 1},
                            { KeyCode.Keypad8, 2},
                            { KeyCode.Keypad7, 3},
                            { KeyCode.Keypad4, 4},
                            { KeyCode.Keypad1, 5},
                            { KeyCode.Keypad2, 6},
                            { KeyCode.Keypad3, 7 },
                        }[k];
                            float fAngle = ixSlice * Mathf.Deg2Rad * 360 / 8;
                            m_CurrentTouchPosition = new Vector2(Mathf.Cos(fAngle), Mathf.Sin(fAngle));
                        }

                        m_nTouchCount++;
                        if (m_nTouchCount == 1)
                        {
                            m_TouchDownPosition = m_CurrentTouchPosition;
                            _onTouchpadDown();
                            if (VRCapabilityManager.IsGazeFallback)
                                _onTriggerDown();
                        }
                    }
                    else
                    {
                        m_nTouchCount--;
                        if (m_nTouchCount == 0)
                        {
                            m_TouchUpPosition = m_CurrentTouchPosition;
                            _onTouchpadUp();
                            if (VRCapabilityManager.IsGazeFallback)
                                _onTriggerUp();
                        }
                    }
                    // Debug.Log( m_nTouchCount );
                    break;
                }
            }

            // Enter is touch click
            if (Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                m_bIsTouchPressed = true;
                _onTouchpadDown();
            }
            else if (Input.GetKeyUp(KeyCode.KeypadEnter))
            {
                m_bIsTouchPressed = false;
                _onTouchpadUp();
            }

            // Second touch option - press control to do a 
            // touchpad down and return to click. The arrow keys
            // control position once the touchpad is down
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                m_nTouchCount++;
                if (m_nTouchCount == 1)
                {
                    m_TouchDownPosition = m_CurrentTouchPosition;
                    _onTouchpadDown();
                    if (VRCapabilityManager.IsGazeFallback)
                        _onTriggerDown();
                }
            }
            else if (Input.GetKeyUp(KeyCode.RightControl))
            {
                m_nTouchCount--;
                if (m_nTouchCount == 0)
                {
                    m_TouchUpPosition = m_CurrentTouchPosition;
                    _onTouchpadUp();
                    if (VRCapabilityManager.IsGazeFallback)
                        _onTriggerUp();
                }
            }

            if (isTouching)
            {
                // Arrow keys for touchpad input
                float fMoveSpeed = 0.025f;
                Dictionary<KeyCode, Vector2> diKeyToTranslate = new Dictionary<KeyCode, Vector2>()
                {
                    { KeyCode.LeftArrow, new Vector2(-fMoveSpeed,0) },
                    { KeyCode.RightArrow, new Vector2(fMoveSpeed,0) },
                    { KeyCode.DownArrow, new Vector2(0, -fMoveSpeed) },
                    { KeyCode.UpArrow, new Vector2(0, fMoveSpeed) },
                };
                foreach (KeyValuePair<KeyCode, Vector2> kv in diKeyToTranslate)
                {
                    if (Input.GetKey(kv.Key))
                        m_CurrentTouchPosition = Vector2.ClampMagnitude(m_CurrentTouchPosition + kv.Value, 1);
                }

                // Enter is touch click
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    m_bIsTouchPressed = true;
                    _onTouchpadDown();
                }
                else if (Input.GetKeyUp(KeyCode.Return))
                {
                    m_bIsTouchPressed = false;
                    _onTouchpadUp();
                }
            }
        }

        public override Vector2 GetTouchPosition()
        {
            return m_CurrentTouchPosition;
        }

        public override bool GetTrigger()
        {
            // If gaze fallback count touch as trigger, otherwise use mouse button
            if (VRCapabilityManager.IsGazeFallback)
            {
                return isTouching;
            }
            else
            {
                return Input.GetMouseButton(ixMouse);
            }
        }

        public override bool GetTouchpadTouch()
        {
            return isTouching;
        }

        public override bool GetTouchpad()
        {
            return m_bIsTouchPressed;
        }

        protected override void HandleTouchHandler(object sender, System.EventArgs e)
        {
            // ? Seems like this is for swipe, which I can live without
        }
        public override SwipeDirection GetHMDTouch()
        {
            // more swipe stuff
            return SwipeDirection.NONE;
        }

        // Use this for initialization
        void Start()
        {

        }
    }
}