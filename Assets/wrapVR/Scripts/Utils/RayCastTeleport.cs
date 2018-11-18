using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(FilterRayCasters))]
    public class RayCastTeleport : MonoBehaviour
    {
        // Callbacks
        public System.Action OnPreTeleport;
        public System.Action OnTeleport;

        [Tooltip("Which transform to teleport (default is Self")]
        public Transform ToTeleport;

        [Tooltip("VR Input activation to teleport")]
        public EActivation Activation;

        // Double click
        public bool DoubleClick;
        public float DoubleClickTimer;
        Coroutine m_coroDoubleClick;

        // Fade parameters
        public bool Fade;
        public float FadeTime;
        public Color FadeColor = Color.black;
        ScreenFade m_ScreenFade;

        public LayerMask ForbiddenLayers;

        Vector3 m_v3Destination;

        // Use this for initialization
        virtual protected void Start()
        {
            // Get teleport transform (use ours if null)
            if (ToTeleport == null)
                ToTeleport = transform;

            // Find screen fade if desired
            if (Fade)
            {
                if (VRCapabilityManager.mainCamera == null)
                {
                    Debug.LogError("Unable to get main camera");
                    Fade = false;
                }
                else
                {
                    m_ScreenFade = VRCapabilityManager.mainCamera.GetComponent<ScreenFade>();
                    if (m_ScreenFade == null || m_ScreenFade.m_FadeMat == null)
                    {
                        Debug.LogError("Error: missing screen fade for teleporter. Disabling fade...");
                        Fade = false;
                    }
                }
            }

            // Capture each controller and use the teleport function
            foreach (VRControllerRaycaster rc in GetComponent<FilterRayCasters>().getRayCasters())
            {
                rc.ActivationDownCallback(Activation, beginTeleport, true);
            }

            // Whenever we teleport clear double-click coroutine and active controller
            OnTeleport += () => 
            {
                m_coroDoubleClick = null;
            };
        }

        // When the fade in completes do the teleport and start the fade out
        private void M_ScreenFade_OnFadeInComplete()
        {
            teleport(m_v3Destination);
            m_ScreenFade.Fade(false, FadeTime, FadeColor);
            m_ScreenFade.OnFadeInComplete -= M_ScreenFade_OnFadeInComplete;
        }

        // While this is not null we're waiting for a double click
        IEnumerator doubleClickCoro()
        {
            yield return new WaitForSeconds(DoubleClickTimer);
            m_coroDoubleClick = null;
            yield break;
        }

        void teleport(Vector3 v3Destination)
        {
            if (OnPreTeleport != null)
                OnPreTeleport();

            if (ToTeleport.GetComponent<UnityEngine.AI.NavMeshAgent>())
                ToTeleport.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(v3Destination);
            else
                ToTeleport.transform.position = v3Destination;

            if (OnTeleport != null)
                OnTeleport();

        }

        protected virtual void beginTeleport(VRRayCaster rc)
        {
            if (!(rc.isRayCasting && rc.CurrentInteractible))
                return;

            if (0 != ((1 << rc.CurrentInteractible.gameObject.layer) & ForbiddenLayers.value)) 
                return;

            if (DoubleClick)
            {
                // If the timer is running stop it and teleport
                if (m_coroDoubleClick != null)
                {
                    StopCoroutine(m_coroDoubleClick);
                    m_coroDoubleClick = null;
                }
                // Otherwise start double click timer and get out
                else
                {
                    m_coroDoubleClick = StartCoroutine(doubleClickCoro());
                    return;
                }
            }

            if (Fade)
            {
                // Subscribe to on fade in completed so we can teleport and start fade out
                m_v3Destination = rc.GetLastHitPosition();
                m_ScreenFade.OnFadeInComplete += M_ScreenFade_OnFadeInComplete;
                m_ScreenFade.Fade(true, FadeTime, FadeColor);
                // startFade();
            }
            // If we aren't fading then just do the teleport
            else if (rc.CurrentInteractible)
            {
                teleport(rc.GetLastHitPosition());
            }
        }
    }
}