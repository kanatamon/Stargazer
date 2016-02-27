using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelMenuManager : MonoBehaviour {

    public float animateTime = .5f;

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
    
    [Header("Confirm Bar")]
    public RectTransform confirmBar;
    public Vector2 confirmBarHidePosition;
    public Vector2 confirmBarShowPosition;

    void Awake(){
        menuSwitchImage = menuSwitch.GetComponent<Image> ();
        menuSwitchBotton = menuSwitch.GetComponent<Button> ();
    }

    void Start(){
        menuSwitchBotton.enabled = true;
        menuSwitchImage.color = Color.white;

        menuBar.anchoredPosition = menuBarHidePosition;
        confirmBar.anchoredPosition = confirmBarHidePosition;
    }

    IEnumerator AnimateMenuSwitchUI(Color fromColor, Color toColor, bool toEnabled){
        float percent = 0;

        while (percent <= 1) {
            percent += Time.deltaTime / animateTime;
            menuSwitchImage.color = Color.Lerp (fromColor, toColor, percent);
            yield return null;
        }

        menuSwitchBotton.enabled = toEnabled;
    }
    
    IEnumerator AnimateMenuBarUI(Vector2 fromPos, Vector2 toPos){
        float percent = 0;

        while (percent <= 1) {
            percent += Time.deltaTime / animateTime;
            menuBar.anchoredPosition = Vector2.Lerp (fromPos, toPos, percent);
            yield return null;
        }
    }
    
    IEnumerator AnimateConfirmBarUI(Vector2 fromPos, Vector2 toPos){
        float percent = 0;

        while (percent <= 1) {
            percent += Time.deltaTime / animateTime;
            confirmBar.anchoredPosition = Vector2.Lerp (fromPos, toPos, percent);
            yield return null;
        }
    }

    public void ShowMenuBar(){
        StartCoroutine (AnimateMenuBarUI (menuBarHidePosition, menuBarShowPosition));
    }

    public void HideMenuBar(){
        StartCoroutine (AnimateMenuBarUI (menuBarShowPosition, menuBarHidePosition));
    }
    
    public void ShowMenuSwitch(){
        StartCoroutine (AnimateMenuSwitchUI (menuSwitchHideColor, menuSwitchShowColor, true));
    }
    
    public void HideMenuSwitch(){
        StartCoroutine (AnimateMenuSwitchUI (menuSwitchShowColor, menuSwitchHideColor, false));
    }
    
    public void ShowConfirmBar(){
        StartCoroutine (AnimateConfirmBarUI (confirmBarHidePosition, confirmBarShowPosition));
    }
    
    public void HideConfirmBar(){
        StartCoroutine (AnimateConfirmBarUI (confirmBarShowPosition, confirmBarHidePosition));
    }
    
    public void GoToCredit(){

    }
    
    public void NewGame(){
        PlayerPrefs.DeleteAll ();
        SceneManager.LoadScene ("Level - Intro");
    }
    
}
