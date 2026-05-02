using System;
using System.Linq.Expressions;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Animator playerAnimator;
    public float moveSpeed = 5.0f;
    public float jumpForce = 10.0f;
    public float groundCheckSpacing = 0.5f;
    public float groundCheckDistance = 0.5f;
    public float maxJumpQueue = 0.1f;
    public float maxFallSpeed = 5f;





    //Global State Control Variables
    public bool canMove = true;
    public bool isGrounded = true;
    public bool isWalled = false;
    public bool isDashing = false;
    public bool isAttacking = false;
    public bool isFacingRight = true;
    public bool jumpBuffer = false;




    //States
    public enum playerState
    {
        SLEEP,
        IDLE,
        RUNNING,
        JUMPING,
        FALLING,
        WALLSLIDING,
        DASHING,
        ATTACKING

    }

    


    //Private Variables
    private float xinput;
    private Rigidbody2D rb;
    private playerState currentState = playerState.SLEEP;
    [SerializeField]private float jumpTimeCtr = 0;

    //Collision Variables
    [SerializeField] private LayerMask whatIsGround;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SwitchState(playerState.IDLE);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        HandleState();
        HandleCollision();
        HandleCounters();
    }


    
    private void FixedUpdate()
    {
        HandleMovement();
        HandleFlip();
        HandleFall();
        HandleJump();
        
    }

    private void HandleInput()
    {
        xinput = Input.GetAxisRaw("Horizontal");
        if(Input.GetKeyDown(KeyCode.Space))
        {
            jumpBuffer = true;
            jumpTimeCtr = maxJumpQueue;
        }
        
    }

    private void HandleMovement()
    {
        if(canMove)
        {
            rb.linearVelocity = new Vector2(xinput * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void HandleFall()
    {

        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 100f * Time.deltaTime);
        }
        else
        {
            if(rb.linearVelocity.y > -maxFallSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 50f * Time.deltaTime);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
            }
            

        }
        

        


    }

    private void HandleJump()
    {
        if(canMove)
        {
            if(jumpBuffer && isGrounded && jumpTimeCtr > 0)
            {
                TryToJump();
                jumpBuffer = false;
            }
        }
    }

    private void TryToJump()
    {
        if (isGrounded)
        {

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        }
    }

    private void HandleFlip()
    {
        if(isFacingRight && rb.linearVelocityX < -0.1f)
        {
            transform.Rotate(0, 180, 0);
            isFacingRight = !isFacingRight;
        }
        else if(!isFacingRight && rb.linearVelocityX > 0.1f)
        {
            transform.Rotate(0, 180, 0);
            isFacingRight = !isFacingRight;
        }
    }

    private void SwitchState(playerState newState)
    {
        if(newState != currentState)
        {
            currentState = newState;
        }
        else
        {
            switch(currentState)
            {
                case playerState.IDLE:
                    playerAnimator.Play("PlayerIdle");
                    break;
                case playerState.RUNNING:
                    playerAnimator.Play("PlayerRun");
                    break;
                case playerState.JUMPING:
                    playerAnimator.Play("PlayerJump");
                    break;
                case playerState.FALLING:
                    playerAnimator.Play("PlayerFall");
                    break;
                case playerState.DASHING:
                    break;
                case playerState.WALLSLIDING:
                    break;
                case playerState.ATTACKING:
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleState()
    {
        if(isGrounded && (Math.Abs(rb.linearVelocityX) > 0))
        {
            SwitchState(playerState.RUNNING);
        }
        else if(isGrounded && (Math.Abs(rb.linearVelocityX) == 0))
        {
            SwitchState(playerState.IDLE);
        }
        if(!isGrounded && rb.linearVelocityY > 0)
        {
            SwitchState(playerState.JUMPING);
        }
        else if(!isGrounded && rb.linearVelocityY <= 0)
        {
            SwitchState(playerState.FALLING);
        }
    }

    private void HandleCounters()
    {
        if(jumpTimeCtr >= 0)
        {
            jumpTimeCtr -= Time.deltaTime;
        }
    }

    private void HandleCollision()
    {
        bool isGrounded1 = Physics2D.Raycast(transform.position + new Vector3(groundCheckSpacing, 0), Vector2.down, groundCheckDistance, whatIsGround);
        bool isGrounded2 = Physics2D.Raycast(transform.position + new Vector3(-groundCheckSpacing, 0), Vector2.down, groundCheckDistance, whatIsGround);
        isGrounded = isGrounded1 || isGrounded2;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position + new Vector3(groundCheckSpacing, 0), transform.position + new Vector3(groundCheckSpacing, 0) + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(transform.position + new Vector3(-groundCheckSpacing, 0), transform.position + new Vector3(-groundCheckSpacing, 0) + new Vector3(0, -groundCheckDistance));
        //Gizmos.DrawLine(transform.position + new Vector3(0, -1, 0), transform.position + new Vector3(0, -1, 0) + new Vector3(wallTouchDistance, 0));
        //Gizmos.DrawWireCube(attackPointForward_2.position, new Vector3(5f, 2f, 0f));
        //Gizmos.DrawWireSphere(attackPointUp.position, attackRadius);
        //Gizmos.DrawWireSphere(attackPointDown.position, attackRadius);
    }


}
