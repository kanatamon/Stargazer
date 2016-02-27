using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class WayPointNode : MonoBehaviour {
    
    public enum UpdateState
    {
        Static,
        Dynamic
    }
    
    public UpdateState updateState = UpdateState.Static;
    public LayerMask wayPointMask;
    
    WayPointEdge[] wayPointEdges;
    WayPointNode[] connectedWayPointNodes;
    
    float skinWidth = .1f;
    
    [HideInInspector]
    public BoxCollider collider;
    
    void Awake(){
        collider = GetComponent<BoxCollider> ();
        MakeWayPointEdges ();
    }
    
    void Start(){
        if (updateState == UpdateState.Static) {
            UpdateEdges ();
        }
    }
    
    void Update(){
        if (updateState == UpdateState.Dynamic) {
            UpdateEdges ();
        }
    }
    
    public static bool Raycast(Ray ray, out WayPointNode hitedNode, float rayLength, LayerMask mask){
        RaycastHit hit;
        if (Physics.Raycast (ray, out hit, rayLength, mask)) {
            if (hit.collider.GetComponent<WayPointNode> () != null) {
                hitedNode = hit.collider.GetComponent<WayPointNode> ();
                return true;
            }
        }
        
        hitedNode = null;
        return false;
    }
    
    public void UpdateEdges(){
        for (int i = 0; i < wayPointEdges.Length; i++) {
            wayPointEdges [i].Update ();
        }
        
        connectedWayPointNodes = GetConnectedWayPointNodes ();
    }
    
    public Vector3 CalculateCorePosition(){
        float halfHeight = collider.size.y * transform.localScale.y / 2;
        return collider.bounds.center + transform.up * halfHeight;
    }
    
    public void MakeWayPointEdges(){
        wayPointEdges = new WayPointEdge[4];

        float halfWidth = collider.size.x * transform.localScale.x / 2;// + skinWidth * 2;
        wayPointEdges [0] = new WayPointEdge (this, WayPointEdge.Direction.Right, skinWidth, halfWidth);
        wayPointEdges [1] = new WayPointEdge (this, WayPointEdge.Direction.Left, skinWidth, halfWidth);

        float halfDepth = collider.size.z * transform.localScale.z / 2;// + skinWidth;// + Mathf.Epsilon;
        wayPointEdges [2] = new WayPointEdge (this, WayPointEdge.Direction.Forward, skinWidth, halfDepth);
        wayPointEdges [3] = new WayPointEdge (this, WayPointEdge.Direction.Back, skinWidth, halfDepth);
        
        connectedWayPointNodes = null;
    }
    
    WayPointNode[] GetConnectedWayPointNodes(){
        List<WayPointNode> connectedNodes = new List<WayPointNode> ();
        
        for (int i = 0; i < wayPointEdges.Length; i++) {
            if (wayPointEdges [i].Connected) {
                connectedNodes.Add (wayPointEdges [i].connectedNode);
            }
        }
        
        return connectedNodes.ToArray ();
    }
    
    public WayPointNode[] ConnectedWayPointNodes{
        get{ 
            if (wayPointEdges == null) {
                MakeWayPointEdges ();
            }
            
            if (connectedWayPointNodes == null) {
                UpdateEdges ();
            }
            
            return connectedWayPointNodes; 
        }
    }
    
    void OnDrawGizmos(){
        if (collider == null) {
            collider = GetComponent<BoxCollider> ();
        }
        
        if (enabled) {
            Gizmos.color = updateState == UpdateState.Static ? Color.blue : Color.yellow;
        }
        else {
            Gizmos.color = Color.gray;
        }
        
        Gizmos.DrawSphere (CalculateCorePosition (), .2f);
        
        if (!Application.isPlaying && (wayPointEdges == null || wayPointEdges.Length == 0)) {
            MakeWayPointEdges ();
//            print ("Create");
        }
        
        for (int i = 0; i < wayPointEdges.Length; i++) {
            if (!Application.isPlaying) {
//                print ("Update");
                if (enabled) {
                    wayPointEdges [i].Update ();
                }
            }
            
            if (enabled) {
                Gizmos.color = wayPointEdges [i].Connected ? Color.magenta : Color.green;
            }
            else {
                Gizmos.color = Color.gray;
            }
            
            Gizmos.DrawSphere (wayPointEdges [i].CalculateOriginPoint (), wayPointEdges [i].radius);
        }
        
//        print (wayPointConnectors [0].CheckConnect () + " "+ wayPointConnectors [2].CheckConnect ());
    }
    
    public struct WayPointEdge{
        
        public enum Direction{
            Forward,
            Back,
            Right,
            Left
        }
        
        public Direction direction;
        
        public WayPointNode connectedNode;
        public WayPointNode wayPointNode;
        public float radius;
        public float length;
        
        private bool connected;
        private Vector3 origin;
        
        Transform attachedT;
        Dictionary<Collider, WayPointNode> dict;
        
        public WayPointEdge(WayPointNode _wayPointNode, Direction _direction, float _radius, float _length){
            wayPointNode = _wayPointNode;
            radius = _radius;
            length = _length;
            direction = _direction;
            
            attachedT = wayPointNode.transform;
            
            origin = Vector3.zero;
            connected = false;
            connectedNode = null;
            
            dict = new Dictionary<Collider, WayPointNode> ();
            
            origin = CalculateOriginPoint();
        }
        
        public void Update(){
            Reset ();
            connected = CheckConnect (out connectedNode);
        }
        
        private void Reset(){
            connected = false;
            connectedNode = null;
        }
        
        private bool CheckConnect(out WayPointNode node){
            origin = CalculateOriginPoint ();
//            Collider[] overlapedColls = Physics.OverlapBox (centre, halfExtents, attachedBox.transform.rotation, attachedBox.boxMask);
            Collider[] overlapedColls = Physics.OverlapSphere (origin, radius, wayPointNode.wayPointMask);
            
            if (overlapedColls.Length > 2) {
                Debug.LogError ("Do not place 'WayPointBox' more than 2 ea");
                node = null;
                return false;
            }
                
            for (int i = 0; i < overlapedColls.Length; i++) {
                if (overlapedColls [i] == wayPointNode.collider) {
                    continue;
                }
                
                if (!dict.ContainsKey (overlapedColls [i])) {
                    dict [overlapedColls [i]] = overlapedColls [i].GetComponent<WayPointNode> ();
                } 
                
                if (dict [overlapedColls [i]].enabled) {
                    node = dict [overlapedColls [i]];
                    return true;
                }
            }
            
            node = null;
            return false;
        }
        
        public Vector3 CalculateOriginPoint(){
            return wayPointNode.CalculateCorePosition () + GetDirectionVector () * length;
        }
        
        private Vector3 GetDirectionVector(){
            switch (direction) {
            case Direction.Forward:
                return attachedT.forward;
            case Direction.Back:
                return attachedT.forward * -1;
            case Direction.Right:
                return attachedT.right;
            case Direction.Left:
                return attachedT.right * -1;
            }
            
            return Vector3.zero;
        }
        
        public bool Connected{
            get{ return connected; }
        }
        
        public Vector3 Origin{
            get{ return origin; }
        }
        
    }
}
