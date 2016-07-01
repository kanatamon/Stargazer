using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class AltarController : MonoBehaviour {

    public LayerMask interactableObjectMask;
    
    public ConstellationsSwitch activator;
    
//    public Vector3 startPosition;
//    public Vector3 endPosition;
    public Vector3[] wayPointPositions;
    public float flyingDuration = 5f;
    bool isRunningWayPoint;
//    public bool startAtFirst = true;
//    Vector3 targetPosition;
//    Vector3 currentVelocity;
    
    public Transform[] animationTransforms;
    public float animationUpY;
    public float animationDownY;
    
    public float animationMoveSmoothTime = 1f;
    public float rotationSmoothTime = 5f;
    public float maxAnimationRotationSpeed = 100f;
    float targetRotationSpeed;
    float currentRotationSpeed;
    float rotationVelocity;

    bool somethingStanding;
    bool prevSomethingStanding;
    
    bool isActivated;
    
    SmoothMovementInfo[] smoothInfo;
    
    BoxCollider chckerCollider;
    
    public event System.Action OnPlayerArrive;
    public event System.Action OnPlayerLeave;
    
    void Awake(){
        chckerCollider = GetComponent<BoxCollider> ();
        chckerCollider.isTrigger = true;
        
        smoothInfo = new SmoothMovementInfo[animationTransforms.Length];
        for (int i = 0; i < smoothInfo.Length; i++) {
            smoothInfo [i] = new SmoothMovementInfo (animationTransforms [i], animationDownY);
            smoothInfo [i].localPositionY = animationDownY;
        }
        
        activator.OnActivate += OnActivatorActivate;
        
        if (wayPointPositions.Length > 0) {
            transform.localPosition = wayPointPositions [0];
        }
//        targetPosition = transform.localPosition;
    }
    
    void Update(){
        somethingStanding = CheckPlayerStandingOn ();
        
        if (somethingStanding && !prevSomethingStanding) {
            // ... something has came
            for (int i = 0; i < smoothInfo.Length; i++) {
                smoothInfo [i].targetY = animationUpY;
            }
            
            if (isActivated) {
                targetRotationSpeed = maxAnimationRotationSpeed;
                StartCoroutine (MoveThroughWayPoints ());
            }
            
            if (OnPlayerArrive != null) {
                OnPlayerArrive ();
            }
        }
        else if(!somethingStanding && prevSomethingStanding) {
            // ... something has gone
            for (int i = 0; i < smoothInfo.Length; i++) {
                smoothInfo [i].targetY = animationDownY;
            }
            
            if (isActivated) {
                targetRotationSpeed = 0f;
            }
            
            if (OnPlayerLeave != null) {
                OnPlayerLeave ();
            }
        }
        
        currentRotationSpeed = Mathf.SmoothDamp (currentRotationSpeed, targetRotationSpeed, ref rotationVelocity, rotationSmoothTime);
        
        for (int i = 0; i < smoothInfo.Length; i++) {
            if (isActivated) {
                smoothInfo [i].transform.Rotate (Vector3.up, Time.deltaTime * currentRotationSpeed);
            }
            
            smoothInfo [i].localPositionY = Mathf.SmoothDamp (smoothInfo [i].localPositionY, smoothInfo [i].targetY, ref smoothInfo [i].currentVelocity, animationMoveSmoothTime);
        }
        
//        transform.localPosition = Vector3.SmoothDamp (transform.localPosition, targetPosition, ref currentVelocity, flyingDuration);
        
        prevSomethingStanding = somethingStanding;
    }
    
    bool CheckPlayerStandingOn(){
        return Physics.CheckBox (chckerCollider.bounds.center, chckerCollider.bounds.size / 2f, transform.rotation, interactableObjectMask);
    }
    
    void OnActivatorActivate(){
        isActivated = true;
        targetRotationSpeed = maxAnimationRotationSpeed;
        
        StartCoroutine (MoveThroughWayPoints ());
    }
    
    IEnumerator MoveThroughWayPoints(){
        yield return new WaitForSeconds (rotationSmoothTime * .8f);
        
        if (wayPointPositions.Length > 0) {
            if (transform.localPosition == wayPointPositions [0]) {
                for (int i = 1; i < wayPointPositions.Length; i++) {
                    Vector3 fromPos = wayPointPositions [i - 1];
                    Vector3 toPos = wayPointPositions [i];
                    
                    StartCoroutine (Move (fromPos, toPos));
                    
                    while (isRunningWayPoint) {
                        yield return null;
                    }
                }
            }
            else if(transform.localPosition == wayPointPositions [wayPointPositions.Length - 1]) {
                for (int i = wayPointPositions.Length - 1; i > 0; i--) {
                    Vector3 fromPos = wayPointPositions [i];
                    Vector3 toPos = wayPointPositions [i - 1];
                    
                    StartCoroutine (Move (fromPos, toPos));
                    
                    while (isRunningWayPoint) {
                        yield return null;
                    }
                }
            }
        }
    }
    
    IEnumerator Move(Vector3 fromPosition, Vector3 toPosition){
        isRunningWayPoint = true;
        
        float percent = 0;
        while (percent < 1) {
            percent += Time.deltaTime / flyingDuration;
            percent = Mathf.Clamp01 (percent);
            float interpolate = Mathf.Pow (percent, 3) / (Mathf.Pow (percent, 3) + Mathf.Pow (1 - percent, 3));
            
            transform.localPosition = Vector3.Slerp (fromPosition, toPosition, interpolate);
            yield return null;
        }
        
        isRunningWayPoint = false;
    }
    
    void OnDrawGizmos(){
        Gizmos.color = Color.blue;
        if (wayPointPositions.Length > 0) {
            for (int i = 1; i < wayPointPositions.Length; i++) {
                Gizmos.DrawLine (wayPointPositions [i - 1], wayPointPositions [i]);
            }
        }
    }
    
    struct SmoothMovementInfo{
        public Transform transform;
        public float targetY;
        public float currentVelocity;
        
        public SmoothMovementInfo(Transform _transform, float _targetY){
            transform = _transform;
            targetY = _targetY;
            
            currentVelocity = 0f;
        }
        
        public float localPositionY{
            get{ 
                return transform.localPosition.y;
            }
            
            set{ 
                transform.localPosition = new Vector3 (transform.localPosition.x, value, transform.localPosition.z);
            }
            
        }
    }
    
}
