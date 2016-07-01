using UnityEngine;
using System.Collections;

public class Star : EventPoint {
    
    public GameObject effect;
    
    public Constellations constellations{ get; private set; }
//    
    public override void Awake (){
        base.Awake ();
        
        constellations = GetComponentInParent<Constellations> ();
        
        constellations.OnActivate += OnConstellationsActivate;
        constellations.OnDeactivate += OnConstellationsDeactivate;
    }
    
    public override void CancelContactConstellations(){
        constellations.CancelContact ();
    }

    public override void ContactConstellations(Vector3 position){
        constellations.MakeContact (this, position);
    }
    
    void OnConstellationsActivate(Constellations constellations){
        if (effect != null) {
            effect.SetActive (true);
        }
        
        collider.enabled = false;
    }
    
    void OnConstellationsDeactivate(Constellations constellations){
        if (effect != null) {
            effect.SetActive (false);
        }
        
        collider.enabled = true;
    }
    
    public override Vector3 constellationsForward {
        get {
            return constellations.transform.forward;
        }
    }
    
}
