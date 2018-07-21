using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class EditorCameraEmulator : MonoBehaviour
    {
        public float _RotationSpeed = 2.0F;

        float _pitch;
        float _yaw;
        bool _captureMouse = false;

        [HideInInspector]
        public float Speed = 0f;

        void Update()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _captureMouse = !_captureMouse;
                if (_captureMouse)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.None;
                }

            }
            if (_captureMouse)
            {
                _pitch += _RotationSpeed * Input.GetAxis("Mouse Y");
                _yaw += _RotationSpeed * Input.GetAxis("Mouse X");

                _pitch = Mathf.Clamp(_pitch, -90f, 90f);

                _yaw %= 360f;

                transform.eulerAngles = new Vector3(-_pitch, _yaw, 0f);
            }

            if (Input.GetKey(KeyCode.W))
                transform.position += Speed * transform.forward.normalized;
            if (Input.GetKey(KeyCode.S))
                transform.position -= Speed * transform.forward.normalized;
            if (Input.GetKey(KeyCode.D))
                transform.position += Speed * transform.right.normalized;
            if (Input.GetKey(KeyCode.A))
                transform.position -= Speed * transform.right.normalized;
        }
    }
}