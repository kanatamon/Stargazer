using UnityEngine;
using System.Collections;

public class Pointer : EventPoint {
    
    public enum TypeOfPointer
    {
        Normal,
        Circle,
        Triangle,
        Square
    }
    
    public TypeOfPointer typeOfPointer;
    public Material activatedLineMaterials;
    public float lineWidth = .2f;
    public Vector3 centre;
    
    [Header("Circle")]
    public float radius;
    public int numberOfVertex;
    
    [Header("Reactangle")]
    public float scale = 1;
    public Vector3[] vertices;

    public ConstellationsSwitch constellationsSwitch{ get; private set; }
    
    LineRenderer lineRenderer;
    MeshRenderer renderer;
    bool isAnimating;
    
    Light light;
    bool animatingLight;
    
    private bool _activated = false;
    
    public bool activated{
        get{ 
            return _activated;
        }
        
        set{ 
            if (lineRenderer != null) {
                lineRenderer.enabled = value;
            }
            
            _activated = value;
        }
    }
        
    public override void Awake (){
        base.Awake ();
        constellationsSwitch = GetComponentInParent <ConstellationsSwitch> ();
        renderer = GetComponentInChildren<MeshRenderer> ();
        
        constellationsSwitch.OnActivate += OnSwitchActivate;
        
        light = GetComponentInChildren<Light> ();
        if (light != null) {
            light.intensity = Random.Range (5f, 8f);
            light.range = Random.Range (0f, .34f);
            light.bounceIntensity = 0f;
            light.color = Color.white;
            light.enabled = false;
        }
        animatingLight = false;
        
        if (typeOfPointer != TypeOfPointer.Normal) {
            lineRenderer = gameObject.AddComponent<LineRenderer> ();
            
            lineRenderer.useWorldSpace = false;
            lineRenderer.material = activatedLineMaterials;
            lineRenderer.SetWidth (lineWidth, lineWidth);
            
            Vector3[] preLineVertices = CalculatePreLineVertices ();
            lineRenderer.SetVertexCount (preLineVertices.Length);
            lineRenderer.SetPositions (preLineVertices);
            
            lineRenderer.enabled = false;
        }
    }
    
    void Update(){
        if (animatingLight) {
            light.intensity = Mathf.PingPong (Time.time * .1f, 3f) + 5f;
            light.range = Mathf.PingPong (Time.time * .025f, .1f) + .3f;
        }
    }
    
    void OnSwitchActivate(){
        if (light != null) {
            animatingLight = true;
            light.enabled = true;
            print ("Switch Activated light");    
        }
        print ("Switch Activated");
    }
    
    Vector3[] CalculatePreLineVertices(){
        Vector3[] preLineVertices = new Vector3[0];
        
        if (typeOfPointer != TypeOfPointer.Normal) {
            // .. Calculate Circle Vertices
            if (typeOfPointer == TypeOfPointer.Circle) {
                if (numberOfVertex > 2) {
                    float x;
                    float y;
                
                    preLineVertices = new Vector3[numberOfVertex + 1];
                    float step = Mathf.PI * 2 / numberOfVertex;
                
                    for (int i = 0; i < numberOfVertex; i++) {
                        x = centre.x + radius * Mathf.Sin (i * step);
                        y = centre.y + radius * Mathf.Cos (i * step);
                    
                        preLineVertices [i] = new Vector3 (x, y, 0f);
                    }
                
                    preLineVertices [preLineVertices.Length - 1] = preLineVertices [0];
                }
            }
            // .. Calculate Polygon Vertices
            else {
                if (vertices.Length > 2) {
                    preLineVertices = new Vector3[vertices.Length + 1];
                    
                    for (int i = 0; i < vertices.Length; i++) {
                        preLineVertices [i] = vertices [i] * scale + centre;
                    }
                    
                    preLineVertices [preLineVertices.Length - 1] = vertices [0] * scale + centre;
                }
            }
        }
        
        return preLineVertices;
    }

