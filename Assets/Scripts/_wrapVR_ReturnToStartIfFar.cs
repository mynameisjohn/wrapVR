using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    namespace Samples
    {
        class _wrapVR_ReturnToStartIfFar : MonoBehaviour
        {
            public float _DistanceForReturn = 50f;

            Vector3 _startingPos;

            void Start()
            {
                _startingPos = transform.position;    
            }

            void Update()
            {
                if (_DistanceForReturn < (transform.position - _startingPos).magnitude)
                {
                    transform.position = _startingPos;
                    GetComponent<Rigidbody>().Sleep();
                }
            }
        }
    }
}