using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class RayCastTeleport : MonoBehaviour
    {
        // Callbacks
        public System.Action<Vector3> OnPreTeleport;
        public System.Action<Vector3> OnTeleport;

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

        public float DisableFor;
        Coroutine m_coroDisableFor;

        public LayerMask ForbiddenLayers;

        Vector3 m_v3PendingDestination;

        // users of preteleport can set this 
        // to cancel the teleportation
        bool _shouldCancelTeleport;
        public bool ShouldCancelTeleport { set { _shouldCancelTeleport = value; } }

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
            var rayCasters = GetComponent<FilterRayCasters>() ? 
                GetComponent<FilterRayCasters>().getRayCasters() : 
                VRCapabilityManager.RayCasters;
            foreach (VRRayCaster rc in rayCasters)
            {
                rc.ActivationDownCallback(Activation, beginTeleport, true);
            }
        }

        // When the fade in completes do the teleport and start the fade out
        private void M_ScreenFade_OnFadeInComplete()
        {
            teleport(m_v3PendingDestination);
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

        IEnumerator disableCoroFor()
        {
            if (DisableFor != 0f)
                yield return new WaitForSeconds(DisableFor);
            m_coroDisableFor = null;
        }

        void teleport(Vector3 v3Destination)
        {
            // clear state now, check it after preteleport
            _shouldCancelTeleport = false;

            if (OnPreTeleport != null)
                OnPreTeleport(v3Destination);

            if (_shouldCancelTeleport)
            {
                _shouldCancelTeleport = false;
                return;
            }

            if (ToTeleport.GetComponent<UnityEngine.AI.NavMeshAgent>())
                ToTeleport.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(v3Destination);
            else
                ToTeleport.transform.position = v3Destination;

            if (OnTeleport != null)
                OnTeleport(v3Destination);

            // Whenever we teleport clear double-click coroutine
            m_coroDoubleClick = null;

            // start coroutine to potentially disable for a duration
            // won't do anything if the duration is zero
            if (m_coroDisableFor != null)
                StopCoroutine(m_coroDisableFor);
            m_coroDisableFor = StartCoroutine(disableCoroFor());
        }

        protected virtual void beginTeleport(VRRayCaster rc)
        {
            if (!(rc.isRayCasting && rc.CurrentInteractible))
                return;

            // return early if this isn't null
            if (m_coroDisableFor != null)
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
                // Otherwise cache the destination, start double click timer and get out
                else
                {
                    m_v3PendingDestination = rc.GetLastHitPosition();
                    m_coroDoubleClick = StartCoroutine(doubleClickCoro());
                    return;
                }
            }

            if (Fade)
            {
                // use previously cached destination if double click
                if (!DoubleClick)
                    m_v3PendingDestination = rc.GetLastHitPosition();

                // Subscribe to on fade in completed so we can teleport and start fade out
                m_ScreenFade.OnFadeInComplete += M_ScreenFade_OnFadeInComplete;
                m_ScreenFade.Fade(true, FadeTime, FadeColor);
            }
            // If we aren't fading then just do the teleport
            else if (rc.CurrentInteractible)
            {

                // Debug.Log("Teleporting " + name + " to " + rc.GetLastHitPosition() + " using " + rc.name);

                // use the cached position if double click
                teleport(DoubleClick ? m_v3PendingDestination : rc.GetLastHitPosition());
            }
        }
    }
}