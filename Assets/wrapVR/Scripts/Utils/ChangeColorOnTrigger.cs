using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(VRInteractiveItem))]
    public class ChangeColorOnTrigger : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            GetComponent<VRInteractiveItem>().OnTriggerOver += (VRRayCaster rc) => 
            {
                GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
            };
        }
    }
}