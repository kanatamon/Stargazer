using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameLevelManager : MonoBehaviour {

    public AudioClip bgm;
    public AudioClip finishMusic;
    
    public Text constellationsNameTag;
    public float showNameDuration;
    
    public ConstellationsInfo[] constellationsInfo;
    public Transform endPoint;
    public LayerMask playerMask;
    
    [Header("Camera Position & Euler")]
    public CameraFollow3D cameraFollower;
    public float duration;
    public Vector3 camEndStagePosition;
    public Vector3 camEndStageEuler;
    
    bool isAllConstellationsActivated;
    bool isFinishCalled;
    
    bool isLoading;
    
    Dictionary<Constellations, ConstellationsInfo> dict = new Dictionary<Constellations, ConstellationsInfo> ();
    
    void Awake(){
//        PlayerPrefs.DeleteAll ();
//        RenderSettings.fogColor = new Color (.9f, .9f, .9f);
        for (int i = 0; i < constellationsInfo.Length; i++) {
            dict.Add (constellationsInfo [i].constellations, constellationsInfo [i]);
            
            constellationsInfo [i].constellations.OnActivate += OnConstellationsActivated;
            constellationsInfo [i].constellations.OnDeactivate += OnConstellationsDeactivated;
        }
        
    }
    
    void Start(){
        isLoading = true;
        for (int i = 0; i < constellationsInfo.Length; i++) {
            if (PlayerPrefs.GetString (constellationsInfo[i].constellations.name, "Deactivated") == "Activated") {
                constellationsInfo [i].constellations.Activate ();
            }
        }
        isLoading = false;
        
        InvokeRepeating ("PlayMusic", 0f, bgm.length - bgm.length * .1f);
        constellationsNameTag.gameObject.SetActive (false);
    }
    
    void Update(){
        if (!isFinishCalled && isAllConstellationsActivated) {
            if (Physics.OverlapSphere (endPoint.position, .4f, playerMask).Length > 0) {
                FinishStage ();
            }
        }
    }
    
    void PlayMusic(){
        AudioManager.instance.PlayMusic (bgm);
    }
    
    void FinishStage(){
        isFinishCalled = true;
        
        cameraFollower.enabled = false;
        StartCoroutine (AnimateEndScene (camEndStagePosition, camEndStageEuler));
        
        for (int i = 0; i < constellationsInfo.Length; i++) {
            constellationsInfo [i].endPointAnimation.ActivateFloating ();
        }
        
        PlayerPrefs.SetString (SceneManager.GetActiveScene ().name, "Finished");
        
        AudioManager.instance.PlayMusic (finishMusic);
//        PlayerPrefs.SetString ("Late Visited Level", SceneManager.GetActiveScene ().name);
    }
    
    bool IsAllConstellationsActivated(){
        for (int i = 0; i < constellationsInfo.Length; i++) {
            if (!constellationsInfo [i].constellations.activated) {
                return false;
            }
        }
        
        return true;
    }
    
    void OnConstellationsActivated(Constellations constellations){
        isAllConstellationsActivated = IsAllConstellationsActivated ();
        PlayerPrefs.SetString (constellations.name, "Activated");
        
        ConstellationsInfo info = dict [constellations];
//        info.image.sprite = info.activatedSprite;
//        info.endPointStarEffect.SetActive (true);
        info.endPointAnimation.ActivateEffect ();
        
        if (isLoading) {
            info.image.color = info.activatedColor;
        }
        else {
            StartCoroutine (ConstellationsIcon (info.image, info.activatedColor));
            StartCoroutine (AnimateConstellationsNameTag (constellations.name, constellations.transform));
            AudioManager.instance.PlayeSound2D ("Complete Constellations");
        }
    }
    
    void OnConstellationsDeactivated(Constellations constellations){
        ConstellationsInfo info = dict [constellations];

//        info.image.sprite = info.deactivatedSprite;
//        StartCoroutine (ConstellationsIcon (info.image, info.deactivatedColor));
//        info.endPointStarEffect.SetActive (false);
        info.endPointAnimation.Deactivate ();
        info.image.color = info.deactivatedColor;
    }
    
    IEnumerator AnimateEndScene(Vector3 targetPosition, Vector3 targetEuler){
        Transform cam = cameraFollower.transform;
        Quaternion fromRotation = cam.rotation;
        Vector3 fromPosition = cam.position;
        
        float percent = 0;
        while (percent <= 1) {
            percent += Time.deltaTime / duration;
            float interpolated = (-Mathf.Pow (percent * .5f, 2) + percent * .5f) * 4;
            cam.rotation = Quaternion.Lerp (fromRotation, Quaternion.Euler (targetEuler), percent);
            cam.position = Vector3.Lerp (fromPosition, targetPosition, percent);
            yield return null;
        }
        
        yield return new WaitForSeconds (2f);
        SceneManager.LoadScene ("Level Selection");
    }
    
    IEnumerator AnimateConstellationsNameTag(string constellationsName, Transform constellationsT){
        constellationsNameTag.gameObject.SetActive (true);
        constellationsNameTag.text = constellationsName;
        float percent = 0;
        
        while (percent <= 1) {
            percent += Time.deltaTime / showNameDuration;
            float interpolation = (-Mathf.Pow (percent, 2) + percent) * 4;
            constellationsNameTag.color = Color.Lerp (Color.clear, Color.yellow, interpolation);
            
//            constellationsNameTag.transform.localPosition = Camera.main.WorldToScreenPoint (constellationsT.transform.position);
            
            yield return null;
        }
        
        constellationsNameTag.gameObject.SetActive (false);
    }
    
    IEnumerator ConstellationsIcon(Image image, Color targetColor){
        Color originColor = image.color;
        float percent = 0;
        
        while (percent <= 1) {
            image.color = Color.Lerp (originColor, targetColor, percent);
            percent += Time.deltaTime / 2f;
            yield return null;
        }
    }
    
    [System.Serializable]
    public struct ConstellationsInfo{
        public Constellations constellations;
        public Image image;
//        public Sprite activatedSprite;
//        public Sprite deactivatedSprite;
        public Color activatedColor;
        public Color deactivatedColor;
        public FloatingAnimation endPointAnimation;
    }
}
