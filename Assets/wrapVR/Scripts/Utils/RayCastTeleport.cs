using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
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

        // The raycasters that affect us
        public List<VRRayCaster> RayCasters;

        Vector3 m_v3Destination;


        // Use this for initialization
        void Start()
        {
            Util.RemoveInvalidCasters(RayCasters);
            
            // Find screen fade if desired
            if (Fade)
            {
                if (Camera.main.GetComponent<ScreenFade>() == null)
                    Camera.main.gameObject.AddComponent<ScreenFade>();
                m_ScreenFade = Camera.main.GetComponent<ScreenFade>();
                if (m_ScreenFade == null)
                {
                    Debug.LogError("Error: missing screen fade for teleporter. Disabling fade...");
                    Fade = false;
                }
            }

            // Get teleport transform (use ours if null)
            if (ToTeleport == null)
                ToTeleport = transform;
            
            // Capture each controller and use the teleport function
            foreach (VRControllerRaycaster rc in RayCasters)
            {
                switch (Activation)
                {
                    case EActivation.NONE:
                        break;
                    case EActivation.TOUCH:
                        rc.OnTouchpadDown += beginTeleport;
                        break;
                    case EActivation.TOUCHPAD:
                        rc.OnTouchpadDown += beginTeleport;
                        break;
                    case EActivation.TRIGGER:
                        rc.OnTriggerDown += beginTeleport;
                        break;
                }
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

        void beginTeleport(VRRayCaster rc)
        {
            if (!(rc.isRayCasting && rc.CurrentHitObject))
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