using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class SmoothFollowRing : MonoBehaviour
    {
        public bool XZOnly = false;
        public bool UseColliderIfPresent = true;

        public RingCollider Target;
        [Range(0, 90)]
        public float FollowTime;

        [Tooltip("Should we stay on the boundary of the ring?")]
        public bool KeepOnBounds = true;

        public float targetDistance { get { return Target ? Vector3.Distance(transform.position, v3Target) : 0f; } }
        public float targetDistanceSq { get { return Target ? Vector3.SqrMagnitude(transform.position - v3Target) : 0f; } }

        Vector3 m_Vel;
        Collider m_LocalCollider;

        private void Start()
        {
            if (UseColliderIfPresent)
                m_LocalCollider = GetComponent<Collider>();
        }

        Vector3 v3Target;
        void Update()
        {
            if (Target)
            {
                v3Target = Target.ClosestPoint(transform.position, KeepOnBounds, m_LocalCollider);
                if (XZOnly)
                    v3Target.y = transform.position.y;
                else
                {
                    RaycastHit hitInfo = new RaycastHit();
                    if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, Target.Height, Target._LayerMask.value))
                    {
                        v3Target.y += Target.Height - hitInfo.distance;
                    }
                }

                transform.position = Vector3.SmoothDamp(transform.position, v3Target, ref m_Vel, FollowTime);
            }
        }
    }
}