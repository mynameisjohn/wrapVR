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
        public static VRCapabilityManager instance { get; private set; }

        public bool _InitOnAwake = true;

        // We look at this on start and deal appropriately
        ESDK _sdkType;
        public static ESDK sdkType { get { return instance._sdkType; } }
        
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
        public VRControllerRaycaster _gazeCasterFallback;
        bool _useGazeFallback = false;

        // We cache the SDK camera rig object so we can track its transform
        GameObject _sdkCameraRig;
        public static GameObject cameraRig { get { return instance._sdkCameraRig; } }

        // Decorations
        public bool _GloballyDisableLaser;
        public static bool isLaserDisabled { get { return instance._GloballyDisableLaser; } }
        public bool _ReloadOnMenu;
        public bool _ForceGaze = false;
        public bool _InteractWhileGrabbing = false;
        public static bool canInteractWhileGrabbing { get { return instance._InteractWhileGrabbing; } }
        public bool _PointIfTrigger = false;
        public static bool canPointIfTrigger{ get { return instance._PointIfTrigger; } }
        public bool _CanGrabMultiple = false;
        public static bool canGrabMultiple { get { return instance._CanGrabMultiple; } }

        public float _EyeResolutionScale = 1f;
        [Range(1,4)]
        public int _OVR_CPU_Level = 2;
        [Range(1,4)]
        public int _OVR_GPU_Level = 2;

        public enum OVR_FFR_Level
        {
            Off,
            Low,
            Medium,
            High
        }
        public OVR_FFR_Level _OVR_FFR_Level;

        public bool _HandleOculusDash = true;

        [Range(0,100f)]
        public float EditorWASDSpeed = 0f;

        public bool AddFPSCounter;

        public static bool IsGazeFallback
        {
            get { return instance._useGazeFallback; }
        }

        private void Awake()
        {
            if (_InitOnAwake)
                init();
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
            else if (instance == this)
                return;

            // Determine which SDK to use
#if UNITY_EDITOR && UNITY_ANDROID
            _sdkType = ESDK.Editor;
            UnityEngine.XR.XRSettings.enabled = false;
#else
            if (!UnityEngine.XR.XRSettings.isDeviceActive)
                _sdkType = ESDK.Editor;
#if WRAPVR_OCULUS
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "Oculus")
                _sdkType = ESDK.Oculus;
#endif
#if WRAPVR_GOOGLE
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "daydream")
                _sdkType = ESDK.Google;
#endif
#if WRAPVR_STEAM
            else if (UnityEngine.XR.XRSettings.loadedDeviceName == "OpenVR")
                _sdkType = ESDK.Steam;
#endif
            else
            {
                Debug.LogError("Invalid VR SDK! Defaulting to Editor...");
                _sdkType = ESDK.Editor;
            }
#endif
            // Debug.Log("SDK is " + _sdkType);

            Transform RightHandInput = null;
            Transform LeftHandInput = null;
            Transform EyeInput = null;
            switch (_sdkType)
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
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + _sdkType);
                        break;
                    }
                    edtCamRig.gameObject.SetActive(true);
                    RightHandInput = edtCamRig.Find("RightHand");
                    LeftHandInput = edtCamRig.Find("LeftHand");
                    EyeInput = edtCamRig.Find("Eye");
                    _sdkCameraRig = edtCamRig.gameObject;
                    edtCamRig.GetComponent<EditorCameraEmulator>().Speed = EditorWASDSpeed;
                    break;
                case ESDK.Oculus:
#if WRAPVR_OCULUS
                    // Find the OVR camera rig in children
                    var ovrCamRig = GetComponentInChildren<OVRCameraRig>(true);
                    if (ovrCamRig == null)
                    {
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + _sdkType);
                        break;
                    }
                    ovrCamRig.gameObject.SetActive(true);
                    Transform ovrTrackingSpace = ovrCamRig.transform.Find("TrackingSpace");
                    if (ovrTrackingSpace == null)
                    {
                        Debug.LogError("Unable to find Oculus tracking space");
                        break;
                    }

                    RightHandInput = ovrTrackingSpace.Find("RightHandAnchor");
                    LeftHandInput = ovrTrackingSpace.Find("LeftHandAnchor");
                    EyeInput = ovrTrackingSpace.Find("CenterEyeAnchor");
                    _sdkCameraRig = ovrCamRig.gameObject;

#if UNITY_ANDROID
                    OVRManager.cpuLevel = _OVR_CPU_Level;
                    OVRManager.gpuLevel = _OVR_GPU_Level;
                    switch(_OVR_FFR_Level)
                    {
                        case OVR_FFR_Level.Off:
                            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Off;
                            break;
                        case OVR_FFR_Level.Low:
                            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Low;
                            break;
                        case OVR_FFR_Level.Medium:
                            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Medium;
                            break;
                        case OVR_FFR_Level.High:
                            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
                            break;
                    }

                    UnityEngine.XR.XRSettings.eyeTextureResolutionScale = _EyeResolutionScale;

                    // Debug.Log("OVR MANAGER CPU LEVEL " + OVRManager.cpuLevel);
                    // Debug.Log("OVR MANAGER GPU LEVEL " + OVRManager.gpuLevel);
                    // Debug.Log("OVR MANAGER FFR LEVEL " + OVRManager.fixedFoveatedRenderingLevel);
#else
                    if (_HandleOculusDash)
                        StartCoroutine(handleOculusDash());
#endif
#endif
                    break;
                case ESDK.Google:
