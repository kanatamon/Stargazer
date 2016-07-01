using UnityEngine;
using System.Collections;

public class FlyController : MonoBehaviour {

    public Transform tagetTransformation;
    public float flyDuration;
    
    public void Fly(){
        StartCoroutine (AnimateFly ());
    }
    
    IEnumerator AnimateFly(){
        Vector3 fromPosition = transform.position;
        Vector3 fromLocalScale = transform.localScale;
        Quaternion fromRotation = transform.rotation;

        float percent = 0;
        float speed = 1 / flyDuration;

        while (percent < 1) {
            percent += Time.deltaTime * speed;
            float interpolate = Mathf.Pow (percent, 3) / (Mathf.Pow (percent, 3) + Mathf.Pow (1 - percent, 3));

            transform.position = Vector3.Lerp (fromPosition, tagetTransformation.position, interpolate);
            transform.localScale = Vector3.Lerp (fromLocalScale, tagetTransformation.localScale, interpolate);
            transform.rotation = Quaternion.Lerp (fromRotation, Quaternion.LookRotation (tagetTransformation.forward), interpolate);

            yield return null;
        }
    }
    
}
