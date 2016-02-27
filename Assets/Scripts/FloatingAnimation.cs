using UnityEngine;
using System.Collections;

public class FloatingAnimation : MonoBehaviour {

    public Vector3 variation;
    public float variationSpeed;
    
    public ParticleSystem particleEffect;
    public bool activateOnAwake;
    
    private Vector3 privot;
    private bool isAnimating;
    private bool eanbleFloating;
    
    void Awake(){
        privot = transform.localPosition;
        
        if (activateOnAwake) {
            ActivateEffect ();
            ActivateFloating ();
        }
        else {
            Deactivate ();
        }
    }
	
	void Update () {
        if (eanbleFloating && !isAnimating) {
            StartCoroutine (Animate (privot + variation));
        }
	}
    
    public void ActivateFloating(){
        eanbleFloating = true;   
    }
    
    public void ActivateEffect(){
        if (particleEffect != null) {
            particleEffect.Play ();
            particleEffect.loop = true;
        }
    }
    
    public void Deactivate(){
        eanbleFloating = false;
        
        if (particleEffect != null) {
            particleEffect.Stop ();
            particleEffect.loop = false;
        }
    }
    
    IEnumerator Animate(Vector3 localTargetPosition){
        isAnimating = true;
        float percent = 0;
        
        while (percent <= 1) {
            percent += Time.deltaTime / variationSpeed;
            float interpolation = (-Mathf.Pow (percent, 2) + percent) * 4;
            transform.localPosition = Vector3.Lerp (privot, localTargetPosition, interpolation);
            yield return null;
        }
        
        isAnimating = false;
    }
}
