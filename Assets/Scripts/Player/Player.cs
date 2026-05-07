using System;
using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting;
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
    public bool isTurning = false;
    public bool isLanding = false;
    public bool isTakingHit = false;
    public bool isKnockedBack = false;





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
        CLIMBINGLEDGE,
        TURNING,
        LANDING,
        OUCH,

    }

    


    //Private Variables
    private float xinput;
    private Rigidbody2D rb;
    private playerState currentState = playerState.SLEEP;
    private bool jumpTerminate = false;
    private float jumpTimeCtr = 0;
    private float wallJumpTimeCtr = 0;
    private float dashTimeCtr = 0;
    private int rollCounter = 0;

    public float knockBackDirectionX = 0;
    public float knockBackDirectionY = 0;


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
            HandleLanding();
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
        HandleKnockBack();
        
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
        
        if (canMove)
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

    private void HandleKnockBack()
    {
        if (isKnockedBack)
        {
            Debug.Log("HandleKnockBackCondition");
            rb.linearVelocity = new Vector2(knockBackDirectionX, knockBackDirectionY);
        }
    }

    private void HandleLanding()
    {
        if (rb.linearVelocityY < -maxFallSpeed/2 && isGrounded)
        {
            isLanding = true;
        }
        else
        {
            if(currentState != playerState.LANDING)
            {
                isLanding = false;
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
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed / 4);
                }
                else if (rb.linearVelocity.y > -maxFallSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - 25f * Time.deltaTime);
                }
                else if(!isGrounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
                }
                else if(isGrounded)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
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
                rb.linearVelocity = new Vector2(-jumpForce/2.0f, jumpForce/1.3f);
                wallJumpTimeCtr = 0.1f;
            }
            else
            {
                rb.linearVelocity = new Vector2(jumpForce /2.0f, jumpForce / 1.3f);
                wallJumpTimeCtr = 0.1f;

            }
        }
        else if(!isGrounded && !isWalled && canDoubleJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce/1.3f);
            canDoubleJump = false;
        }
    }

    private void HandleFlip()
    {
        if (isFacingRight && rb.linearVelocityX < -0.1f)
        {
            transform.Rotate(0, 180, 0);
            isFacingRight = !isFacingRight;
            if (currentState == playerState.IDLE || currentState == playerState.RUNNING)
            {
                isTurning = true;
            }
            
        }
        else if(!isFacingRight && rb.linearVelocityX > 0.1f)
        {
            transform.Rotate(0, 180, 0);
            isFacingRight = !isFacingRight;
            if (currentState == playerState.IDLE || currentState == playerState.RUNNING)
            {
                isTurning = true;
            }
        }
    }

    public void TurnComplete()
    {
        isTurning = false;
    }

    public void LandingComplete()
    {
        isLanding = false;
    }

    private void SwitchState(playerState newState)
    {
        if(newState != currentState)
        {
            currentState = newState;

            if(currentState != playerState.TURNING)
            {
                isTurning = false;
            }
            if(currentState != playerState.LANDING)
            {
                isLanding = false;
            }
            if(currentState != playerState.DOUBLEJUMPING)
            {
                rollCounter = 0;
            }
        
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
                    Debug.Log("Falling");
                    break;
                case playerState.CLIMBINGLEDGE:
                    playerAnimator.Play("PlayerClimbLedge");
                    Debug.Log("ClimbingLedge");
                    break;
                case playerState.DASHING:
                    playerAnimator.Play("PlayerDash");
                    Debug.Log("Dashed");
                    break;
                case playerState.WALLSLIDING:
                    playerAnimator.Play("PlayerWallHang");
                    Debug.Log("WallHang");
                    break;
                case playerState.TURNING:
                    playerAnimator.Play("PlayerTurn");
                    Debug.Log("Turned");
                    break;
                case playerState.LANDING:
                    playerAnimator.Play("PlayerLand");
                    Debug.Log("Landed");
                    break;
                case playerState.OUCH:
                    playerAnimator.Play("PlayerOuch");
                    Debug.Log("Took a hit");
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

        if(isTakingHit)
        {
            SwitchState(playerState.OUCH);
        }

        else if (isDashing)
        {
            SwitchState(playerState.DASHING);
        }
        else
        {
            if (isGrounded && (Math.Abs(rb.linearVelocityX) > 0))
            {
                if(isTurning)
                {
                    SwitchState(playerState.TURNING);
                }
                else
                {
                    SwitchState(playerState.RUNNING);
                }
                
            }
            else if (isGrounded && (Math.Abs(rb.linearVelocityX) == 0))
            {
                if(isLanding)
                {
                    SwitchState(playerState.LANDING);
                }
                else
                {
                    SwitchState(playerState.IDLE);
                }
                
            }
            if (!isGrounded && rb.linearVelocityY > 0.6f * jumpForce)
            {
                if (canDoubleJump)
                {
                    SwitchState(playerState.JUMPING);
                }
                else
                {
                    SwitchState(playerState.DOUBLEJUMPING);
                    rollCounter = 1;
                }

            }
            else if (!isGrounded && !isWalled && rb.linearVelocityY <= 0 && rollCounter <= 0)
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

    public void DecrementRoll()
    {
        if(rollCounter > 0)
        {
            rollCounter--;
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

    [ContextMenu("TAKE A HIT")]
    public void TakeHit()
    {
        isTakingHit = true;
        isDashing = false;
        KnockBack();
    }

    public void HitOver()
    {
        isTakingHit = false;
        KnockBackOver();
        ReleaseActionLock();
    }

    public void KnockBack()
    {
        isKnockedBack = true;
    }

    public void KnockBackOver()
    {
        isKnockedBack = false;
    }


    public void SetActionLock()
    {
        actionLock = true;
    }

    public void ReleaseActionLock()
    {

        actionLock = false;
    }

    public void LedgeClimbPositionReset()
    {
        if (isFacingRight)
        {
            transform.position = new Vector3(transform.position.x + 0.55f, transform.position.y + 1.40f);
        }
        else
        {
            transform.position = new Vector3(transform.position.x - 0.55f, transform.position.y + 1.40f);
        }
        
    }

    IEnumerator DelayedEnd()
    {
        yield return new WaitForEndOfFrame();

        LedgeClimbPositionReset();
        ReleaseActionLock();
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
