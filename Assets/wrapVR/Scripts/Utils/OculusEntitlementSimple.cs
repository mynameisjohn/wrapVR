using UnityEngine;

// Taken from 
// https://developer.oculus.com/documentation/platform/latest/concepts/pgsg-unity-gsg/#initialize-the-sdk-and-perform-the-entitlement-check
public class OculusEntitlementSimple : MonoBehaviour
{
    // eat your heart out zuck
#if WRAPVR_OCULUS
    public void DoOculusEntitlementsCheck()
    {
        if (wrapVR.VRCapabilityManager.sdkType != wrapVR.VRCapabilityManager.ESDK.Oculus)
        {
            Destroy(this);
            return;
        }

        // so I can test on quest without releasing...
        if (wrapVR.OculusVRInput.isOculusQuest)
        {
            Debug.Log("Skipping entitlements check for Quest");
            return;
        }

        try
        {
            Oculus.Platform.Core.AsyncInitialize();
            Oculus.Platform.Entitlements.IsUserEntitledToApplication().OnComplete(GetEntitlementCallback);
        }
        catch (UnityException e)
        {
            Debug.LogError("Platform failed to initialize due to exception.");
            Debug.LogException(e);
            // Immediately quit the application.
            UnityEngine.Application.Quit();
        }
    }

    void GetEntitlementCallback(Oculus.Platform.Message msg)
    {
        if (msg.IsError)
        {
            Debug.LogError("You are NOT entitled to use this app.");
            Application.Quit();
        }
        else
        {
            Debug.Log("You are entitled to use this app.");
        }
    }
#endif
}