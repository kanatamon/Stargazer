using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class WayPoint : MonoBehaviour {
    
    public enum UpdateType
    {
        Once,
        Always
    }
    
    public UpdateType updateType = UpdateType.Once;
    
    public LayerMask groundMask;
    public float groundRayLength;
    private Vector3 prevPosition;
    public Vector3 GroundPoint{ get; private set; }
    
    public LayerMask wayPointMask;
    public WayPointConnectionRayChecker[] connectionCheckers;
    
    SphereCollider sphereCollider;
    
    public static Vector3 infinitePoint = new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    
//    public WayPoint[] neighborWayPoints{ get; private set; }
    public NeighborInfo[] neighborInfo{ get; private set; }
    
    void Awake(){
        sphereCollider = GetComponent<SphereCollider> ();
        sphereCollider.isTrigger = true;
        
        GroundPoint = FindGroundPoint ();
    }
    
    void Start(){
        UpdateNeighborWayPoint ();
    }
    
    void LateUpdate(){
        if (updateType == UpdateType.Always) {
            UpdateNeighborWayPoint ();
        }
        
        if (transform.position != prevPosition) {
            GroundPoint = FindGroundPoint ();
        }
    }
    
    void UpdateNeighborWayPoint(){
        Dictionary<WayPoint, NeighborInfo> neighborDict = new Dictionary<WayPoint, NeighborInfo> ();
//        List<NeighborInfo> neighborList = new List<NeighborInfo> ();
        
        for (int i = 0; i < connectionCheckers.Length; i++) {
            connectionCheckers [i].Update ();
            
            if (connectionCheckers [i].connected) {
                if (!neighborDict.ContainsKey (connectionCheckers [i].connectedWayPoint)) {
                    neighborDict.Add (connectionCheckers [i].connectedWayPoint, new NeighborInfo (connectionCheckers [i].connectedWayPoint, (int)connectionCheckers [i].realDistance));
                }
            }
        }
        
        neighborInfo = new NeighborInfo[neighborDict.Count];
        neighborDict.Values.CopyTo (neighborInfo, 0);
    }
    
    public static bool Raycast (Ray ray, out WayPoint hittedWayPoint, float rayLength, LayerMask mask){
        RaycastHit hit;
        if (Physics.Raycast (ray, out hit, rayLength, mask, QueryTriggerInteraction.Collide)) {
            if (hit.collider.GetComponent<WayPoint> () != null) {
                hittedWayPoint = hit.collider.GetComponent<WayPoint> ();
                
                return true;
            }
        }
        
        hittedWayPoint = null;
        return false;
    }
    
    public Vector3 FindGroundPoint(){
        RaycastHit hit;
        if (Physics.Raycast (transform.position, transform.up * -1, out hit, groundRayLength, groundMask, QueryTriggerInteraction.Ignore)) {
            return hit.point;
        }

        return infinitePoint;
    }
    
    void OnDrawGizmos(){
        if (sphereCollider == null) {
            sphereCollider = GetComponent<SphereCollider> ();
        }
        
        // ... Draw Way Point
        Gizmos.color = (updateType == UpdateType.Always) ? new Color (1, 0, 1, .25f) : new Color (0, 0, 1, .25f); 
        Gizmos.DrawSphere (transform.position, sphereCollider.radius);
        
        // ... Draw Ground Ray
        Vector3 groundPoint = !Application.isPlaying ? FindGroundPoint () : GroundPoint;
        if (groundPoint != infinitePoint) {
            Gizmos.DrawLine (transform.position, groundPoint);
        }
        else {
            Gizmos.color = Color.green;
            Gizmos.DrawRay (transform.position, transform.up * -groundRayLength);
        }
        
        // ... Draw Connection-Checkers
        for (int i = 0; i < connectionCheckers.Length; i++) {
            Vector3 origin = connectionCheckers [i].motherWayPoint.transform.position;
//            Vector3 direction = transform.TransformDirection (Quaternion.Euler (connectionCheckers [i].localEuler) * Vector3.forward);
            Vector3 direction = connectionCheckers [i].direction;
            
            // ... Draw Connection-Checker's Line
            Gizmos.color = (connectionCheckers [i].connected) ? Color.green : Color.red;
            float rayDst = (connectionCheckers [i].connected) ? connectionCheckers [i].realDistance : connectionCheckers [i].distanceRay;
            Gizmos.DrawRay (transform.position, direction * connectionCheckers [i].distanceRay);
            
            // ... Draw Connection-Checker's Direction
            if (!Application.isPlaying) {
                connectionCheckers [i].Update ();
            }
            Gizmos.color = (connectionCheckers [i].connected) ? Color.yellow : Color.red;
            Gizmos.DrawRay (transform.position, direction * sphereCollider.radius);
            
            string iconName = (connectionCheckers [i].connected) ? "correct" : "incorrect";
            Gizmos.DrawIcon (transform.position + direction * sphereCollider.radius, iconName);
        }
    }
    
    [System.Serializable]
    public class NeighborInfo{
        public WayPoint wayPoint{ get; private set; }
        public int distance{ get; private set;}
        
        public NeighborInfo(WayPoint _wayPoint, int _distance){
            wayPoint = _wayPoint;
            distance = _distance;
        }
    }
    
    [System.Serializable]
    public class WayPointConnectionRayChecker{
        public float distanceRay = 1;
        public Vector3 localEuler;
        
        [HideInInspector]
        public WayPoint motherWayPoint;
        [HideInInspector]
        public WayPoint connectedWayPoint;
        [HideInInspector]
        public float realDistance;
        
        public void Update(){
            connectedWayPoint = null;
            
            Vector3 origin = motherWayPoint.transform.position;
//            Vector3 direction = motherWayPoint.transform.TransformDirection (Quaternion.Euler (localEuler) * Vector3.forward);
            
            RaycastHit hit;
            if (Physics.Raycast (origin, direction, out hit, distanceRay, motherWayPoint.wayPointMask, QueryTriggerInteraction.Collide)) {
                if (hit.collider.GetComponent<WayPoint> () != null) {
                    connectedWayPoint = hit.collider.GetComponent<WayPoint> ();
                    realDistance = hit.distance;
                }
            }
        }
        
        public Vector3 direction{
            get{ 
                return motherWayPoint.transform.TransformDirection (Quaternion.Euler (localEuler) * Vector3.forward);
            }
        }
        
        public bool connected{
            get{ 
                return connectedWayPoint != null;
            }
        }
        
    }
}
