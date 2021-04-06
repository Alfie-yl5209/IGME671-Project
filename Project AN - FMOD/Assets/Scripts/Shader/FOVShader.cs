using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVShader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<Renderer>().material.SetFloat("_Range", 20f);
        gameObject.GetComponent<Renderer>().material.SetVector("_PlayerPos", PlayerMovement.instance.transform.position);
    }
}

