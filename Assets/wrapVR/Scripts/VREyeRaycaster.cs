using System;
using UnityEngine;

namespace wrapVR
{
    // In order to interact with objects in the scene
    // this class casts a ray into the scene and if it finds
    // a VRInteractiveItem it exposes it for other classes to use.
    // This script should be generally be placed on the camera.
    public class VREyeRaycaster : VRRayCaster
    {
        Camera m_Camera;
        public void SetCamera(Camera cam)
        {
            m_Camera = cam;
        }
        Transform CameraT
        {
            get { return m_Camera.transform; }
        }

        protected override void setCallbacks()
        {
            if (m_VrInput == null)
                return;
            m_VrInput.OnClick += HandleClick;
            m_VrInput.OnDoubleClick += HandleDoubleClick;
            m_VrInput.OnUp += HandleUp;
            m_VrInput.OnDown += HandleDown;
        }

        protected override void clearCallbacks()
        {
            if (m_VrInput == null)
                return;
            m_VrInput.OnClick -= HandleClick;
            m_VrInput.OnDoubleClick -= HandleDoubleClick;
            m_VrInput.OnUp -= HandleUp;
            m_VrInput.OnDown -= HandleDown;
        }
        
        protected override void doRaycast()
        {
            // Show the debug ray if required
            if (ShowDebugRay)
            {
                Debug.DrawRay(CameraT.position, CameraT.forward * DebugRayLength, Color.blue, DebugRayDuration);
            }

            // Create a ray that points forwards from the camera.
            Ray ray = new Ray(CameraT.position, CameraT.forward);
            RaycastHit hit;
            
            // Do the raycast forweards to see if we hit an interactive item
            if (Physics.Raycast(ray, out hit, RayLength, ~ExclusionLayers))
            {
                // Something was hit, set at the hit position.
                m_HitPosition = hit.point;
                m_HitAngle = Quaternion.FromToRotation(Vector3.forward, hit.normal);

                VRInteractiveItem interactible = hit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                m_CurrentInteractible = interactible;

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (interactible && interactible != m_LastInteractible)
                    interactible.GazeOver(); 

                // Deactive the last interactive item 
                if (interactible != m_LastInteractible)
                    deactiveLastInteractible();

                m_LastInteractible = interactible;

                // Something was hit, set at the hit position.
                if (Reticle)
                    Reticle.SetPosition(hit);

                _onRaycastHit(hit);
            }
            else
            {
                // Nothing was hit, deactive the last interactive item.
                deactiveLastInteractible();
                m_CurrentInteractible = null;

                // Position the reticle at default distance.
                if (Reticle)
                    Reticle.SetPosition();
            }
        }

        protected override void deactiveLastInteractible()
        {
            if (m_LastInteractible == null)
                return;

            m_LastInteractible.GazeOut();
            m_LastInteractible = null;
        }
    }
}