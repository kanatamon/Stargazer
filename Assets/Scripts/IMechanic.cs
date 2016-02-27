using UnityEngine;
using System.Collections;

public interface IMechanic {
    
    void MovePercentage (float value);
    
    WayPointNode[] GetWayPointNodes ();
}
