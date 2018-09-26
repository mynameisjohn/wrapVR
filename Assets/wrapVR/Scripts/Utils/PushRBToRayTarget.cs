using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Allow a rigid body to be pushed toward a raycaster's target
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(FilterRayCasters))]
    public class PushRBToRayTarget : MonoBehaviour
    {
        public float Speed = 5;
        public EActivation Activation;
        
        // Update is called once per frame
        void Update()
        {
            // If our activation is down then apply a force to the RB toward its target
            foreach (VRControllerRaycaster rc in GetComponent<FilterRayCasters>().getRayCasters())
            {
                if (rc.IsActivationDown(Activation))
                {
                    if (rc.CurrentInteractible)
                        GetComponent<Rigidbody>().velocity = Speed * (rc.GetLastHitPosition() - transform.position).normalized;//.AddForce(Speed * (rc.GetLastHitPosition() - transform.position).normalized, ForceMode.Force);
                }
            }
        }
    }
}