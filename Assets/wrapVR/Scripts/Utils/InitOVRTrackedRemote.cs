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

        // Use this for initialization
        void Start()
        {
            // As of OVRSDK 1.2.7 these aren't initialized in code
            OVRTrackedRemote remote = GetComponent<OVRTrackedRemote>();
            if (remote.m_modelGearVrController == null)
                remote.m_modelGearVrController = transform.Find("GearVrControllerModel").gameObject;
            if (remote.m_modelOculusGoController == null)
                remote.m_modelOculusGoController = transform.Find("OculusGoControllerModel").gameObject;
            if (TouchControllerModel == null)
                TouchControllerModel = transform.Find("TouchControllerModel").gameObject;
#if UNITY_ANDROID
            if (TouchControllerModel)
                TouchControllerModel.SetActive(false);
#endif
        }
#endif
    }
}