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

        public override bool contains(VRRayCaster rc)
        {
            foreach (var filterRC in RayCasters)
                if (filterRC == rc)
                    return true;
            return false;
        }
    }
}