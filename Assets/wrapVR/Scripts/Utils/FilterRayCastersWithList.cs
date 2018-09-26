using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class FilterRayCastersWithList : FilterRayCasters
    {
        public List<VRRayCaster> RayCasters;
        public override List<VRRayCaster> getRayCasters()
        {
            return RayCasters;
        }
        public override void filterListOfRayCasters(List<VRRayCaster> allRayCastersInScene)
        {
            return; // we don't need to do anything here
        }
    }
}