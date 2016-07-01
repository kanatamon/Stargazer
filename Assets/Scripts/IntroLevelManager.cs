using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroLevelManager : MonoBehaviour {

    public AudioClip bgm;
    
    public Constellations activator;
    public FlyController flyController;
    
    public MonoBehaviour[] toDisableComponentOnFinal;
    public Image tapToStartUI;
//    float target
    
    [Header("Title")]
    public Transform titleTransform;
    public float delayToDisplayAfterMoved;
    public float fadeSmoothTime;
    LineRenderer[] titleLines;
    MeshRenderer[] titleMeshs;
    
    [Header("Camera")]
    public Transform cameraTransfrom;
    public float cameraMoveDuration;
    public Transform finalTransforomation;
    
    void Awake(){
        titleTransform.gameObject.SetActive (false);

        activator.OnActivate += OnActivatorActivate;
        tapToStartUI.gameObject.SetActive (false);        
    }
    
    void Start(){
        titleLines = titleTransform.GetComponentsInChildren<LineRenderer> ();
        titleMeshs = titleTransform.GetComponentsInChildren<MeshRenderer> ();
        
        InvokeRepeating ("PlayMusic", 0f, bgm.length - bgm.length * .1f);
    }

    void Update(){
        if (tapToStartUI.gameObject.activeInHierarchy) {
            if (Input.GetMouseButton (0)) {
                SceneManager.LoadScene ("Level Selection");
            }
        }
    }
    
    void PlayMusic(){
        AudioManager.instance.PlayMusic (bgm);
    }
    
    void OnActivatorActivate(Constellations activatedConstellation){
        AudioManager.instance.PlayeSound2D ("Complete Constellations");
        
        PlayerPrefs.SetString (SceneManager.GetActiveScene ().name, "Finished");
        PlayerPrefs.SetString (activator.name, "Activated");      
        
        for (int i = 0; i < toDisableComponentOnFinal.Length; i++) {
            toDisableComponentOnFinal [i].enabled = false;
        }
        
        flyController.Fly ();
        StartCoroutine (MoveCameraFinal ());
    }
    
    IEnumerator MoveCameraFinal(){
        cameraTransfrom.parent = null;
        Vector3 fromPosition = cameraTransfrom.position;
        Quaternion fromRotation = cameraTransfrom.rotation;
        
        float percent = 0;
        float speed = 1 / cameraMoveDuration;
        
        while (percent < 1) {
            percent += Time.deltaTime * speed;
            float interpolate = Mathf.Pow (percent, 3) / (Mathf.Pow (percent, 3) + Mathf.Pow (1 - percent, 3));
            
            cameraTransfrom.position = Vector3.Lerp (fromPosition, finalTransforomation.position, interpolate);
            cameraTransfrom.rotation = Quaternion.Lerp (fromRotation, Quaternion.LookRotation (finalTransforomation.forward), interpolate);
            
            yield return null;
        }
        
        yield return new WaitForSeconds (delayToDisplayAfterMoved);
        
        StartCoroutine (FadeTitle ());
    }
    
    IEnumerator FadeTitle(){
        titleTransform.gameObject.SetActive (true);
        
        float percent = 0;
        float speed = 1 / fadeSmoothTime;
        
        while (percent < 1) {
            percent += Time.deltaTime * speed;
            
            for (int i = 0; i < titleLines.Length; i++) {
                Color color = Color.Lerp (new Color (1f, 1f, 1f, 0f), Color.white, percent);
                titleLines [i].SetColors (color, color);
            }
            
            for (int i = 0; i < titleMeshs.Length; i++) {
                titleMeshs [i].material.color = Color.Lerp (new Color (1f, 1f, 1f, 0f), Color.white, percent);
            }
            
            yield return null;
        }
        
        tapToStartUI.gameObject.SetActive (true);
        StartCoroutine (FadeInUI ());
    }
    
    IEnumerator FadeInUI(){
        float percent = 0;
        
        while (percent < 1) {
            percent += Time.deltaTime / 2f;
            tapToStartUI.color = Color.Lerp (new Color (1f, 1f, 1f, 0f), Color.white, percent);
            yield return null;
        }
    }
    
    
 
}
