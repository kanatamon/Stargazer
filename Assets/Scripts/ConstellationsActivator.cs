using UnityEngine;
using System.Collections;

public class ConstellationsActivator : MonoBehaviour {

    public Constellations[] constellationes;
    public bool activateOnStart = false;
    
    void Start(){
        if (activateOnStart) {
            Activate ();
        }
    }
    
    public void Activate (){
        for (int i = 0; i < constellationes.Length; i++) {
            constellationes [i].Activate ();
        }
    }
    
    public void Deactivate(){
        for (int i = 0; i < constellationes.Length; i++) {
            constellationes [i].Deactivate ();
        }
    }
    
}
