using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterMover))]
public class PathFinder : MonoBehaviour {

    public const int FIXED_DST = 1;
    
    Dictionary<WayPoint, Dictionary<WayPoint, int>> graph = new Dictionary<WayPoint, Dictionary<WayPoint, int>> ();
    List<WayPoint> nodesCollector = new List<WayPoint> ();
    
    CharacterMover mover;
    
    void Awake(){
        mover = GetComponent<CharacterMover> ();
    }
    
    public bool CheckAndFixPath (Queue<WayPoint> path, out Queue<WayPoint> rePath){
        WayPoint standingOnWayPoint;
        if (mover.GetWayPointNearestFeet (out standingOnWayPoint)) {
            var nodes = FindReachableNodesFrom (standingOnWayPoint);
            CreateGraph (nodes);
            
            if (!CanTravelTo (FindDestination(path))) {
                rePath = ReconstructPath (path);
                return true;
            }
        }
        
        rePath = null;
        return false;
    }
    
    public bool ShortestPath (WayPoint startWayPoint, WayPoint destinationWayPoint, out Queue<WayPoint> path){
        var nodes = FindReachableNodesFrom (startWayPoint);
        CreateGraph (nodes);
        
        if (CanTravelTo(destinationWayPoint)) {
            var pathList = Dijkstra (startWayPoint, destinationWayPoint);
            
            pathList.Add (startWayPoint);
            pathList.Reverse ();
            
            path = new Queue<WayPoint> (pathList);
            return true;
        }
        
        path = null;
        return false;
    }
    
    WayPoint FindDestination(Queue<WayPoint> path){
//        print ("FindDestination() => " + path.ToArray () [path.Count - 1].name);
        return path.ToArray () [path.Count - 1];
    }

    Queue<WayPoint> ReconstructPath(Queue<WayPoint> path){
        print ("Start ReconstructPath, path count = " + path.Count);
        var rePath = new Queue<WayPoint> ();
//        rePath.Enqueue (path.Dequeue ());
            
        foreach (WayPoint node in path) {
            if (!CanTravelTo (node)) {
                print ("Finish ReconstructPath, path count = " + path.Count);
                break;
            }
            
            rePath.Enqueue (node);
        }
        
        return rePath;
    }

    bool CanTravelTo(WayPoint destination){
        return graph.ContainsKey (destination);
    }
    
    WayPoint[] FindReachableNodesFrom(WayPoint mother){
        nodesCollector.Clear ();
        
        nodesCollector.Add (mother);
        RecursionSearchAllReachableNodes (mother, nodesCollector);
        
        return nodesCollector.ToArray ();
    }
    
    void RecursionSearchAllReachableNodes(WayPoint motherNode, List<WayPoint> collector){
        if (collector == null || motherNode == null) {
            Debug.LogError ("FindConnectedNode(null?, null?) has some null-parameter");
            return;
        }

        for (int i = 0; i < motherNode.neighborInfo.Length; i++) {
            WayPoint child = motherNode.neighborInfo [i].wayPoint;

            if (!collector.Contains (child)) {
                collector.Add (child);
                RecursionSearchAllReachableNodes (child, collector);
            }
        }
        
        return;
    }
    
    void CreateGraph(WayPoint[] nodes){
        graph.Clear ();
        for (int i = 0; i < nodes.Length; i++) {
            Dictionary<WayPoint, int> connectedNodesDict = new Dictionary<WayPoint, int> ();

            for (int c = 0; c < nodes [i].neighborInfo.Length; c++) {
                connectedNodesDict.Add (nodes [i].neighborInfo [c].wayPoint, nodes [i].neighborInfo [c].distance);
            }

            graph.Add (nodes [i], connectedNodesDict);
        }
    }
   
    List<WayPoint> Dijkstra(WayPoint startNode, WayPoint destinationNode){
        var previous = new Dictionary<WayPoint, WayPoint>();
        var distances = new Dictionary<WayPoint, int>();
        var nodes = new List<WayPoint> ();

        List<WayPoint> path = null;

        foreach (var vertex in graph)
        {
            if (vertex.Key == startNode)
            {
                distances[vertex.Key] = 0;
            }
            else
            {
                distances[vertex.Key] = int.MaxValue;
            }

            nodes.Add(vertex.Key);
        }

        while (nodes.Count != 0){
            nodes.Sort((x, y) => distances[x] - distances[y]);

            var smallest = nodes[0];
            nodes.Remove(smallest);

            if (smallest == destinationNode){
                path = new List<WayPoint>();
                
                while (previous.ContainsKey(smallest)){
                    path.Add(smallest);
                    smallest = previous[smallest];
                }

                break;
            }

            if (distances[smallest] == int.MaxValue){
                break;
            }

            foreach (var neighbor in graph[smallest]){
                var alt = distances[smallest] + neighbor.Value;
                if (alt < distances[neighbor.Key]){
                    distances[neighbor.Key] = alt;
                    previous[neighbor.Key] = smallest;
                }
            }
        }
        
        return  path;
    }
}
