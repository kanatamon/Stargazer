using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour {
    
//    public GameObject menuButtonUI;
    public float animateTime = .5f;
    
    [Header("Constellations UI")]
    public RectTransform constellationsUI;
    public Vector2 onMenuHidePosition;
    public Vector2 onMenuShowPosition;
    
    [Header("Menu Switch")]
    public RectTransform menuSwitch;
    public Color menuSwitchHideColor;
    public Color menuSwitchShowColor;
    private Image menuSwitchImage;
    private Button menuSwitchBotton;
    
    [Header("Menu Bar")]
    public RectTransform menuBar;
    public Vector2 menuBarHidePosition;
    public Vector2 menuBarShowPosition;
    
    [Header("Level Label")]
    public RectTransform labelTransform;
    public Vector2 labelShowPosition;
    public Vector2 labelHidePosition;
    public float labelMoveDuration;
    public float labelWaitDuration;
    
//    private Vector3 labelMovePosition;
    
    bool menuDisplayed;
   
    void Awake(){
        menuSwitchImage = menuSwitch.GetComponent<Image> ();
        menuSwitchBotton = menuSwitch.GetComponent<Button> ();
    }
    
    void Start(){
//        menuButtonUI.SetActive (false);
        menuSwitchBotton.enabled = true;
        menuSwitchImage.color = Color.white;
        
        menuBar.anchoredPosition = menuBarHidePosition;
        
        constellationsUI.anchoredPosition = onMenuHidePosition;
        
        StartCoroutine (AnimateLabelNameUI ());
    }
    
    IEnumerator AnimateLabelNameUI(){
        float percent = 0;
        bool hasWait = false;
        
        while (percent <= 1) {
            if(percent >= 0.5f && !hasWait){
                hasWait = true;
                yield return new WaitForSeconds (labelWaitDuration);
            }
            
            percent += Time.deltaTime / labelMoveDuration;
            float interpolation = (-Mathf.Pow (percent, 2) + percent) * 4;
            labelTransform.localPosition = Vector2.Lerp (labelHidePosition, labelShowPosition, interpolation);
           
            yield return null;
        }
    }
    
    IEnumerator AnimateConstellationsUI(Vector2 fromPosition, Vector2 toPosition){
        float percent = 0;

        while (percent <= 1) {
            percent += Time.deltaTime / animateTime;
            constellationsUI.anchoredPosition = Vector2.Lerp (fromPosition, toPosition, percent);
//            ConstellationsUI.localPosition = (Vector3)Vector2.Lerp (fromPosition, toPosition, percent);
            yield return null;
        }
    }
    
    IEnumerator AnimateMenuUI(Vector2 fromPos, Vector2 toPos, Color fromColor, Color toColor, bool toEnabled){
        float percent = 0;
        
        while (percent <= 1) {
            percent += Time.deltaTime / animateTime;
            menuBar.anchoredPosition = Vector2.Lerp (fromPos, toPos, percent);
            menuSwitchImage.color = Color.Lerp (fromColor, toColor, percent);
            
            yield return null;
        }
        
        menuSwitchBotton.enabled = toEnabled;
    }
    
    public void ShowMenuBar(){
//        menuButtonUI.SetActive (true);
//        menuSwitch.enabled = false;
        
        StartCoroutine (AnimateMenuUI (menuBarHidePosition, menuBarShowPosition, menuSwitchHideColor, menuSwitchShowColor, false));
        StartCoroutine (AnimateConstellationsUI (onMenuHidePosition, onMenuShowPosition));
    }
    
    public void BackToLevelSelection(){
        SceneManager.LoadScene ("Level Selection");
//        PlayerPrefs.SetString ("Late Visited Level", SceneManager.GetActiveScene ().name);
//        PlayerPrefs.SetString ("Is Stage Finished", "False");
    }
    
    public void HideMenuBar(){
//        menuButtonUI.SetActive (false);
//        menuSwitch.enabled = true;
        
        StartCoroutine (AnimateMenuUI (menuBarShowPosition, menuBarHidePosition, menuSwitchShowColor, menuSwitchHideColor, true));
        StartCoroutine (AnimateConstellationsUI (onMenuShowPosition, onMenuHidePosition));
    }
}
