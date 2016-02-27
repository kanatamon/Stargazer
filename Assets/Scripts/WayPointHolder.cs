using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

public class WayPointHolder : MonoBehaviour {
    
//    public Vector3 graphicsLocalScale;
    
    [HideInInspector]
    public bool hided;
    
    [HideInInspector]
    public Dictionary<WayPointNode, MeshRenderer> dict = new Dictionary<WayPointNode, MeshRenderer> ();
    
    public void ApplyLocalScaleToGraphics(){
        foreach (WayPointNode node in GetComponentsInChildren<WayPointNode> ()){
//            node.transform.localScale = new Vector3 (1, .3f, 1);
            node.transform.GetChild (0).localScale = new Vector3 (.7f, .5f, .7f);
            node.transform.GetChild (0).localPosition = new Vector3 (0, .25f, 0);
        }   
    }
    
    public void SwitchGraphics(){
        hided = !hided;
        
        if (hided) {
            HideGraphics ();
        }
        else {
            DisplayGraphics ();
        }
    }
    
    void HideGraphics() {
        foreach (WayPointNode node in GetComponentsInChildren<WayPointNode> ()){
            if (!dict.ContainsKey (node)) {
                dict.Add (node, node.GetComponent<MeshRenderer> ());
            }
            
            dict [node].enabled = true;
            
            for (int i = 0; i < node.transform.childCount; i++) {
                node.transform.GetChild (i).gameObject.SetActive (false);
            }
        }
    }
    
    void DisplayGraphics() {
        foreach (WayPointNode node in GetComponentsInChildren<WayPointNode> ()){
            if (!dict.ContainsKey (node)) {
                dict.Add (node, node.GetComponent<MeshRenderer> ());
            }

            dict [node].enabled = false;
            
            for (int i = 0; i < node.transform.childCount; i++) {
                node.transform.GetChild (i).gameObject.SetActive (true);
            }
        }
    }
}

#endif
