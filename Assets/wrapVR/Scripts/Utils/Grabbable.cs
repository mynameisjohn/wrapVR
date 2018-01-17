using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(VRInteractiveItem))]
    public class Grabbable : MonoBehaviour
    {
        [Tooltip("How quickly we follow the input direction")]
        [Range(0.1f, 10f)]
        public float FollowSpeed;

        [Tooltip("How far the object should go in front of the input")]
        [Range(1f, 1000f)]
        public float PullDistance;
        
        // These are invoked when we get grabbed / released
        public System.Action<Grabbable, VRInput> OnGrab;
        public System.Action<Grabbable, VRInput> OnRelease;

        Transform m_InputFollow;        // We smooth follow this transform
        Vector3 m_v3CurrentVelocity;    // Current vel, used for smooth follow
        VRInput m_ActiveInput;          // VR input that is controlling us
        Rigidbody m_RigidBody;          // Our object's rigid body - optional

        public Transform Followed { get { return m_InputFollow.transform; } }

        // Use this for initialization
        void Start()
        {
            // When our object is triggered we begin the grab
            GetComponent<VRInteractiveItem>().OnTriggerDown += Attach;

            // Get rigid body if we have one
            m_RigidBody = GetComponent<Rigidbody>();
        }

        // On grab we create an object to follow at our position
        // but as a child of the input - when the input moves, 
        // our tracked object moves with it and we respond
        private void Attach(VRInput input)
        {
            // Detach, just in case
            Detach();

            // Create object to follow at our pull distance from the input
            m_InputFollow = new GameObject("GrabFollow").transform;

            Vector3 v3PullDist = PullDistance * input.transform.forward.normalized;
            m_InputFollow.transform.position = input.transform.position + v3PullDist;
            m_InputFollow.transform.parent = input.transform;

            // Subscribe to this input's OnTriggerUp and cache
            // it so that we can unsubscribe from OnTriggerUp
            m_ActiveInput = input;
            input.OnTriggerUp += Detach;

            if (OnGrab != null)
                OnGrab(this, input);
        }

        // On trigger up, if we have a follow object and our input is its parent
        // (which is guaranteed, I think), then destroy the tracked object and unsubscribe
        private void Detach()
        {
            // Destroy tracked object
            if (m_InputFollow)
            {
                if (OnRelease != null)
                    OnRelease(this, m_ActiveInput);

                Destroy(m_InputFollow.gameObject);
                m_InputFollow = null;
            }

            // Unsubscribe if we've assigned this (only assigned when we subscribe)
            if (m_ActiveInput)
            {
                m_ActiveInput.OnTriggerUp -= Detach;
                m_ActiveInput = null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // If we're following an object
            if (m_InputFollow)
            {
                // Smooth follow the object
                Vector3.SmoothDamp(transform.position, m_InputFollow.transform.position, ref m_v3CurrentVelocity, FollowSpeed);

                // Update object velocity with smoothed value
                if (m_RigidBody)
                    m_RigidBody.velocity = m_v3CurrentVelocity;
            }
        }
    }
}