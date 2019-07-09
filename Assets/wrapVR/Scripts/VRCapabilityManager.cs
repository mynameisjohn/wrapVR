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

        [Tooltip("Left Hand - parent of left hand raycasters")]
        public GameObject LeftHand;
        [Tooltip("Right Hand - parent of right hand raycasters")]
        public GameObject RightHand;
        [Tooltip("Head - parent of gaze controller and raycaster")]
        public GameObject Head;

        // We'll get this once we find the head in Awake
        Camera _camera;

        [Tooltip("How often we check for change to controllers")]
        [Range(0.01f, 60f)]
        public float CheckCtrlEvery = 1f;

        [Tooltip("Prototype camera - any changes to this object will be reflected in the SDK main camera")]
        public Camera PrototypeCamera;
        
        [Tooltip("Mobile platforms do not have a grip trigger, so choose Touchpad or Trigger as a proxy (or leave none)")]
        public EActivation GripIfMobile = EActivation.NONE;
        public static EActivation mobileGrip { get { return instance.GripIfMobile; } }

        public static VRInput rightInput { get; private set; }//{ return instance.RightHand.transform.parent.GetComponent<VRInput>(); } }
        public static VRInput leftInput { get; private set; }// { return instance.LeftHand.transform.parent.GetComponent<VRInput>(); } }
        public static VRInput eyeInput { get; private set; }// { return instance.Head.transform.parent.GetComponent<VRInput>(); } }
        public static Camera mainCamera { get { return instance._camera; } }
        
        // We expect the head to have an eye ray caster
        // however in the absence of a hand controller
        // the eye can be used as a controller (GearVR)
        // (this should be a child of Head)
        // GameObject m_GazeCaster;
        bool m_bUseGazeFallback = false;

        // We cache the SDK camera rig object so we can track its transform
        GameObject m_SDKCameraRig;
        public static GameObject CameraRig { get { return instance.m_SDKCameraRig; } }

        // Decorations
        public bool globallyDisableLaser;
        public static bool isLaserDisabled { get { return instance.globallyDisableLaser; } }
        public bool reloadSceneOnCancel;
        public static bool doReloadSceneOnCancel { get { return instance.reloadSceneOnCancel; } }
        public bool ForceGaze = false;
        public bool InteractWhileGrabbing = false;
        public static bool canInteractWhileGrabbing { get { return instance.InteractWhileGrabbing; } }
        public bool PointIfTrigger = false;
        public static bool canPointIfTrigger{ get { return instance.PointIfTrigger; } }

        [Range(0,100f)]
        public float EditorWASDSpeed = 0f;

        public bool AddFPSCounter;

        public static bool IsGazeFallback
        {
            get { return instance.m_bUseGazeFallback; }
        }

        List<VRRayCaster> _raycasters;
        public static List<VRRayCaster> RayCasters { get { return instance._raycasters; } }

        public static Action OnControllerConnected;

        public static void Init()
        {
            FindObjectOfType<VRCapabilityManager>().init(); // ?
        }
        protected virtual void init()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                // Bail if an instance already exists
                Destroy(this);
            }

            // Determine which SDK to use
#if UNITY_EDITOR && UNITY_ANDROID
            m_eSDK = ESDK.Editor;
            UnityEngine.XR.XRSettings.enabled = false;
#else
            if (!UnityEngine.XR.XRSettings.isDeviceActive)
                m_eSDK = ESDK.Editor;
#if WRAPVR_OCULUS
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "Oculus")
                m_eSDK = ESDK.Oculus;
#endif
#if WRAPVR_GOOGLE
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "daydream")
                m_eSDK = ESDK.Google;
#endif
#if WRAPVR_STEAM
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "OpenVR")
                m_eSDK = ESDK.Steam;
#endif
            else
            {
                Debug.LogError("Invalid VR SDK! Defaulting to Editor...");
                m_eSDK = ESDK.Editor;
            }
