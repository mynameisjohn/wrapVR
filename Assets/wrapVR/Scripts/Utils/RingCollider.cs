using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class RingCollider : MonoBehaviour
    {
        [Range(0.5f, 1000f)]
        public float Radius;
        public bool KeepOnBounds = true;
        public Vector3 ClosestPoint(Vector3 v3Src)
        {
            float dX = v3Src.x - transform.position.x;
            float dZ = v3Src.z - transform.position.z;
            Vector2 v2Ofs = new Vector2(dX, dZ);
            if (KeepOnBounds || v2Ofs.sqrMagnitude > Radius * Radius)
                v2Ofs = Radius * v2Ofs.normalized;
            Vector3 v3Ret = transform.position + new Vector3(v2Ofs.x, 0, v2Ofs.y);
            return v3Ret;
        }

        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, Radius);
        }
    }
}