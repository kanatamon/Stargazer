using UnityEngine;
using System.Collections;

public interface IContactable {
    
    void MakeContact (EventPoint fromNode, Vector3 contactPosition);
    void CancelContact ();
    
}
