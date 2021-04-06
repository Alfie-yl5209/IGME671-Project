using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{

    public LayerMask entityMask;

    private bool isClosed;
    private Rigidbody rig;
    private Quaternion closeRotation;
    private Vector3 closePosition;

    // Start is called before the first frame update
    void Start()
    {
        rig = gameObject.GetComponent<Rigidbody>();
        entityMask = LayerMask.GetMask("Entity");

        closePosition = transform.position;
        closeRotation = transform.rotation;

        isClosed = true;

        if (isClosed)
            rig.isKinematic = true;
        else
            rig.isKinematic = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Use()
    {
        if (isClosed)
        {
            Open();
        }
        else
        {
            Close();
        }
        isClosed = !isClosed;
    }

    void Open()
    {
        Debug.Log("Open");
        rig.isKinematic = false;
        rig.velocity = (transform.position - PlayerMovement.instance.transform.position).normalized * 2f;
    }

    void Close()
    {
        if (Physics.OverlapSphere(closePosition, 0.3f, entityMask).Length > 0)
        {
            return;
        }

        Debug.Log("Close");
        rig.isKinematic = true;
        transform.rotation = closeRotation;
        transform.position = closePosition;
    }

    void SmashedOpen()
    {
        Debug.Log("Smashed Open");
        rig.isKinematic = false;
        rig.velocity = (transform.position - PlayerMovement.instance.transform.position).normalized * 10f;
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(closePosition, 0.3f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }

}
