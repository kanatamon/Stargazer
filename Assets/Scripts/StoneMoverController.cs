using UnityEngine;
using System.Collections;

public class StoneMoverController : MonoBehaviour {

    public ConstellationsSwitch activator;
    
    public Vector3 positionA;
    public Vector3 positionB;
    public bool usedWorld;
    Vector3 globalPositionA;
    Vector3 globalPositionB;
    
    public float smoothTime;
    Vector3 targetPosition;
    Vector3 currentVelocity;
    
    AudioSource audioSource;
    
    void Awake(){
        if (!usedWorld) {
            globalPositionA = transform.position + positionA;
            globalPositionB = transform.position + positionB;
        }
        else {
            globalPositionA = positionA;
            globalPositionB = positionB;
        }
        
        targetPosition = globalPositionA;
        transform.position = globalPositionA;
        
        activator.OnActivate += OnActivated;
    }
    
    void Update(){
        Move ();
    }
    
    void Move(){
        transform.position = Vector3.SmoothDamp (transform.position, targetPosition, ref currentVelocity, smoothTime);
        
    }
    
    void OnActivated(){
        targetPosition = globalPositionB;
        
    }
   
}
