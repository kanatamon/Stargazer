using UnityEngine;
using System.Collections;

public class WheelSwitchController : MonoBehaviour {
    
    public LayerMask mechanicMask;
    
    ContactInfo contactInfo;
    ContactInfo prevContactInfo;
    
    void Start(){
        prevContactInfo = ContactInfo.nothig;
    }
    
    void Update(){
        if (Input.GetMouseButton (0)) {
            if (Raycast (out contactInfo)) {
                contactInfo.MakeContact ();
                prevContactInfo = contactInfo;
            }
            else if (prevContactInfo != ContactInfo.nothig) {
                prevContactInfo.CancelContact ();
                prevContactInfo = ContactInfo.nothig;
            }
        }
        else {
            if (prevContactInfo != ContactInfo.nothig) {
                prevContactInfo.CancelContact ();
                prevContactInfo = ContactInfo.nothig;
            } 
        }
    }
    
    bool Raycast(out ContactInfo info){
        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast (ray, out hit, Mathf.Infinity, mechanicMask)) {
            if (hit.collider.GetComponentInParent<WheelSwitch> () != null) {
                var mechanic = hit.collider.GetComponentInParent<WheelSwitch> ();
                var contactPoint = CalculateContactPoint (ray, mechanic.transform);
                
                info = new ContactInfo (hit.collider, mechanic, contactPoint);
                
                return true;
            }
        }

        info = new ContactInfo ();
        return false;
    }
    
    Vector3 CalculateContactPoint(Ray ray, Transform mechanicT){
//        Vector3 normal = Camera.main.transform.forward;
//        Vector3 normal = (mechanicT.position - Camera.main.transform.position).normalized;
        Plane plane = new Plane (mechanicT.forward, mechanicT.position);
//        Plane plane = new Plane (normal, mechanicT.position);

        float rayDistance;
        plane.Raycast (ray, out rayDistance);
       
        Debug.DrawRay (ray.origin, ray.direction * rayDistance);
        return ray.GetPoint (rayDistance);
    }
    
    struct ContactInfo{
        public Collider interactedColl;
        public WheelSwitch mechanic;
        public Vector3 position;
        
        public static ContactInfo nothig{ get{ return _nothig; } }
        private static ContactInfo _nothig = new ContactInfo(null, null, Vector3.zero);
        
        public ContactInfo(Collider _interactedColl ,WheelSwitch _mechanic, Vector3 _position){
            interactedColl = _interactedColl;
            mechanic = _mechanic;
            position = _position;
        }
        
        public void MakeContact(){
            mechanic.MakeContact (interactedColl, position);
        }
        
        public void CancelContact(){
            mechanic.CancelContact ();
        }
        
        public static bool operator ==(ContactInfo a, ContactInfo b){
            return a.mechanic == b.mechanic;
        }
        
        public static bool operator !=(ContactInfo a, ContactInfo b){
            return a.mechanic != b.mechanic;
        }
        
    }
}