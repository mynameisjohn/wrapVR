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
        private void Start()
        {
            if (Reticle == null)
                Reticle = GetComponentInChildren<Reticle>();
        }

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
                RaycastHit hit;

                // Do the raycast forwards to see if we hit an interactive item
                if (Physics.Raycast(ray, out hit, RayLength, ~ExclusionLayers))
                {
                    // Something was hit, set at the hit position.
                    m_HitPosition = hit.point;
                    m_HitAngle = Quaternion.FromToRotation(Vector3.forward, hit.normal);

                    VRInteractiveItem interactible = hit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                    m_CurrentInteractible = interactible;
                    //print( m_CurrentInteractible.name );

                    // If we hit an interactive item and it's not the same as the last interactive item, then call PointerOver
                    if (interactible && interactible != m_LastInteractible)
                    {
                        interactible.PointerOver(m_VrInput);
                        if (m_VrInput.GetTrigger())
                            interactible.TriggerOver(m_VrInput);
                    }

                    // Deactive the last interactive item 
                    if (interactible != m_LastInteractible)
                        deactiveLastInteractible();
                    m_LastInteractible = interactible;

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
        }

        protected override void deactiveLastInteractible()
        {
            if (m_LastInteractible == null)
                return;

            m_LastInteractible.PointerOut(m_VrInput);
            if (m_VrInput.GetTrigger())
                m_LastInteractible.TriggerOut(m_VrInput);
            m_LastInteractible = null;
        }
    }
}