using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class PushBall : MonoBehaviour
    {
        public float Speed = 5;

        public wrapVR.VRRayCaster RayCaster;

        private void Start()
        {
            RayCaster = wrapVR.Util.DestroyEnsureComponent(gameObject, RayCaster);
        }

        // Update is called once per frame
        void Update()
        {
            if (RayCaster.Input.GetTrigger())
            {
                if (RayCaster.CurrentInteractible)
                    GetComponent<Rigidbody>().AddForce(Speed * (RayCaster.GetLastHitPosition() - transform.position).normalized, ForceMode.Force);
            }
            //if (Input.GetKey(KeyCode.W))
            //    GetComponent<Rigidbody>().AddForce(Speed * Vector3.forward, ForceMode.Force);
            //if (Input.GetKey(KeyCode.S))
            //    GetComponent<Rigidbody>().AddForce(Speed * Vector3.back, ForceMode.Force);
            //if (Input.GetKey(KeyCode.A))
            //    GetComponent<Rigidbody>().AddForce(Speed * Vector3.left, ForceMode.Force);
            //if (Input.GetKey(KeyCode.D))
            //    GetComponent<Rigidbody>().AddForce(Speed * Vector3.right, ForceMode.Force);
        }
    }
}