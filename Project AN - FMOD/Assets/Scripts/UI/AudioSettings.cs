using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioSettings : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string dragPath;

    EventInstance drag;


    // Start is called before the first frame update
    void Start()
    {
        drag = RuntimeManager.CreateInstance(dragPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayDrag()
    {
        drag.start();
    }
}
