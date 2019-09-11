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
            if (_rayCasters == null)
                initCasterList();

            return _rayCasters;
        }
        void initCasterList()
        {
            _rayCasters = new List<VRRayCaster>();
            foreach (string tagFilter in TagFilterArray)
                foreach (VRRayCaster rc in wrapVR.VRCapabilityManager.RayCasters)
                    if (rc.tag == tagFilter)
                        _rayCasters.Add(rc);
        }

        public override bool contains(VRRayCaster rc)
        {
            foreach (var tag in TagFilterArray)
                if (rc.tag == tag)
                    return true;
            return false;
        }
    }
}