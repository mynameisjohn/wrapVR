using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(VRInteractiveItem))]
    public class ChangeColorWithActivation : MonoBehaviour
    {
        public EActivation _Activation;

        // Use this for initialization
        void Start()
        {
            GetComponent<VRInteractiveItem>().ActivationOverCallback(_Activation, (VRRayCaster rc) =>
            {
                GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
            });
        }
    }
}