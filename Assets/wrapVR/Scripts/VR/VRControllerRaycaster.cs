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
            if (_vrInput == null)
                return;
            _vrInput.OnTouchUp += HandleTouchUp;
            _vrInput.OnTouchDown += HandleTouchDown;
            _vrInput.OnTouchpadUp += HandleTouchpadUp;
            _vrInput.OnTouchpadDown += HandleTouchpadDown;
            _vrInput.OnTriggerUp += HandleTriggerUp;
            _vrInput.OnTriggerDown += HandleTriggerDown;
            _vrInput.OnGripUp += HandleGripUp;
            _vrInput.OnGripDown += HandleGripDown;
        }

        protected override void clearCallbacks()
        {
            if (_vrInput == null)
                return;
            _vrInput.OnTouchUp -= HandleTouchUp;
            _vrInput.OnTouchDown -= HandleTouchDown;
            _vrInput.OnTouchpadUp -= HandleTouchpadUp;
            _vrInput.OnTouchpadDown -= HandleTouchpadDown;
            _vrInput.OnTriggerUp -= HandleTriggerUp;
            _vrInput.OnTriggerDown -= HandleTriggerDown;
            _vrInput.OnGripUp -= HandleGripUp;
            _vrInput.OnGripDown -= HandleGripDown;
        }

        protected virtual bool castRayFromController(out RaycastHit hit)
        {
            Ray ray = new Ray(FromTransform.position, FromTransform.forward);
            return Physics.Raycast(ray, out hit, _RayLength, ~_ExclusionLayers);
        }

        protected override void doRaycast()
        {
            // Show the debug ray if required
            if (_ShowDebugRay)
            {
                Debug.DrawRay(FromTransform.position, FromTransform.forward * _DebugRayLength, Color.blue, _DebugRayDuration);
            }

            // Create a ray that points forwards from the controller.
            _currentHit = new RaycastHit();

            // Do the raycast forwards to see if we hit an interactive item
            bool bValidHit = castRayFromController(out _currentHit);

            if (bValidHit)
            {
                var filter = _currentHit.collider.GetComponent<FilterRayCasters>();
                if (filter)
                    bValidHit = filter.contains(this);
            }

            // Maybe filter out for navmesh
            if (bValidHit && _ForNavMesh)
            {
                // Sample the navmesh
                UnityEngine.AI.NavMeshHit nmHit = new UnityEngine.AI.NavMeshHit();

                if (UnityEngine.AI.NavMesh.SamplePosition(_currentHit.point, out nmHit, _NavMeshSampleDistance, _NavMeshAreaFilter))
                {
                    // Move hit position to navmesh
                    _currentHit.point = nmHit.position;
                }
                else
                {
                    bValidHit = false;
                }
            }

            // If hit is still valid then trigger interaction
            if (bValidHit)
            {
                VRInteractiveItem interactible = _currentHit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                _currentInteractible = interactible;

                // If we hit an interactive item and it's not the same as the last interactive item, then call PointerOver
                if (interactible && interactible != _lastInteractible)
                {
                    _currentInteractible = interactible;
                    if (_Log)
                        Debug.Log(name + " " + _currentInteractible.name + " " + _currentHit.point);

                    if (shouldInteract)
                    {
                        if (!isTriggerDown || VRCapabilityManager.canPointIfTrigger)
                        {
                            if (_Log)
                                Debug.Log(name + " PointerOver " + _currentInteractible.name);
                            _currentInteractible.PointerOver(this);
                        }

                        if (_vrInput.GetTrigger())
                        {
                            if (_Log)
                                Debug.Log(name + " TriggerOver " + _currentInteractible.name);
                            _currentInteractible.TriggerOver(this);
                        }
                        if (_vrInput.GetTouch())
                        {
                            if (_Log)
                                Debug.Log(name + " TouchOver " + _currentInteractible.name);
                            _currentInteractible.TouchOver(this);
                        }
                        if (_vrInput.GetTouchpad())
                        {
                            if (_Log)
                                Debug.Log(name + " TouchpadOver " + _currentInteractible.name);
                            _currentInteractible.TouchpadOver(this);
                        }
                        if (_vrInput.GetGrip())
                        {
                            if (_Log)
                                Debug.Log(name + " GripOver " + _currentInteractible.name);
                            _currentInteractible.GripOver(this);
                        }
                    }
                }

                // Deactive the last interactive item 
                if (interactible != _lastInteractible)
                    deactiveLastInteractible();
                _lastInteractible = interactible;

                // Signal raycast hit
                _onRaycastHit(_currentHit);
            }
            else
            {
                // Nothing was hit, deactive the last interactive item.
                deactiveLastInteractible();
                _currentInteractible = null;
                _currentHit = new RaycastHit();
            }
        }

        protected override void deactiveLastInteractible()
        {
            if (_lastInteractible == null || !shouldInteract)
                return;

            if (!isTriggerDown || VRCapabilityManager.canPointIfTrigger)
            {
                if (_Log)
                    Debug.Log(name + " PointerOut " + _currentInteractible.name);
                _lastInteractible.PointerOut(this);
            }
            if (_vrInput.GetTouch())
            {
                if (_Log)
                    Debug.Log(name + " TouchOut " + _currentInteractible.name);
                _lastInteractible.TouchOut(this);
            }
            if (_vrInput.GetTouchpad())
            {
                if (_Log)
                    Debug.Log(name + " TouchpadOut " + _currentInteractible.name);
                _lastInteractible.TouchpadOut(this);
            }
            if (_vrInput.GetTrigger())
            {
                if (_Log)
                    Debug.Log(name + " TriggerOut " + _currentInteractible.name);
                _lastInteractible.TriggerOut(this);
            }
            if (_vrInput.GetGrip())
            {
                if (_Log)
                    Debug.Log(name + " GripOut " + _currentInteractible.name);
                _lastInteractible.GripOut(this);
            }

            _lastInteractible = null;
        }
    }
}