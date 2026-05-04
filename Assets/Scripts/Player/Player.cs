using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Animator playerAnimator;
    public float moveSpeed = 5.0f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 1.0f;
    public float jumpForce = 10.0f;
    public float groundCheckSpacing = 0.5f;
    public float groundCheckDistance = 0.5f;
    public float wallCheckSpacing = 0.5f;
    public float wallCheckDistance = 0.5f;
    public float maxJumpQueue = 0.1f;
    public float maxFallSpeed = 5f;




    //Global State Control Variables
    public bool canMove = true;
    public bool isGrounded = true;
    public bool isWalled = false;
    public bool isDashing = false;
    public bool canDash = true;
    public bool isAttacking = false;
    public bool isFacingRight = true;
    public bool jumpBuffer = false;
    public bool isLedge = false;
    public bool canDoubleJump = false;
    public bool actionLock = false;
    
    



    //States
    public enum playerState
    {
        SLEEP,
        IDLE,
        RUNNING,
        JUMPING,
        DOUBLEJUMPING,
        FALLING,
        WALLSLIDING,
        DASHING,
        ATTACKING,
        CLIMBINGLEDGE

    }

    


    //Private Variables
    private float xinput;
    private Rigidbody2D rb;
    private playerState currentState = playerState.SLEEP;
    private bool jumpTerminate = false;
    private float jumpTimeCtr = 0;
    private float wallJumpTimeCtr = 0;
    private float dashTimeCtr = 0;

    //Collision Variables
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsWall;


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
        if(!actionLock)
        {
            HandleInput();
            HandleState();
            HandleCollision();
            HandleCounters();
        }
        
    }




    private void FixedUpdate()
    {
        if(!actionLock)
        {
            HandleMovement();
            HandleFlip();
            HandleFall();
            HandleJump();
        }
        
        
    }

    private void HandleInput()
    {
        xinput = Input.GetAxisRaw("Horizontal");

        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBuffer = true;
            jumpTimeCtr = maxJumpQueue;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpTerminate = true;
        }
        
        
        if(wallJumpTimeCtr > 0)
        {
            canMove = false;
        }
        else
        {
            canMove = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && canDash)
        {
            dashTimeCtr = dashDuration;
            canDash = false;
        }

    }

    private void HandleMovement()
    {
        if(canMove)
        {
            if(!isDashing)
            {
                rb.linearVelocity = new Vector2(xinput * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                if(isFacingRight)
                {
                    rb.linearVelocity = new Vector2(dashSpeed, 0);
                }
                else
                {
                    rb.linearVelocity = new Vector2(-dashSpeed, 0);
                }
                
            }
        }
    }

    private void HandleFall()
    {
        if(!isDashing)
        {
            if (rb.linearVelocity.y > 0)
            {
                if (jumpTerminate)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y / 2);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 100f * Time.deltaTime);
                }


            }
            else
            {
                if (isWalled && ((isFacingRight && Input.GetKey(KeyCode.D)) || (!isFacingRight && Input.GetKey(KeyCode.A))))
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed / 3);
                }
                else if (rb.linearVelocity.y > -maxFallSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 50f * Time.deltaTime);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
                }


            }
            jumpTerminate = false;
        }

    else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        
    }

    private void HandleJump()
    {
        if(canMove)
        {
            if(((jumpBuffer && (isGrounded || isWalled) && jumpTimeCtr > 0) || (jumpBuffer && canDoubleJump)) && !isDashing)
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
        else if(!isGrounded && isWalled)
        {
            if(isFacingRight)
            {
                rb.linearVelocity = new Vector2(-jumpForce/2.0f, jumpForce/1.2f);
                wallJumpTimeCtr = 0.1f;
            }
            else
            {
                rb.linearVelocity = new Vector2(jumpForce /2.0f, jumpForce / 1.2f);
                wallJumpTimeCtr = 0.1f;

            }
        }
        else if(!isGrounded && !isWalled && canDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce/1.2f);
            canDoubleJump = false;
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
        
            switch(currentState)
            {
                case playerState.IDLE:
                    playerAnimator.Play("PlayerIdle");
                    Debug.Log("Idle");
                    break;
                case playerState.RUNNING:
                    playerAnimator.Play("PlayerRun");
                    Debug.Log("Run");
                    break;
                case playerState.JUMPING:
                    playerAnimator.Play("PlayerJump");
                    Debug.Log("Jump");
                    break;
                case playerState.DOUBLEJUMPING:
                    playerAnimator.Play("PlayerDoubleJump");
                    Debug.Log("DoubleJump");
                    break;
                case playerState.FALLING:
                    playerAnimator.Play("PlayerFall");
                    Debug.Log("Fall");
                    break;
                case playerState.CLIMBINGLEDGE:
                    playerAnimator.Play("PlayerClimbLedge");
                    Debug.Log("ClimbingLedge");
                    break;
                case playerState.DASHING:
                    playerAnimator.Play("PlayerDash");
                    Debug.Log("Dash");
                    break;
                case playerState.WALLSLIDING:
                    playerAnimator.Play("PlayerWallHang");
                    Debug.Log("WallHang");
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
        if (isDashing)
        {
            SwitchState(playerState.DASHING);
        }
        else
        {
            if (isGrounded && (Math.Abs(rb.linearVelocityX) > 0))
            {
                SwitchState(playerState.RUNNING);
            }
            else if (isGrounded && (Math.Abs(rb.linearVelocityX) == 0))
            {
                SwitchState(playerState.IDLE);
            }
            if (!isGrounded && rb.linearVelocityY > 0)
            {
                if (canDoubleJump)
                {
                    SwitchState(playerState.JUMPING);
                }
                else
                {
                    SwitchState(playerState.DOUBLEJUMPING);
                }

            }
            else if (!isGrounded && !isWalled && rb.linearVelocityY <= 0)
            {
                SwitchState(playerState.FALLING);
            }
            if (!isGrounded && isWalled && rb.linearVelocityY <= 0)
            {
                SwitchState(playerState.WALLSLIDING);
            }
            if (isLedge && rb.linearVelocityY <= 0)
            {
                if (((isFacingRight && Input.GetKey(KeyCode.D)) || (!isFacingRight && Input.GetKey(KeyCode.A))))
                {
                    rb.linearVelocity = new Vector2(0, 0);
                    SwitchState(playerState.CLIMBINGLEDGE);
                }

            }
        }
        
        
    }

    private void HandleCounters()
    {
        if(jumpTimeCtr >= 0)
        {
            jumpTimeCtr -= Time.deltaTime;
        }
        else
        {
            jumpBuffer = false;
        }
        if(wallJumpTimeCtr >= 0)
        {
            wallJumpTimeCtr -= Time.deltaTime;
        }
        if(dashTimeCtr > -dashCooldown)
        {
            dashTimeCtr -= Time.deltaTime;
        }
        if(dashTimeCtr > 0)
        {
            isDashing = true;
        }
        else
        {
            isDashing = false;
        }
        if(dashTimeCtr <= -dashCooldown && (isGrounded || isWalled))
        {
            canDash = true;
        }

    }

    private void HandleCollision()
    {
        bool isWalled1 = false;
        bool isWalled2 = false;
        bool isGrounded1 = Physics2D.Raycast(transform.position + new Vector3(groundCheckSpacing, 0), Vector2.down, groundCheckDistance, whatIsGround);
        bool isGrounded2 = Physics2D.Raycast(transform.position + new Vector3(-groundCheckSpacing, 0), Vector2.down, groundCheckDistance, whatIsGround);
        isGrounded = isGrounded1 || isGrounded2;

        if(isFacingRight)
        {
            isWalled1 = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckSpacing), Vector2.right, wallCheckDistance, whatIsWall);
            isWalled2 = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckSpacing*0.7f), Vector2.right, wallCheckDistance, whatIsWall);
        }
        else
        {
            isWalled1 = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckSpacing), Vector2.left, wallCheckDistance, whatIsWall);
            isWalled2 = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckSpacing*0.7f), Vector2.left, wallCheckDistance, whatIsWall);
        }
        isWalled = isWalled1 && isWalled2;

        if(isWalled2 && !isWalled1)
        {
            isLedge = true;
        }
        else
        {
            isLedge = false;
        }
        if(isWalled || isGrounded)
        {
            canDoubleJump = true;
        }


    }


    public void SetActionLock()
    {
        actionLock = true;
    }

    public void ReleaseActionLock()
    {

        actionLock = false;
        LedgeClimbPositionReset();
    }

    public void LedgeClimbPositionReset()
    {
        if (isFacingRight)
        {
            transform.position = new Vector3(transform.position.x + 0.55f, transform.position.y + 1.45f);
        }
        else
        {
            transform.position = new Vector3(transform.position.x - 0.55f, transform.position.y + 1.45f);
        }
        
    }

    IEnumerator DelayedEnd()
    {
        yield return new WaitForEndOfFrame();
        ReleaseActionLock();// Logic here
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position + new Vector3(groundCheckSpacing, 0), transform.position + new Vector3(groundCheckSpacing, 0) + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(transform.position + new Vector3(-groundCheckSpacing, 0), transform.position + new Vector3(-groundCheckSpacing, 0) + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(transform.position + new Vector3(0, wallCheckSpacing), transform.position + new Vector3(0, wallCheckSpacing) + new Vector3(wallCheckDistance, 0));
        Gizmos.DrawLine(transform.position + new Vector3(0, wallCheckSpacing*0.7f), transform.position + new Vector3(0, wallCheckSpacing*0.7f) + new Vector3(wallCheckDistance, 0));
        //Gizmos.DrawWireSphere(attackPointUp.position, attackRadius);
        //Gizmos.DrawWireSphere(attackPointDown.position, attackRadius);
    }


}
