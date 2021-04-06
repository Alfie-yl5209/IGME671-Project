using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{

    public LayerMask interactable;

    [SerializeField]
    [Range(1f, 3f)]
    public float radius;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            if (Physics.SphereCast(CameraManager.instance.cam.transform.position, 0.5f, CameraManager.instance.mousePosition - CameraManager.instance.cam.transform.position, out hit, 50, interactable))
            {
                Debug.Log("Hit");
                if (hit.transform.GetComponent<Interactable>())
                    hit.transform.GetComponent<Interactable>().Interact();
            }
        }
    }
}
