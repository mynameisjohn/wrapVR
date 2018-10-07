using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
#if WRAPVR_OCULUS
    [RequireComponent(typeof(OVRTrackedRemote))]
#endif
    public class InitOVRTrackedRemote : MonoBehaviour
    {
#if WRAPVR_OCULUS
        public GameObject TouchControllerModel;

        private void Awake()
        {
            OVRTrackedRemote remote = GetComponent<OVRTrackedRemote>();
            bool right = transform.parent.name.ToLower().StartsWith("right");
            foreach (Transform controller in transform)
            {
                string controllerName = controller.name.ToLower();
                if (controllerName.StartsWith("gearvr"))
                {
                    remote.m_modelGearVrController = controller.gameObject;
                    remote.m_controller = right ? OVRInput.Controller.RTrackedRemote : OVRInput.Controller.LTrackedRemote;
                }
                else if (controllerName.StartsWith("oculusgo"))
                {
                    remote.m_modelOculusGoController = controller.gameObject;
                    remote.m_controller = right ? OVRInput.Controller.RTrackedRemote : OVRInput.Controller.LTrackedRemote;
                }
                else if (TouchControllerModel == null && controllerName.StartsWith("touch"))
                {
                    TouchControllerModel = controller.gameObject;
                    remote.m_controller = right ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch;
                }

#if UNITY_ANDROID
                if (TouchControllerModel)
                    TouchControllerModel.SetActive(false);
#endif
            }
        }
#endif
    }
}