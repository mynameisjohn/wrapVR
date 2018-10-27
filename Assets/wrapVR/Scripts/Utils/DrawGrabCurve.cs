using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    [RequireComponent(typeof(Grabbable))]
    public class DrawGrabCurve : DrawCurve
    {
        public uint NumCurvePoints { get { return 1 + (uint)(GetComponent<Grabbable>().PullDistance / UnitsPerCurvePoint); } }

        void OnGrab(Grabbable gr, VRRayCaster rc)
        {
            // First activate curve to generate points
            Transform FollowedTransform = gr.Followed;
            Transform SourceTransform = rc.transform;
            createCurvePoints( NumCurvePoints, SourceTransform, FollowedTransform, gr.transform);
        }

        void OnRelease(Grabbable gr, VRRayCaster rc)
        {
            destroyCurvePoints();
        }

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            // Draw and Deactivate curve when we are grabbed / released
            GetComponent<Grabbable>().OnGrab += OnGrab;
            GetComponent<Grabbable>().OnRelease += OnRelease;
        }
    }
}