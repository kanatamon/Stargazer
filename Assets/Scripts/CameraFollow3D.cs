using UnityEngine;
using System.Collections;

public class CameraFollow3D : MonoBehaviour {

    public CharacterController target;
//    public float depthOffset;
//    public float verticalOffset;
    public Vector3 offset;
    public float lookDistance;
    public float lookSmoothTime;
    public Vector3 focusAreaSize;

    Vector3 currentLookAhead;
    Vector3 targetLookAhead;
    Vector3 currentVelocity;
    
    FocusArea focusArea;

    void Start(){
        focusArea = new FocusArea(target.bounds, focusAreaSize);
    }

    void LateUpdate(){
        focusArea.Update(target.bounds);
        Vector3 focusPosition = focusArea.centre + offset;
        
        if (focusArea.velocity.x != 0 || focusArea.velocity.z != 0) {
            Vector3 targetDir = focusArea.velocity.normalized;
            targetDir.y = 0;
            
            targetLookAhead = targetDir * lookDistance;
        }
        
        currentLookAhead = Vector3.SmoothDamp (currentLookAhead, targetLookAhead, ref currentVelocity, lookSmoothTime);

        transform.position = focusPosition + currentLookAhead;
    }

    struct FocusArea {
        public Vector3 centre;
        public Vector3 velocity;
        float left, right;
        float top,buttom;
        float front,rear;

        public FocusArea(Bounds targetBounds, Vector3 size) {
            left = targetBounds.center.x - size.x/2;
            right = targetBounds.center.x + size.x/2;
            top = targetBounds.min.y + size.y;
            buttom = targetBounds.min.y;
            front = targetBounds.center.z + size.z/2;
            rear = targetBounds.center.z - size.z/2;

            velocity = Vector3.zero;
            centre = new Vector3((right + left)/2, (top + buttom)/2, (front + rear)/2);
        }

        public void Update(Bounds targetBounds){
            float shiftX = 0;
            if(targetBounds.max.x > right){
                shiftX = targetBounds.max.x - right;
            }
            else if(targetBounds.min.x < left){
                shiftX = targetBounds.min.x - left;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if(targetBounds.max.y > top){
                shiftY = targetBounds.max.y - top;
            }
            else if(targetBounds.min.y < buttom){
                shiftY = targetBounds.min.y - buttom;
            }
            top += shiftY;
            buttom += shiftY;

            float shiftZ = 0;
            if(targetBounds.max.z > front){
                shiftZ = targetBounds.max.z - front;
            }
            else if(targetBounds.min.z < rear){
                shiftZ = targetBounds.min.z - rear;
            }
            front += shiftZ;
            rear += shiftZ;

            centre = new Vector3((right + left)/2, (top + buttom)/2, (front + rear)/2);
            velocity = new Vector3(shiftX, shiftY, shiftZ);
        }
    }

    void OnDrawGizmos(){
        Gizmos.color = new Color(1, 0, 0, .2f);
        Gizmos.DrawCube(focusArea.centre, focusAreaSize);
    }

}