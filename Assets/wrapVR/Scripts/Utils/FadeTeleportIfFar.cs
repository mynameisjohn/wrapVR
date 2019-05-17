using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class FadeTeleportIfFar : MonoBehaviour
    {
        public float _FadeTime = 0.3f;
        public float _FadeCutoff = 50f;

        public RayCastTeleport _Teleporter;
        SmoothFollowRing _ringFollower;
        ScreenFade _fader;
        Coroutine _coroFadeOut;
        float _followTime;

        bool _enable = true;

        // Start is called before the first frame update
        void Start()
        {
            _Teleporter = wrapVR.Util.DestroyEnsureComponent<RayCastTeleport>(gameObject, _Teleporter);
            _fader = VRCapabilityManager.mainCamera.GetComponent<ScreenFade>();
            _ringFollower = VRCapabilityManager.instance.GetComponent<SmoothFollowRing>();

            if (_fader == null || _Teleporter == null || _ringFollower == null || _Teleporter.Fade)
                Destroy(this);

            _followTime = _ringFollower.FollowTime;

            _Teleporter.OnPreTeleport += onPreTeleport;
            _fader.OnFadeInComplete += _fader_OnFadeInComplete;
            _fader.OnFadeOutComplete += _fader_OnFadeOutComplete;
        }

        private void _fader_OnFadeOutComplete()
        {
            if (_coroFadeOut != null)
            {
                StopCoroutine(_coroFadeOut);
                _coroFadeOut = null;
            }

            _ringFollower.FollowTime = _followTime;
        }

        private void _fader_OnFadeInComplete()
        {
            _ringFollower.enabled = true;
            _ringFollower.FollowTime = 0f;

            if (_coroFadeOut != null)
            {
                StopCoroutine(_coroFadeOut);
                _coroFadeOut = null;
            }

            _coroFadeOut = StartCoroutine(deferredFadeOut());
        }

        void onPreTeleport(Vector3 dest)
        {
            if (!_enable)
                return;

            float teleportDist = Vector3.Distance(_Teleporter.transform.position, dest);
            if (_FadeCutoff < teleportDist)
            {
                _fader.Fade(true, _FadeTime, Color.black);
                _ringFollower.enabled = false;
            }
        }

        IEnumerator deferredFadeOut()
        {
            while (Mathf.Pow(_FadeCutoff, 2) < _ringFollower.targetDistanceSq)
                yield return false;

            _fader.Fade(false, _FadeTime, Color.black);
            yield break;
        }
    }
}
