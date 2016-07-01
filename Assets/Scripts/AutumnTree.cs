using UnityEngine;
using System.Collections;

public class AutumnTree : MonoBehaviour {

    public WheelSwitch wheelController;
    
    AudioSource audioSource;
    
    public float smoothTime;
    
    float targetScale;
    float currentVelocity;
    
    public Transform bakeTransform;
    public Transform leaveTransform;
    
    void Awake(){
        wheelController.OnValueChange += OnWheelChangeValue;
        targetScale = leaveTransform.localScale.x;
    }
    
    void Update(){
        float currentScale = leaveTransform.localScale.x;
        
        currentScale = Mathf.SmoothDamp (currentScale, targetScale, ref currentVelocity, smoothTime);
        leaveTransform.localScale = new Vector3 (currentScale, currentScale, currentScale);
    }
    
    void OnWheelChangeValue(float value){
        targetScale = 1 - value;
    }
}
