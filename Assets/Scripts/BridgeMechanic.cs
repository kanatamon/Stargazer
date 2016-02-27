using UnityEngine;
using System.Collections;

public class BridgeMechanic : Mechanic {

    public bool reversed = false;
    public BridgeUnit[] units;

#if UNITY_EDITOR
    
    [RangeAttribute(0,1)]
    public float controllingValue;

#endif
    
    
    public override void MovePercentage(float value){
        if (reversed) {
            value = Mathf.Clamp (value, 0, 1);
            value = 1 - value;
        }
        
        for (int i = 0; i < units.Length; i++) {
            units [i].node.enabled = units [i].alwaysEnable || (units [i].enabledOnZero && value == 0) || value >= units [i].threshold;
            
//            units [i].node.collider.enabled = value >= units [i].threshold;
            
            float unitsValue = (units [i].threshold == 0) ? 0 : value / units [i].threshold;
            units [i].transform.localPosition = Vector3.Lerp (units [i].origin, units [i].target, unitsValue);
        }
    }
    
    public override WayPointNode[] GetWayPointNodes (){
        WayPointNode[] nodes = new WayPointNode[units.Length];
        
        for (int i = 0; i < units.Length; i++) {
            nodes [i] = units [i].node;
        }
        
        return nodes;
    }

#if UNITY_EDITOR
    void OnDrawGizmos(){
        if (!Application.isPlaying) {
            MovePercentage (controllingValue);
        }
    }
#endif
    
    [System.Serializable]
    public struct BridgeUnit{
        public WayPointNode node;
        public Vector3 origin;
        public Vector3 target;
        public bool enabledOnZero;
        public bool alwaysEnable;
        
        [RangeAttribute(0,1)]
        public float threshold;
        
        public Transform transform{ get{ return node.transform; } }
    }
}
