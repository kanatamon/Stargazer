using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstellationsSwitch : EventGraph {
    
    public enum ConstellationsState
    {
        Idle,
        Rest,
        Travel,
        Activated
    }

    public PointMover pointMover; 
    public Connection[] connections;
    public float connectionsWidth;
    public Transform linesHolder;

    [Header("Node Materials")]
    public Material activatedNodeMaterial;
    public Material deactivatedNodeMaterial;

    [Header("Line Materials")]
    public Material passedPathMaterial;
    public Material unpassedPathMaterial;

//    Pointer[] pointers;
    List<Pointer> pointers = new List<Pointer> (); 

    Stack<Pointer> path = new Stack<Pointer> ();

    ConstellationsState curState = ConstellationsState.Idle;
    ConstellationsState prevState;

    TravelInfo travelInfo;

    Dictionary<Pointer, List<Pointer>> neighborDict = new Dictionary<Pointer, List<Pointer>> ();
    Dictionary<Pointer, Dictionary<Pointer, Connection>> connectionDict = new Dictionary<Pointer, Dictionary<Pointer, Connection>>();

    Dictionary<Pointer, MeshRenderer> rendererDict = new Dictionary<Pointer, MeshRenderer> ();

//    Pointer firstPointer;
    Pointer currentPointer;
    Pointer beginPointer;

    float snappingThreshold = .25f;

    public event System.Action OnActivate;

    public bool activated{ get{ return curState == ConstellationsState.Activated; } }

    void Awake(){
        LoadPointers ();
        GenerateConnections ();
        SetAllPointerMaterial (deactivatedNodeMaterial);
        
        pointMover.SetActive (false);
    }
    
    void Start(){
        SetUpTheFirstBeginablePointer ();
        
        if (beginPointer == null) {
            Debug.LogError (name + " has no a 'beginablePointer'");
        }
    }
    
    void Activate(){
        if (OnActivate != null) {
            OnActivate ();
        }
        
//        pointMover.
        SetAllPointerMaterial (deactivatedNodeMaterial);
    }
    
    bool IsPointerBeginable(Pointer pointer){
        return pointer != null && beginPointer != null && pointer == beginPointer;
    }
    
    void SetUpTheFirstBeginablePointer(){
        foreach (Pointer pointer in pointers) {
            if (pointer.typeOfPointer == Pointer.TypeOfPointer.Circle) {
                beginPointer = pointer;
                beginPointer.activated = true;
                SetPointerMaterial (beginPointer, activatedNodeMaterial, false);
                
                return;
            }
        }
    }
    
    public override void CancelContact(){
        if (curState == ConstellationsState.Rest && path.Count == 1 && !travelInfo.isSuccess) {
            path.Clear ();
            
            pointMover.SetActive (false);
            ChangeState (ConstellationsState.Idle);
        }
    }

    public override void MakeContact(EventPoint fromEventPoint, Vector3 contractPosition){
        Pointer pointer = fromEventPoint as Pointer;
        
        if (curState == ConstellationsState.Idle && IsPointerBeginable(pointer)) {
            StartContact (pointer);
        }

        if (pointMover.IsMover(pointer)) {
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

    void StartContact(Pointer _firstToken){
        AddNewVisitedPath (_firstToken);
        
        travelInfo = TravelInfo.nothing;
        pointMover.SetActive (true);
        pointMover.SetMoverPosition (_firstToken.transform.position, TravelInfo.nothing);

        ChangeState (ConstellationsState.Rest);
    }

    void SetDestination(Vector3 contactPosition){   
        if ((prevState == ConstellationsState.Idle)) {
            currentPointer = path.Peek ();
        }
        else if (travelInfo.isSuccess) {
            currentPointer = travelInfo.destination;
        }
        else {
            currentPointer = travelInfo.start;
        }

        Pointer destination = SelectDestinationFromPointer (currentPointer, contactPosition);
        TravelInfo trip = new TravelInfo (currentPointer, destination);

        float moveDst = 0f;// = CheckOverlap (currentStar, destination) ? 0f : CalculateMoveDistance (trip, contactPosition);

        if (CheckIsPreviousPath (destination)) {
            trip = new TravelInfo (destination, currentPointer);
            moveDst = CalculateMoveDistance (trip, contactPosition);

            travelInfo = trip;
            pointMover.SetMoverPosition (trip.MovedPosition (moveDst), trip);

            pointMover.SetMaterial (passedPathMaterial);
            RemovePreviousPath (unpassedPathMaterial);

            ChangeState (ConstellationsState.Travel);

            return;
        }
        else if (!CheckOverlap (currentPointer, destination)) {
            moveDst = CalculateMoveDistance (trip, contactPosition);
            pointMover.SetMaterial (passedPathMaterial);
        }
        
        if (moveDst >= snappingThreshold) {
            travelInfo = trip;
            ChangeState (ConstellationsState.Travel);
        }

        pointMover.SetMoverPosition (trip.MovedPosition (moveDst), trip);
    }

    void MoveConnector(Vector3 contactPosition){
        float moveDst = CalculateMoveDistance (travelInfo, contactPosition);

        if (moveDst >= travelInfo.distance) {
            travelInfo.Success ();

            AddNewVisitedPath (travelInfo.destination, pointMover.LineMaterial);
            
            var pointerToCheck = travelInfo.destination.activated ? beginPointer : travelInfo.destination;
            
            if (CheckPointerCompletedByType (pointerToCheck)) {
                beginPointer.activated = !travelInfo.destination.activated;
                ActivatePointer (travelInfo.destination);
                
                if (pointerToCheck.typeOfPointer != Pointer.TypeOfPointer.Circle && CheckActivation ()) {
                    Activate ();
                    print ("Actuvated");
                    ChangeState (ConstellationsState.Activated);
                }
                else {
                    ChangeState (ConstellationsState.Idle);
                }
            }
            else {
                ChangeState (ConstellationsState.Rest);
            }
        }
        else if(moveDst < snappingThreshold){
            ChangeState (ConstellationsState.Rest);
        }

        pointMover.SetMoverPosition (travelInfo.MovedPosition (moveDst), travelInfo);
    }
    
    float CalculateMoveDistance(TravelInfo trip, Vector3 contactPosition){
        float toContactDst = trip.DistanceTo (contactPosition);
        Vector3 toContactDir = trip.DirectionTo(contactPosition);
        float angle = Vector3.Angle (trip.direction, toContactDir);

        return toContactDst * Mathf.Cos (angle * Mathf.Deg2Rad);
    }

    Pointer SelectDestinationFromPointer(Pointer startPointer, Vector3 contactPosition){
        Vector3 startToContact =  Utility.CalculateDirection (contactPosition, startPointer.transform.position);//(contactPosition - startStar.transform.position).normalized;
        Pointer[] allConnectedToken = FindAllConnectedStarFrom (startPointer);

        Vector3 startToConnected = Utility.CalculateDirection (allConnectedToken [0], startPointer);
        float closestAngle = Vector3.Angle (startToContact, startToConnected);
        Pointer closestToken = allConnectedToken [0];

        for (int i = 1; i < allConnectedToken.Length; i++) {
            startToConnected = Utility.CalculateDirection (allConnectedToken [i], startPointer);
            float angle = Vector3.Angle (startToContact, startToConnected);

            if (angle < closestAngle) {
                closestAngle = angle;
                closestToken = allConnectedToken [i];
            }
        }

        return closestToken;
    }

    Pointer[] FindAllConnectedStarFrom(Pointer targetToken){
        return neighborDict [targetToken].ToArray();
    } 

    void AddNewVisitedPath(Pointer newPointer, Material targetMat = null){
        path.Push (newPointer);

        Pointer[] pathArray = path.ToArray ();
        if (pathArray.Length >= 2) {
            connectionDict [pathArray [0]] [pathArray [1]].lineRenderer.material = targetMat;
        }

        SetPointerMaterial (newPointer, activatedNodeMaterial, true);
    }

    void RemovePreviousPath(Material targetMat){
        Pointer[] pathArray = path.ToArray ();

        if (pathArray.Length >= 2) {
            connectionDict [pathArray [0]] [pathArray [1]].lineRenderer.material = targetMat;
        }

        Pointer removedPointer = path.Pop ();

        SetPointerMaterial (removedPointer, deactivatedNodeMaterial, true);
    }
    
    void ActivatePointer(Pointer pointer){
        pointer.activated = true;

        pointMover.SetActive (false);

        Pointer[] targets = new Pointer[path.Count];
        path.CopyTo (targets, 0);
        ResetConnectionsMaterials (targets);
        ResetPointersMaterial (targets);

        path.Clear ();

        beginPointer = pointer;
        SetPointerMaterial (pointer, activatedNodeMaterial, false);
    }
    
    bool CheckPointerCompletedByType(Pointer toPointer){
        if (path.Count > 0) {
            switch (toPointer.typeOfPointer) {
            case Pointer.TypeOfPointer.Circle:
                return true;
                break;
            case Pointer.TypeOfPointer.Triangle:
                return path.Count - 1 == 3;
                break;
            case Pointer.TypeOfPointer.Square:
                return path.Count - 1 == 4;
                break;
            default:
                break;
            }
        }
        
        return false;
    }
    
    bool CheckActivation(){
        foreach (var pointer in pointers) {
            if (pointer.typeOfPointer != Pointer.TypeOfPointer.Normal && !pointer.activated) {
                return false;
            }
        }
        
        return true;
    }

    bool CheckIsPreviousPath(Pointer _currentToken){
        Pointer[] pathArray = path.ToArray ();
        return pathArray.Length > 1 && pathArray [1] == _currentToken;
    }

    bool CheckOverlap(Pointer fromToken, Pointer toToken){
        Pointer[] pathArray = path.ToArray ();

        for (int i = 1; i < pathArray.Length; i++) {
            if ((pathArray [i] == fromToken && pathArray [i - 1] == toToken) || (pathArray [i] == toToken && pathArray [i - 1] == fromToken)) {
                return true;
            }
        }

        return false;
    }

    bool CheckWasPathVisited(Pointer a, Pointer b){
        Pointer[] pathArray = path.ToArray ();

        for (int i = 1; i < pathArray.Length; i++) {
            bool hasPathAB = pathArray [i] == b && pathArray [i - 1] == a;
            bool hasPathBA = pathArray [i] == a && pathArray [i - 1] == b;

            if (hasPathAB || hasPathBA) {
                return true;
            }
        }

        return false;
    }
    
    void LoadPointers(){
        pointers.Clear ();
        
        foreach (Pointer child in GetComponentsInChildren<Pointer> ()) {
            if (!pointMover.IsMover (child)) {
                pointers.Add (child);
            }
        }
    }

    void ResetConnectionsMaterials(Pointer[] targets){
        if (targets.Length > 0) {
            for (int i = 1; i < targets.Length; i++) {
                Pointer a = targets [i - 1];
                Pointer b = targets [i];

                connectionDict [a] [b].lineRenderer.material = unpassedPathMaterial;
            }
        }
    }
    
    void ResetPointersMaterial(Pointer[] targets){
        for (int i = 0; i < targets.Length; i++) {
            SetPointerMaterial (targets [i], deactivatedNodeMaterial, true);
        }
    }
    
    void SetAllPointerMaterial(Material targetMat){
        for (int i = 0; i < pointers.Count; i++) {
            if (pointers [i] != null) {
                if (!rendererDict.ContainsKey (pointers [i])) {
                    rendererDict.Add (pointers [i], pointers [i].GetComponentInChildren<MeshRenderer> ());
                }

                rendererDict [pointers [i]].material = targetMat;
            }
        }
    }

    void SetPointerMaterial(Pointer pointer, Material targetMat, bool isAnimated = false){
        if (pointer != null) {
            if (!rendererDict.ContainsKey (pointer)) {
                rendererDict.Add (pointer, pointer.GetComponentInChildren<MeshRenderer> ());
            }

            if (isAnimated) {
                pointer.AnimateMaterial (rendererDict [pointer].material, targetMat, 1f);
            }
            else {
                rendererDict [pointer].material = targetMat;
            }
        }
    }

    public void GenerateConnections(){
        var children = new List<GameObject>();
        foreach (Transform child in linesHolder) children.Add(child.gameObject);

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

            GameObject lineGameObj = new GameObject ("Line GameObject");
            lineGameObj.transform.parent = linesHolder;
            lineGameObj.transform.localRotation = Quaternion.identity;
            lineGameObj.transform.localScale = Vector3.one;
            lineGameObj.transform.localPosition = Vector3.zero;

            LineRenderer line = lineGameObj.AddComponent<LineRenderer> ();
            line.useWorldSpace = false;
            line.material = unpassedPathMaterial;
            line.SetWidth (connectionsWidth, connectionsWidth);
            line.SetVertexCount (2);
            line.SetPosition (0, line.transform.InverseTransformPoint(connections [i].a.transform.position));
            line.SetPosition (1, line.transform.InverseTransformPoint(connections [i].b.transform.position));

            connections [i].lineRenderer = line;

            // ... Set-Up Neighbor Dictionary
            if (!neighborDict.ContainsKey (connections [i].a)) {
                neighborDict.Add (connections [i].a, new List<Pointer> ());
            }   
            neighborDict [connections [i].a].Add (connections [i].b);

            if (!neighborDict.ContainsKey (connections [i].b)) {
                neighborDict.Add (connections [i].b, new List<Pointer> ());
            }
            neighborDict [connections [i].b].Add (connections [i].a);

            // ... Set-Up Connections Dictionary
            if (!connectionDict.ContainsKey (connections [i].a)) {
                connectionDict.Add (connections [i].a, new Dictionary<Pointer, Connection> ());
            }
            connectionDict [connections [i].a].Add (connections [i].b, connections [i]);

            if (!connectionDict.ContainsKey (connections [i].b)) {
                connectionDict.Add (connections [i].b, new Dictionary<Pointer, Connection> ());
            }
            connectionDict [connections [i].b].Add (connections [i].a, connections [i]);
        }

        // Setting Star Mover
        pointMover.line.SetWidth (connectionsWidth, connectionsWidth);
    }
    
    public int pathCount{
        get{ 
            return path.Count;
        }
    }

    void OnDrawGizmos(){
        for (int i = 0; i < connections.Length; i++) {
            if (connections [i].a == null || connections [i].b == null) {
                continue;
            }
            
            Gizmos.color = new Color (0, 0, 1, .2f);
            Gizmos.DrawLine (connections[i].a.transform.position, connections[i].b.transform.position);
        }
    }

    [System.Serializable]
    public class Connection{

        public Pointer a;
        public Pointer b;
//        public TypeOfLine typeOfLine;
        [HideInInspector]
        public LineRenderer lineRenderer;

    }

    [System.Serializable]
    public class PointMover{
        //        public Star starMover;
        public Pointer mover;
        public LineRenderer line;
//        public Transform surfaceTransform;

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

        public bool IsMover(Pointer target){
            return target == mover;
        }

        void Update(){
            line.useWorldSpace = false;
            Vector3[] vertise = new Vector3[2];

            if (tripInfo != TravelInfo.nothing) {
                vertise [0] = line.transform.InverseTransformPoint (tripInfo.startPosition);
                vertise [1] = Vector3.zero;//line.transform.InverseTransformPoint (mover.transform.position);
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

        public Pointer start{ get; private set; }
        public Pointer destination{ get; private set; }

        public float distance{ get; private set; }
        public Vector3 direction{ get; private set; }
        //        
        public static TravelInfo nothing{ get{ return _nothing; } }
        private static TravelInfo _nothing = new TravelInfo();

        public bool isSuccess{ get ; private set; }

        public TravelInfo(Pointer _start, Pointer _destination){
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
