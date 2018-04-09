using System;
using UnityEngine;

namespace wrapVR
{
    // In order to interact with objects in the scene
    // this class casts a ray into the scene and if it finds
    // a VRInteractiveItem it exposes it for other classes to use.
    // This script should be generally be placed on the controller.
    public class VRControllerRaycaster : VRRayCaster
    {
        protected override void setCallbacks()
        {
            if (m_VrInput == null)
                return;
            m_VrInput.OnTriggerUp += HandleTriggerUp;
            m_VrInput.OnTriggerDown += HandleTriggerDown;
            m_VrInput.OnTouchpadUp += HandleTouchpadUp;
            m_VrInput.OnTouchpadDown += HandleTouchpadDown;
            m_VrInput.OnTouchpadTouchUp += HandleTouchUp;
            m_VrInput.OnTouchpadTouchDown += HandleTouchDown;
        }

        protected override void clearCallbacks()
        {
            if (m_VrInput == null)
                return;
            m_VrInput.OnTriggerUp -= HandleTriggerUp;
            m_VrInput.OnTriggerDown -= HandleTriggerDown;
            m_VrInput.OnTouchpadUp -= HandleTouchpadUp;
            m_VrInput.OnTouchpadDown -= HandleTouchpadDown;
            m_VrInput.OnTouchpadTouchUp -= HandleTouchUp;
            m_VrInput.OnTouchpadTouchDown -= HandleTouchDown;
        }

        protected virtual bool castRayFromController(out RaycastHit hit)
        {
            Ray ray = new Ray(ctrlT.position, ctrlT.forward);
            return Physics.Raycast(ray, out hit, RayLength, ~ExclusionLayers);
        }

        protected override void doRaycast()
        {
            if (m_Enabled)
            {
                // Show the debug ray if required
                if (ShowDebugRay)
                {
                    Debug.DrawRay(ctrlT.position, ctrlT.forward * DebugRayLength, Color.blue, DebugRayDuration);
                }

                // Create a ray that points forwards from the controller.
                Ray ray = new Ray(ctrlT.position, ctrlT.forward);
                m_CurrentHit = new RaycastHit();

                // Do the raycast forwards to see if we hit an interactive item
                bool bValidHit = castRayFromController(out m_CurrentHit);

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

                // If hit is still valid then trigger interaction
                if (bValidHit)
                {
                    VRInteractiveItem interactible = m_CurrentHit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                    m_CurrentInteractible = interactible;
                    // print(transform.name + " " + m_CurrentHit.point);

                    // If we hit an interactive item and it's not the same as the last interactive item, then call PointerOver
                    if (interactible && interactible != m_LastInteractible)
                    {
                        interactible.PointerOver(this);
                        if (m_VrInput.GetTrigger())
                            interactible.TriggerOver(this);
                    }

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
        }

        protected override void deactiveLastInteractible()
        {
            if (m_LastInteractible == null)
                return;

            m_LastInteractible.PointerOut(this);
            if (m_VrInput.GetTrigger())
                m_LastInteractible.TriggerOut(this);
            m_LastInteractible = null;
        }
    }
}