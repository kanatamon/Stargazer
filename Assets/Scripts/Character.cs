using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PathFinder), typeof(CharacterMover))]
public class Character : MonoBehaviour {
     
    public WayPoint startWayPoint;    
    public LayerMask blockMask;
    public LayerMask wayPointMask;
    
    public float speed = 3f;

#if UNITY_EDITOR    
    Queue<WayPoint> pathGizmos;
#endif    
    
    Queue<WayPoint> path;
    WayPoint prevPathTargetNode;
    bool isBetweenPath;
    bool isMovingOnPath;
    bool isChangingNewPath;
    
    WayPoint startNode;
    WayPoint hitNode;
    
    CharacterMover mover;
    PathFinder pathFinder;
    
    Animation anim;
    
    void Awake(){
        anim = GetComponent<Animation> ();
        
        mover = GetComponent<CharacterMover> ();
        pathFinder = GetComponent<PathFinder> ();
    }
    
    void Start(){
        mover.MoveFeetDirectlyTo (startWayPoint);
    }
    
    void Update(){
        if (Input.GetMouseButtonDown (0)) {
            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            if (!Physics.Raycast (ray, Mathf.Infinity, blockMask)) {
                if (WayPoint.Raycast (ray, out hitNode, Mathf.Infinity, wayPointMask)) {
                    SetDestination (hitNode);
                }
            }
        }
    }
    
    void SetDestination(WayPoint destinationNode){
        print ("SetDestination");
        if (mover.CheckWayPointNearestFeet (out startNode)) {
            print ("CheckWayPointNodeBelowFeet Completed");
            if (pathFinder.ShortestPath (startNode, destinationNode, out path)) {
                
#if UNITY_EDITOR
                pathGizmos = path;
#endif
                
                StopCoroutine ("SwitchPath");
                StartCoroutine (SwitchPath (path));
            }
        }
    }
    
    IEnumerator SwitchPath(Queue<WayPoint> path){
        isChangingNewPath = true;
        
        while (isMovingOnPath) {
            yield return null;
        }
        
        StartCoroutine (MoveWithPath (path));
    }
    
    IEnumerator MoveWithPath(Queue<WayPoint> path){
        if (path.Count < 2) {
            yield break;
        }
        
        isChangingNewPath = false; 
        isMovingOnPath = true;
        
        isBetweenPath = true;

        WayPoint targetWayPoint = path.Dequeue ();
        float moveStep;
        float moreDst;
        
        if (targetWayPoint != prevPathTargetNode) {
            targetWayPoint = path.Dequeue ();
        }
//        Quaternion.Lerp
        anim ["Walk"].speed = speed / 2f;
        anim.Play ("Walk");
        
        while (isBetweenPath) {
            var targetPosition = targetWayPoint.GroundPoint;
            
            Vector3 direction = targetPosition - mover.CalculateFeetPosition ();
            direction.y = 0;
            direction.Normalize ();
            
//            mover.Move (CalculateMotion (mover.CalculateFeetPosition (), targetPosition, out moveStep));
            mover.Move (CalculateMotion (direction, out moveStep));
            transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (direction), Time.deltaTime * 800f);
            
            if (mover.CheckIsFeetBeingNear (targetWayPoint, moveStep * 2, out moreDst)) {
                if (moreDst * 2 > moveStep) {
                    yield return null;
                    mover.Move (CalculateMotion (mover.CalculateFeetPosition (), targetPosition, moreDst));
                }
                
                isBetweenPath = !isChangingNewPath && path.Count > 0;
                
                if (isBetweenPath) {
//                    print ("Start CheckAndFixPath, path count = " + path.Count);
                    Queue<WayPoint> rePath;
                    if (pathFinder.CheckAndFixPath (path, out rePath)) {
                        path = rePath;
                    }
//                    print ("Finish CheckAndFixPath, path count = " + path.Count);
                    
#if UNITY_EDITOR
                    pathGizmos = path;
#endif
                    
                    targetWayPoint = path.Dequeue ();
                    prevPathTargetNode = targetWayPoint;
                    
                }
            }
            
            yield return null;
        }
        
        anim.Play ("Wait");
        isMovingOnPath = false;
    }
    
    Vector3 CalculateMotion(Vector3 direction, out float moveStep){
        moveStep = speed * Time.deltaTime;
        return direction * moveStep;
    }
    
    Vector3 CalculateMotion(Vector3 start, Vector3 destination, float moveStep){
        Vector3 direction = destination - start;
        direction.y = 0;
        direction.Normalize ();

        return direction * moveStep;
    }
    
    Vector3 CalculateMotion(Vector3 start, Vector3 destination, out float moveStep){
        moveStep = speed * Time.deltaTime;
        return CalculateMotion (start, destination, moveStep);
    }
    
    float CalculateDuration(float distance, float velocity){
        return distance / velocity;
    }
    
    void OnDrawGizmos(){
        // ... Draw Path
        if (pathGizmos != null && pathGizmos.Count != 0) {
            foreach(WayPoint node in pathGizmos){
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere (node.GroundPoint, .25f);
            }
        }
        
        // ... Draw The Start Point
        if (startWayPoint != null) {
            Gizmos.DrawIcon (startWayPoint.transform.position, "here", false);
        }
    }
    
}
