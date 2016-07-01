using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Constellations : EventGraph {
        
    public enum ConstellationsState
    {
        Idle,
        Rest,
        Travel,
        Activated
    }
    
//    public NodeMove<Star> starMover;
    public StarMover starMover;
    public Connection[] connections;
    public float connectionsWidth;
    public Transform connectionsHolder;

    [Header("Star Materials")]
    public Material awakenStarMaterials;
    public Material unawakenStarMaterials;
//    public Material startStarMaterial;

    [Header("Line Materials")]
    public Material activatedConnectionMaterial;
    public Material deactivatedConnectionMaterial;
    
    public Material visitablePathMaterial;
    public Material unvisitablePathMaterial;
    public Material doubleLinePathMaerial;
    
    Star[] childrentStars;

    Stack<Star> path = new Stack<Star> ();
    
    ConstellationsState curState = ConstellationsState.Idle;
    ConstellationsState prevState;
    
    TravelInfo travelInfo;
    
    Dictionary<Star, List<Star>> neighborDict = new Dictionary<Star, List<Star>> ();
    Dictionary<Star, Dictionary<Star, Connection>> connectionDict = new Dictionary<Star, Dictionary<Star, Connection>>();

    Dictionary<Star, MeshRenderer> starRendererDict = new Dictionary<Star, MeshRenderer> ();
    
//    Star firstStar;
    Star currentStar;
   
    float threshold = .75f;
    
    public event System.Action<Constellations> OnActivate;
    public event System.Action<Constellations> OnDeactivate;
    
    public bool generateOnAwake = true;
    private bool isAwaken;
    
    public bool activated{ get{ return curState == ConstellationsState.Activated; } }
    
    void Awake(){
        LoadChildrentStar ();
        
        if (generateOnAwake) {
            GenerateConnections ();
            Deactivate ();
            
            isAwaken = true;
        }
        else {
            SetChildrentsStarMaterial (unawakenStarMaterials);
        }
    }
    
    public void ForceAwake(bool _activated = false){
        isAwaken = true;
        
        GenerateConnections ();
        
        if (_activated) {
            Activate ();
        }
        else{
            Deactivate ();
        }
    }
    
    void LoadChildrentStar(){
        var children = new List<Star> ();
        foreach (Star child in GetComponentsInChildren<Star> ()) {
            if (!starMover.IsMover (child)) {
                children.Add (child);
            }
        }
        
        childrentStars = new Star[children.Count];
        children.CopyTo (childrentStars);
    }
    
//    void MarkStarWhereTravelFrom(Star hightligtStar){
//        if (hightligtStar != null) {
//            if (!starRendererDict.ContainsKey (hightligtStar)) {
//                starRendererDict.Add (hightligtStar, firstStar.GetComponent<MeshRenderer> ());
//            }
//            starRendererDict [hightligtStar].material = startStarMaterial;
//        }
//    }
    
    void SetChildrentsStarMaterial(Material targetMat){
        for (int i = 0; i < childrentStars.Length; i++) {
            if (childrentStars [i] != null) {
                if (!starRendererDict.ContainsKey (childrentStars [i])) {
                    starRendererDict.Add (childrentStars [i], childrentStars [i].GetComponent<MeshRenderer> ());
                }

                starRendererDict [childrentStars [i]].material = targetMat;
            }
        }
        
    }
    
    public override void CancelContact(){
        if (curState == ConstellationsState.Rest && path.Count == 1 && !travelInfo.isSuccess) {
            Deactivate ();
        }
    }
    
    public override void MakeContact(EventPoint fromStar, Vector3 contractPosition){
        if (curState == ConstellationsState.Idle) {
            StartContact (fromStar as Star);
        }
        
        if (starMover.IsMover(fromStar as Star)) {
            if (curState == ConstellationsState.Rest) {
                SetDestination (contractPosition);
            }
            
            if (curState == ConstellationsState.Travel) {
                MoveConnector (contractPosition);
            }
        }
    }
    
    void ChangeState(ConstellationsState targetState){
        prevState = curState;
        curState = targetState;
    }
    
    void StartContact(Star _firstStar){
//        firstStar = _firstStar;
        AddNewVisitedPath (_firstStar);
        
        starMover.SetActive (true);
        starMover.SetMoverPosition (_firstStar.transform.position, TravelInfo.nothing);
        
        SetAllConnectionsLineMaterialInteractiveMode ();
        
        ChangeState (ConstellationsState.Rest);
    }
    
    void SetDestination(Vector3 contactPosition){   
        if ((prevState == ConstellationsState.Idle)) {
            currentStar = path.Peek ();
        }
        else if (travelInfo.isSuccess) {
            currentStar = travelInfo.destination;
        }
        else {
            currentStar = travelInfo.start;
        }
        
        Star destination = SelectDestinationStart (currentStar, contactPosition);
        var trip = new TravelInfo (currentStar, destination);
        
        float moveDst = 0f;// = CheckOverlap (currentStar, destination) ? 0f : CalculateMoveDistance (trip, contactPosition);
        
        // ... Handle with Double's Type-of-Line 
        if (connectionDict [currentStar] [destination].typeOfLine == TypeOfLine.Double) {
            print ("Double, " + currentStar.name + " and " + destination.name);
            int visitTime;
            if (CanPassDoubleVisitablePath (currentStar, destination, out visitTime)) {
                print ("CanPassDoubleVisitablePath");
                if (travelInfo.isSuccess || prevState == ConstellationsState.Idle) {
                    print ("Success");
                    moveDst = CalculateMoveDistance (trip, contactPosition);
                    starMover.SetMaterial (visitTime == 0 ? visitablePathMaterial : unvisitablePathMaterial);
                }
                else {
                    print ("Not Success");
                    trip = new TravelInfo (destination, currentStar);
                    moveDst = CalculateMoveDistance (trip, contactPosition);

                    travelInfo = trip;
                    starMover.SetMoverPosition (trip.MovedPosition (moveDst), trip);
//                    starMover.LineMaterial = doubleLinePathMaerial;
                    starMover.SetMaterial (visitablePathMaterial);
                    
                    RemovePreviousPath (doubleLinePathMaerial);
                    ChangeState (ConstellationsState.Travel);
                    
                    return;
                }

            }
            else if(CheckIsPreviousPath(destination)) {
                print ("None CanPassDoubleVisitablePath");
                trip = new TravelInfo (destination, currentStar);
                moveDst = CalculateMoveDistance (trip, contactPosition);

                travelInfo = trip;
                starMover.SetMoverPosition (trip.MovedPosition (moveDst), trip);
                starMover.SetMaterial (unvisitablePathMaterial);

                RemovePreviousPath (visitablePathMaterial);
                ChangeState (ConstellationsState.Travel);

                return;
            }
            
        }
        // ... Handle with Single's Type-of-Line 
        else {
            if (CheckIsPreviousPath (destination)) {
                trip = new TravelInfo (destination, currentStar);
                moveDst = CalculateMoveDistance (trip, contactPosition);

                travelInfo = trip;
                starMover.SetMoverPosition (trip.MovedPosition (moveDst), trip);

                starMover.SetMaterial (unvisitablePathMaterial);
                RemovePreviousPath (visitablePathMaterial);
                
                ChangeState (ConstellationsState.Travel);
                print ("case : CheckIsRemovablePath()");

                return;
            }
            else if (!CheckOverlap (currentStar, destination)) {
                moveDst = CalculateMoveDistance (trip, contactPosition);
                starMover.SetMaterial (unvisitablePathMaterial);
                print ("case : !CheckOverlap()");
            }
        }
        
        if (moveDst >= threshold) {
            travelInfo = trip;
            ChangeState (ConstellationsState.Travel);
        }

        starMover.SetMoverPosition (trip.MovedPosition (moveDst), trip);
    }
    
    void MoveConnector(Vector3 contactPosition){
        float moveDst = CalculateMoveDistance (travelInfo, contactPosition);
        
        if (moveDst >= travelInfo.distance) {
            travelInfo.Success ();

            AddNewVisitedPath (travelInfo.destination, starMover.LineMaterial);
            ChangeState (ConstellationsState.Rest);
            
            if (CheckAnswer ()) {
                Activate ();
            }
        }
        else if(moveDst < threshold){
            ChangeState (ConstellationsState.Rest);
        }
        
        starMover.SetMoverPosition (travelInfo.MovedPosition (moveDst), travelInfo);
    }
    
    float CalculateMoveDistance(TravelInfo trip, Vector3 contactPosition){
        float toContactDst = trip.DistanceTo (contactPosition);
        Vector3 toContactDir = trip.DirectionTo(contactPosition);
        float angle = Vector3.Angle (trip.direction, toContactDir);

        return toContactDst * Mathf.Cos (angle * Mathf.Deg2Rad);
    }
    
    Star SelectDestinationStart(Star startStar, Vector3 contactPosition){
        Vector3 startToContact =  Utility.CalculateDirection (contactPosition, startStar.transform.position);//(contactPosition - startStar.transform.position).normalized;
        Star[] allConnectedStar = FindAllConnectedStarFrom (startStar);
        
        Vector3 startToConnected = Utility.CalculateDirection (allConnectedStar [0], startStar);
        float closestAngle = Vector3.Angle (startToContact, startToConnected);
        Star closestStar = allConnectedStar [0];
       
        for (int i = 1; i < allConnectedStar.Length; i++) {
            startToConnected = Utility.CalculateDirection (allConnectedStar [i], startStar);
            float angle = Vector3.Angle (startToContact, startToConnected);
            
            if (angle < closestAngle) {
                closestAngle = angle;
                closestStar = allConnectedStar [i];
            }
        }
        
        return closestStar;
    }
    
    Star[] FindAllConnectedStarFrom(Star targetStar){
        return neighborDict [targetStar].ToArray();
    } 
    
    void AddNewVisitedPath(Star nextStar, Material targetMat = null){
        path.Push (nextStar);
        Utility.PrintPath (path, "AddNewVisitedPath()");
        
        Star[] pathArray = path.ToArray ();
        if (pathArray.Length >= 2) {
            connectionDict [pathArray [0]] [pathArray [1]].lineRenderer.material = targetMat;
            SetChildrentsStarMaterial (awakenStarMaterials);
        }
        
//        MarkStarWhereTravelFrom (nextStar);
    }
    
    void RemovePreviousPath(Material targetMat){
        Star[] pathArray = path.ToArray ();
        
        if (pathArray.Length >= 2) {
            connectionDict [pathArray [0]] [pathArray [1]].lineRenderer.material = targetMat;
            
        }
        
        path.Pop ();
        
        Utility.PrintPath (path, "After RemovePreviousPath()");
        
        SetChildrentsStarMaterial (awakenStarMaterials);
//        MarkStarWhereTravelFrom (path.Peek ());
    }
    
    bool CheckIsPreviousPath(Star currentStar){
        Star[] pathArray = path.ToArray ();
        return pathArray.Length > 1 && pathArray [1] == currentStar;
    }
    
    bool CheckOverlap(Star fromStar, Star toStar){
        Star[] pathArray = path.ToArray ();
        
        for (int i = 1; i < pathArray.Length; i++) {
            if ((pathArray [i] == fromStar && pathArray [i - 1] == toStar) || (pathArray [i] == toStar && pathArray [i - 1] == fromStar)) {
                return true;
            }
        }
        
        return false;
    }
    
    bool CheckAnswer(){
        for (int i = 0; i < connections.Length; i++) {
            if (!CheckWasPathVisited (connections [i].a, connections [i].b)) {
                return false;
            }
        }
        
        return true;
    }
    
    bool CanPassDoubleVisitablePath (Star a, Star b, out int visitTime){
        int numberOfVisit; 
        visitTime = CheckWasPathVisited (a, b, out numberOfVisit) ? numberOfVisit : 0;
        
        return visitTime < 2;
    }
    
    bool CheckWasPathVisited(Star a, Star b, out int totalVisit){
        Star[] pathArray = path.ToArray ();
        totalVisit = 0;
        
        for (int i = 1; i < pathArray.Length; i++) {
            bool hasPathAB = pathArray [i] == b && pathArray [i - 1] == a;
            bool hasPathBA = pathArray [i] == a && pathArray [i - 1] == b;

            if (hasPathAB || hasPathBA) {
                totalVisit++;
            }
        }

        return totalVisit > 0;
    }
    
    bool CheckWasPathVisited(Star a, Star b){
        Star[] pathArray = path.ToArray ();
        
        for (int i = 1; i < pathArray.Length; i++) {
            bool hasPathAB = pathArray [i] == b && pathArray [i - 1] == a;
            bool hasPathBA = pathArray [i] == a && pathArray [i - 1] == b;
            
            if (hasPathAB || hasPathBA) {
                return true;
            }
        }
        
        return false;
    }
    
    public void Activate(){
        SetAllConnectionsLineMaterial (activatedConnectionMaterial);
    
        starMover.SetActive (false);
        ChangeState (ConstellationsState.Activated);
        
        SetChildrentsStarMaterial (awakenStarMaterials);
        
        if (OnActivate != null) {
            OnActivate (this);
        }
    }

    public void Deactivate(){ 
//        firstStar = null;
        
        ChangeState (ConstellationsState.Idle);
        
        path.Clear ();
        starMover.SetActive (false);
        
        SetAllConnectionsLineMaterial (deactivatedConnectionMaterial);
        SetChildrentsStarMaterial (awakenStarMaterials);
        
        if (OnDeactivate != null) {
            OnDeactivate (this);
        }
    }
    
    void SetAllConnectionsLineMaterialInteractiveMode(){
        for (int i = 0; i < connections.Length; i++) {
            bool isSingle = connections [i].typeOfLine == TypeOfLine.Single;
            connections [i].lineRenderer.material = isSingle ? visitablePathMaterial : doubleLinePathMaerial;
        }
    }
    
    void SetAllConnectionsLineMaterial(Material targetMat){
        for (int i = 0; i < connections.Length; i++) {
            connections [i].lineRenderer.material = targetMat;
        }
    }
    
    public void GenerateConnections(){
        var children = new List<GameObject>();
        foreach (Transform child in connectionsHolder) children.Add(child.gameObject);
        
        if (Application.isEditor) {
            children.ForEach(child => DestroyImmediate(child));
        }
        else {
            children.ForEach(child => Destroy(child));
        }
        
        // Setting Connections
        neighborDict.Clear ();
        connectionDict.Clear ();
        
        for (int i = 0; i < connections.Length; i++) {
            if (connections [i].a == null || connections [i].b == null) {
                continue;
            }
            
//            GameObject connection = new GameObject ("Connection");
//            LineRenderer line = connection.AddComponent<LineRenderer> ();
//            line.material = connections [i].typeOfLine == TypeOfLine.Double ? doubleLinePathMaerial : visitablePathMaterial;
//            line.SetWidth (connectionsWidth, connectionsWidth);
//            line.SetVertexCount (2);
//            line.SetPosition (0, connections [i].a.transform.position);
//            line.SetPosition (1, connections [i].b.transform.position);
//            
            
            GameObject connection = new GameObject ("Line GameObject");
            connection.transform.parent = connectionsHolder;
            connection.transform.localRotation = Quaternion.identity;
            connection.transform.localScale = Vector3.one;
            connection.transform.localPosition = Vector3.zero;

            LineRenderer line = connection.AddComponent<LineRenderer> ();
            line.useWorldSpace = false;
            line.material = deactivatedConnectionMaterial;
            line.SetWidth (connectionsWidth, connectionsWidth);
            line.SetVertexCount (2);
            line.SetPosition (0, line.transform.InverseTransformPoint(connections [i].a.transform.position));
            line.SetPosition (1, line.transform.InverseTransformPoint(connections [i].b.transform.position));

            connections [i].lineRenderer = line;
            
            // ... Set-Up Neighbor Dictionary
            if (!neighborDict.ContainsKey (connections [i].a)) {
                neighborDict.Add (connections [i].a, new List<Star> ());
            }   
            neighborDict [connections [i].a].Add (connections [i].b);
            
            if (!neighborDict.ContainsKey (connections [i].b)) {
                neighborDict.Add (connections [i].b, new List<Star> ());
            }
            neighborDict [connections [i].b].Add (connections [i].a);
            
            // ... Set-Up Connections Dictionary
            if (!connectionDict.ContainsKey (connections [i].a)) {
                connectionDict.Add (connections [i].a, new Dictionary<Star, Connection> ());
            }
            connectionDict [connections [i].a].Add (connections [i].b, connections [i]);
            
            if (!connectionDict.ContainsKey (connections [i].b)) {
                connectionDict.Add (connections [i].b, new Dictionary<Star, Connection> ());
            }
            connectionDict [connections [i].b].Add (connections [i].a, connections [i]);
            
            connection.transform.parent = connectionsHolder;
        }
        
        // Setting Star Mover
        starMover.line.SetWidth (connectionsWidth, connectionsWidth);
    }
    
    void OnDrawGizmos(){
        for (int i = 0; i < connections.Length; i++) {
            if (connections [i].a == null || connections [i].b == null) {
                continue;
            }
            Gizmos.color = connections [i].typeOfLine == TypeOfLine.Single ? new Color (0, 0, 1, .2f) : new Color (1, 0, 1, .2f);
            Gizmos.DrawLine (connections[i].a.transform.position, connections[i].b.transform.position);
        }
    }
    
    public enum TypeOfLine
    {
        Single,
        Double
    }
    
    [System.Serializable]
    public class Connection{
       
        public Star a;
        public Star b;
        public TypeOfLine typeOfLine;
        [HideInInspector]
        public LineRenderer lineRenderer;
        
    }
    
    [System.Serializable]
    public class StarMover{
//        public Star starMover;
        public Star mover;
        public LineRenderer line;
//        public Transform constellationsTransform;
        
        private TravelInfo tripInfo = TravelInfo.nothing;
        
        public void SetMoverPosition(Vector3 position, TravelInfo _tripInfo){
            mover.transform.position = position;
            tripInfo = _tripInfo;

            Update ();
        }
        
        public Material LineMaterial{
            set{ 
                line.material = value;
            }
            
            get{ 
                return line.material;
            }
        }
        
        public void SetMaterial(Material mat){
            line.material = mat;
        }
        
        public Material GetMaterial(){
            return line.material;
        }
        
        public void SetActive(bool enable){
            mover.gameObject.SetActive (enable);
        }
        
        public bool IsMover(Star target){
            return target == mover;
        }
        
        void Update(){
//            Vector3[] vertise = new Vector3[2];
//            
//            if (tripInfo != TravelInfo.nothing) {
//                Vector3 startToEye = (Camera.main.transform.position - tripInfo.startPosition).normalized;
//                Vector3 moverToEye = (Camera.main.transform.position - mover.transform.position).normalized;
//                
//                vertise [0] = tripInfo.startPosition + startToEye * .25f;
//                vertise [1] = mover.transform.position + moverToEye * .25f;
//            }
//            else {
//                vertise [0] = mover.transform.position;
//                vertise [1] = mover.transform.position;
//            }
//            
//            line.SetVertexCount (2);
//            line.SetPositions (vertise);  
            line.useWorldSpace = false;
            Vector3[] vertise = new Vector3[2];

            if (tripInfo != TravelInfo.nothing) {
                Vector3 startToEye = (Camera.main.transform.position - tripInfo.startPosition).normalized;
                Vector3 moverToEye = (Camera.main.transform.position - mover.transform.position).normalized;
                
                vertise [0] = line.transform.InverseTransformPoint (tripInfo.startPosition + startToEye * .25f);
                vertise [1] = line.transform.InverseTransformPoint (mover.transform.position + moverToEye * .25f);//Vector3.zero + moverToEye * .25f;//line.transform.InverseTransformPoint (mover.transform.position);
            }
            else {
                vertise [0] = mover.transform.position;
                vertise [1] = mover.transform.position;
            }

            line.SetVertexCount (2);
            line.SetPositions (vertise);
        }
        
    }
    
    public struct TravelInfo{
        
        public Star start{ get; private set; }
        public Star destination{ get; private set; }
        
        public float distance{ get; private set; }
        public Vector3 direction{ get; private set; }
//        
        public static TravelInfo nothing{ get{ return _nothing; } }
        private static TravelInfo _nothing = new TravelInfo();
        
        public bool isSuccess{ get ; private set; }
        
        public TravelInfo(Star _start, Star _destination){
            start = _start;   
            destination = _destination; 
            
            isSuccess = false;
            distance = Vector3.Distance(start.transform.position, destination.transform.position);
            direction = (destination.transform.position - start.transform.position).normalized;
        }
        
        public Vector3 startPosition{ get{ return start.transform.position; } }
        
        public void Success(){
            isSuccess = true;
        }
        
        public Vector3 MovedPosition(float moveDst){
            float dst = Mathf.Clamp (moveDst, 0, distance);
            return startPosition + direction * dst;
        }
        
        public float DistanceTo(Vector3 targetPosition){
            return Vector3.Distance (start.transform.position, targetPosition);
        }
        
        public Vector3 DirectionTo(Vector3 targetPosition){
            return (targetPosition - start.transform.position).normalized;
        }
        
        public static bool operator ==(TravelInfo a, TravelInfo b){
            return a.start == b.start && a.destination == b.destination;
        }
        
        public static bool operator !=(TravelInfo a, TravelInfo b){
            return a.start != b.start || a.destination != b.destination;
        }
    }
}