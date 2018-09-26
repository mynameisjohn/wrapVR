using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace wrapVR
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(FilterRayCasters))]
    public class NavMeshAgentToRayCastTarget : MonoBehaviour
    {
        public EActivation Activation;

        // Update is called once per frame
        void Update()
        {
            // If our activation is down then apply a force to the RB toward its target
            foreach (VRControllerRaycaster rc in GetComponent<FilterRayCasters>().getRayCasters())
            {
                if (rc.IsActivationDown(Activation) && rc.CurrentHitObject)
                {
                    GetComponent<NavMeshAgent>().SetDestination(rc.CurrentHitPosition);
                }
            }
        }
    }
}