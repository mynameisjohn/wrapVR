using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(TextMesh))]
    public class FPSCounter : MonoBehaviour
    {
        public float _SmoothFactor = 0.1f;

        float _dT;

        void Update()
        {
            _dT += (Time.deltaTime - _dT) * _SmoothFactor;
            float FPS = 1f / _dT;
            GetComponent<TextMesh>().text = (Mathf.FloorToInt(FPS)) + " FPS";
        }
    }
}