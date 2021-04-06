using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    [SerializeField]
    [Range(0.1f,0.5f)]
    private float distance_ratio;

    private Vector3 velocity;

    private Vector3 midPoint;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        SmoothFollowDamp();
    }

    void SmoothFollowDamp()
    {
        midPoint = PlayerMovement.instance.transform.position + (CameraManager.instance.mousePosition - PlayerMovement.instance.transform.position) * distance_ratio;
        midPoint.y = CameraManager.instance.height;
        transform.position = Vector3.SmoothDamp(transform.position, midPoint, ref velocity, 0.05f);
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(new Vector3(midPoint.x,0,midPoint.z), 0.5f);
    }
}
