using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum MovementState
    {
        idle,
        walking,
        running,
        sneaking
    }

    //singleton
    public static PlayerMovement instance;

    //player controller component
    public CharacterController controller;

    //Object Push
    private PushObject pushObj;

    public MovementState state;

    public float walkSpeed;
    public float sprintSpeed;
    public float sneakSpeed;

    public float accelerationSpeed;

    public float maxSpeed;
    public float friction;

    public Vector3 velocity;

    private void Awake()
    {
        instance = this;
        state = MovementState.idle;
    }
    // Start is called before the first frame update
    void Start()
    {
        pushObj = gameObject.GetComponent<PushObject>();
        accelerationSpeed = walkSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        //Get player movement input
        float x = PlayerInput.playerInput.raw.x;
        float z = PlayerInput.playerInput.raw.y;

        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            state = MovementState.walking;

            if (PlayerInput.playerInput.run)
            {
                state = MovementState.running;
            }

            if (PlayerInput.playerInput.sneaking)
            {
                state = MovementState.sneaking;
            }
        }
        else
        {
            state = MovementState.idle;
        }

        switch (state)
        {
            case MovementState.idle:
                break;
            case MovementState.walking:
                accelerationSpeed = walkSpeed;
                break;
            case MovementState.sneaking:
                accelerationSpeed = sneakSpeed;
                break;
            case MovementState.running:
                accelerationSpeed = sprintSpeed;
                break;
            default:
                break;
        }

        velocity += (Vector3.right * x + Vector3.forward * z) * accelerationSpeed * Time.deltaTime;

        //Clamp velocity using max speed
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        //Debug.Log(velocity);

        //Move player horizontally
        controller.Move(velocity * Time.deltaTime);

        //Slow player down
        velocity *= friction;

        // Set low speed to zero
        if (velocity.magnitude <= 0.05f)
        {
            velocity = Vector3.zero;
        }

    }
}
