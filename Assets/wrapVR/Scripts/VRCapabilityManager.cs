using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace wrapVR
{
    public class VRCapabilityManager : MonoBehaviour
    {
        // Singleton instance
        public static VRCapabilityManager instance;

        // Possible SDK options
        public enum ESDK
        {
            Editor, // In editor
            Oculus, // OVR
            Google, // Daydream
            Steam   // Vive
        }

        // We look at this on start and deal appropriately
        ESDK m_eSDK;
        public static ESDK sdkType { get { return instance.m_eSDK; } }

        // Aliases to caster objects - they start null and are
        // assigned depending on the choice of SDK
        // These objects are the ones who raycast to objects
        // in the scene and call stuff on them, unless we disable them
        // We always try to use the controllers, but if they are 
        // not present we try to use the eye caster. I'm not sure
        // about using both controllers yet...
        public VRControllerRaycaster LeftController;
        public VRControllerRaycaster RightController;
        public VRControllerRaycaster GazeController;
        public VREyeRaycaster GazeCaster;
        public float SecondsPerCheck = 1f;
        public Camera DummyCamera;

        bool m_bUseGazeFallback = false;

        // Decorations
        public bool globallyDisableLaser;
        public static bool isLaserDisabled { get { return instance.globallyDisableLaser; } }
        public bool reloadSceneOnCancel;
        public static bool doReloadSceneOnCancel { get { return instance.reloadSceneOnCancel; } }
        public bool ForceGaze = false;
        public bool PointWhileGrabbing = false;
        public static bool canPointWhileGrabbing { get { return instance.PointWhileGrabbing; } }
        public bool PointIfTrigger = false;
        public static bool canPointIfTrigger{ get { return instance.PointIfTrigger; } }

        public static bool IsGazeFallback
        {
            get { return instance.m_bUseGazeFallback; }
        }

        public static Action OnControllerConnected;

        public virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                // Bail if an instance already exists
                DestroyObject(this);
            }

            // Determine which SDK to use
            if (!UnityEngine.XR.XRSettings.isDeviceActive)
                m_eSDK = ESDK.Editor;
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "Oculus")
                m_eSDK = ESDK.Oculus;
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "daydream")
                m_eSDK = ESDK.Google;
            else
            {
                Debug.LogError("Invalid VR SDK! Destroying...");
                Destroy(gameObject);
            }

            Transform RightHand = null;
            Transform LeftHand = null;
            Transform Eye = null;
            switch (m_eSDK)
            {
                case ESDK.Editor:
                    // Let's say the editor camera rig is
                    // EditorCameraRig
                    //     RightHand
                    //     LeftHand
                    //     Eye (camera)
                    Transform edtCamRig = transform.Find("EditorCameraRig");
                    if (edtCamRig == null)
                    {
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + m_eSDK);
                        break;
                    }
                    edtCamRig.gameObject.SetActive(true);
                    RightHand = edtCamRig.Find("RightHand");
                    LeftHand = edtCamRig.Find("LeftHand");
                    Eye = edtCamRig.Find("Eye");
                    break;
                case ESDK.Oculus:
                    // Find the OVR camera rig in children
                    Transform ovrCamRig = transform.Find("OVRCameraRig");
                    if (ovrCamRig == null)
                    {
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + m_eSDK);
                        break;
                    }
                    ovrCamRig.gameObject.SetActive(true);
                    Transform ovrTrackingSpace = ovrCamRig.Find("TrackingSpace");
                    if (ovrTrackingSpace == null)
                    {
                        Debug.LogError("Unable to find Oculus tracking space");
                        break;
                    }

                    RightHand = ovrTrackingSpace.Find("RightHandAnchor");
                    LeftHand = ovrTrackingSpace.Find("LeftHandAnchor");
                    Eye = ovrTrackingSpace.Find("CenterEyeAnchor");
                    break;
                case ESDK.Google:
                    // Find GVR Camera Rig
                    Transform gvrCameraRig = transform.Find("GVRCameraRig");
                    if (gvrCameraRig == null)
                    {
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + m_eSDK);
                        break;
                    }
                    gvrCameraRig.gameObject.SetActive(true);
                    RightHand = gvrCameraRig.transform.Find("GvrControllerPointer");
                    LeftHand = null; // Always null - if handedness if left it won't matter
                    Eye = gvrCameraRig.GetComponentInChildren<Camera>().transform;
                    break;
                case ESDK.Steam:
                    // Find Steam Camera Rig
                    Transform steamVrCameraRig = transform.Find("SteamVRCameraRig");
                    if (steamVrCameraRig == null)
                    {
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + m_eSDK);
                        break;
                    }
                    steamVrCameraRig.gameObject.SetActive(true);
                    RightHand = steamVrCameraRig.transform.Find("Controller (right)");
                    LeftHand = steamVrCameraRig.transform.Find("Controller (left)");
                    Eye = steamVrCameraRig.transform.Find("Camera (head)");
                    break;
            }

            // We only allow gaze fallback on oculus and editor
            if (m_eSDK != ESDK.Oculus && m_eSDK != ESDK.Editor)
            {
                ForceGaze = false;
            }

            if (!(RightHand || LeftHand) && !Eye)
            {
                Debug.Log("Error finding SDK camera " + m_eSDK.ToString() + "rig");
                Destroy(gameObject);
            }

            // Do this to them all
            Func<Transform, GameObject, int> initController = (Transform real, GameObject ctrlr) =>
            {
                if (ctrlr != null)
                {
                    // controller can have multiple ray casters, so find them all and set their input
                    foreach (VRRayCaster rc in ctrlr.gameObject.GetComponents<VRRayCaster>())
                    {
                        // Deactivate all now - we'll activate the right one in update
                        if (real != null && real.GetComponent<VRInput>() != null)
                        {
                            // Set input, which will make the caster a child of the transform
                            rc.SetInput(real.GetComponent<VRInput>());

                            // If it's the eye caster set its camera
                            if (rc.GetType() == typeof(VREyeRaycaster))
                                ((VREyeRaycaster)rc).SetCamera(real.GetComponent<Camera>());

                            if (doReloadSceneOnCancel)
                            {
                                rc.Input.OnCancel += () =>
                                {
                                    SceneManager.LoadScene(0);
                                };
                            }
                        }
                        ctrlr.SetActive(false);
                    }
                }
                return 0;
            };
            
            initController(RightHand, RightController.gameObject);
            initController(LeftHand, LeftController.gameObject);
            initController(Eye, GazeController.gameObject);
            initController(Eye, GazeCaster.gameObject);

            if (DummyCamera != null)
                Destroy(DummyCamera);

            StartCoroutine(CheckControllerStatus());
        }

        // Check controller state every so often
        // I don't think reenabling/disabling things is bad... ?
        IEnumerator CheckControllerStatus()
        {
            while (true)
            {
                // False now, we'll check below
                m_bUseGazeFallback = false;

                // If we're forcing gaze fallback and we have a gaze control input, make sure it's enabled
                if (ForceGaze && GazeController && GazeController.HasInput())
                {
                    RightController.gameObject.SetActive(false);
                    LeftController.gameObject.SetActive(false);
                    GazeController.gameObject.SetActive(true);
                    m_bUseGazeFallback = true;
                }
                else if (!ForceGaze)
                {
                    // Not forcing gaze and we have any controller, use it
                    if ((RightController && RightController.HasInput()) || (LeftController && LeftController.HasInput()))
                    {
                        foreach (var crc in new VRControllerRaycaster[] { RightController, LeftController })
                        {
                            if (crc != null && crc.HasInput())
                            {
                                GazeController.gameObject.SetActive(false);
                                crc.gameObject.SetActive(true);
                            }
                            else if (crc)
                            {
                                crc.gameObject.SetActive(false);
                            }
                        }
                    }
                    // We don't have any controllers, fall back to gaze
                    else if (GazeController && GazeController.HasInput())
                    {
                        RightController.gameObject.SetActive(false);
                        LeftController.gameObject.SetActive(false);
                        GazeController.gameObject.SetActive(true);
                        m_bUseGazeFallback = true;
                    }
                }

                if (GazeCaster != null)
                {
                    GazeCaster.enabled = true;
                }

                yield return new WaitForSeconds(SecondsPerCheck);
            }
        }

        private void Update()
        {
        }
    }
}