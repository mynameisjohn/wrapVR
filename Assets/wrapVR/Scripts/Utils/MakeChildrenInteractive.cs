using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class MakeChildrenInteractive : MonoBehaviour
    {
        // Use this for initialization
        void Start()
        {
            foreach (Collider c in GetComponentsInChildren<Collider>())
                if (c.GetComponent<VRInteractiveItem>() == null)
                    c.gameObject.AddComponent<VRInteractiveItem>();
        }
    }
}