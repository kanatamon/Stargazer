using UnityEngine;
using System.Collections;
using UnityEditor;

#if UNITY_EDITOR

[CustomEditor (typeof (WayPointHolder))]
public class WayPointHolderEditor : Editor {

    public override void OnInspectorGUI(){
        WayPointHolder holder = target as WayPointHolder;

        if (GUILayout.Button("Switch Graphics")){
            holder.SwitchGraphics ();
        }
        
        if (GUILayout.Button("Apply Graphics")){
            holder.ApplyLocalScaleToGraphics ();
        }
    }
}

#endif
