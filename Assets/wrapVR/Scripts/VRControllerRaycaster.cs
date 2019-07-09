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
            m_VrInput.OnTouchUp += HandleTouchUp;
            m_VrInput.OnTouchDown += HandleTouchDown;
            m_VrInput.OnTouchpadUp += HandleTouchpadUp;
            m_VrInput.OnTouchpadDown += HandleTouchpadDown;
            m_VrInput.OnTriggerUp += HandleTriggerUp;
            m_VrInput.OnTriggerDown += HandleTriggerDown;
            m_VrInput.OnGripUp += HandleGripUp;
            m_VrInput.OnGripDown += HandleGripDown;
        }

        protected override void clearCallbacks()
        {
            if (m_VrInput == null)
                return;
            m_VrInput.OnTouchUp -= HandleTouchUp;
            m_VrInput.OnTouchDown -= HandleTouchDown;
            m_VrInput.OnTouchpadUp -= HandleTouchpadUp;
            m_VrInput.OnTouchpadDown -= HandleTouchpadDown;
            m_VrInput.OnTriggerUp -= HandleTriggerUp;
            m_VrInput.OnTriggerDown -= HandleTriggerDown;
            m_VrInput.OnGripUp -= HandleGripUp;
            m_VrInput.OnGripDown -= HandleGripDown;
        }

        protected virtual bool castRayFromController(out RaycastHit hit)
        {
            Ray ray = new Ray(FromTransform.position, FromTransform.forward);
            return Physics.Raycast(ray, out hit, RayLength, ~ExclusionLayers);
        }

        protected override void doRaycast()
        {
            if (m_Enabled)
            {
                // Show the debug ray if required
                if (ShowDebugRay)
                {
                    Debug.DrawRay(FromTransform.position, FromTransform.forward * DebugRayLength, Color.blue, DebugRayDuration);
                }

                // Create a ray that points forwards from the controller.
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
                        m_CurrentHit = new RaycastHit();
                    }
                }

                // If hit is still valid then trigger interaction
                if (bValidHit)
                {
                    VRInteractiveItem interactible = m_CurrentHit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                    m_CurrentInteractible = interactible;

                    // If we hit an interactive item and it's not the same as the last interactive item, then call PointerOver
                    if (interactible && interactible != m_LastInteractible)
                    {
                        m_CurrentInteractible = interactible;
                        // Debug.Log(m_CurrentInteractible.name + " " + m_CurrentHit.point);

                        if (shouldInteract)
                        {
                            if (!isTriggerDown || VRCapabilityManager.canPointIfTrigger)
                                m_CurrentInteractible.PointerOver(this);

                            if (m_VrInput.GetTrigger())
                                m_CurrentInteractible.TriggerOver(this);
                            if (m_VrInput.GetTouch())
                                m_CurrentInteractible.TouchOver(this);
                            if (m_VrInput.GetTouchpad())
                                m_CurrentInteractible.TouchpadOver(this);
                            if (m_VrInput.GetGrip())
                                m_CurrentInteractible.GripOver(this);
                        }
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
            if (m_LastInteractible == null || !shouldInteract)
                return;

            if (!isTriggerDown || VRCapabilityManager.canPointIfTrigger)
                m_LastInteractible.PointerOut(this);

            if (m_VrInput.GetTouch())
                m_LastInteractible.TouchOut(this);
            if (m_VrInput.GetTouchpad())
                m_LastInteractible.TouchpadOut(this);
            if (m_VrInput.GetTrigger())
                m_LastInteractible.TriggerOut(this);
            if (m_VrInput.GetGrip())
                m_LastInteractible.GripOut(this);

            m_LastInteractible = null;
        }
    }
}