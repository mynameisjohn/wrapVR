using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Straight pointer renderer
    // Uses a line renderer to draw curves
    // for the various input activation states
    public class LaserPointer : MonoBehaviour
    {
        public VRRayCaster _SourceCaster;
        public bool _DisableWhileGrabbing = true;
        public EActivation _Activation = EActivation.TRIGGER;

        // Off renderer is a single line
        public Color _OffColor = Color.white;
        public Material _OffMaterial;
        LineRenderer _offRenderer;
        public float _OffStartWidth = 0.005f;
        public float _OffEndWidth = 0f;
        public float _OffLength = 2f;

        public Color _OnColor;
        public Material _OnMaterial;
        LineRenderer _onRendererColor;
        LineRenderer _onRendererWhite;
        public float _OnWidth = 0.5f;
        public int _CapVerts = 8;
        [Range(0, 1)]
        public float _CenterWidthRel = 0.25f;

        public bool hasOff { get { return _OffMaterial; } }
        public bool hasOn { get { return _OnMaterial; } }

        protected bool isPointerActive
        {
            get
            {
                return _SourceCaster.IsActivationDown(_Activation);
            }
        }

        // Use this for initialization
        void Start()
        {
            // Make sure we have a raycast source
            _SourceCaster = Util.DestroyEnsureComponent(gameObject, _SourceCaster);
            
            if (hasOff)
            {
                _offRenderer = new GameObject("OffLine").AddComponent<LineRenderer>();
                _offRenderer.startWidth = _OffStartWidth;
                _offRenderer.endWidth = _OffEndWidth;
                _offRenderer.startColor = _OffColor;
                _offRenderer.material = _OffMaterial;
                _offRenderer.endColor = new Color(_OffColor.r, _OffColor.g, _OffColor.b, 0);
                _offRenderer.useWorldSpace = true;
                _offRenderer.enabled = false;
            }

            if (hasOn)
            {
                _onRendererColor = new GameObject("OnLineColor").AddComponent<LineRenderer>();
                _onRendererWhite = new GameObject("OnLineWhite").AddComponent<LineRenderer>();

                Vector3[] v3LinePositions = new Vector3[] { Vector3.zero, new Vector3(0, 0, 1) };
                foreach (LineRenderer r in new LineRenderer[] { _onRendererColor, _onRendererWhite })
                {
                    r.numCapVertices = _CapVerts;
                    r.positionCount = v3LinePositions.Length;
                    r.SetPositions(v3LinePositions);
                    r.useWorldSpace = true;
                    r.material = _OnMaterial;
                }

                _onRendererColor.startWidth = _OnWidth;
                _onRendererColor.endWidth = _OnWidth;
                _onRendererColor.startColor = _OnColor;
                _onRendererColor.endColor = _OnColor;
                _onRendererWhite.startWidth = _CenterWidthRel * _OnWidth;
                _onRendererWhite.endWidth = _CenterWidthRel * _OnWidth;
                _onRendererWhite.startColor = Color.white;
                _onRendererWhite.endColor = Color.white;
                _onRendererColor.enabled = false;
                _onRendererWhite.enabled = false;
            }

            foreach (LineRenderer r in new LineRenderer[] { _offRenderer, _onRendererColor, _onRendererWhite })
            {
                if (r)
                {
                    r.transform.SetParent(transform);
                    r.transform.localPosition = Vector3.zero;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            // Don't do anything if laser is disabled
            if (VRCapabilityManager.isLaserDisabled)
                return;

            if (!_SourceCaster.isActiveAndEnabled)
            {
                if (hasOff)
                {
                    _offRenderer.enabled = false;
                }
                if (hasOn)
                {
                    _onRendererColor.enabled = false;
                    _onRendererWhite.enabled = false;
                }
                return;
            }

            Vector3 v3PointDir = _SourceCaster.CurrentHitObject ? (_SourceCaster.CurrentHitPosition - _SourceCaster.FromTransform.position) : _SourceCaster.FromTransform.forward;
            if (!(_DisableWhileGrabbing && _SourceCaster.isGrabbing) && isPointerActive)
            {
                if (hasOff)
                {
                    _offRenderer.enabled = false;
                }
                if (hasOn)
                {
                    _onRendererColor.enabled = true;
                    _onRendererWhite.enabled = true;

                    Vector3 v3Dst = _SourceCaster.CurrentHitObject ? _SourceCaster.CurrentHitPosition : (_SourceCaster.FromTransform.position + v3PointDir.normalized * _SourceCaster._RayLength);
                    foreach (LineRenderer r in new LineRenderer[] { _onRendererColor, _onRendererWhite })
                    {
                        r.SetPosition(0, _SourceCaster.FromTransform.position);
                        r.SetPosition(1, v3Dst);
                    }
                }
            }
            else
            {
                if (hasOff)
                {
                    _offRenderer.enabled = true;
                    _offRenderer.SetPosition(0, _SourceCaster.FromTransform.position);
                    _offRenderer.SetPosition(1, _SourceCaster.FromTransform.position + _OffLength * v3PointDir.normalized);
                }
                if (hasOn)
                {
                    _onRendererColor.enabled = false;
                    _onRendererWhite.enabled = false;
                }
            }
        }
    }
}
