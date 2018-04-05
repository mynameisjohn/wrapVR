using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class MaintainInitialPositionOffset : MonoBehaviour
    {
        public Transform Target;
        Vector3 m_v3Offset;

        // Use this for initialization
        void Start()
        {
            if (Target == null)
                Destroy(gameObject);

            m_v3Offset = transform.position - Target.position;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Target.position + m_v3Offset;
        }
    }
}