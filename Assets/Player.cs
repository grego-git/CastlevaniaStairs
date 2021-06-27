using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum States
    {
        NORMAL,
        STAIRS
    };

    public States currentState;

    private Rigidbody rb;
    private Animator anim;

    private bool facingRight;

    [SerializeField]
    private Stairs stairs;
    [SerializeField]
    private Transform stairsTop;
    [SerializeField]
    private Transform stairsBottom;

    [SerializeField]
    private float stairsTimer;

    [SerializeField]
    private float walkSpeed;

    // Start is called before the first frame update
    void Start()
    {
        currentState = States.NORMAL;

        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        float verticalAxis = Input.GetAxisRaw("Vertical");

        anim.speed = 1.0f;

        switch (currentState)
        {
            case States.NORMAL:
                anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
                break;
            case States.STAIRS:
                if (verticalAxis == 0.0f)
                    anim.speed = 0.0f;

                anim.SetFloat("Speed", verticalAxis);
                break;
        }

        anim.SetBool("OnStairs", currentState == States.STAIRS);

        if (facingRight)
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalAxis = Input.GetAxisRaw("Horizontal");
        float verticalAxis = Input.GetAxisRaw("Vertical");

        switch (currentState) 
        {
            case States.NORMAL:
                // Player's on top of a set of stairs and pressing down
                if (stairsTop && verticalAxis < 0.0f)
                {
                    GetOnStairs(false);
                    break;
                }

                // Player's on the bottom of a set of stairs and pressing up
                if (stairsBottom && verticalAxis > 0.0f)
                {
                    GetOnStairs(true);
                    break;
                }

                if (horizontalAxis < 0.0f)
                {
                    facingRight = false;
                }
                else if (horizontalAxis > 0.0f)
                {
                    facingRight = true;
                }

                rb.velocity = new Vector3(horizontalAxis * walkSpeed, rb.velocity.y, 0.0f);
                break;
            case States.STAIRS:
                if (verticalAxis < 0.0f)
                {
                    if (stairs.bottom.position.x < transform.position.x)
                        facingRight = false;
                    else if (stairs.bottom.position.x > transform.position.x)
                        facingRight = true;

                    // Player has reached the bottom of stairs, can return to normal
                    if (Mathf.Clamp(stairsTimer, 0.0f, 1.0f) == 0.0f)
                    {
                        rb.isKinematic = false;
                        rb.useGravity = true;

                        currentState = States.NORMAL;
                        break;
                    }

                    // Move player down the stairs
                    stairsTimer -= Time.deltaTime / (Vector3.Distance(stairs.top.position, stairs.bottom.position) / walkSpeed);
                }
                else if (verticalAxis > 0.0f)
                {
                    if (stairs.top.position.x < transform.position.x)
                        facingRight = false;
                    else if (stairs.top.position.x > transform.position.x)
                        facingRight = true;

                    // Player has reached the top of stairs, can return to normal
                    if (Mathf.Clamp(stairsTimer, 0.0f, 1.0f) == 1.0f)
                    {
                        rb.isKinematic = false;
                        rb.useGravity = true;

                        currentState = States.NORMAL;
                        break;
                    }

                    // Move player up the stairs
                    stairsTimer += Time.deltaTime / (Vector3.Distance(stairs.top.position, stairs.bottom.position) / walkSpeed);
                }

                transform.position = Vector3.Lerp(stairs.bottom.position, stairs.top.position, stairsTimer);
                break;
        }
    }

    public void GetOnStairs(bool goingUp)
    {
        Transform currentStairsSide;
        float timer;

        if (goingUp)
        {
            currentStairsSide = stairsBottom;
            timer = 0.0f;
        }
        else
        {
            currentStairsSide = stairsTop;
            timer = 1.0f;
        }

        // Move player towards the stairs
        if (currentStairsSide.position.x < transform.position.x)
        {
            facingRight = false;
            rb.velocity = new Vector3(-walkSpeed, rb.velocity.y, 0.0f);
        }
        else
        {
            facingRight = true;
            rb.velocity = new Vector3(walkSpeed, rb.velocity.y, 0.0f);
        }

        // Player has reached stairs
        if (Vector3.Distance(currentStairsSide.position, transform.position) < 0.1f)
        {
            // Adjust player position
            transform.position = currentStairsSide.position;

            // Store reference of the current stairs
            stairs = currentStairsSide.parent.GetComponent<Stairs>();

            rb.isKinematic = true;
            rb.useGravity = false;

            // Set timer accordingly: 0 if on bottom, 1 if on top
            stairsTimer = timer;

            // Change state
            currentState = States.STAIRS;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        string collisionLayer = LayerMask.LayerToName(other.gameObject.layer);

        if (collisionLayer == "StairsTop")
        {
            stairsTop = other.transform;
        }

        if (collisionLayer == "StairsBottom")
        {
            stairsBottom = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        string collisionLayer = LayerMask.LayerToName(other.gameObject.layer);

        if (collisionLayer == "StairsTop")
        {
            stairsTop = null;
        }

        if (collisionLayer == "StairsBottom")
        {
            stairsBottom = null;
        }
    }
}
