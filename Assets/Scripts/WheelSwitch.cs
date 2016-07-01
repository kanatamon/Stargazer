using UnityEngine;
using System.Collections;

public class WheelSwitch : MonoBehaviour {
    
    public LayerMask stopperMask;
    
    [Range(1,18)]
    public int totalStep = 4;
    public float returnSpeed = 20f;
    
    public float idleColliderRadius = .8f;
    public float activeColliderRaidus = 2.5f;
    private  SphereCollider controllerCollider;
    
    private float minExceedAngle = 1f;
    
    private float value;
    private float totalMovedAngle;
    private bool interacted;
    
    public bool isAlwaysActive;
//    public MeshRenderer controllerRenderer;
//    public Material switchActivatedMaterials;
//    public Material switchDeactivatedMaterials;
    private bool isActive;
    
    public event System.Action<float> OnValueChange;

    void Awake(){
        controllerCollider = GetComponentInChildren<SphereCollider> ();    
        controllerCollider.isTrigger = true;
    }
    
    void Start(){
        controllerCollider.radius = idleColliderRadius;
        
        MovePercentage ();
    }
    
    void Update(){
        isActive = isAlwaysActive || !CheckIsStopperOnControlledMechanic ();
        
//        controllerRenderer.material = isActive ? switchActivatedMaterials : switchDeactivatedMaterials;
            
        if (!interacted) {
            float exceed = CalculateMovedExceed (totalMovedAngle, 0, 360);
            
            if (exceed != 0) {
                float absExceed = Mathf.Abs (exceed);
                float returnAngleStep = Mathf.Sign (exceed) * (absExceed <= minExceedAngle ? absExceed : (absExceed + returnSpeed) * Time.deltaTime);
//                print ("returnAngleStep = " + returnAngleStep + ", exceed = " + exceed);
                Move (returnAngleStep);
                MovePercentage ();
            }
            else {
                float exceedToClosestAngle;
//                float closestAngle = CalculateClosestAngle (out exceedToClosestAngle);
                CalculateClosestAngle (out exceedToClosestAngle);
                
                if (exceedToClosestAngle != 0) {
                    float absExceed = Mathf.Abs (exceedToClosestAngle);
                    float returnAngleStep = Mathf.Sign (exceedToClosestAngle) * (absExceed < minExceedAngle ? absExceed : (absExceed + returnSpeed) * Time.deltaTime);
//                    print ("returnAngleStep = " + returnAngleStep + ", exceed = " + exceedToClosestAngle);
                    Move (returnAngleStep);
                    MovePercentage ();
                }
//                print ("closestAngle = " + closestAngle + ", exceedToClosestAngle = " + exceedToClosestAngle + ", totalMovedAngle = " + totalMovedAngle);
            }
        }
    }
    
    void MovePercentage(){
        if (OnValueChange != null) {
            float clampedValue = Mathf.Clamp01 (value);
            OnValueChange (clampedValue);
        }
    }
    
    public void MakeContact(Collider ctrlColl, Vector3 contactPosition){
        if (!isActive) {
            return;
        }
        
        controllerCollider.radius = activeColliderRaidus;
        
        interacted = true;
        Move (ctrlColl, contactPosition);
        
        MovePercentage ();
    }
    
    public void CancelContact(){
        interacted = false;
        controllerCollider.radius = idleColliderRadius;
    }
    
    public void Move(Collider ctrlColl, Vector3 contactPosition){
        Vector3 originToContact = (contactPosition - transform.position).normalized;
        Vector3 originToCtrlColl = (ctrlColl.transform.position - transform.position).normalized;
        float moveAngle = Vector3.Angle (originToContact, originToCtrlColl);
        
        Vector3 ctrlToContact = (contactPosition - ctrlColl.transform.position).normalized;
        float moveDir = Vector3.Angle (ctrlToContact, ctrlColl.transform.right) > 90 ? 1 : -1;
        
//        value = Mathf.Clamp (value, 0, 1);
        Move (moveDir * moveAngle);
    }
    
    public void Move(float moveAmount){
        totalMovedAngle -= moveAmount;
        value = totalMovedAngle / 360f;
        
//        if (totalMovedAngle == 0f || totalMovedAngle == 90f || totalMovedAngle == 180f || totalMovedAngle == 270f || totalMovedAngle == 360f) {
//            AudioManager.instance.PlayeSound ("Wheel Move", transform.position);
//        }
        
        if (Mathf.FloorToInt (totalMovedAngle) == 0 || Mathf.FloorToInt (totalMovedAngle) == 90 || Mathf.FloorToInt (totalMovedAngle) == 180) {
//            AudioManager.instance.PlayeSound ("Wheel Move", transform.position);
        }
//        print ("totalMovedAngle = " + totalMovedAngle + " , value = " + value);
        transform.Rotate (Vector3.forward, moveAmount);
    }
    
    bool CheckIsStopperOnControlledMechanic(){
//        for (int i = 0; i < controlledMechanics.Length; i++) {
//            WayPointNode[] nodes = controlledMechanics [i].GetWayPointNodes ();
//            for (int n = 0; n < nodes.Length; n++) {
//                if (Physics.OverlapSphere (nodes[n].CalculateCorePosition(), .4f, stopperMask).Length > 0) {
//                    return true;
//                }
//            }
//        }
        
        return false;
    }
    
    float CalculateMovedExceed(float moveValue, float min, float max){
        if (moveValue > 360) {
            return Mathf.Abs (max - moveValue);
        }else if (moveValue < 0) {
            return -Mathf.Abs (min - moveValue);
        }
        
        return 0;
    }
    
    float CalculateClosestAngle(out float exceed){
//        int totalStep = 5;
        float step = 360f / (float)totalStep;
        
        float closestDst = Mathf.Infinity;
        float closestAngle = 0;
        
        for (int i = 0; i <= totalStep; i++) {
            float angle = ((float)i * step);
            float dst = Mathf.Abs (angle - totalMovedAngle);
            
            if (dst < closestDst) {
                closestDst = dst;
                closestAngle = angle;
            }
        }
        
        exceed = totalMovedAngle - closestAngle;
        return closestAngle;
    }
    
//    void OnDrawGizmos(){
//        Gizmos.color = Color.green;
//        for (int i = 0; i < controlledMechanics.Length; i++) {
//            Gizmos.DrawLine (transform.position, controlledMechanics [i].transform.position);
//        }
//    }
    
}
