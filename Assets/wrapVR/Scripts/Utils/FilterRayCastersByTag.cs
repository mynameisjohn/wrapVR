using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace wrapVR
{
    public class FilterRayCastersByTag : FilterRayCasters
    {
        [TagSelector]
        public string[] TagFilterArray = new string[] { };

        List<VRRayCaster> _rayCasters;
        public override List<VRRayCaster> getRayCasters()
        {
            return _rayCasters;
        }
        public override void filterListOfRayCasters(List<VRRayCaster> allRayCastersInScene)
        {
            _rayCasters = new List<VRRayCaster>();
            foreach (string tagFilter in TagFilterArray)
                foreach (VRRayCaster rc in allRayCastersInScene)
                    if (rc.tag == tagFilter)
                        _rayCasters.Add(rc);
        }
    }
}