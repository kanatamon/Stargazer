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
    
    [HideInInspector] [SerializeField] bool isActivatable;
    
    [ExposeProperty]
    public bool IsActivatable{
        get{ 
            return isActivatable;
        }   
        
        set{ 
            if (value == true) {
                gameObject.layer = LayerMask.NameToLayer ("Selectable Waypoint");
            }
            else {
                gameObject.layer = LayerMask.NameToLayer ("Unselectable Waypoint");
            }
            
            isActivatable = value;
        }
    }
    
    public UpdateType updateType = UpdateType.Once;
    
    public LayerMask groundMask;
    public float groundRayLength;
    private Vector3 prevPosition;
    public Vector3 GroundPoint{ get; private set; }
    
    public LayerMask neighborWayPointMask;
//    public WayPointConnectionRayChecker[] connectionCheckers;

    public List<WayPointConnectionRayChecker> connectionCheckers;
    
    SphereCollider sphereCollider;
    MeshRenderer graphics;
    
    public NeighborInfo[] neighborInfo{ get; private set; }
    
    public static Vector3 infinitePoint = new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    
    void Awake(){
        graphics = GetComponentInChildren<MeshRenderer> ();
        
        sphereCollider = GetComponent<SphereCollider> ();
        sphereCollider.isTrigger = true;
        
        GroundPoint = FindGroundPoint ();
        UpdateNeighborWayPoint ();
        
        graphics.gameObject.SetActive (isActivatable);
    }
    
    void LateUpdate(){
        if (updateType == UpdateType.Always) {
            UpdateNeighborWayPoint ();
        }
        
        if (transform.position != prevPosition) {
            GroundPoint = FindGroundPoint ();
        }
    }
    
    public void Activate(bool startAnimate = true){
        if (isActivatable) {
            gameObject.layer = LayerMask.NameToLayer ("Selectable Waypoint");
            
            if (startAnimate) {
                StartCoroutine (Animate (Color.clear, Color.white));
            }
            else {
                graphics.material.color = Color.white;
            }
        }
    }
    
    public void Deactivate(bool startAnimate = true){
        if (isActivatable) {
            gameObject.layer = LayerMask.NameToLayer ("Unselectable Waypoint");
            
            if (startAnimate) {
                StartCoroutine (Animate (Color.white, Color.clear));
            }
            else {
                graphics.material.color = Color.clear;
            }
        }
    }
    
    public void UpdateNeighborWayPoint(){
        Dictionary<WayPoint, NeighborInfo> neighborDict = new Dictionary<WayPoint, NeighborInfo> ();
//        List<NeighborInfo> neighborList = new List<NeighborInfo> ();
        
        for (int i = 0; i < connectionCheckers.Count; i++) {
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
    
    IEnumerator Animate(Color fromColor, Color toColor){
        float percent = 0;
        
        while (percent <= 1) {
            percent += Time.deltaTime * 2f;
            graphics.material.color = Color.Lerp (fromColor, toColor, percent);
            
            yield return null;
        }
    }
    
    public static bool Raycast (Ray ray, out WayPoint hittedWayPoint, float rayLength, LayerMask mask){
        RaycastHit hit;
        if (Physics.Raycast (ray, out hit, rayLength, mask, QueryTriggerInteraction.Collide)) {
            WayPoint wayPoint = hit.collider.GetComponent<WayPoint> ();
            if (wayPoint != null && wayPoint.IsInteractable) {
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
    
    public bool IsInteractable{
        get{ 
            return gameObject.layer == LayerMask.NameToLayer ("Selectable Waypoint");
        }
    }
    
    void OnDrawGizmos(){
        if (sphereCollider == null) {
            sphereCollider = GetComponent<SphereCollider> ();
        }
        
        // ... Draw Connection-Checkers
        for (int i = 0; i < connectionCheckers.Count; i++) {
            Vector3 origin = connectionCheckers [i].motherWayPoint.transform.position;
            Vector3 direction = connectionCheckers [i].forward;
            
            // ... Draw Connection-Checker's Line
            Gizmos.color = (connectionCheckers [i].connected) ? Color.green : Color.red;
            float rayDst = (connectionCheckers [i].connected) ? connectionCheckers [i].realDistance : connectionCheckers [i].distanceRay;
            Gizmos.DrawRay (transform.position, direction * rayDst);
            
            // ... Draw Connection-Checker's Direction
            if (!Application.isPlaying) {
                connectionCheckers [i].Update ();
            }
            Gizmos.color = (connectionCheckers [i].connected) ? Color.yellow : Color.red;
            Gizmos.DrawRay (transform.position, direction * sphereCollider.radius);
            
            string iconName = (connectionCheckers [i].connected) ? "correct" : "incorrect";
            Gizmos.DrawIcon (transform.position + direction * sphereCollider.radius, iconName);
        }
        
        // ... Draw Way Point
        Gizmos.color = (updateType == UpdateType.Always) ? new Color (1, 0, 1, .5f) : new Color (0, 0, 1, .5f); 
        Gizmos.DrawSphere (transform.position, sphereCollider.radius);

        // ... Draw Ground Ray
        Vector3 groundPoint = !Application.isPlaying ? FindGroundPoint () : GroundPoint;
        if (groundPoint != infinitePoint) {
            Gizmos.DrawLine (transform.position, groundPoint);
            Gizmos.DrawIcon (transform.position, "location", false);
        }
        else {
            Gizmos.color = Color.green;
            Gizmos.DrawRay (transform.position, transform.up * -groundRayLength);
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
        public Vector3 localFoward;
        
        [HideInInspector]
        public WayPoint motherWayPoint;
        [HideInInspector]
        public WayPoint connectedWayPoint;
        
        public float realDistance{ get; private set; }
        
        public void Update(){
            connectedWayPoint = null;
            
            Vector3 origin = motherWayPoint.transform.position;
            RaycastHit hit;
            
            if (Physics.Raycast (origin, forward, out hit, distanceRay, motherWayPoint.neighborWayPointMask, QueryTriggerInteraction.Collide)) {
                if (hit.collider.GetComponent<WayPoint> () != null) {
                    connectedWayPoint = hit.collider.GetComponent<WayPoint> ();
                    realDistance = hit.distance;
                }
            }
        }
        
        public Vector3 forward{
            get{ 
                return motherWayPoint.transform.TransformDirection (localFoward);
            }
        }
        
        public bool connected{
            get{ 
                return connectedWayPoint != null;
            }
        }
        
    }
}
