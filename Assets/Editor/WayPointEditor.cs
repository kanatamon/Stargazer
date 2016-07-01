using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;

#if UNITY_EDITOR

[CustomEditor (typeof (WayPoint))]
[CanEditMultipleObjects]
public class WayPointEditor : Editor {

    private WayPoint instance;
    private PropertyField[] fields;
    
    private string[] wayPointLayerName = {
        "Unselectable Waypoint",
        "Selectable Waypoint"
    };
    private bool isEditMode = false;
    
    WayPoint.WayPointConnectionRayChecker editingConnection;
    
    public void OnEnable(){
        instance = target as WayPoint;
        fields = ExposeProperties.GetProperties (instance);
    }
    
    public override void OnInspectorGUI(){
        if (instance == null) {
            return;
        }
        
        if (DrawDefaultInspector()){
            for (int i = 0; i < instance.connectionCheckers.Count; i++) {
                instance.connectionCheckers [i].motherWayPoint = instance;
            }
        }
        
        ExposeProperties.Expose (fields);
        
        if (GUILayout.Button ("Add Connection")) {
            isEditMode = true;
            
            editingConnection = new WayPoint.WayPointConnectionRayChecker ();
            editingConnection.motherWayPoint = instance;
            
            instance.connectionCheckers.Add (editingConnection);
        }
        
        if (GUILayout.Button ("Remove Last Connection")) {
            instance.connectionCheckers.RemoveAt (instance.connectionCheckers.Count - 1);
        }
        
        if (GUILayout.Button ("Remove All Connection")) {
            instance.connectionCheckers.Clear ();
        }
        
        if (GUILayout.Button ("Set As StartingWayPoint")) {
            Character player = FindObjectOfType<Character> ();
            player.startWayPoint = instance;
            
            EditorUtility.SetDirty (player);
        }
        
        EditorUtility.SetDirty (instance);
    }
    
    void OnSceneGUI(){
        if (isEditMode){
            if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) {
                if (Event.current.type == EventType.MouseDown) {
                    bool success = false;
                    Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
                    RaycastHit hitInfo;
                    
                    if (Physics.Raycast (worldRay, out hitInfo, 10000, LayerMask.GetMask (wayPointLayerName))) {
                        WayPoint targetWayPoint = hitInfo.collider.GetComponent<WayPoint> ();
                        success = targetWayPoint != null && targetWayPoint != instance && !DidAHasConnectionToB (instance, targetWayPoint);
                        
                        if (success) {
                            var direction = (targetWayPoint.transform.position - instance.transform.position).normalized;
                            var localEuler = Quaternion.LookRotation (direction, Vector3.up).eulerAngles;
                            
                            float distance = Vector3.Distance (instance.transform.position, targetWayPoint.transform.position);
                            
                            // Configuration the editing connectionChecher
                            editingConnection.localFoward = instance.transform.InverseTransformDirection (direction);
                            editingConnection.distanceRay = distance;
                            
                            // Configuration the targetWayPoint's connection to the instance
                            if (!DidAHasConnectionToB(targetWayPoint, instance)) {
                                var newConnection = new WayPoint.WayPointConnectionRayChecker ();
                                newConnection.motherWayPoint = targetWayPoint;
                                newConnection.localFoward = targetWayPoint.transform.InverseTransformDirection (direction * -1f);
                                newConnection.distanceRay = distance;
                                
                                targetWayPoint.connectionCheckers.Add (newConnection);
                                
                                EditorUtility.SetDirty (targetWayPoint);
                            }
                        }
                    }
                    
                    if (!success) {
                        instance.connectionCheckers.Remove (editingConnection);
                        editingConnection = null;
                    }
                    
                    isEditMode = false;
                    EditorUtility.SetDirty (instance);
                }

                Event.current.Use();
            }
        }
    }
    
    bool DidAHasConnectionToB(WayPoint a, WayPoint b){
        foreach (var connection in a.connectionCheckers) {
            connection.Update ();

            if (connection.connected && connection.connectedWayPoint == b) {
                return true;
            }
        }
        
        return false;
    }
}

#endif
