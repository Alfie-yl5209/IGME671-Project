using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager manager;

    public StudioEventEmitter ambience;

    public StudioEventEmitter playerWalk;
    public StudioEventEmitter playerRun;
    public StudioEventEmitter playerSneak;
    private StudioEventEmitter playerMove;
    private string state;

    private void Awake()
    {
        manager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        playerMove = playerWalk;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayerMovementState(string state)
    {
        if (!(state == "Run" || state == "Walk" || state == "Sneak"))
            return;

        if (this.state == state)
            return;

        this.state = state;
        playerMove.Stop();
        switch (state)
        {
            case "Walk":
                playerMove = playerWalk;
                break;
            case "Run":
                playerMove = playerRun;
                break;
            case "Sneak":
                playerMove = playerSneak;
                break;
        }
    }

    public void PlayerMove()
    {
        if (!playerMove.IsPlaying())
            playerMove.Play();
    }

    public void PlayerStop()
    {
        if (playerMove.IsPlaying())
            playerMove.Stop();
    }
}
