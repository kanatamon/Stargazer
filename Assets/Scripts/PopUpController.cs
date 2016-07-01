using UnityEngine;
using System.Collections;

public class PopUpController : MonoBehaviour {
    
    public AltarController comander;
    public ConstellationsSwitch activator;

    public float smoothTime = 1f;
    public float upPosition;
    public float downPosition;
    public bool startOnUp;
    
    float targetY;
    float currentVelocity;
    
//    bool isSwitchActivated;
    
    void Awake(){
        targetY = startOnUp ? upPosition : downPosition;
        transform.localPosition = new Vector3 (transform.localPosition.x, startOnUp ? upPosition : downPosition, transform.localPosition.z);
        
        comander.OnPlayerArrive += OnPlayerArrive;
        comander.OnPlayerLeave += OnPlayerLeave;
        
        activator.OnActivate += OnSwitchActivate;
    }
    
    void Update(){
        float currentY = transform.localPosition.y;
        currentY = Mathf.SmoothDamp (currentY, targetY, ref currentVelocity, smoothTime);
    
        transform.localPosition = new Vector3 (transform.localPosition.x, currentY, transform.localPosition.z);
    }
    
    void OnSwitchActivate(){
        smoothTime *= 3f;
        targetY = downPosition;
        
        comander.OnPlayerArrive -= OnPlayerArrive;
        comander.OnPlayerLeave -= OnPlayerLeave;
    }
    
    void OnPlayerArrive(){
        targetY = upPosition;
    }
    
    void OnPlayerLeave(){
        targetY = downPosition;
    }
}
