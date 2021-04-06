using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput playerInput;

    public Vector2 input
    {
        get
        {
            Vector2 i = Vector2.zero;
            i.x = Input.GetAxis("Horizontal");
            i.y = Input.GetAxis("Vertical");
            i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
            return i;
        }
    }

    public Vector2 raw
    {
        get
        {
            Vector2 i = Vector2.zero;
            i.x = Input.GetAxisRaw("Horizontal");
            i.y = Input.GetAxisRaw("Vertical");
            i *= (i.x != 0.0f && i.y != 0.0f) ? .7071f : 1.0f;
            return i;
        }
    }

    public bool run
    {
        get { return Input.GetKey(KeyCode.LeftShift); }
    }

    public bool sneak
    {
        get { return Input.GetKeyDown(KeyCode.C); }
    }

    public bool sneaking
    {
        get { return Input.GetKey(KeyCode.C); }
    }

    public bool interact
    {
        get { return Input.GetMouseButtonDown(0); }
    }

    public bool aim
    {
        get { return Input.GetMouseButton(1); }
    }

    private void Awake()
    {
        playerInput = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
