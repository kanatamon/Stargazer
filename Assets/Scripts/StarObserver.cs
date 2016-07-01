using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarObserver : MonoBehaviour {

    public LayerMask starMask;
    
    EventPoint rayedNode;
    EventPoint prevRayedNode;
    
    Dictionary<Collider, EventPoint> nodeDict = new Dictionary<Collider, EventPoint>();
    
    void Update(){
       #if UNITY_EDITOR
        if (Input.GetMouseButton (0)) {
            if (FindAStarByRaycast (out rayedNode)) {
                ContactConstellationsFrom (rayedNode);

                prevRayedNode = rayedNode;
            }
            else if (prevRayedNode != null) {
                prevRayedNode.CancelContactConstellations ();
                prevRayedNode = null;
            }
        }
        else {
            if (prevRayedNode != null) {
                prevRayedNode.CancelContactConstellations ();
            }

            prevRayedNode = null;        
        }
        #elif UNITY_IOS
        if (Input.touchCount == 1) {
            if (Input.GetMouseButton (0)) {
                if (FindAStarByRaycast (out rayedNode)) {
                ContactConstellationsFrom (rayedNode);
    
                prevRayedNode = rayedNode;
                }
                else if (prevRayedNode != null) {
                    prevRayedNode.CancelContactConstellations ();
                    prevRayedNode = null;
                }
            }
            else {
                if (prevRayedNode != null) {
                    prevRayedNode.CancelContactConstellations ();
                }
    
                prevRayedNode = null;        
            }
        }
        #endif

    }
    
    void ContactConstellationsFrom(EventPoint fromNode){
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

        Plane plane = new Plane (fromNode.constellationsForward, fromNode.transform.position);
        
        float rayDistance;
        plane.Raycast (ray, out rayDistance);
        fromNode.ContactConstellations (ray.GetPoint (rayDistance));
    }
    
    
    bool FindAStarByRaycast(out EventPoint foundNode){
        RaycastHit hit;

        if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, starMask)) {
//            print ("Hit " + hit.collider.name);
            if (hit.collider.GetComponent<EventPoint> () != null) {
                if (!nodeDict.ContainsKey (hit.collider)) {
                    var newStar = hit.collider.GetComponent<EventPoint> ();
                    nodeDict.Add (hit.collider, newStar);
                }
                foundNode = nodeDict [hit.collider];
                return true;
            }
        }

        foundNode = null;
        return false;
    }
 
}
