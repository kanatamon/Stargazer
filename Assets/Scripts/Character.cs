using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PathFinder), typeof(CharacterMover), typeof(PlayerMotor))]
public class Character : MonoBehaviour {
     
    public WayPoint startWayPoint;    
    public LayerMask blockMask;
    public LayerMask selectableWayPointMask;
    
    public Transform bodyLocator;
    public Transform headLocator;
    public float bodyRotationSpeed = 400f;
    public float headAngleSmoothTime = .8f;
    float currentAngleVelocity;
    
    public float speed = 3f;
    
#if UNITY_EDITOR    
    Vector3 prevMousePosition;
    bool prevMouseTouch;

    Queue<WayPoint> pathGizmos;
#endif    
    
    Queue<WayPoint> path;
    WayPoint prevPathTargetNode;
    bool isBetweenPath;
    public bool isMovingOnPath{ get; private set;}
    bool isChangingNewPath;
    
    WayPoint startNode;
    WayPoint hittedWayPoint;
    
    CharacterMover mover;
    PathFinder pathFinder;
    PlayerMotor motor;
    WayPointFetcher wayPointFetcher;
    
    Animation anim;
    
    public event System.Action OnStartMoving;
    
    void Awake(){
        anim = GetComponent<Animation> ();
        
        mover = GetComponent<CharacterMover> ();
        pathFinder = GetComponent<PathFinder> ();
        motor = GetComponent<PlayerMotor> ();
        wayPointFetcher = GetComponent<WayPointFetcher> ();
    }
    
    void Start(){
        foreach (WayPoint wayPoint in FindObjectsOfType<WayPoint>()) {
            wayPoint.Deactivate (false);
        }
        
        mover.MoveFeetDirectlyTo (startWayPoint);
        wayPointFetcher.ShowActiveWayPoints (false);
    }
    
    void Update(){
        
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown (0)) {
            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            if (!Physics.Raycast (ray, Mathf.Infinity, blockMask)) {
                if (WayPoint.Raycast (ray, out hittedWayPoint, Mathf.Infinity, selectableWayPointMask)) {
                    SetDestination (hittedWayPoint);
                    //                    hittedWayPoint.Interact ();
                }
            }
        }
        
        if (Input.GetMouseButton (1) && prevMouseTouch) {
            Vector3 deltaMove = Input.mousePosition - prevMousePosition;
//            print (Vector3.Magnitude (deltaMove));
            motor.Rotation (deltaMove);
        }
        
        prevMousePosition = Input.mousePosition;
        prevMouseTouch = Input.GetMouseButton (1);
#elif UNITY_IOS
        if (Input.touchCount > 1 && Input.GetTouch(1).phase == TouchPhase.Moved) {
            // Move object across XY plane
            motor.Rotation (Input.GetTouch(1).deltaPosition);
        }
        else if (Input.touchCount == 0){
            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            if (!Physics.Raycast (ray, Mathf.Infinity, blockMask)) {
                if (WayPoint.Raycast (ray, out hittedWayPoint, Mathf.Infinity, selectableWayPointMask)) {
                    SetDestination (hittedWayPoint);
                }
            }
        }
#endif
//        if(Input.GetTouch(0).position)
        MoveHeadLocator ();
    }
    
    void MoveHeadLocator(){
        float currentAngle = headLocator.localEulerAngles.y;
        currentAngle = Mathf.SmoothDampAngle (currentAngle, bodyLocator.localEulerAngles.y, ref currentAngleVelocity, headAngleSmoothTime);

        headLocator.localEulerAngles = new Vector3 (headLocator.localEulerAngles.x, currentAngle, headLocator.localEulerAngles.z);
    }
    
    void SetDestination(WayPoint destinationNode){
//        print ("SetDestination");
        if (mover.GetWayPointNearestFeet (out startNode)) {
//            print ("CheckWayPointNodeBelowFeet Completed");
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
        
        if (OnStartMoving != null) {
            OnStartMoving ();
        }
        
        StartCoroutine (MoveWithPath (path));
    }
    
    IEnumerator MoveWithPath(Queue<WayPoint> path){
        if (path.Count < 2) {
            yield break;
        }
        
//        wayPointFetcher.HideWayPoint ();
//        motor.ResetRotation ();
        wayPointFetcher.isUpdate = false;
        wayPointFetcher.HideWayPoint (true);
        
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
            bodyLocator.transform.rotation = Quaternion.RotateTowards (bodyLocator.transform.rotation, Quaternion.LookRotation (direction), Time.deltaTime * bodyRotationSpeed);
            
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
        
        wayPointFetcher.isUpdate = true;
        wayPointFetcher.ShowActiveWayPoints ();
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
    
#if UNITY_EDITOR
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
#endif
    
}