#endif
            // Debug.Log("SDK is " + m_eSDK);

            Transform RightHandInput = null;
            Transform LeftHandInput = null;
            Transform EyeInput = null;
            switch (m_eSDK)
            {
                case ESDK.Editor:
                    // The editor camera rig is
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
                    RightHandInput = edtCamRig.Find("RightHand");
                    LeftHandInput = edtCamRig.Find("LeftHand");
                    EyeInput = edtCamRig.Find("Eye");
                    m_SDKCameraRig = edtCamRig.gameObject;
                    edtCamRig.GetComponent<EditorCameraEmulator>().Speed = EditorWASDSpeed;
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

                    RightHandInput = ovrTrackingSpace.Find("RightHandAnchor");
                    LeftHandInput = ovrTrackingSpace.Find("LeftHandAnchor");
                    EyeInput = ovrTrackingSpace.Find("CenterEyeAnchor");
                    m_SDKCameraRig = ovrCamRig.gameObject;
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
                    RightHandInput = gvrCameraRig.transform.Find("GvrControllerPointer");
                    LeftHandInput = null; // Always null - if handedness if left it won't matter
                    if (LeftHand)
                        LeftHand.SetActive(false);
                    EyeInput = gvrCameraRig.GetComponentInChildren<Camera>().transform;
                    m_SDKCameraRig = gvrCameraRig.gameObject;
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
                    RightHandInput = steamVrCameraRig.transform.Find("Controller (right)");
                    LeftHandInput = steamVrCameraRig.transform.Find("Controller (left)");
                    EyeInput = steamVrCameraRig.transform.Find("Camera (head)");
                    if (EyeInput == null)
                        EyeInput = steamVrCameraRig.transform.Find("Camera (eye)");
                    else if (EyeInput.Find("Camera (eye)"))
                        EyeInput = EyeInput.Find("Camera (eye)");

                    m_SDKCameraRig = steamVrCameraRig.gameObject;
                    break;
            }

            // We only allow gaze fallback on oculus and editor
            if (m_eSDK != ESDK.Oculus && m_eSDK != ESDK.Editor)
            {
                ForceGaze = false;
            }

            if (!(RightHandInput || LeftHandInput) && !EyeInput)
            {
                Debug.Log("Error finding SDK camera " + m_eSDK.ToString() + "rig");
                Destroy(gameObject);
            }

            // Init all controller objects, which means properly reparenting the hand/head
            // aliases to the SDK input objects and build a list of raycasters in our hierarchy
            _raycasters = new List<VRRayCaster>();
            Func<VRInput, GameObject, InputType, int> initController = (VRInput input, GameObject alias, InputType type) =>
            {
                if (input && alias && input.GetComponent<VRInput>())
                {
                    foreach (VRRayCaster rc in alias.GetComponentsInChildren<VRRayCaster>(true))
                    {
                        // Set input, which will make the caster a child of the input transform
                        rc.SetInput(input);

                        // Add reload scene on cancel callback if desired
                        if (doReloadSceneOnCancel)
                            rc.Input.OnCancel += () => { SceneManager.LoadScene(0); };

                        // If it's the eye caster set its camera
                        if (rc.GetType() == typeof(VREyeRaycaster))
                            ((VREyeRaycaster)rc).SetCamera(input.GetComponent<Camera>());

                        _raycasters.Add(rc);
                    }

                    if (type == InputType.GAZE)
                        eyeInput = input;
                    else if (type == InputType.LEFT)
                        leftInput = input;
                    else if (type == InputType.RIGHT)
                        rightInput = input;
                    
                    alias.transform.SetParent(input.transform);
                    alias.SetActive(false);
                }
                return 0;
            };
            initController(RightHandInput.GetComponent<VRInput>(), RightHand, InputType.RIGHT);
            initController(LeftHandInput.GetComponent<VRInput>(), LeftHand, InputType.LEFT);
            initController(EyeInput.GetComponent<VRInput>(), Head, InputType.GAZE);

            // Copy components from the dummy camera and destroy it now
            if (PrototypeCamera != null)
            {
                // Copy prototype camera properties (there should be a better way of doing this)
                if (EyeInput.GetComponent<Camera>())
                {
                    // Screen Fade
                    if (PrototypeCamera.GetComponent<ScreenFade>())
                        Util.CopyAddComponent<ScreenFade>(PrototypeCamera.gameObject, EyeInput.gameObject);

                    // far clip plane
                    EyeInput.GetComponent<Camera>().farClipPlane = PrototypeCamera.farClipPlane;
                    EyeInput.GetComponent<Camera>().useOcclusionCulling = PrototypeCamera.useOcclusionCulling;

                    _camera = EyeInput.GetComponent<Camera>();
                }

                Destroy(PrototypeCamera.gameObject);
                PrototypeCamera = null;
            }

            if (AddFPSCounter && Head)
            {
                Transform fpsCounter = Head.transform.Find("FPSCounter");
                if (fpsCounter)
                {
                    fpsCounter.gameObject.SetActive(true);
                }
            }

            // Start check status coroutine
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

                // The head should always be active
                if (Head)
                    Head.SetActive(true);

                // If we're forcing gaze fallback and we have a gaze control input, make sure it's enabled
                if (ForceGaze && Head && Head.GetComponentInChildren<VRControllerRaycaster>(true))
                {
                    if (RightHand)
                        RightHand.SetActive(false);
                    if (LeftHand)
                        LeftHand.SetActive(false);
                    foreach (VRControllerRaycaster crc in Head.GetComponentsInChildren<VRControllerRaycaster>(true))
                        crc.gameObject.SetActive(true);
                    m_bUseGazeFallback = true;
                }
                else if (!ForceGaze)
                {
                    // Not forcing gaze, enable all controllers in hands
                    bool bUsingHand = false;
                    foreach (GameObject hand in new GameObject[] { RightHand, LeftHand })
                    {
                        if (!hand)
                            continue;

                        bool bThisHandActive = false;
                        foreach (VRControllerRaycaster crc in hand.GetComponentsInChildren<VRControllerRaycaster>(true))
                        {
                            // If there's a controller here with input 
                            // then daectivate gaze caster and activate hand
                            if (crc && crc.HasInput())
                            {
                                foreach (VRControllerRaycaster crcGaze in Head.GetComponentsInChildren<VRControllerRaycaster>(true))
                                    crcGaze.gameObject.SetActive(false);

                                hand.SetActive(true);
                                bThisHandActive = true;
                                bUsingHand = true;
                            }
                        }

                        // Deactivate hand if it has no controllers
                        if (!bThisHandActive)
                        {
                            hand.SetActive(false);
                        }
                    }

                    // We don't have any controllers, fall back to gaze if possible
                    if (!bUsingHand && Head && Head.GetComponentInChildren<VRControllerRaycaster>(true))
                    {
                        if (RightHand)
                            RightHand.SetActive(false);
                        if (LeftHand)
                            LeftHand.SetActive(false);
                        foreach (VRControllerRaycaster crc in Head.GetComponentsInChildren<VRControllerRaycaster>(true))
                            crc.gameObject.SetActive(true);
                        m_bUseGazeFallback = true;
                    }
                }

                // Wait for specified time
                yield return new WaitForSeconds(CheckCtrlEvery);
            }
        }

        // This includes inputs for gaze and hands regardless of whether or not they're active
        public static VRInput[] GetInputs()
        {
            return instance.GetComponentsInChildren<VRInput>(true);
        }
    }
}