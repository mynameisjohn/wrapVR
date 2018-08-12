﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Different ways of activating a controller
    public enum EActivation
    {
        NONE,
        TOUCH,
        TOUCHPAD,
        TRIGGER, 
        GRIP,
        GAZE
    };

    //Swipe directions
    public enum SwipeDirection
    {
        NONE,
        UP,
        DOWN,
        LEFT,
        RIGHT
    };

    // Different kinds of VR inputs
    public enum InputType
    {
        LEFT,
        RIGHT,
        GAZE, 
        NONE
    }

    public class Util
    {
        public static float remap(float v, float m1, float m2, float m3, float m4)
        {
            return (((v - m1) / (m2 - m1)) * (m4 - m3) + m3);
        }

        // Make sure we're using positive angles. if it's negative, add 360 to make it positive.
        public static float EnsurePositiveAngle(float inputAngle)
        {
            if (inputAngle < 0) { inputAngle += 360; }
            return inputAngle;
        }

        public static T DestroyEnsureComponent<T>(GameObject gameObject, T possible = null) where T : Component
        {
            if (possible != null)
                return possible;
            T comp = gameObject.GetComponent<T>();
            if (comp == null && gameObject.transform.parent != null)
                comp = gameObject.transform.parent.GetComponent<T>();
            if (comp == null)
                comp = gameObject.GetComponentInChildren<T>();
            if (comp == null)
                GameObject.Destroy(gameObject);
            return comp;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static void RemoveInvalidCasters(List<VRRayCaster> liRayCasters)
        {
            for (int i = liRayCasters.Count - 1; i >= 0; i--)
            {
                if (liRayCasters[i] == null || liRayCasters[i].Input == null)
                    liRayCasters.RemoveAt(i);
            }
        }

        // https://answers.unity.com/questions/740055/can-i-move-component-of-one-gameobject-to-other.html
        public static T CopyComponent<T>(T sourceComp, T targetComp) where T : Component
        {
            if (!(sourceComp && targetComp))
                return null;

            System.Reflection.FieldInfo[] sourceFields = sourceComp.GetType().GetFields(System.Reflection.BindingFlags.Public |
                                                             System.Reflection.BindingFlags.NonPublic |
                                                             System.Reflection.BindingFlags.Instance);
            int i = 0;
            for (i = 0; i < sourceFields.Length; i++)
            {
                var value = sourceFields[i].GetValue(sourceComp);
                sourceFields[i].SetValue(targetComp, value);
            }
            return targetComp;
        }
        public static T CopyAddComponent<T>(GameObject src, GameObject dst) where T : Component
        {
            Util.EnsureComponent<T>(dst);
            return CopyComponent(src.GetComponent<T>(), dst.GetComponent<T>());
        }

        public static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            if (!gameObject.GetComponent<T>())
            {
                gameObject.AddComponent<T>();
            }
            return gameObject.GetComponent<T>();
        }

        public static Vector3[] GetCurve(Vector3 origin, Vector3 influence, Vector3 destination, int pointCount)
        {
            Vector3[] result = new Vector3[pointCount];
            if (pointCount == 0)
                return result;

            result[0] = origin;
            for (int i = 1; i < pointCount - 1; i++)
            {
                float percent = (1f / pointCount) * i;
                Vector3 point1 = Vector3.Lerp(origin, influence, percent);
                Vector3 point2 = Vector3.Lerp(influence, destination, percent);
                result[i] = Vector3.Lerp(point1, point2, percent);
            }
            result[pointCount - 1] = destination;

            return result;
        }
    }

    [System.Serializable]
    public class CurveBetweenTwo
    {
        public CurveBetweenTwo(AnimationCurve curve)
        {
            _curve = curve;
        }

        // This should go between 0 and 1 in both axes
        [Tooltip("Weight of curve between first and second target")]
        public AnimationCurve _curve;

        // Curve goes from start to end1 / end2 based on curve shape
        Transform m_tStart, m_tEnd1, m_tEnd2;

        // Start drawing the curve
        public void ActivateCurve(Transform tStart, Transform tEnd1, Transform tEnd2)
        {
            DeactivateCurve();
            if (tStart && tEnd1 && tEnd2)
            {
                m_tStart = tStart;
                m_tEnd1 = tEnd1;
                m_tEnd2 = tEnd2;
            }
        }

        // Turn the curve off
        public void DeactivateCurve()
        {
            m_tStart = null;
            m_tEnd1 = null;
            m_tEnd2 = null;
        }

        public Vector3 Evaluate(float fX)
        {
            float fCurve = _curve.Evaluate(fX);
            Vector3 v3Start = m_tStart.transform.position;
            Vector3 v3End1 = m_tEnd1.transform.position;
            Vector3 v3End2 = m_tEnd2.transform.position;

            // The lines from start to p1/2 are straight
            Vector3 p1 = Vector3.Lerp(m_tStart.transform.position, v3End1, fX);
            Vector3 p2 = Vector3.Lerp(v3Start, v3End2, fX);

            // But we mix them for the final point
            return Vector3.Lerp(p1, p2, fCurve);
        }
    }
}