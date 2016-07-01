using UnityEngine;
using System.Collections;

public class PlayerMotor : MonoBehaviour {

    [Header("Camera Eye")]
    public Transform camera;
    public float startCameraRotationEuler;
    
    [Range(0f,1f)]
    public float moveFactorX = 1f;
    
    [Range(0f,100f)]
    public float thresholdX;
    
    public float moveSensitivityX = .3f;
    public Vector2 limitRotationX;
    
    [Header("Head Motor")]
    public Transform head;
    public float startHeadRotationEuler;
    
    [Range(0f,1f)]
    public float moveFactorY = 1f;
    
    [Range(0f,100f)]
    public float thresholdY;
    
    public float moveSensitivityY = .3f;
    public Vector2 limitRotationY;
    
    float targetEulerX;
    float targetEulerY;
    float playerTargetEulerY;
    
    float currentEulerX;
    float currentEulerY;
    
    float currentVelocityX;
    float currentVelocityY;
    float currentPlayerVelocityY;
    
    Character player;
    
    void Awake(){
        player = GetComponent<Character> ();
    }
    
    void Start(){
        currentEulerX = startCameraRotationEuler;
        currentEulerY = startHeadRotationEuler;
        
        playerTargetEulerY = transform.localEulerAngles.y;
        
        ResetRotation ();
    }
    
    void LateUpdate(){
        MoveRotation ();
    }
     
    public void Rotation(Vector3 deltaMove){
        FilterThresholdToMove (ref deltaMove);
        
        targetEulerX -= deltaMove.y;
        targetEulerX = Mathf.Clamp (targetEulerX, limitRotationX.x, limitRotationX.y);
        
        if (player.isMovingOnPath) {
            targetEulerY += deltaMove.x;
            targetEulerY = Mathf.Clamp (targetEulerY, limitRotationY.x, limitRotationY.y);
        }
        else {
            playerTargetEulerY += deltaMove.x;
        }
    }
    
    public void ResetRotation(){
        targetEulerX = startCameraRotationEuler;
        targetEulerY = startHeadRotationEuler;
    }
    
    void MoveRotation(){
        MovePlayerRotation ();
        
        MoveHeadRotation ();
        MoveCameraRotation ();
    }
    
    Vector3 FilterThresholdToMove(ref Vector3 deltaMove){
        deltaMove.y *= moveFactorX;
        deltaMove.x *= moveFactorY;
        
        if (Mathf.Abs (deltaMove.x) < thresholdY * moveFactorY) {
            deltaMove.x = 0f;
        }
        
        if (Mathf.Abs (deltaMove.y) < thresholdX * moveFactorX) {
            deltaMove.y = 0f;
        }
        
        return deltaMove;
    }
    
    void MoveHeadRotation(){
        currentEulerY = Mathf.SmoothDampAngle (currentEulerY, targetEulerY, ref currentVelocityY, moveSensitivityY);
        head.localEulerAngles = new Vector3 (0f, currentEulerY, 0f);
    }
    
    void MoveCameraRotation(){
        currentEulerX = Mathf.SmoothDampAngle (currentEulerX, targetEulerX, ref currentVelocityX, moveSensitivityX);
        camera.localEulerAngles = new Vector3 (currentEulerX, 0f, 0f);
    }
    
    void MovePlayerRotation(){
        float currentPlayerEulerY = transform.localEulerAngles.y;
        currentPlayerEulerY = Mathf.SmoothDampAngle (currentPlayerEulerY, playerTargetEulerY, ref currentPlayerVelocityY, moveFactorY);
        
        transform.localEulerAngles = new Vector3 (0f, currentPlayerEulerY, 0f);
    }
}
