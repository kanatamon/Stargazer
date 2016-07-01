using UnityEngine;
using System.Collections;

public class EventGraph : MonoBehaviour, IContactable {

    public virtual void MakeContact (EventPoint fromNode, Vector3 contactPosition){
        
    }
    public virtual void CancelContact (){
        
    }
    
}
