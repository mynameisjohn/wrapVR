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
    }
}