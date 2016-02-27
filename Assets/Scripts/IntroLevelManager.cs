using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroLevelManager : MonoBehaviour {

    public Constellations switchConstellations;
    
    public AudioClip bgm;
    public AudioClip finishMusic;
    public Text tapToPlay;
    
    [Header("TARGAZER")]
    public ConstellationsActivator activator;
    
    [Header("To Hide Unit")]
    public Character player;
    public Transform map;
    public Vector3 settlePosition;
    public float settleDuration;
    
    [Header("Camera")]
    public  CameraFollow3D camController;
    public Vector3 finalCameraPosition;
    public float moveDuration;
    
    bool isFinished;
    
    Transform camTransform;
    
    void Start(){
        if (PlayerPrefs.GetString (SceneManager.GetActiveScene ().name, "Not Yet") == "Finished") {
            SceneManager.LoadScene ("Level Selection");
        }
        
        AudioManager.instance.PlayMusic (bgm);
        
        tapToPlay.enabled = false;
        camTransform = camController.transform;
        switchConstellations.OnActivate += OnConstellationsActivated;
    }
    
    void Update(){
        if (isFinished && Input.GetMouseButtonDown (0)) {
            SceneManager.LoadScene ("Level Selection");
        }
    }
    
    void OnConstellationsActivated(Constellations constellations){
//        print ("Intro Runn !");
        camController.enabled = false;
        player.enabled = false;
        
        player.transform.parent = map;
        
        activator.Activate ();
        
        StartCoroutine (MoveCamera ());
        StartCoroutine (MoverMap ());
        
        PlayerPrefs.SetString (SceneManager.GetActiveScene ().name, "Finished");
        
        AudioManager.instance.PlayMusic (finishMusic);
    }
//    
//    IEnumerator AnimateTapToPlay(){
//        float percent = 0;
//        
//        while (true) {
//            percent += Time.deltaTime;
//            tapToPlay.color = Color.Lerp ();
//            yield return null;
//        }
//    }
    
    IEnumerator MoveCamera(){
        float percent = 0;
        Vector3 startPosition = camTransform.position;
        
        yield return new WaitForSeconds (.5f);
        
        while (percent <= 1) {
            percent += Time.deltaTime / moveDuration;
            camTransform.position = Vector3.Slerp (startPosition, finalCameraPosition, percent);
            
            yield return null;
        }
        
        tapToPlay.enabled = true;
        isFinished = true;
//        StartCoroutine (AnimateTapToPlay ());
    }
    
    IEnumerator MoverMap(){
        float percent = 0;
        Vector3 startPosition = map.position;
        
        yield return new WaitForSeconds (1f);

        while (percent <= 1) {
            percent += Time.deltaTime / settleDuration;
            map.position = Vector3.Slerp (startPosition, settlePosition, percent);

            yield return null;
        }
    }
}
