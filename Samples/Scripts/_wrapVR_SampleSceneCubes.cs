using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    namespace Samples
    {
        public class _wrapVR_SampleSceneCubes : MonoBehaviour
        {
            [Tooltip("The activations to test; NONE will PointerOver/Out")]
            public List<EActivation> _ActivationsToTest = new List<EActivation> { EActivation.NONE };

            Material _originalMaterial;

            void Start()
            {
                // Apply to all renderers
                foreach (Transform child in transform)
                {
                    // add a VRInteractiveItem
                    var v = Util.EnsureComponent<VRInteractiveItem>(child.gameObject);

                    // cache original material
                    if (!_originalMaterial)
                        _originalMaterial = v.GetComponent<Renderer>().sharedMaterial;

                    foreach (var activation in _ActivationsToTest)
                    {
                        // randomize color on Over, restore on Out
                        v.ActivationOverCallback(activation, (VRRayCaster rc) =>
                        {
                            v.GetComponent<Renderer>().material.color = Random.ColorHSV();
                        });
                        v.ActivationOutCallback(activation, (VRRayCaster rc) =>
                        {
                            v.GetComponent<Renderer>().sharedMaterial = _originalMaterial;
                        });
                    }
                }
            }
        }
    }
}