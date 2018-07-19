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
            if (FromTransform == null)
                FromTransform = m_Camera.transform;
        }
        
        protected override void setCallbacks()
        {
            if (m_VrInput == null)
                return;
        }

        protected override void clearCallbacks()
        {
            if (m_VrInput == null)
                return;
        }
        
        protected override void doRaycast()
        { 
            // Show the debug ray if required
            if (ShowDebugRay)
            {
                Debug.DrawRay(FromTransform.position, FromTransform.forward * DebugRayLength, Color.blue, DebugRayDuration);
            }
            
            // Create a ray that points forwards from the camera.
            Ray ray = new Ray(FromTransform.position, FromTransform.forward);
            m_CurrentHit = new RaycastHit();
            
            // See if we hit anything
            bool bValidHit = Physics.Raycast(ray, out m_CurrentHit, RayLength, ~ExclusionLayers);

            // Maybe filter out for navmesh
            if (bValidHit && ForNavMesh)
            {                
                // Sample the navmesh
                UnityEngine.AI.NavMeshHit nmHit = new UnityEngine.AI.NavMeshHit();

                if (UnityEngine.AI.NavMesh.SamplePosition(m_CurrentHit.point, out nmHit, NavMeshSampleDistance, NavMeshAreaFilter))
                {
                    // Move hit position to navmesh
                    m_CurrentHit.point = nmHit.position;
                }
                else
                {
                    bValidHit = false;
                }
            }

            // Do the raycast forweards to see if we hit an interactive item
            if (bValidHit)
            {
                VRInteractiveItem interactible = m_CurrentHit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                m_CurrentInteractible = interactible;

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (interactible && interactible != m_LastInteractible)
                    interactible.GazeOver(this); 

                // Deactive the last interactive item 
                if (interactible != m_LastInteractible)
                    deactiveLastInteractible();

                m_LastInteractible = interactible;

                // Signal raycast hit
                _onRaycastHit(m_CurrentHit);
            }
            else
            {
                // Nothing was hit, deactive the last interactive item.
                deactiveLastInteractible();
                m_CurrentInteractible = null;
            }
        }

        protected override void deactiveLastInteractible()
        {
            if (m_LastInteractible == null)
                return;

            m_LastInteractible.GazeOut(this);
            m_LastInteractible = null;
        }
    }
}