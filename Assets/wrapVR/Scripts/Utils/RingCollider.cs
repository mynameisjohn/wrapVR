﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class RingCollider : MonoBehaviour
    {
        [Tooltip("The radius of the ring")]
        [Range(0.5f, 10000f)]
        public float Radius;
        [Tooltip("The height of the ring relative to our game object")]
        [Range(0, 1000f)]
        public float Height;

        public SmoothFollowRing ToMatch;

#if UNITY_EDITOR
        // Draw yellow circle indicating our radius
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.yellow;
            if (ToMatch && !Application.isPlaying)
            {
                Radius = Vector2.Distance(CenterXZ, new Vector3(ToMatch.transform.position.x, ToMatch.transform.position.z));
                Height = ToMatch.transform.position.y - transform.position.y;
                UnityEditor.Handles.DrawWireDisc(transform.position + new Vector3(0, Height, 0), Vector3.up, Radius);
            }
            else
            {
                UnityEditor.Handles.DrawWireDisc(transform.position + new Vector3(0, Height, 0), Vector3.up, Radius);
            }
        }
#endif

        private void Start()
        {
            if (ToMatch)
            {
                Radius = Vector2.Distance(CenterXZ, new Vector3(ToMatch.transform.position.x, ToMatch.transform.position.z));
                Height = ToMatch.transform.position.y - transform.position.y;
                ToMatch.Target = this;
            }
        }

        // The 2D center
        public Vector2 CenterXZ
        {
            get
            {
                return new Vector2(transform.position.x, transform.position.z);
            }
        }

        // 3D center, factoring in height
        public Vector3 Center
        {
            get
            {
                return transform.position + new Vector3(0, Height, 0);
            }
        }

        public Vector2 GetXZOffset(Vector3 v3Pos)
        {
            Vector2 v2Src = new Vector2(v3Pos.x, v3Pos.z);
            return (v2Src - CenterXZ);
        }

        // Find closest point from v3Src to us, 
        // optionally use collider to avoid returning a point inside a wall
        public Vector3 ClosestPoint(Vector3 v3Src, bool bKeepOnBounds, Collider colliderToCheck = null)
        {
            // Find XZ offset
            float dX = v3Src.x - transform.position.x;
            float dZ = v3Src.z - transform.position.z;
            Vector2 v2Ofs = new Vector2(dX, dZ);

            // find distance from v3Src to our center
            Vector3 v3Center = Center;
            float fMag = v2Ofs.magnitude;
            float fRadius = bKeepOnBounds ? Radius : Mathf.Min(fMag, Radius);

            // if there are any colliders between the center and v3Src
            // then take them in to account by offsetting the radius
            Vector3 v3Dir = new Vector3(v2Ofs.x, 0, v2Ofs.y) / fMag;
            RaycastHit hit;
            if (Physics.Raycast(v3Center, v3Dir, out hit, fRadius))
            {
                // If we hit something move in radius by extents
                float fDist = Vector3.Distance(hit.point, v3Center);
                if (colliderToCheck != hit.collider)
                    return v3Center + Mathf.Max(fRadius - fDist, 0) * v3Dir;
            }

            return v3Center + fRadius * v3Dir;
        }

        // Get angle along circle for a given point
        public float GetAngleAway(Vector2 v2Dir)
        {
            float fAngle = Mathf.Rad2Deg * Mathf.Atan2(v2Dir.y, v2Dir.x);
            return fAngle;
        }

        public float GetAngleAway(Vector3 v3Dir)
        {
            Vector2 v2FwdXZ = new Vector2(v3Dir.x, v3Dir.z).normalized;
            return GetAngleAway(v2FwdXZ);
        }

        public float GetAngle(Vector3 v3SrcPos)
        {
            Vector2 v2DiffN = GetXZOffset(v3SrcPos).normalized;
            return GetAngleAway(v2DiffN);
        }
        public float GetAngle(Transform t)
        {
            return GetAngle(t.position) + GetAngleAway(t.forward);
        }

        // Find the point on our circle at fAngle degrees
        // optionally use collider to avoid returning a point inside a wall
        public Vector3 PointFromAngle(float fAngle, Collider c = null)
        {
            // Convert angle to radians
            float fRad = Mathf.Deg2Rad * fAngle;

            // No collider, just compute position from angle
            if (c == null)
            {
                Vector2 v2Pos = CenterXZ + Radius * new Vector2(Mathf.Cos(fRad), Mathf.Sin(fRad));
                return new Vector3(v2Pos.x, transform.position.y + Height, v2Pos.y);
            }
            
            // Cast ray in the direction of the point
            Vector3 v3Dir = new Vector3(Mathf.Cos(fRad), 0, Mathf.Sin(fRad));
            float fRadius = Radius;
            Vector3 v3Center = Center;
            RaycastHit hit;
            if (Physics.Raycast(v3Center, v3Dir, out hit, fRadius))
            {
                // If we hit something shrink in distance
                float fDist = Vector3.Distance(hit.point, v3Center);
                float fExtent = Vector3.Dot(v3Dir, c.bounds.extents) + 0.01f;
                fRadius = Mathf.Max(fDist - fExtent, 0f);
            }

            Vector3 v3Pos = v3Center + fRadius * v3Dir;
            return v3Pos;
        }

        public Vector3 PointFromOrientation(Vector2 v2Dir, Collider c = null)
        {
            float fAngle = Mathf.Rad2Deg * Mathf.Atan2(v2Dir.y, v2Dir.x);
            return PointFromAngle(fAngle, c);
        }
    }
}