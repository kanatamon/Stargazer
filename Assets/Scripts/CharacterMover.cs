using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class CharacterMover : MonoBehaviour {
    
    public LayerMask wayPointMask;
    public LayerMask groundMask;

    float gravity = 9.8f;
    Vector3 velocity;
    
    Dictionary<Collider, WayPoint> wayPointDict = new Dictionary<Collider, WayPoint> ();
    
    CharacterController controller;
    
    void Awake(){
        controller = GetComponent<CharacterController> ();
    }
   
    void LateUpdate(){
        velocity.y -= gravity * Time.deltaTime;
        controller.Move (velocity);
        ResetVelocity ();
    }
    
    void ResetVelocity(){
        velocity.x = 0;
        velocity.z = 0;
        
        if (controller.isGrounded) {
            velocity.y = 0;
        }
    }
    
    public void Move(Vector3 motion){
        controller.Move (motion);
    }
    
    public void MoveFeetDirectlyTo(WayPoint wayPoint){
        MoveFeetDirectlyTo (wayPoint.GroundPoint);
    }
    
    public void MoveFeetDirectlyTo(Vector3 targetPosition){
//        Vector3 currectedHeightPoint = transform.up * transform.localScale.y * controller.height / 2;
        transform.position = targetPosition;// + currectedHeightPoint;
    }
    
    public Vector3 CalculateFeetPosition(){
        if (controller == null) {
            controller = GetComponent<CharacterController> ();
        }
        
        RaycastHit hit;
        if (Physics.Raycast (transform.position + controller.center, Vector3.down, out hit, 10f, groundMask)) {
            return hit.point;
        }
        
        return Vector3.zero;
//        return transform.position - transform.up * (transform.localScale.y * controller.height / 2 + controller.skinWidth);
    }
    
    public bool CheckWayPointNearestFeet(out WayPoint wayPoint){
        Vector3 feetPosition = CalculateFeetPosition ();
        Collider[] wayPointsNearMe = Physics.OverlapSphere(feetPosition, .5f, wayPointMask, QueryTriggerInteraction.Collide);
//        print ("CheckWayPointNodeBelowFeet wayPointsNearMe.");
        if (wayPointsNearMe.Length > 0) {
            float minDst = Mathf.Infinity;
            int minDstIndex = 0;
            
            for (int i = 0; i < wayPointsNearMe.Length; i++) {
                float dst = Vector3.Distance (feetPosition, wayPointsNearMe [i].transform.position);
                if (dst < minDst) {
                    minDst = dst;
                    minDstIndex = i;
                }
            }
            
            if (!wayPointDict.ContainsKey (wayPointsNearMe [minDstIndex])) {
                wayPointDict.Add (wayPointsNearMe [minDstIndex], wayPointsNearMe [minDstIndex].GetComponent<WayPoint> ());
            }
            
            wayPoint = wayPointDict [wayPointsNearMe [minDstIndex]];
            print (wayPoint.name);
            return true;
        }
        
        wayPoint = null;
        return false;
    }
    
    public bool CheckIsFeetBeingNear(WayPoint targetWayPoint, float threshold, out float moreDst){
        if (targetWayPoint != null) {
            float distance = Vector3.Distance (CalculateFeetPosition (), targetWayPoint.GroundPoint);
            moreDst = distance;
            
            return distance < threshold;
        }
        
        Debug.LogError ("CheckIsFeetBeingNear(null)");
        moreDst = Mathf.Infinity;
        
        return false;
    }
    
    void OnDrawGizmos(){
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere (CalculateFeetPosition (), .5f);
    }
}
