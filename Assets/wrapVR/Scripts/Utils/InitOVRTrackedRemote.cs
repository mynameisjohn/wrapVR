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
        // Use this for initialization
        void Start()
        {
            // As of OVRSDK 1.2.7 these aren't initialized in code
            OVRTrackedRemote remote = GetComponent<OVRTrackedRemote>();
            if (remote.m_modelGearVrController == null)
                remote.m_modelGearVrController = transform.Find("GearVrControllerModel").gameObject;
            if (remote.m_modelOculusGoController == null)
                remote.m_modelOculusGoController = transform.Find("OculusGoControllerModel").gameObject;
        }
#endif
    }
}