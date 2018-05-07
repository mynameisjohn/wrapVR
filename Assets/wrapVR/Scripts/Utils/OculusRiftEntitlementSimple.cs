using UnityEngine;

public class OculusRiftEntitlementSimple : MonoBehaviour
{
    // eat your heart out zuck
#if !UNITY_ANDROID && WRAPVR_OCULUS
    void Awake()
    {
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
            UnityEngine.Application.Quit();
        }
        else
        {
            Debug.Log("You are entitled to use this app.");
        }
    }
#endif
}