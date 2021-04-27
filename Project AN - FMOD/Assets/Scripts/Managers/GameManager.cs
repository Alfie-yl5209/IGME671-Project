using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 0, 200, 20), "WASD: Move Esc: Quit");
        GUI.Box(new Rect(10, 20, 200, 20), "Click doors to interact.");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
