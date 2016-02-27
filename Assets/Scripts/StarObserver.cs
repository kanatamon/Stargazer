using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarObserver : MonoBehaviour {

    public LayerMask starMask;
    
    Star rayedStar;
    Star prevRayedStar;
    
    Dictionary<Collider, Star> starDict = new Dictionary<Collider, Star>();
    
    void Update(){
        if (Input.GetMouseButton (0)) {
            if (FindAStarByRaycast (out rayedStar)) {
                ContactConstellationsFrom (rayedStar);
                
                prevRayedStar = rayedStar;
            }
            else if (prevRayedStar != null) {
                prevRayedStar.CancelContactConstellations ();
                prevRayedStar = null;
            }
        }
        else {
            if (prevRayedStar != null) {
                prevRayedStar.CancelContactConstellations ();
            }

            prevRayedStar = null;        
        }

    }
    
//    bool CheckIsBeingOnSameConstellations(Star fromStar, Star toStar){
//        return fromStar.constellations == toStar.constellations;
//    }
    
    void ContactConstellationsFrom(Star fromStar){
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
//        Plane plane = new Plane (Vector3.forward, fromStar.transform.position);
//        print (fromStar.transform.position.ToString ());
//        print ("fromStar == null" + (fromStar.Mother == null));
        Plane plane = new Plane (fromStar.constellations.transform.forward, fromStar.transform.position);
        
        float rayDistance;
        plane.Raycast (ray, out rayDistance);
//        Vector3 onPlanePosition = ray.origin + ray.direction * rayDistance;  
        fromStar.ContactConstellations (ray.GetPoint (rayDistance));
    }
    
    
    bool FindAStarByRaycast(out Star foundStar){
        RaycastHit hit;

        if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, starMask)) {
            if (hit.collider.GetComponent<Star> () != null) {
                if (!starDict.ContainsKey (hit.collider)) {
                    Star newStar = hit.collider.GetComponent<Star> ();
                    starDict.Add (hit.collider, newStar);
                }
                foundStar = starDict [hit.collider];
                return true;
            }
        }

//        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
//        float rayLength = hit.collider != null ? hit.distance : Mathf.Infinity;
//        Debug.DrawRay (ray.origin, ray.direction * rayLength, hit.collider != null ? Color.red : Color.green, 1);
        foundStar = null;
        return false;
    }
 
}
