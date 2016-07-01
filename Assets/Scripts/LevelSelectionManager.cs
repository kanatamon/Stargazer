using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class LevelSelectionManager : MonoBehaviour {

    public LayerMask levelMask;
    
    public AudioClip bgm;
    
    public Level defaultSelectedLevel;
    Level startSelectedLevel;
    public Color finishedLevelParticleColor;
    public Color unfinishedLevelParticleColor;
    
    public Level[] sortedLevel;
    public Level[] alwaysLevel;
    
    Level prevLevel;
    bool levelSelected;
    
    Dictionary<string, Level> levelStringDict = new Dictionary<string, Level>();
    Dictionary<Collider, Level> levelDict = new Dictionary<Collider, Level>();
    List<Level> visitableLevels = new List<Level>();
    
    void Awake(){
//        PlayerPrefs.DeleteAll ();
        for (int i = 0; i < sortedLevel.Length; i++) {
            levelStringDict.Add (sortedLevel [i].name, sortedLevel [i]);
        }
        
        for (int i = 0; i < alwaysLevel.Length; i++) {
            if (!levelStringDict.ContainsKey (alwaysLevel [i].name)) {
                levelStringDict.Add (alwaysLevel [i].name, alwaysLevel [i]);
            }
        }
        
//        LoadStartSelectedLevel ();
    }
    
    void Start(){
        InvokeRepeating ("PlayMusic", 0f, bgm.length - bgm.length * .1f);
        LoadSave ();
    }

    void Update(){
        if (!levelSelected) {
            if (Input.GetMouseButtonDown (0)) {
                RaycastHit hit;
                if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, Mathf.Infinity, levelMask)) {
                    if (!levelDict.ContainsKey (hit.collider)) {
                        if (hit.collider.GetComponent<Level> () != null) {
                            levelDict.Add (hit.collider, hit.collider.GetComponent<Level> ());
                        }
                    }
                
                    if (levelDict.ContainsKey (hit.collider ) && visitableLevels.Contains(levelDict[hit.collider])) {
                        if (prevLevel == levelDict [hit.collider]) {
                            print ("Load Level : " + levelDict [hit.collider].name);
                            LoadScene (levelDict [hit.collider].name);
                        }
                    
                        if (prevLevel != null) {
                            prevLevel.particle.loop = false;
                            prevLevel.particle.Stop ();
                        }
                        
                        bool isStageFinished = PlayerPrefs.GetString (levelDict [hit.collider].name, "NotYet").Equals ("Finished");
                        levelDict [hit.collider].particle.startColor = isStageFinished ? finishedLevelParticleColor : unfinishedLevelParticleColor;
                        
                        levelDict [hit.collider].particle.loop = true;
                        levelDict [hit.collider].particle.Play ();
                    
                        prevLevel = levelDict [hit.collider];
                    }
                
                }
            }
        }
    }
    
    void PlayMusic(){
        AudioManager.instance.PlayMusic (bgm);
    }
    
    void LoadScene(string sceneName){
        levelSelected = true;
        PlayerPrefs.SetString ("Late Visited Level", sceneName);
        
        SceneManager.LoadScene (sceneName);
    }
    
    void LoadStartSelectedLevel(){
        if (!PlayerPrefs.GetString ("Late Visited Level", "None").Equals ("None")) {
            startSelectedLevel = levelStringDict [PlayerPrefs.GetString ("Late Visited Level")];
        }
        else {
            startSelectedLevel = defaultSelectedLevel;
        }

        if (startSelectedLevel != null) {
            bool isStageFinished = PlayerPrefs.GetString (startSelectedLevel.name, "NotYet").Equals ("Finished");
            startSelectedLevel.particle.startColor = isStageFinished ? finishedLevelParticleColor : unfinishedLevelParticleColor;
            
            startSelectedLevel.particle.loop = true;
            startSelectedLevel.particle.Play ();
            
            prevLevel = startSelectedLevel;
        }
    }
    
    void LoadSave(){
        visitableLevels.Clear ();
        LoadAlwaysLevel ();
        LoadSavedLevel ();
    }
    
    void LoadAlwaysLevel(){
        for (int i = 0; i < alwaysLevel.Length; i++) {
            visitableLevels.Add (alwaysLevel [i]);
//            PlayerPrefs.SetString (alwaysLevel [i].name, "Finished");
            
            for (int cstIndex = 0; cstIndex < alwaysLevel [i].constellations.Length; cstIndex++) {
                bool isActivated = PlayerPrefs.GetString (alwaysLevel [i].constellations [cstIndex].name, "Deactivate").Equals ("Activated");
                alwaysLevel [i].constellations [cstIndex].ForceAwake (isActivated);
            }
        }
    }

    void LoadSavedLevel(){
        for (int i = 0; i < sortedLevel.Length; i++) {
            visitableLevels.Add (sortedLevel [i]);
            bool isStageFinished = PlayerPrefs.GetString (sortedLevel [i].name, "NotYet").Equals ("Finished");

            if (isStageFinished) {
                for (int cstIndex = 0; cstIndex < sortedLevel [i].constellations.Length; cstIndex++) {
                    sortedLevel [i].constellations [cstIndex].ForceAwake (true);
                }
            }
            else {
                for (int cstIndex = 0; cstIndex < sortedLevel [i].constellations.Length; cstIndex++) {
                    bool isActivated = PlayerPrefs.GetString (sortedLevel [i].constellations [cstIndex].name, "Deactivate").Equals ("Activated");
                    sortedLevel [i].constellations [cstIndex].ForceAwake (isActivated);
                }
                
                startSelectedLevel = sortedLevel [i];
                startSelectedLevel.particle.loop = true;
                startSelectedLevel.particle.Play ();

                prevLevel = startSelectedLevel;
                
                return;
            }
        }
    }
    
   
}
