using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

[CustomEditor (typeof (WayPoint))]
public class WayPointEditor : Editor {

    public override void OnInspectorGUI(){
        WayPoint wayPoint = target as WayPoint;

        if (DrawDefaultInspector()){
            for (int i = 0; i < wayPoint.connectionCheckers.Length; i++) {
                wayPoint.connectionCheckers [i].motherWayPoint = wayPoint;
            }
        }
        
        if (GUILayout.Button ("Set As StartingWayPoint")) {
            Character player = FindObjectOfType<Character> ();
            player.startWayPoint = wayPoint;
            
            EditorUtility.SetDirty (player);
//            EditorUtility.SetDirty (player.startWayPoint); // Not working !!
        }
    }
}
