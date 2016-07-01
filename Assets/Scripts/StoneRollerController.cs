using UnityEngine;
using System.Collections;

public class StoneRollerController : MonoBehaviour {
    
    public ConstellationsSwitch activator;

    public Transform stoneTransform;
    public Vector3 localStoneEulerA;
    public Vector3 localStoneEulerB;

    public float smoothTime;
    Vector3 targetEuler;
    Vector3 currentVelocity;

    void Awake(){
        targetEuler = localStoneEulerA;
        stoneTransform.localEulerAngles = localStoneEulerA;
        activator.OnActivate += OnActivated;
    }

    void Update(){
        Vector3 currentEuler = stoneTransform.localEulerAngles;
        
        currentEuler.x = Mathf.SmoothDampAngle (currentEuler.x, targetEuler.x, ref currentVelocity.x, smoothTime);
        currentEuler.y = Mathf.SmoothDampAngle (currentEuler.y, targetEuler.y, ref currentVelocity.y, smoothTime);
        currentEuler.z = Mathf.SmoothDampAngle (currentEuler.z, targetEuler.z, ref currentVelocity.z, smoothTime);
        
        stoneTransform.localEulerAngles = currentEuler;
    }
    
    void OnActivated(){
        targetEuler = localStoneEulerB;
    }
}
