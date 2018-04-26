using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularVelocityTracker : MonoBehaviour
{
    Quaternion m_qLast; // last rotation

    Quaternion m_qLastRot;
    float m_fRotationDelta;
    Vector3 m_v3RotationAxis;

    public Vector3 AngularVelocity
    {
        get
        {
            return (m_fRotationDelta * m_v3RotationAxis) / Time.fixedDeltaTime;
        }
    }

    public Vector3 AngularVelocityAxis
    {
        get
        {
            return m_v3RotationAxis;
        }
    }
    public float AngularVelocityMagnitude
    {
        get
        {
            return m_fRotationDelta;
        }
    }


    void Start()
    {
        m_qLast = transform.rotation;
    }

    void FixedUpdate()
    {
        Quaternion qDelta = transform.rotation * Quaternion.Inverse(m_qLastRot);
        qDelta.ToAngleAxis(out m_fRotationDelta, out m_v3RotationAxis);
        Vector3 v3AngularVelDir = Vector3.Cross(AngularVelocityAxis.normalized, transform.forward.normalized);
        m_qLastRot = transform.rotation;
    }
}
