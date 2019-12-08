# wrapVR
Wraps various VR SDKs in Unity with unified interface. 
<br>
<br>

# Getting Started

wrapVR can be cloned or extracted into an existing Unity project. The samples folder,  which contains scenes demonstrating the API, can be removed without issue in shipping code. 

The first sample, SimpleDemo, contains an instance of the `VRCapabilityManager` prefab. The `VRCapabilityManager` is the central point for configuring the VR experience, but generally the default settings should be fine. The VRCapabilityManager will the parent of all VR SDK specific Camera Rigs in the scene. 

By default, the VRCapabilityManager instance has one camera rig - the `EditorCameraRig`. This object allows you to emulate a VR headset in the Unity Editor while debugging your scene. If you press play, you can use the Enter key to capture input focus in  the Game window and look around. 

### EditorCameraRig controls:

- Left/Right Alt: Enable or disable the hand input (by default right is enabled, left is disabled)
- Right Ctrl: Touchpad Touch
- Right Ctrl + Right Shift: Touchpad Click
- Right/Left Mouse click: Trigger
- Left Shift: Grip

# Adding other VR SDKs

Adding a VR SDK involves adding the SDK camera rig as a disabled child of the `VRCapabilityManager` and enabling a switch in the Unity project Player Settings. The relevant VR SDK must also be enabled in the XR Settings page of Unity as well. 

![Add the camera rig](https://i.imgur.com/IxOn2NM.png "Adding the camera rig")

![Enabling the SDK switch](https://i.imgur.com/sFbOUAr.png "Enabling the SDK switch")

![Enabling Unity XR](https://i.imgur.com/91Iqr3z.png "Enabling Unity XR")

## SteamVR

wrapVR is compatible with [SteamVR 1.2.3](https://github.com › download › 1.2.3 › SteamVR.Plugin.unitypackage). Import the asset, then 
1. add the SteamVRCameraRig prefab as a child of the VRCapabilityManager, then
2. define `WRAPVR_STEAM` in the Unity Player settings. 

## Oculus

wrapVR is compatible with recent versions of the [OVR Unity Integration](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022) (last tested with 1.4.3.) Import the asset, then
1. add the OVRCameraRig prefab as a child of the VRCapabilityManager, then
2. define `WRAPVR_OCULUS` in the Unity Player settings. 

## Daydream

wrapVR is compatible with recent versions of the [GoogleVr Unity SDK](https://developers.google.com/vr/develop/unity/download) (last tested with 1.200.1) Import the asset, then
1. add the GVRCameraRig prefab as a child of the VRCapabilityManager, then
2. define `WRAPVR_GOOGLE`

# Sample Scenes

## SimpleDemo

This example demonstrates the use of three important classes: `VRInput`, `VRRayCaster`, `VRInteractiveItem`. 

### `VRInput`

A VR input device, i.e the left/right hand controllers or the eyes. 

### `VRRayCaster`

A raycaster coming from one of the scene inputs, i.e the left/right hands or the eyes. 

### `VRInteractiveItem`

An item in the scene that responds when hit by a VRRayCaster. These hits are accompanied by Activations, which can be one of the following:

- `NONE` (the object was pointed at with nothing pressed)
- `TOUCH` (the object was pointed at with the touchpad / thumbstick touched)
- `TOUCHPAD` (the object was pointed at with the touchpad/ thumbstick pressed)
- `GRIP` (the object was pointed at with the grip button pressed)
- `TRIGGER` (the object was pointed at with the trigger button pressed)

<br>

The Cubes object in the scene makes each cube a `VRInteractiveItem` and responds to activations by changing color. 

## Grabbable

![Grabbable](https://i.imgur.com/Kl4WP5q.gif "Grabbable")

This scene demonstrates the `Grabbable` utility script, which can be used to grab objects and throw them around with the controller. In this scene the sphere is a grabbable with a RigidBody, allowing it to be bounced off of the walls. 

## Raycast Teleport

This scene demonstrates the `RayCastTeleport` utility script, which can use a VRRayCaster to teleport an object (the player, in this case.) The teleport activation is customizable - by default it's the TOUCHPAD activation, meaning clicking the touchpad / thumbstick will telepor the camera where the controller points. 
