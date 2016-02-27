using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utility {

    public static Vector3 CalculateDirection(Star a, Star b){
        return (b.transform.position - a.transform.position).normalized;
    }

    public static Vector3 CalculateDirection(Vector3 a, Vector3 b){
        return (b - a).normalized;
    }
    
    public static void PrintPath(Stack<Star> path, string message){
        string debugMes = "[" + message + "] - Path : ";
        
        Star[] pathArray = path.ToArray ();
        
        if (pathArray.Length > 0) {
            if (pathArray.Length > 2) {
                for (int i = pathArray.Length - 1; i > 0; i--) {
                    debugMes += pathArray [i].name + " -> ";
                }
            }
            
            debugMes += pathArray [0];
        }
        
        Debug.Log (debugMes);
    }

}