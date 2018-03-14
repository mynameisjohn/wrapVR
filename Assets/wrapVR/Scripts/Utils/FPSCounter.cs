using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(TextMesh))]
    public class FPSCounter : MonoBehaviour
    {
        float m_fDt;
        const float m_fSmooth = 0.1f;
        void Update()
        {
            m_fDt += (Time.deltaTime - m_fDt) * m_fSmooth;
            float FPS = 1f / m_fDt;
            GetComponent<TextMesh>().text = (Mathf.FloorToInt(FPS)) + " FPS";
        }
    }
}