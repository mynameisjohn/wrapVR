using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class Grabbable : MonoBehaviour
    {
        [Tooltip("How quickly we follow the input direction")]
        [Range(0.1f, 10f)]
        public float _FollowSpeed;

        [Tooltip("How far the object should go in front of the input")]
        [Range(1f, 1000f)]
        public float _PullDistance = 10f;

        [Tooltip("Optional force to add to object when released")]
        [Range(0f, 1f)]
        public float _ImpulseOnRelease = 0;
        
        [Tooltip("Input Activation to trigger the grab")]
        public EActivation _Activation = EActivation.GRIP;

        // These are invoked when we get grabbed / released
        public System.Action<Grabbable, VRRayCaster> OnGrab;
        public System.Action<Grabbable, VRRayCaster> OnRelease;

        public Transform _GrabbableTransform;
        Transform _inputFollow;        // We smooth follow this transform
        Vector3 _targetVelocity;       // Current vel, used for smooth follow
        VRRayCaster _grabbingRC;       // The raycaster that's grabbing us
        Rigidbody _rigidBody;          // Our object's rigid body - optional

        public Transform _FollowOverride;

        public Transform followed { get { return _FollowOverride ? _FollowOverride.transform : _inputFollow ? _inputFollow.transform : null; } }

        public float _SleepIfGrabbedFor = 0.1f;
        public float _IncreasingInertia = 0.1f;
        public float _DecreasingInertia = 0.05f;
        Vector3 _currentVel, _accelleration;

        float _timeGrabbed;

        // Use this for initialization
        void Start()
        {
            if (_GrabbableTransform == null)
                _GrabbableTransform = transform;

            var grabPoints = new List<GameObject>() { gameObject };
            if (_GrabbableTransform && _GrabbableTransform != transform)
                grabPoints.Add(_GrabbableTransform.gameObject);

            foreach (var v in grabPoints)
                Util.EnsureComponent<VRInteractiveItem>(v).ActivationDownCallback(_Activation, Attach, true);

            // Get rigid body if we have one
            _rigidBody = _GrabbableTransform.GetComponent<Rigidbody>();
        }

        public bool isGrabbed { get { return _inputFollow; } }

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
            _inputFollow = new GameObject("GrabFollow").transform;

            Vector3 v3PullDist = _PullDistance * rc.FromTransform.forward.normalized;
            _inputFollow.transform.position = rc.FromTransform.position + v3PullDist;
            _inputFollow.transform.parent = rc.FromTransform;

            // Subscribe to this input's OnTriggerUp and cache
            // it so that we can unsubscribe from OnTriggerUp
            _grabbingRC = rc;
            rc.ActivationUpCallback(_Activation, Detach, true);

            if (_grabbingRC._Log)
                Debug.Log(name + " was grabbed by " + _grabbingRC.name);

            rc._onGrab(this);
            if (OnGrab != null)
                OnGrab(this, rc);

            _timeGrabbed = Time.time;
        }

        // On trigger up, if we have a follow object and our input is its parent
        // (which is guaranteed, I think), then destroy the tracked object and unsubscribe
        private void Detach(VRRayCaster rc)
        {
            // Destroy tracked object
            if (followed)
            {
                _grabbingRC._onRelease(this);

                Destroy(_inputFollow.gameObject);
                _inputFollow = null;
                _FollowOverride = null;

                if (OnRelease != null)
                    OnRelease(this, _grabbingRC);

                if (_rigidBody)
                {
                    float timeSpentGrabbed = Time.time - _timeGrabbed;
                    if (timeSpentGrabbed < _SleepIfGrabbedFor)
                        _rigidBody.Sleep();
                    else if (_rigidBody && _ImpulseOnRelease > 0)
                        _rigidBody.AddForce(_ImpulseOnRelease * _rigidBody.velocity, ForceMode.Impulse);
                }

                _timeGrabbed = -1f;
                _currentVel = Vector3.zero;
                _accelleration = Vector3.zero;
            }

            // Unsubscribe if we've assigned this (only assigned when we subscribe)
            if (_grabbingRC)
            {
                if (_grabbingRC._Log)
                    Debug.Log(name + " was released by " + _grabbingRC.name);
                _grabbingRC.ActivationUpCallback(_Activation, Detach, false);
                _grabbingRC = null;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // If we're following an object
            if (followed)
            {
                // Smooth follow the object
                Vector3 v3Target = Vector3.SmoothDamp(_GrabbableTransform.position, followed.position, ref _targetVelocity, _FollowSpeed);
                bool increasing = (_targetVelocity.sqrMagnitude > _currentVel.sqrMagnitude);
                _currentVel = Vector3.SmoothDamp(_currentVel, _targetVelocity, ref _accelleration, increasing ?  _IncreasingInertia : _DecreasingInertia);

                // Update object velocity with smoothed value
                if (_rigidBody)
                    _rigidBody.velocity = _currentVel;
                // Move position to smooth target
                // This looks ok while grabbing, but on release the object freezes
                else
                    _GrabbableTransform.position = v3Target;
            }
        }
    }
}