#if WRAPVR_GOOGLE
                    // Find GVR Camera Rig
                    var gvrCameraRig = GetComponentInChildren<GVRCameraRig>(true);
                    if (gvrCameraRig == null)
                    {
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + _sdkType);
                        break;
                    }
                    gvrCameraRig.gameObject.SetActive(true);
                    RightHandInput = gvrCameraRig.transform.Find("GvrControllerPointer");
                    LeftHandInput = null; // Always null - if handedness if left it won't matter
                    if (LeftHand)
                        LeftHand.SetActive(false);
                    EyeInput = gvrCameraRig.GetComponentInChildren<Camera>().transform;
                    _sdkCameraRig = gvrCameraRig.gameObject;
#endif
                    break;
                case ESDK.Steam:
#if WRAPVR_STEAM
                    // Find Steam Camera Rig
                    var steamVrCameraRig = GetComponentInChildren<SteamVR_PlayArea>(true);
                    if (steamVrCameraRig == null)
                    {
                        Debug.LogError("Unable to find VR Camera Rig for SDK " + _sdkType);
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

                    _sdkCameraRig = steamVrCameraRig.gameObject;
#endif
                    break;
            }

            // We only allow gaze fallback on oculus and editor
            if (_sdkType != ESDK.Oculus && _sdkType != ESDK.Editor)
            {
                _ForceGaze = false;
            }

            if (!(RightHandInput || LeftHandInput) && !EyeInput)
            {
                Debug.Log("Error finding SDK camera " + _sdkType.ToString() + "rig");
                Destroy(gameObject);
            }

            _camera = EyeInput.GetComponent<Camera>();
            if (_camera == null)
            {
                Debug.LogError("Error: missing camera from SDK " + _sdkType.ToString());
                Destroy(gameObject);
            }

            // Init all controller objects, which means properly reparenting the hand/head
            // aliases to the SDK input objects and build a list of raycasters in our hierarchy
            _raycasters = new List<VRRayCaster>();
            Func<Transform, GameObject, InputType, int> initController = (Transform inputTransform, GameObject alias, InputType type) =>
            {
                if (inputTransform && inputTransform.GetComponent<VRInput>() && alias)
                {
                    var input = inputTransform.GetComponent<VRInput>();
                    input.Init();

                    foreach (var rc in alias.GetComponentsInChildren<VRRayCaster>(true))
                    {
                        // Set input, which will make the caster a child of the input transform
                        rc.SetInput(input);

                        // If it's the eye caster set its camera
                        if (rc.GetType() == typeof(VREyeRaycaster))
                            ((VREyeRaycaster)rc).SetCamera(input.GetComponent<Camera>());

                        _raycasters.Add(rc);
                    }

                    foreach(var ic in input.GetComponentsInChildren<InputControllerRenderers>())
                        ic.Init(input);

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
            initController(RightHandInput, RightHand, InputType.RIGHT);
            initController(LeftHandInput, LeftHand, InputType.LEFT);
            initController(EyeInput, Head, InputType.GAZE);

            // Copy components from the dummy camera and destroy it now
            if (PrototypeCamera != null)
            {
                // Copy prototype camera properties (there should be a better way of doing this)
                // Screen Fade
                if (PrototypeCamera.GetComponent<ScreenFade>())
                    Util.CopyAddComponent<ScreenFade>(PrototypeCamera.gameObject, EyeInput.gameObject);

                // clip planes
                _camera.nearClipPlane = PrototypeCamera.nearClipPlane;
                _camera.farClipPlane = PrototypeCamera.farClipPlane;
                _camera.useOcclusionCulling = PrototypeCamera.useOcclusionCulling;

                // clear color
                _camera.backgroundColor = PrototypeCamera.backgroundColor;

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
                _useGazeFallback = false;
                _gazeCasterFallback.gameObject.SetActive(false);

                // The head should always be active
                if (Head)
                    Head.SetActive(true);

                // If we're forcing gaze fallback and we have a gaze control input, make sure it's enabled
                if (_ForceGaze && Head && Head.GetComponentInChildren<VRControllerRaycaster>(true))
                {
                    if (RightHand)
                        RightHand.SetActive(false);
                    if (LeftHand)
                        LeftHand.SetActive(false);
                    _gazeCasterFallback.gameObject.SetActive(true);
                    _useGazeFallback = true;
                }
                else if (!_ForceGaze)
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
                        _useGazeFallback = true;
                    }
                }

                // Wait for specified time
                yield return new WaitForSeconds(CheckCtrlEvery);
            }
        }

        // This includes inputs for gaze and hands regardless of whether or not they're active
        public static VRInput[] inputs
        {
            get
            {
                return instance.GetComponentsInChildren<VRInput>();
            }
        }

#if WRAPVR_OCULUS && !UNITY_ANDROID
        IEnumerator handleOculusDash()
        {
            float originalTimeScale = Time.timeScale;
            for (bool hasInputFocus = true; ;)
            {
                yield return true;

                if (hasInputFocus != OVRManager.hasInputFocus)
                {
                    hasInputFocus = !hasInputFocus;

                    rightInput.gameObject.SetActive(hasInputFocus);
                    leftInput.gameObject.SetActive(hasInputFocus);

                    AudioListener.pause = !hasInputFocus;

                    if (hasInputFocus)
                    {
                        Time.timeScale = originalTimeScale;
                    }
                    else
                    {
                        originalTimeScale = Time.timeScale;
                        Time.timeScale = 0f;
                    }
                }
            }
        }
#endif
    }
}