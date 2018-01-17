using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(Grabbable))]
    [RequireComponent(typeof(CurveBetweenTwo))]
    public class DrawGrabCurve : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            // Draw and Deactivate curve when we are grabbed / released
            GetComponent<Grabbable>().OnGrab += (Grabbable gr, VRInput input) =>
            {
                Transform FollowedTransform = gr.Followed;
                GetComponent<CurveBetweenTwo>().ActivateCurve(input.transform, FollowedTransform, transform);
            };
            GetComponent<Grabbable>().OnRelease += (Grabbable gr, VRInput input) =>
            {
                GetComponent<CurveBetweenTwo>().DeactivateCurve();
            };
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.Space))
            Debug.Break();
        }
    }
}