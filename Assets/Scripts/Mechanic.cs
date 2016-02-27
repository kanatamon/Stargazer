using UnityEngine;
using System.Collections;

public class Mechanic : MonoBehaviour, IMechanic {

    public virtual void MovePercentage(float value) {
        // ... Do Nothing
    }
    
    public virtual WayPointNode[] GetWayPointNodes() {
        // ... Do Nothing
        return null;
    }
    
}
