using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class SmoothFollowRing : MonoBehaviour
    {
        public RingCollider Target;
        [Range(0, 90)]
        public float FollowTime;

        [Tooltip("Should we stay on the boundary of the ring?")]
        public bool KeepOnBounds = true;

        Vector3 m_Vel;

        void Update()
        {
            if (Target)
            {
                Vector3 v3Target = Target.ClosestPoint(transform.position, KeepOnBounds, GetComponent<Collider>());
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, Target.Height))
                {
                    v3Target.y += Target.Height - hitInfo.distance;
                }

                transform.position = Vector3.SmoothDamp(transform.position, v3Target, ref m_Vel, FollowTime);
            }
        }
    }
}