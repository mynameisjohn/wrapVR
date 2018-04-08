using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    // Simple utility function to unparent on start
    public class UnparentOnStart : MonoBehaviour
    {
        void Start()
        {
            transform.parent = null;
        }
    }
}