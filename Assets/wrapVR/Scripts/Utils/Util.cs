using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public enum EActivation
    {
        NONE,
        TOUCH,
        TOUCHPAD,
        TRIGGER
    };

    public class Util : MonoBehaviour
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
            return comp;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
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
}