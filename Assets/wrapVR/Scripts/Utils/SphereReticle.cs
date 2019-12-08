using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class SphereReticle : Reticle
    {
        public float _Radius = 0.01f;
        public Color _Color;

        Renderer _sphereRenderer;

        protected override void Start()
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere.GetComponent<Collider>());
            sphere.transform.parent = transform;
            sphere.transform.localScale = _Radius * Vector3.one;
            _sphereRenderer = sphere.GetComponent<Renderer>();
            _sphereRenderer.material.color = _Color;

            base.Start();
        }

        public override void Hide()
        {
            _sphereRenderer.enabled = false;
        }

        public override void Show()
        {
            _sphereRenderer.enabled = true;
        }
    }
}