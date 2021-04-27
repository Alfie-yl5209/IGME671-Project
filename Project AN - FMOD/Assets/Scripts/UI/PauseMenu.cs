using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject menu;
    private bool state;

    [FMODUnity.EventRef]
    public string pausePath;

    EventInstance pause;

    // Start is called before the first frame update
    void Start()
    {
        state = false;
        pause = RuntimeManager.CreateInstance(pausePath);
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            state = !state;
            if (state)
            {
                pause.start();
                Debug.Log("Snapshot Start");
            }
            else
            {
                pause.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                Debug.Log("Snapshot End");
            }
            menu.SetActive(state);
        }
    }

    public void Toggle()
    {
        state = !state;

        if (state)
        {
            pause.start();
            Debug.Log("Snapshot Start");
        }
        else
        {
            pause.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            Debug.Log("Snapshot End");
        }

        menu.SetActive(state);

    }
}
