using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(VRInteractiveItem))]
    public class TestVRInteractiveItem : MonoBehaviour
    {
        public List<EActivation> ActivationsToTest;

        // Use this for initialization
        void Start()
        {
            foreach(EActivation activation in ActivationsToTest)
            {
                GetComponent<VRInteractiveItem>().ActivationDownCallback(activation, (VRRayCaster rc) => { Debug.Log(rc.name + ", " + activation + ", Down"); });
                GetComponent<VRInteractiveItem>().ActivationUpCallback(activation, (VRRayCaster rc) => { Debug.Log(rc.name + ", " + activation + ", Up"); });
                GetComponent<VRInteractiveItem>().ActivationOverCallback(activation, (VRRayCaster rc) => { Debug.Log(rc.name + ", " + activation + ", Over"); });
                GetComponent<VRInteractiveItem>().ActivationOutCallback(activation, (VRRayCaster rc) => { Debug.Log(rc.name + ", " + activation + ", Out"); });
            }
        }
    }
}