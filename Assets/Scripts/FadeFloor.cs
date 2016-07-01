using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class FadeFloor : MonoBehaviour {

    public ConstellationsSwitch activator;
    public WayPoint[] standinWayPoints;
    
    public float fadeSmoothTime;
    float targetPercent;
    float currentPercent;
    float currentVelocity;
    
    MeshRenderer meshRenderer;
    
    void Awake(){
        meshRenderer = GetComponent<MeshRenderer> ();
        activator.OnActivate += OnSwitchActivated;
        
        targetPercent = 0f;
        currentPercent = 0f;
    }
    
    void Update(){
        currentPercent = Mathf.SmoothDamp (currentPercent, targetPercent, ref currentVelocity, fadeSmoothTime);
        meshRenderer.material.color = Color.Lerp (new Color (1f, 1f, 1f, 0f), Color.white, currentPercent);
        
        for (int i = 0; i < standinWayPoints.Length; i++) {
            standinWayPoints [i].gameObject.SetActive (currentPercent > .6f);
        }
    }
    
    void OnSwitchActivated(){
        targetPercent = 1f;
    }
}
