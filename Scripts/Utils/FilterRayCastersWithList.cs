using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class FilterRayCastersWithList : FilterRayCasters
    {
        public List<VRRayCaster> _RayCasters;
        public override List<VRRayCaster> getRayCasters()
        {
            return _RayCasters;
        }

        public override bool contains(VRRayCaster rc)
        {
            foreach (var filterRC in _RayCasters)
                if (filterRC == rc)
                    return true;
            return false;
        }
    }
}