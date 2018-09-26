using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public abstract class FilterRayCasters : MonoBehaviour
    {
        public abstract List<VRRayCaster> getRayCasters();
        public abstract void filterListOfRayCasters(List<VRRayCaster> allRayCastersInScene);
    }
}