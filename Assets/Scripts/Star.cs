using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class Star : MonoBehaviour {
    
    public GameObject effect;
    
    [HideInInspector]
    public SphereCollider collider;
    
    public Constellations constellations{ get; private set; }
    
    void Awake(){
        constellations = GetComponentInParent <Constellations> ();
        collider = GetComponent<SphereCollider> ();
        
        constellations.OnActivate += OnConstellationsActivate;
        constellations.OnDeactivate += OnConstellationsDeactivate;
    }
    
    public void CancelContactConstellations(){
        constellations.CancelContact ();
    }
    
    public void ContactConstellations(Vector3 position){
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
    
    void OnDrawGizmos(){
        SphereCollider triggerCollider = GetComponent<SphereCollider> ();

        Gizmos.color = new Color (0, 0, 1, .2f);
        Gizmos.DrawSphere (transform.position, triggerCollider.radius / 2);
    }
}
