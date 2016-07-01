using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterMover))]
public class WayPointFetcher : MonoBehaviour {

    public int depthSearch = 10;
    
//    Vector3 prevPosition;
    List<WayPoint> activeWayPoint = new List<WayPoint> ();
    
    CharacterMover mover;
    
    [HideInInspector]
    public bool isUpdate;
    
    void Start(){
        mover = GetComponent<CharacterMover> ();
//        prevPosition = transform.position;
        
//        ShowActiveWayPoints (false);
        isUpdate = true;
        
//        activeWayPoint = FetchActiveWayPoint (depthSearch);
//        foreach (WayPoint wayPoint in activeWayPoint) {
//            wayPoint.Activate (false);
//        }
//        ShowActiveWayPoints (false);
    }
    
    void LateUpdate(){
        if (isUpdate) {
            List<WayPoint> currentActiveWayPoint = FetchActiveWayPoint (depthSearch);
            
            // Show New Active Waypoint
            foreach (WayPoint wayPoint in currentActiveWayPoint) {
                if (!activeWayPoint.Contains (wayPoint)) {
                    wayPoint.Activate (true);
                }
            }
            
            // Hide None-Active Waypoint
            foreach (WayPoint wayPoint in activeWayPoint) {
                if (!currentActiveWayPoint.Contains (wayPoint)) {
                    wayPoint.Deactivate (true);
                }
            }
            
            activeWayPoint = currentActiveWayPoint;
        }
    }
    
    public void HideWayPoint(bool startAnimate = true){
        foreach (WayPoint wayPoint in activeWayPoint) {
            wayPoint.Deactivate (startAnimate);
        } 
    }
    
    public void ShowActiveWayPoints(bool startAnimate = true){
        activeWayPoint = FetchActiveWayPoint (depthSearch);
        
        foreach (WayPoint wayPoint in activeWayPoint) {
            wayPoint.Activate (startAnimate);    
        }
    }
    
    List<WayPoint> FetchActiveWayPoint(int depth){
//        activeWayPoint.Clear ();
        List<WayPoint> currentActiveWayPoint = new List<WayPoint> ();
        
        WayPoint root;
        if (depth >= 0 && mover.GetWayPointNearestFeet (out root)) {
            currentActiveWayPoint.Add (root);
            RecursionSearch (root, depth, currentActiveWayPoint);
            
            currentActiveWayPoint.Remove (root);
        }
        
        return currentActiveWayPoint;
    }
    
    void RecursionSearch(WayPoint root, int depthRemaining, List<WayPoint> collector){
        if (depthRemaining <= 0) {
            return;
        }
        
        for (int i = 0; i < root.neighborInfo.Length; i++) {
            if (!collector.Contains (root.neighborInfo [i].wayPoint)) {
                collector.Add (root.neighborInfo [i].wayPoint);
                RecursionSearch (root.neighborInfo [i].wayPoint, depthRemaining - 1, collector); 
            }
        }
    }
}
