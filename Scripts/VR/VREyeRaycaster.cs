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
            if (_vrInput == null)
                return;
        }

        protected override void clearCallbacks()
        {
            if (_vrInput == null)
                return;
        }
        
        protected override void doRaycast()
        { 
            // Show the debug ray if required
            if (_ShowDebugRay)
            {
                Debug.DrawRay(FromTransform.position, FromTransform.forward * _DebugRayLength, Color.blue, _DebugRayDuration);
            }
            
            // Create a ray that points forwards from the camera.
            Ray ray = new Ray(FromTransform.position, FromTransform.forward);
            _currentHit = new RaycastHit();
            
            // See if we hit anything
            bool bValidHit = Physics.Raycast(ray, out _currentHit, _RayLength, ~_ExclusionLayers);

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

            if (bValidHit)
            {
                var filter = _currentHit.collider.GetComponent<FilterRayCasters>();
                if (filter)
                    bValidHit = filter.contains(this);
            }

            // Do the raycast forweards to see if we hit an interactive item
            if (bValidHit)
            {
                VRInteractiveItem interactible = _currentHit.collider.GetComponent<VRInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                _currentInteractible = interactible;
                if (_Log)
                    Debug.Log(name + " " + _currentInteractible.name + " " + _currentHit.point);

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (interactible && interactible != _lastInteractible)
                {
                    if (_Log)
                        Debug.Log(name + " GazeOver " + _currentInteractible.name);
                    interactible.GazeOver(this); 
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
            }
        }

        protected override void deactiveLastInteractible()
        {
            if (_lastInteractible == null)
                return;

            if (_Log)
                Debug.Log(name + " GazeOut " + _currentInteractible.name);

            _lastInteractible.GazeOut(this);
            _lastInteractible = null;
        }
    }
}