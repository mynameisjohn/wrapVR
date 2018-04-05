using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class SmoothFollowRing : MonoBehaviour
    {
        public RingCollider Target;
        [Range(0, 5)]
        public float FollowTime;

        Vector3 vel;
        void Update()
        {
            if (Target)
            {
                Vector3 v3Target = Target.ClosestPoint(transform.position);
                transform.position = Vector3.SmoothDamp(transform.position, v3Target, ref vel, FollowTime);
            }
        }
    }
}