using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    namespace Samples
    {
        public class _wrapVR_SampleSceneInputLogger : MonoBehaviour
        {
            [Tooltip("The activations to test; NONE will have no effect")]
            public List<EActivation> _ActivationsToTest = new List<EActivation> { EActivation.TRIGGER };

            void logInputActivationDown(VRInput input)
            {
                Debug.Log("VRInput " + input.Type + " is down");
            }

            void Start()
            {
                foreach (var input in VRCapabilityManager.inputs)
                {
                    foreach (var activation in _ActivationsToTest)
                    {
                        input.ActivationDownCallback(activation, logInputActivationDown);
                    }
                }
            }
        }
    }
}