using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameLevelManager : MonoBehaviour {

//    public AdvancedFlyController cameraFlyController;
    public MonoBehaviour[] toDisableComponentOnFinal;
    
    public AudioClip bgm;
    public AudioClip stageFinishedBgm;
    
    public Image tapUI;
    public float delayToShowTap = 3f;
    
    public ConstellationsInfo[] constellationsInfo;
    
    [Header("Camera")]
    public Transform cameraTransfrom;
    public Vector3 locaFirstPosition;
    public float cameraMoveUpDuration;
    public Transform finalTransforomation;
    public float cameraMoveDuration;
    
    bool isAllConstellationsActivated;
    bool isLoading;
    
//    bool isLevelCompleted;
    
    Dictionary<Constellations, ConstellationsInfo> dict = new Dictionary<Constellations, ConstellationsInfo> ();
    
    void Awake(){
//        PlayerPrefs.DeleteAll ();
//        RenderSettings.fogColor = new Color (.9f, .9f, .9f);
        for (int i = 0; i < constellationsInfo.Length; i++) {
            dict.Add (constellationsInfo [i].constellations, constellationsInfo [i]);
            
            constellationsInfo [i].constellations.OnActivate += OnConstellationsActivated;
        }
        
        tapUI.gameObject.SetActive (false);
    }
    
    void Start(){
        isLoading = true;
        for (int i = 0; i < constellationsInfo.Length; i++) {
            if (PlayerPrefs.GetString (constellationsInfo [i].constellations.name, "Deactivated") == "Activated") {
                constellationsInfo [i].constellations.Activate ();
            }
            else {
                constellationsInfo [i].icon.color = constellationsInfo [i].iconDeactivatedColor;
            }
        }
        isLoading = false;
        
        InvokeRepeating ("PlayMusic", 0f, bgm.length - bgm.length * .1f);
    }
    
    void Update(){
        if (tapUI.gameObject.activeInHierarchy) {
            if (Input.GetMouseButton (0)) {
                SceneManager.LoadScene ("Level Selection");
            }
        }
    }
    
    void PlayMusic(){
        AudioManager.instance.PlayMusic (bgm);
    }
    
    void FinishStage(){
        // ... Save Finish
        PlayerPrefs.SetString (SceneManager.GetActiveScene ().name, "Finished");
        
        for (int i = 0; i < toDisableComponentOnFinal.Length; i++) {
            toDisableComponentOnFinal [i].enabled = false;
        }
        
        StartCoroutine (MoveCameraFinal ());
        
        // ... Let Constellations Fly
        for (int i = 0; i < constellationsInfo.Length; i++) {
            constellationsInfo [i].flyController.Fly ();
        }
        
        AudioManager.instance.PlayMusic (stageFinishedBgm);
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
        // ... Save Constellations Activation
        PlayerPrefs.SetString (constellations.name, "Activated");        
        ConstellationsInfo info = dict [constellations];

        // ... Animate Icon
        if (isLoading) {
            info.icon.color = info.iconActivatedColor;
        }
        else {
            StartCoroutine (AnimateConstellationsIcon (info.icon, info.iconActivatedColor));
            AudioManager.instance.PlayeSound2D ("Complete Constellations");
        }
        
        if (IsAllConstellationsActivated ()) {
            if (!isLoading) {
                FinishStage ();
            }
        }
    }
    
    IEnumerator AnimateConstellationsIcon(Image image, Color targetColor){
        Color originColor = image.color;
        float percent = 0;
        
        while (percent <= 1) {
            percent += Time.deltaTime / 6f;
            image.color = Color.Lerp (originColor, targetColor, percent);
            yield return null;
        }
    }
    
    IEnumerator MoveCameraFinal(){
        cameraTransfrom.parent = null;
        Vector3 worldFirstPosition = locaFirstPosition + cameraTransfrom.transform.position;
        
        for (int i = 0; i < 2; i++) {
            Vector3 fromPosition = cameraTransfrom.position;
            Vector3 toPosition = i == 0 ? worldFirstPosition : finalTransforomation.position;
            
            Quaternion fromRotation = cameraTransfrom.rotation;
            Quaternion toRotation = i == 0 ? cameraTransfrom.rotation : Quaternion.LookRotation (finalTransforomation.forward);
            
            float percent = 0;
            float speed = 1 / (i == 0 ? cameraMoveUpDuration : cameraMoveDuration);

            while (percent < 1) {
                percent += Time.deltaTime * speed;
                float interpolate = Mathf.Pow (percent, 3) / (Mathf.Pow (percent, 3) + Mathf.Pow (1 - percent, 3));

                cameraTransfrom.position = Vector3.Lerp (fromPosition, toPosition, interpolate);
                cameraTransfrom.rotation = Quaternion.Lerp (fromRotation, toRotation, interpolate);

                yield return null;
            }
        }
        
        yield return new WaitForSeconds (delayToShowTap);
        
        StartCoroutine (FadeInTapUI ());
//        isLevelCompleted = true;
    }
    
    IEnumerator FadeInTapUI(){
        tapUI.gameObject.SetActive (true);
        float percent = 0;

        while (percent < 1) {
            percent += Time.deltaTime / 2f;
            tapUI.color = Color.Lerp (new Color (1f, 1f, 1f, 0f), Color.white, percent);
            yield return null;
        }
    }
    
    [System.Serializable]
    public struct ConstellationsInfo{
        public Constellations constellations;
        public FlyController flyController;
        public Image icon;
        public Color iconActivatedColor;
        public Color iconDeactivatedColor;
    }
}
