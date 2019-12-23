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
        public VRRayCaster Source;
        public bool DisableWhileGrabbing = true;
        public EActivation Activation = EActivation.TRIGGER;

        // Off renderer is a single line
        public Color OffColor = Color.white;
        public Material OffMaterial;
        LineRenderer m_OffRenderer;
        public float OffStartWidth = 0.005f;
        public float OffEndWidth = 0f;
        public float OffLength = 2f;

        public Color OnColor;
        public Material OnMaterial;
        LineRenderer m_OnRendererColor;
        LineRenderer m_OnRendererWhite;
        public float OnWidth = 0.5f;
        public int CapVerts = 8;
        [Range(0, 1)]
        public float CenterWidthRel = 0.25f;

        public bool hasOff { get { return OffMaterial; } }
        public bool hasOn { get { return OnMaterial; } }

        protected bool isPointerActive
        {
            get
            {
                return Source.IsActivationDown(Activation);
            }
        }

        // Use this for initialization
        void Start()
        {
            // Make sure we have a raycast source
            Source = Util.DestroyEnsureComponent(gameObject, Source);
            
            if (hasOff)
            {
                m_OffRenderer = new GameObject("OffLine").AddComponent<LineRenderer>();
                m_OffRenderer.startWidth = OffStartWidth;
                m_OffRenderer.endWidth = OffEndWidth;
                m_OffRenderer.startColor = OffColor;
                m_OffRenderer.material = OffMaterial;
                m_OffRenderer.endColor = new Color(OffColor.r, OffColor.g, OffColor.b, 0);
                m_OffRenderer.useWorldSpace = true;
                m_OffRenderer.enabled = false;
            }

            if (hasOn)
            {
                m_OnRendererColor = new GameObject("OnLineColor").AddComponent<LineRenderer>();
                m_OnRendererWhite = new GameObject("OnLineWhite").AddComponent<LineRenderer>();

                Vector3[] v3LinePositions = new Vector3[] { Vector3.zero, new Vector3(0, 0, 1) };
                foreach (LineRenderer r in new LineRenderer[] { m_OnRendererColor, m_OnRendererWhite })
                {
                    r.numCapVertices = CapVerts;
                    r.positionCount = v3LinePositions.Length;
                    r.SetPositions(v3LinePositions);
                    r.useWorldSpace = true;
                    r.material = OnMaterial;
                }

                m_OnRendererColor.startWidth = OnWidth;
                m_OnRendererColor.endWidth = OnWidth;
                m_OnRendererColor.startColor = OnColor;
                m_OnRendererColor.endColor = OnColor;
                m_OnRendererWhite.startWidth = CenterWidthRel * OnWidth;
                m_OnRendererWhite.endWidth = CenterWidthRel * OnWidth;
                m_OnRendererWhite.startColor = Color.white;
                m_OnRendererWhite.endColor = Color.white;
                m_OnRendererColor.enabled = false;
                m_OnRendererWhite.enabled = false;
            }

            foreach (LineRenderer r in new LineRenderer[] { m_OffRenderer, m_OnRendererColor, m_OnRendererWhite })
            {
                if (r)
                {
                    r.transform.SetParent(transform);
                    r.transform.localPosition = Vector3.zero;
                }
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            // Don't do anything if laser is disabled
            if (VRCapabilityManager.isLaserDisabled)
                return;

            if (!Source.isActiveAndEnabled)
            {
                if (hasOff)
                {
                    m_OffRenderer.enabled = false;
                }
                if (hasOn)
                {
                    m_OnRendererColor.enabled = false;
                    m_OnRendererWhite.enabled = false;
                }
                return;
            }

            Vector3 v3PointDir = Source.CurrentHitObject ? (Source.CurrentHitPosition - Source.FromTransform.position) : Source.FromTransform.forward;
            if (!(DisableWhileGrabbing && Source.isGrabbing) && isPointerActive)
            {
                if (hasOff)
                {
                    m_OffRenderer.enabled = false;
                }
                if (hasOn)
                {
                    m_OnRendererColor.enabled = true;
                    m_OnRendererWhite.enabled = true;

                    Vector3 v3Dst = Source.CurrentHitObject ? Source.CurrentHitPosition : (Source.FromTransform.position + v3PointDir.normalized * Source.RayLength);
                    foreach (LineRenderer r in new LineRenderer[] { m_OnRendererColor, m_OnRendererWhite })
                    {
                        r.SetPosition(0, Source.FromTransform.position);
                        r.SetPosition(1, v3Dst);
                    }
                }
            }
            else
            {
                if (hasOff)
                {
                    m_OffRenderer.enabled = true;
                    m_OffRenderer.SetPosition(0, Source.FromTransform.position);
                    m_OffRenderer.SetPosition(1, Source.FromTransform.position + OffLength * v3PointDir.normalized);
                }
                if (hasOn)
                {
                    m_OnRendererColor.enabled = false;
                    m_OnRendererWhite.enabled = false;
                }
            }
        }
    }
}
