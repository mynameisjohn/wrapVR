using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class UnparentOnStart : MonoBehaviour
    {
        void Start()
        {
            transform.parent = null;
        }
    }
}