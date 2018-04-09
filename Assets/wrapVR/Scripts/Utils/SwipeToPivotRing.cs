using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace wrapVR
{
    // Allows a swipe gesture to pivot this gameobject 
    // about a ring collider by a specified amount
    public class SwipeToPivotRing : MonoBehaviour
    {
        [Tooltip("The amount in degrees that a swipe left/right will pivot us")]
        [Range(0, 360)]
        public float PivotDegrees;

        [Tooltip("The ring collider object that we rotate around")]
        public RingCollider RingTarget;

        public bool FaceRing;

        public bool Fade;
        public float FadeTime;
        public Color FadeColor = Color.black;
        ScreenFade m_ScreenFade;

        float m_fPivotAmount = 0;

        // Use this for initialization
        void Start()
        {
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

            // We need a ring collider one way or another
            RingTarget = Util.DestroyEnsureComponent(gameObject, RingTarget);

            // Subscribe to all inputs (we don't care about a specific raycaster)
            foreach(VRInput input in VRCapabilityManager.GetInputs())
            {
                input.OnSwipe += Input_OnSwipe;
            }
        }

        // On fade in complete we actually do the pivot, unsubscribe, and start the fade out
        private void M_ScreenFade_OnFadeInComplete()
        {
            pivot(m_fPivotAmount);
            m_ScreenFade.OnFadeInComplete -= M_ScreenFade_OnFadeInComplete;
            m_ScreenFade.Fade(false, FadeTime, FadeColor);
        }

        // Swipe left / right to pivot
        private void Input_OnSwipe(SwipeDirection eDir)
        {
            if (eDir != SwipeDirection.LEFT && eDir != SwipeDirection.RIGHT)
                return;

            switch (eDir)
            {
                case SwipeDirection.LEFT:
                    if (Fade)
                        m_fPivotAmount = -PivotDegrees;
                    else
                        pivot(-PivotDegrees);
                    break;
                case SwipeDirection.RIGHT:
                    if (Fade)
                        m_fPivotAmount = PivotDegrees;
                    else
                        pivot(PivotDegrees);
                    break;
            }

            if (Fade)
            {
                // Subscribe to on fade in completed so we can teleport and start fade out
                m_ScreenFade.OnFadeInComplete += M_ScreenFade_OnFadeInComplete;
                m_ScreenFade.Fade(true, FadeTime, FadeColor);
            }
        }

        // 
        void pivot(float fDegrees)
        {
            // If we want to face the ring when we finish teleporting, we've got to 
            // position ourselves such that the current SDK head's forward points at
            // the center. To do this negate the XZ forward direction and find where
            // the point on the ring would be from the center in that direction
            if (FaceRing)
            {
                float fAngle = RingTarget.GetAngle(VRCapabilityManager.CameraRig.transform);
                float fNewAngle = fAngle + fDegrees;
                transform.position = RingTarget.PointFromAngle(fNewAngle, GetComponent<Collider>());
                Debug.Log(fNewAngle);
            }
            // Otherwise just move
            else
            {
                float fAngle = RingTarget.GetAngle(transform.position);
                float fNewAngle = fAngle + fDegrees;
                transform.position = RingTarget.PointFromAngle(fNewAngle, GetComponent<Collider>());
                VRCapabilityManager.CameraRig.transform.LookAt(RingTarget.Center);
            }
        }
    }
}