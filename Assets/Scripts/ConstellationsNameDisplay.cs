using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Constellations))]
public class ConstellationsNameDisplay : MonoBehaviour {
    
    public Text nameTag;
//    RectTransform rectTransform;
    
    Constellations constellations;
    
    void Awake(){
//        rectTransform = nameTag.GetComponent<RectTransform> ();
        constellations = GetComponent<Constellations> ();
        constellations.OnActivate += OnConstellationsActivated;
        
        foreach (var found in FindObjectsOfType<Constellations> ()) {
            if (found != constellations) {
                found.OnActivate += OnAnyConstellationsActivated;
            }
        }
        
        if (FindObjectOfType<Character> () != null) {
            FindObjectOfType<Character> ().OnStartMoving += OnPlayerStartMoving;
        }
        
        nameTag.gameObject.SetActive (false);
    }
    
    void LateUpdate(){
        if (nameTag.IsActive ()) {
            nameTag.transform.position = Camera.main.WorldToScreenPoint (transform.position);    
//            rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint (transform.position);    
        }
    }
    
    void OnConstellationsActivated(Constellations _constellations){
        StartCoroutine (FadeIn (true));
    }
    
    void OnAnyConstellationsActivated(Constellations _constellations){
        if (constellations.activated && !nameTag.IsActive ()) {
            StartCoroutine (FadeIn (true));
        }
    }
    
    void OnPlayerStartMoving(){
        if (nameTag.IsActive ()) {
            StartCoroutine (FadeIn (false));
        }
    }
    
    IEnumerator FadeIn(bool fadingIn){
        Color fromColor = fadingIn ? new Color (1f, 1f, 1f, 0f) : new Color (1f, 1f, 1f, .9f);
        Color toColor = fadingIn ? new Color (1f, 1f, 1f, .9f) : new Color (1f, 1f, 1f, 0f);
        
        if (fadingIn) {
            nameTag.gameObject.SetActive (true);
        }
        
        float percent = 0;
        
        while (percent < 1) {
            percent += Time.deltaTime * .5f;
            float interpolate = Mathf.Pow (percent, 3) / (Mathf.Pow (percent, 3) + Mathf.Pow (1 - percent, 3));
            nameTag.color = Color.Lerp (fromColor, toColor, interpolate);
            
            yield return null;
        }
        
        if (!fadingIn) {
            nameTag.gameObject.SetActive (false);
        }
    }
    
    
}
