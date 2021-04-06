using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    public Camera cam;

    public Vector3 mousePosition;

    public float height;
    public int minHeight;

    public Quaternion rotation;

    private void Awake()
    {
        instance = this;
        height = minHeight;
    }

    void Start()
    {
    }

    void Update()
    {
        CameraZoomInput();

        Vector3 vector3 = Vector3.zero;
        vector3 = Input.mousePosition;
        vector3.z = height;
        /*mousePosition = Input.mousePosition;
        mousePosition.z = height;*/
        mousePosition = cam.ScreenToWorldPoint(vector3);
    }

    void CameraZoomInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (height > minHeight)
            {
                height -= 1;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            height += 1;
        }
    }

    void CameraRotateInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {

        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(new Vector3(midPoint.x, 0, midPoint.z), 0.5f);

        Gizmos.DrawLine(cam.transform.position, mousePosition);
        Gizmos.DrawWireSphere(mousePosition, 1f);
    }
}
