using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{

    [SerializeField]
    [Range(1f, 5f)]
    private float distance;

    [SerializeField]
    [Range(0.1f, 2f)]
    private float speed;

    private Vector3 look_point;
    private Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        look_point = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 target = CameraManager.instance.mousePosition + Vector3.up * transform.position.y;
        float angle = Mathf.Atan2(target.z - transform.position.z, target.x - transform.position.x);
        target = transform.position + new Vector3(distance * Mathf.Cos(angle), 0, distance * Mathf.Sin(angle));

        if (Vector3.Distance(look_point, transform.position) < distance)
        {
            float temp = Mathf.Atan2(look_point.z - transform.position.z, look_point.x - transform.position.x);
            look_point = transform.position + new Vector3(distance * Mathf.Cos(temp), 0, distance * Mathf.Sin(temp));
        }

        look_point = Vector3.SmoothDamp(look_point, target, ref velocity, speed, 30f);
        transform.LookAt(look_point);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(look_point, 1f);
    }
}
