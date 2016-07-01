using UnityEngine;
using System.Collections;

public class UpDownWheelControllee : MonoBehaviour {

    public WheelSwitch wheel;
    public Vector3 upPosition;
    public Vector3 downPosition;
    public bool usedWorld = true;
    
    Vector3 gobalUpPosition;
    Vector3 gobalDownPosition;
    
    void Awake(){
        wheel.OnValueChange += OnWheelChangeValue;
        
        gobalUpPosition = usedWorld ? upPosition : upPosition + transform.position;
        gobalDownPosition = usedWorld ? downPosition : downPosition + transform.position;
    }
    
    void OnWheelChangeValue(float value){
        transform.position = Vector3.Lerp (gobalDownPosition, gobalUpPosition, value);
    }
     
}
