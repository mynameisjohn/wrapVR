using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class FollowRingOnTeleport : MonoBehaviour
    {
        [Tooltip("Teleporting object - should have a ring collider component")]
        public RayCastTeleport RingTargetTeleporter;

        [Tooltip("Should we face the ring collider object on teleport?")]
        public bool FaceRing = true;

        [Tooltip("Should we stay on the boundary of the ring?")]
        public bool KeepOnBounds = true;

        RingCollider m_RingTarget;

        // Use this for initialization
        void Start()
        {
            RingTargetTeleporter = Util.DestroyEnsureComponent(gameObject, RingTargetTeleporter);
            m_RingTarget = Util.DestroyEnsureComponent<RingCollider>(RingTargetTeleporter.gameObject);
            RingTargetTeleporter.OnTeleport += onTeleport;
        }

        void onTeleport()
        {
            // If we want to face the ring when we finish teleporting, we've got to 
            // position ourselves such that the current SDK head's forward points at
            // the center. To do this negate the XZ forward direction and find where
            // the point on the ring would be from the center in that direction
            if (FaceRing)
            {
                Vector3 v3EyeFwd = VRCapabilityManager.CameraRig.transform.forward;
                transform.position = m_RingTarget.PointFromOrientation(-new Vector2(v3EyeFwd.x, v3EyeFwd.z), GetComponent<Collider>());
            }
            // Otherwise just move
            else
            {
                transform.position = m_RingTarget.ClosestPoint(transform.position, KeepOnBounds, GetComponent<Collider>());
            }
        }
    }
}