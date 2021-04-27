using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;


public class PushEntity : MonoBehaviour
{
    public StudioEventEmitter frictionSound;
    private Rigidbody rig;
    private const float freezeTime = 2f;
    private float time;
    
    // Start is called before the first frame update
    void Start()
    {
        frictionSound = gameObject.GetComponent<StudioEventEmitter>();
        rig = gameObject.GetComponent<Rigidbody>();
        time = Time.time + freezeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(rig != null && time < Time.time)
        {
            CheckStatus();
        }
    }

    private void CheckStatus()
    {
        if(rig.velocity.magnitude > 0f && !frictionSound.IsPlaying())
        {
            frictionSound.Play();
        }

        if(rig.velocity.magnitude <= 0f && frictionSound.IsPlaying())
        {
            frictionSound.Stop();
        }
    }
}