    public override void CancelContactConstellations(){
        constellationsSwitch.CancelContact ();
    }

    public override void ContactConstellations(Vector3 position){
        constellationsSwitch.MakeContact (this, position);
//        print ("Contact");
    }

    public void AnimateMaterial(Material matA, Material matB, float time){
//        if (isAnimating) {
//            StopCoroutine ("Animate");
//        }
        
//        StartCoroutine (Animate (matA, matB, time));
        renderer.material = matB;
    }
    
    IEnumerator AnimateShining(Material matA, Material matB, float time){
        isAnimating = true;
        
        float percent = 0;
        
//        renderer.material = matB;
        
        while (percent <= 1) {
            percent += Time.deltaTime / 10;
//            renderer.material.Lerp (matA, matB, percent);
//            renderer.material.color = Color.Lerp (matA.color, matB.color, percent);
//            renderer.material.set
//            renderer.material.SetFloat( "_Blend", percent ); // http://forum.unity3d.com/threads/cant-get-material-lerp-to-work.8936/
            yield return null;
        }
        
        isAnimating = false;
    }
    
    public override Vector3 constellationsForward {
        get {
            return constellationsSwitch.transform.forward;
        }
    }
    
    void OnDrawGizmos(){
        Gizmos.color = Color.cyan;
        
        switch (typeOfPointer) {
        case TypeOfPointer.Circle:
            if (numberOfVertex > 2) {
                Vector3 pointA;
                Vector3 pointB;
                
                Vector3[] preVertices = CalculatePreLineVertices ();
                
                for (int i = 1; i < preVertices.Length; i++) {
                    pointA = transform.TransformPoint (preVertices [i - 1]);
                    pointB = transform.TransformPoint (preVertices [i]);
                    Gizmos.DrawLine (pointA, pointB);
                }
                
                pointA = transform.TransformPoint (preVertices [0]);
                pointB = transform.TransformPoint (preVertices [preVertices.Length - 1]);
                Gizmos.DrawLine (pointA, pointB);
            }
            else {
                Debug.Log ("[ " + name + " ] is TypeOfPointer.Begin, Need more 'numberOfVertex', the minimum is 3");
            }
            break;
        case TypeOfPointer.Triangle:
            if (vertices.Length == 3) {
                Vector3 pointA;
                Vector3 pointB;
                
                for (int i = 1; i < vertices.Length; i++) {
                    pointA = transform.TransformPoint (vertices [i - 1] * scale + centre);
                    pointB = transform.TransformPoint (vertices [i] * scale + centre);
                    Gizmos.DrawLine (pointA, pointB);
                }
                
                pointA = transform.TransformPoint (vertices [vertices.Length - 1] * scale + centre);
                pointB = transform.TransformPoint (vertices [0] * scale + centre);
                Gizmos.DrawLine (pointA, pointB);
            }
            else {
                Debug.Log ("[ " + name + " ] is TypeOfPointer.Triangle but vertices count is not equal to 3");
            }
            break;
        case TypeOfPointer.Square:
            if (vertices.Length == 4) {
                Vector3 pointA;
                Vector3 pointB;
                
                for (int i = 1; i < vertices.Length; i++) {
                    pointA = transform.TransformPoint (vertices [i - 1] * scale + centre);
                    pointB = transform.TransformPoint (vertices [i] * scale + centre);
                    Gizmos.DrawLine (pointA, pointB);
                }

                pointA = transform.TransformPoint (vertices [vertices.Length - 1] * scale + centre);
                pointB = transform.TransformPoint (vertices [0] * scale + centre);
                Gizmos.DrawLine (pointA, pointB);
            }
            else {
                Debug.Log ("[ " + name + " ] is TypeOfPointer.Rectangle but vertices count is not equal to 4");
            }
            break;
        default:
            break;
        }
    }
    
}
