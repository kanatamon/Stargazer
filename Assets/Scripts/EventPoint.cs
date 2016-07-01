using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class EventPoint : MonoBehaviour {

    [HideInInspector]
    public SphereCollider collider;

//    public EventGraph eventGraph{ get; private set; }
    
    public virtual void Awake(){
        collider = GetComponent<SphereCollider> ();
//        eventGraph = GetComponentInParent<EventGraph> ();
//        print ("Awake");
    }
    
    public virtual void CancelContactConstellations(){
//        eventGraph.CancelContact ();
    }

    public virtual void ContactConstellations(Vector3 position){
//        eventGraph.MakeContact (this, position);
    }
    
    public virtual Vector3 constellationsForward{
        get{ 
//            return eventGraph.transform.forward;
            return Vector3.zero;
        }
    }
    
    void OnDrawGizmos(){
        SphereCollider triggerCollider = GetComponent<SphereCollider> ();

        Gizmos.color = new Color (0, 0, 1, .2f);
        Gizmos.DrawSphere (transform.position, triggerCollider.radius * transform.localScale.x);
    }
}
