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
        public Transform _ToTeleport;

        [Tooltip("VR Input activation to teleport")]
        public EActivation _Activation;

        // Double click
        public bool _DoubleClick;
        public float _DoubleClickTimer;
        Coroutine _coroDoubleClick;

        // Fade parameters
        public bool _Fade;
        public float _FadeTime;
        ScreenFade _screenFade;

        public float _DisableFor;
        Coroutine _coroDisableFor;

        public LayerMask _ForbiddenLayers;

        Vector3 _pendingDestination;

        public VRRayCaster teleportingCaster { get; protected set; }

        public bool isDoubleClickTimerRunning { get { return _coroDoubleClick != null; } }

        // users of preteleport can set this 
        // to cancel the teleportation
        bool _shouldCancelTeleport;
        public bool shouldCancelTeleport { set { _shouldCancelTeleport = value; } }

        // Use this for initialization
        virtual protected void Start()
        {
            // Get teleport transform (use ours if null)
            if (_ToTeleport == null)
                _ToTeleport = transform;

            // Find screen fade if desired
            if (_Fade)
            {
                if (VRCapabilityManager.mainCamera == null)
                {
                    Debug.LogError("Unable to get main camera");
                    _Fade = false;
                }
                else
                {
                    _screenFade = VRCapabilityManager.mainCamera.GetComponent<ScreenFade>();
                    if (_screenFade == null)
                    {
                        Debug.LogError("Error: missing screen fade for teleporter. Disabling fade...");
                        _Fade = false;
                    }
                }
            }

            // Capture each controller and use the teleport function
            var rayCasters = GetComponent<FilterRayCasters>() ? 
                GetComponent<FilterRayCasters>().getRayCasters() : 
                VRCapabilityManager.RayCasters;
            foreach (VRRayCaster rc in rayCasters)
            {
                rc.ActivationDownCallback(_Activation, beginTeleport, true);
            }
        }

        // When the fade in completes do the teleport and start the fade out
        private void ScreenFade_OnFadeInComplete()
        {
            teleport(_pendingDestination);
            _screenFade.Fade(false, _FadeTime);
            _screenFade.OnFadeInComplete -= ScreenFade_OnFadeInComplete;
        }

        // While this is not null we're waiting for a double click
        IEnumerator doubleClickCoro()
        {
            yield return new WaitForSeconds(_DoubleClickTimer);
            _coroDoubleClick = null;
            yield break;
        }

        IEnumerator disableCoroFor()
        {
            if (_DisableFor != 0f)
                yield return new WaitForSeconds(_DisableFor);
            _coroDisableFor = null;
        }

        protected virtual bool teleport(Vector3 v3Destination)
        {
            // clear state now, check it after preteleport
            _shouldCancelTeleport = false;

            if (OnPreTeleport != null)
                OnPreTeleport(v3Destination);

            if (_shouldCancelTeleport)
            {
                _shouldCancelTeleport = false;
                return false;
            }

            if (_ToTeleport.GetComponent<UnityEngine.AI.NavMeshAgent>())
                _ToTeleport.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(v3Destination);
            else
                _ToTeleport.transform.position = v3Destination;

            if (OnTeleport != null)
                OnTeleport(v3Destination);

            teleportingCaster = null;

            // Whenever we teleport clear double-click coroutine
            _coroDoubleClick = null;

            // start coroutine to potentially disable for a duration
            // won't do anything if the duration is zero
            if (_coroDisableFor != null)
                StopCoroutine(_coroDisableFor);
            _coroDisableFor = StartCoroutine(disableCoroFor());

            return true;
        }

        protected virtual void beginTeleport(VRRayCaster rc)
        {
            if (!(rc.isRayCasting && rc.CurrentInteractible))
                return;

            // return early if this isn't null
            if (_coroDisableFor != null)
                return;

            if (0 != ((1 << rc.CurrentInteractible.gameObject.layer) & _ForbiddenLayers.value)) 
                return;

            if (_DoubleClick)
            {
                // If the timer is running stop it and teleport
                if (_coroDoubleClick != null)
                {
                    StopCoroutine(_coroDoubleClick);
                    _coroDoubleClick = null;
                }
                // Otherwise cache the destination, start double click timer and get out
                else
                {
                    _pendingDestination = rc.GetLastHitPosition();
                    _coroDoubleClick = StartCoroutine(doubleClickCoro());
                    return;
                }
            }

            if (_Fade)
            {
                // use previously cached destination if double click
                if (!_DoubleClick)
                    _pendingDestination = rc.GetLastHitPosition();

                // Subscribe to on fade in completed so we can teleport and start fade out
                teleportingCaster = rc;
                _screenFade.OnFadeInComplete += ScreenFade_OnFadeInComplete;
                _screenFade.Fade(true, _FadeTime);
            }
            // If we aren't fading then just do the teleport
            else if (rc.CurrentInteractible)
            {

                // Debug.Log("Teleporting " + name + " to " + rc.GetLastHitPosition() + " using " + rc.name);

                // use the cached position if double click
                teleportingCaster = rc;
                teleport(_DoubleClick ? _pendingDestination : rc.GetLastHitPosition());
            }
        }
    }
}