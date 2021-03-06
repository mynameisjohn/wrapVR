﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class EditorVRInput : VRInput
    {
        protected int _touchCount;                            // The current count of touches
        protected Vector2 _currentTouchPos;                     // The current touch position
        protected Vector2 _touchDownPos;                        // The touch position when the touchpad is touched.
        protected Vector2 _touchUpPos;                          // The touch position when the touchpad is untouched.
        protected Vector2 _touchPadDownPos;                     // The touch position when the touchpad is pressed.
        protected Vector2 _touchPadUpPos;                       // The touch position when the touchpad is released.
        protected float _lastMouseUpTime;                       // The time when Fire1 was last released.
        protected float _lastHorizontalValue;                   // The previous value of the horizontal axis used to detect keyboard swipes.
        protected float _lastVerticalValue;                     // The previous value of the vertical axis used to detect keyboard swipes.

        public override InputControllerRenderers getController()
        {
            return GetComponent<EditorControllerRenderers>();
        }

        // Returns true if any touch points are down
        public bool isTouching { get { return _touchCount > 0; } }  // 

        // Returns true if the touchpad is pressed (clicked) in
        public bool isTouchPressed { get; protected set; }

        // Used internally if we are the right or left hand
        int ixMouse { get { return Type == InputType.LEFT ? 1 : 0; } }
        
        // Touch state management
        void incTouch()
        {
            _touchCount++;
            if (_touchCount == 1)
            {
                _touchDownPos = _currentTouchPos;
                _onTouchDown();

                // For gaze fallback treat touch as grip and trigger
                if (VRCapabilityManager.IsGazeFallback)
                {
                    _onTriggerDown();
                    _onGripDown();
                }
            }
        }
        void decTouch()
        {
            if (_touchCount == 0)
                return;

            _touchCount--;
            if (_touchCount == 0)
            {
                _touchUpPos = _currentTouchPos;
                _onTouchUp();

                // For gaze fallback treat touch as grip and trigger
                if (VRCapabilityManager.IsGazeFallback)
                {
                    _onTriggerUp();
                    _onGripUp();
                }
            }
        }

        bool m_bIsHandActive = false;
        public override void Init()
        {
            m_bIsHandActive = (Type != InputType.LEFT);
        }

        // We only activate one hand at a time
        // (otherwise you'll get a lot of double casts)
        public bool IsHandActive { get { return m_bIsHandActive; } }

        protected override void CheckInput()
        {
            if (Type == InputType.LEFT && Input.GetKeyDown(KeyCode.LeftAlt))
                m_bIsHandActive = !m_bIsHandActive;
            else if (Type == InputType.RIGHT && Input.GetKeyDown(KeyCode.RightAlt))
                m_bIsHandActive = !m_bIsHandActive;
            
            if (!m_bIsHandActive)
                return;

            // Swipe emulation
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
                // Left shift for Grip
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    _onGripDown();
                }
                if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    _onGripUp();
                }
            }

            // Treat escape as cancel
            if (Input.GetKeyDown(KeyCode.Escape))
                _onMenuDown();
            else if (Input.GetKeyUp(KeyCode.Escape))
                _onMenuUp();

            // Pressing 0 on the numpad forces any touch state to end
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                _touchCount = 1;
                decTouch();
            }

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
                            _currentTouchPos = new Vector2();
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
                            _currentTouchPos = new Vector2(Mathf.Cos(fAngle), Mathf.Sin(fAngle));
                        }

                        incTouch();
                    }
                    else
                    {
                        decTouch();
                    }
                    // Debug.Log( m_nTouchCount );
                    break;
                }
            }

            // numpad enter and right shift is touch click
            foreach (KeyCode touchPadKey in new KeyCode[] { KeyCode.KeypadEnter, KeyCode.RightShift }) 
            {
                if (Input.GetKeyDown(touchPadKey))
                {
                    incTouch();
                    isTouchPressed = true;
                    _onTouchpadDown();
                }
                else if (Input.GetKeyUp(touchPadKey))
                {
                    decTouch();
                    isTouchPressed = false;
                    _onTouchpadUp();
                }
            }

            // Second touch option - press right control to do a 
            // touchpad down and return to click. The arrow keys
            // control position once the touchpad is down
            if (Input.GetKeyDown(KeyCode.RightControl))
            {
                incTouch();
            }
            else if (Input.GetKeyUp(KeyCode.RightControl))
            {
                decTouch();
            }

            if (isTouching)
            {
                // Arrow keys for touchpad input
                const float fMoveSpeed = 0.0025f;
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
                        _currentTouchPos = Vector2.ClampMagnitude(_currentTouchPos + kv.Value, 1);
                }

                // Enter is touch click
                if (!isTouchPressed && Input.GetKeyDown(KeyCode.Return))
                {
                    isTouchPressed = true;
                    _onTouchpadDown();
                }
                else if (Input.GetKeyUp(KeyCode.Return))
                {
                    isTouchPressed = false;
                    _onTouchpadUp();
                }
                // Swipe emulation
                if (Input.GetKeyDown(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
                {
                    _onSwipe(SwipeDirection.RIGHT);
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow))
                {
                    _onSwipe(SwipeDirection.LEFT);
                }
                if (Input.GetKeyDown(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow))
                {
                    _onSwipe(SwipeDirection.UP);
                }
                if (Input.GetKeyDown(KeyCode.DownArrow) && Input.GetKey(KeyCode.UpArrow))
                {
                    _onSwipe(SwipeDirection.DOWN);
                }
            }
        }

        public override Vector2 GetTouchPosition()
        {
            return _currentTouchPos;
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

        // Grip is left shift
        public override bool GetGrip()
        {
            // If gaze fallback count touch as trigger, otherwise use left shift
            if (VRCapabilityManager.IsGazeFallback)
            {
                return isTouching;
            }
            else
            {
                return Input.GetKey(KeyCode.LeftShift);
            }
        }

        public override bool GetTouch()
        {
            return isTouching;
        }

        public override bool GetTouchpad()
        {
            return isTouchPressed;
        }
    }
}