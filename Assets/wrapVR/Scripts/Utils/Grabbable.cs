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
        public float PullDistance = 10f;

        [Tooltip("How far the object should go in front of the input")]
        [Range(0f, 1f)]
        public float ImpulseOnRelease = 0;
        
        // These are invoked when we get grabbed / released
        public System.Action<Grabbable, VRRayCaster> OnGrab;
        public System.Action<Grabbable, VRRayCaster> OnRelease;

        public EActivation Activation = EActivation.GRIP;

        public Transform _GrabbableTransform;
        Transform m_InputFollow;        // We smooth follow this transform
        Vector3 m_v3CurrentVelocity;    // Current vel, used for smooth follow
        VRRayCaster m_GrabbingRC;      // The raycaster that's grabbing us
        Rigidbody m_RigidBody;          // Our object's rigid body - optional

        public Transform _FollowOverride;

        public Transform Followed { get { return _FollowOverride ? _FollowOverride.transform : m_InputFollow ? m_InputFollow.transform : null; } }

        // Use this for initialization
        void Start()
        {
            // When our object is triggered we begin the grab
            Util.EnsureComponent<VRInteractiveItem>(gameObject).ActivationDownCallback(Activation, Attach, true);

            if (_GrabbableTransform == null)
                _GrabbableTransform = transform;

            // Get rigid body if we have one
            m_RigidBody = _GrabbableTransform.GetComponent<Rigidbody>();
        }

        public bool isGrabbed { get { return m_InputFollow; } }

        // On grab we create an object to follow at our position
        // but as a child of the input - when the input moves, 
        // our tracked object moves with it and we respond
        public void Attach(VRRayCaster rc)
        {
            if (!VRCapabilityManager.canGrabMultiple && rc.isGrabbing)
                return;

            // Detach, just in case
            Detach(rc);

            // Create object to follow at our pull distance from the input
            m_InputFollow = new GameObject("GrabFollow").transform;

            Vector3 v3PullDist = PullDistance * rc.FromTransform.forward.normalized;
            m_InputFollow.transform.position = rc.FromTransform.position + v3PullDist;
            m_InputFollow.transform.parent = rc.FromTransform;

            // Subscribe to this input's OnTriggerUp and cache
            // it so that we can unsubscribe from OnTriggerUp
            m_GrabbingRC = rc;
            rc.ActivationUpCallback(Activation, Detach, true);

            rc._onGrab(this);
            if (OnGrab != null)
                OnGrab(this, rc);
        }

        // On trigger up, if we have a follow object and our input is its parent
        // (which is guaranteed, I think), then destroy the tracked object and unsubscribe
        private void Detach(VRRayCaster rc)
        {
            // Destroy tracked object
            if (Followed)
            {
                m_GrabbingRC._onRelease(this);

                if (OnRelease != null)
                    OnRelease(this, m_GrabbingRC);

                Destroy(m_InputFollow.gameObject);
                m_InputFollow = null;
                _FollowOverride = null;

                if (m_RigidBody)
                {
                    if (m_RigidBody && ImpulseOnRelease > 0)
                        m_RigidBody.AddForce(ImpulseOnRelease * m_RigidBody.velocity, ForceMode.Impulse);
                }
            }

            // Unsubscribe if we've assigned this (only assigned when we subscribe)
            if (m_GrabbingRC)
            {
                m_GrabbingRC.ActivationUpCallback(Activation, Detach, false);
                m_GrabbingRC = null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // If we're following an object
            if (Followed)
            {
                // Smooth follow the object
                Vector3 v3Target = Vector3.SmoothDamp(_GrabbableTransform.position, Followed.position, ref m_v3CurrentVelocity, FollowSpeed);

                // Update object velocity with smoothed value
                if (m_RigidBody)
                    m_RigidBody.velocity = m_v3CurrentVelocity;
                // Move position to smooth target
                // This looks ok while grabbing, but on release the object freezes
                else
                    _GrabbableTransform.position = v3Target;
            }
        }
    }
}