using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{

    [SerializeField]
    [Range(1f, 3f)]
    protected float range = 1f;

    abstract public void Use();

    public void Interact()
    {
        if(Vector3.Distance(transform.position, PlayerMovement.instance.transform.position) < range + PlayerStats.stats.Range_interact)
        {
            Use();
        }
    }



}
