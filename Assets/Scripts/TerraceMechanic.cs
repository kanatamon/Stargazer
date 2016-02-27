using UnityEngine;
using System.Collections;

public class TerraceMechanic : MonoBehaviour {
    
    [Range(0.0f, 1.0f)]
    public float percent = 0f;
    
    public TerraceChild[] terraceChilds;
    
    TerraceMechanic[] childMechanics;
    
    void Start(){
        TerraceMechanic[] mechanics = GetComponentsInChildren<TerraceMechanic> ();
        
        if (mechanics.Length > 0) {
            childMechanics = new TerraceMechanic[mechanics.Length - 1];
            for (int i = 0; i < childMechanics.Length; i++) {
                childMechanics [i] = mechanics [i + 1];
            }
        }
        else {
            Debug.LogError ("No any 'TerraceMechanic' attached as child");
        }
        
    }
    
    void Update(){
        for (int i = 0; i < childMechanics.Length; i++) {
            childMechanics [i].percent = percent;
        }
        
        for (int i = 0; i < terraceChilds.Length; i++) {
            terraceChilds [i].Move (percent);
        }
    }
    
    [System.Serializable]
    public struct TerraceChild{
        public Transform terrace;
        public Vector3 localStartPosition;
        public Vector3 localFinishPosition;
        
        public void Move(float _percent){
            terrace.localPosition = Vector3.Lerp (localStartPosition, localFinishPosition, _percent);
        }
    }
    
}
