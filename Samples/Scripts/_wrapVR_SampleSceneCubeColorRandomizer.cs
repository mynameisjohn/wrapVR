using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    namespace Samples
    {
        public class _wrapVR_SampleSceneCubeColorRandomizer : MonoBehaviour
        {
            private void Start()
            {
                GetComponent<Renderer>().material.color = Random.ColorHSV();
            }
        }
    }
}