using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushObject : MonoBehaviour
{
    public float pushPower;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rig = hit.collider.attachedRigidbody;

        if (rig == null || rig.isKinematic)
            return;

        if (hit.moveDirection.magnitude < -0.2f)
            return;

        Debug.Log("Push");
        Vector3 dir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        rig.velocity = dir * pushPower * Time.deltaTime / rig.mass;
    }

    public void SetPushPower(int p)
    {
        if (p > 0)
            pushPower = p;
    }
}
