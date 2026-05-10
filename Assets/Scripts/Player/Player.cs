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
    public float jumpDuration = 0.5f;
    public float peakJumpGravity = 100f;
    public float jumpFloatGravity = 1f;
    public float groundCheckSpacing = 0.5f;
    public float groundCheckDistance = 0.5f;
    public float wallCheckSpacing = 0.5f;
    public float wallCheckDistance = 0.5f;
    public float maxJumpQueue = 0.1f;
    public float maxFallSpeed = 5f;
    public float wallSlideSpeed = 5f;
    public float wallJumpDuration = 0.15f;



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
        WALLJUMPING,
        FALLING,
        WALLSLIDING,
        DASHING,
        ATTACKING,
        CLIMBINGLEDGE,
        TURNING,
        LANDING,
        OUCH,
        DEATH

    }

    


    //Private Variables
    private float xinput;
    private Rigidbody2D rb;
    private playerState currentState = playerState.SLEEP;
    [SerializeField]private float jumpTimeCtr = 0;
    private bool jumpTerminate = false;
    private float jumpBufferTimeCtr = 0;
    private float wallJumpTimeCtr = 0;
    private float dashTimeCtr = 0;
    public float damageCooldownCtr = 0;

    public float knockBackDirectionX = 0;
    public float knockBackDirectionY = 0;
    [SerializeField] private float gravityValue = 10f;


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
            if(currentState != playerState.DEATH)
            {
                HandleInput();
                HandleState();
                HandleCounters();
                HandleLanding();
            }
            HandleCollision();
            HandleDeath();
        }
        
    }




    private void FixedUpdate()
    {
        if(!actionLock)
        {
            if (currentState != playerState.DEATH)
            {
                HandleMovement();
                HandleFlip();
                HandleJump();
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            HandleFall();
            
        }
        if (currentState != playerState.DEATH)
        {
            HandleKnockBack();
        }
        
        
    }

    private void HandleInput()
    {
        xinput = Input.GetAxisRaw("Horizontal");

        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBuffer = true;
            jumpBufferTimeCtr = maxJumpQueue;
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

            if(jumpTimeCtr >0)
            {
                

                if (jumpTimeCtr > 0.3f* jumpDuration)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - peakJumpGravity * Time.deltaTime);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - jumpFloatGravity * Time.deltaTime);
                }
                if (jumpTimeCtr > 0.66f * jumpDuration && jumpTerminate)
                {
                    if(rb.linearVelocity.y > 0)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
                    }
                    
                    jumpTerminate = false;
                    jumpTimeCtr = 0.1f * jumpDuration;
                }
            }
            else if(!isGrounded)
            {
                if(isWalled && rb.linearVelocityY <= -wallSlideSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
                }

                else if(rb.linearVelocityY > -maxFallSpeed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y - gravityValue * Time.deltaTime);
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x,  - maxFallSpeed);
                }

            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            }

            
            
            
        }

    else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);            
        }
        jumpTerminate = false;

    }

    private void HandleJump()
    {
        if(canMove)
        {
            if(((jumpBuffer && (isGrounded || isWalled) && jumpBufferTimeCtr > 0) || (jumpBuffer && canDoubleJump)) && !isDashing)
            {
                TryToJump();
                
            }

        }
    }

    private void TryToJump()
    {
        if (isGrounded)
        {
            jumpBuffer = false;
            jumpTimeCtr = jumpDuration;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        }
        else if(!isGrounded && isWalled)
        {
            if(isFacingRight)
            {
                jumpBuffer = false;
                jumpTimeCtr = 0.7f * jumpDuration;
                rb.linearVelocity = new Vector2(-jumpForce*0.6f, jumpForce*0.7f);
                wallJumpTimeCtr = wallJumpDuration;
            }
            else
            {
                jumpBuffer = false;
                jumpTimeCtr = 0.7f * jumpDuration;
                rb.linearVelocity = new Vector2(jumpForce*0.6f, jumpForce*0.7f);
                wallJumpTimeCtr = wallJumpDuration;

            }
        }
        else if(!isGrounded && !isWalled && canDoubleJump)
        {
            jumpBuffer = false;
            jumpTimeCtr = 0.7f * jumpDuration;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.7f);
            canDoubleJump = false;
        }
    }

    private void HandleFlip()
    {
        if(wallJumpTimeCtr <=0)
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
            else if (!isFacingRight && rb.linearVelocityX > 0.1f)
            {
                transform.Rotate(0, 180, 0);
                isFacingRight = !isFacingRight;
                if (currentState == playerState.IDLE || currentState == playerState.RUNNING)
                {
                    isTurning = true;
                }
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
                case playerState.WALLJUMPING:
                    playerAnimator.Play("PlayerWallJump");
                    Debug.Log("WallJump");
                    break;
                case playerState.FALLING:
                    playerAnimator.Play("PlayerFall");
                    //Debug.Log("Falling");
                    break;
                case playerState.CLIMBINGLEDGE:
                    playerAnimator.Play("PlayerClimbLedge");
                    //Debug.Log("ClimbingLedge");
                    break;
                case playerState.DASHING:
                    playerAnimator.Play("PlayerDash");
                   // Debug.Log("Dashed");
                    break;
                case playerState.WALLSLIDING:
                    playerAnimator.Play("PlayerWallHang");
                    //Debug.Log("WallHang");
                    break;
                case playerState.TURNING:
                    playerAnimator.Play("PlayerTurn");
                    //Debug.Log("Turned");
                    break;
                case playerState.LANDING:
                    playerAnimator.Play("PlayerLand");
                    //Debug.Log("Landed");
                    break;
                case playerState.OUCH:
                    playerAnimator.Play("PlayerOuch");
                    //Debug.Log("Took a hit");
                    break;
                case playerState.DEATH:
                    playerAnimator.Play("PlayerDeath");
                    //Debug.Log("Died");
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
            if (!isGrounded && rb.linearVelocityY > 0.1f * jumpForce)
            {
                if (canDoubleJump && currentState != playerState.DOUBLEJUMPING)
                {
                    if(wallJumpTimeCtr > 0)
                    {
                        SwitchState(playerState.WALLJUMPING);
                       
                    }
                    else if(currentState == playerState.IDLE || currentState == playerState.RUNNING || currentState == playerState.LANDING || currentState == playerState.TURNING)
                    {
                        SwitchState(playerState.JUMPING);
                    }
                    
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
            else if (!isGrounded && isWalled && rb.linearVelocityY <= wallSlideSpeed)
            {
                SwitchState(playerState.WALLSLIDING);
            }
            else if (isGrounded && (Math.Abs(rb.linearVelocityX) > 0))
            {
                if(isTurning)
                {
                    SwitchState(playerState.TURNING);
                }
                else if(isLanding)
                {
                    SwitchState(playerState.LANDING);
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
            
            
            if (isLedge && rb.linearVelocityY <= 0)
            {
                if (((isFacingRight && xinput>0) || (!isFacingRight && xinput<0)))
                {
                    rb.linearVelocity = new Vector2(0, 0);
                    SwitchState(playerState.CLIMBINGLEDGE);
                }

            }
        }
        
        
    }


    private void HandleCounters()
    {
        
        if(jumpTimeCtr>0)
        {
            jumpTimeCtr -= Time.deltaTime;
        }
        if(jumpBufferTimeCtr >= 0)
        {
            jumpBufferTimeCtr -= Time.deltaTime;
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
        if(damageCooldownCtr >= 0)
        {
            damageCooldownCtr -= Time.deltaTime;
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
            isWalled2 = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckSpacing*0.5f), Vector2.right, wallCheckDistance, whatIsWall);
        }
        else
        {
            isWalled1 = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckSpacing), Vector2.left, wallCheckDistance, whatIsWall);
            isWalled2 = Physics2D.Raycast(transform.position + new Vector3(0, wallCheckSpacing*0.5f), Vector2.left, wallCheckDistance, whatIsWall);
        }

        if(isWalled1)
        {
            if((isFacingRight && xinput > 0)||(!isFacingRight && xinput < 0))
            {
                isWalled = true;
            }
            else
            {
                isWalled = false;
            }
        }
        else
        {
            isWalled = false;
        }


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

    public void HandleDeath()
    {
        if(gameObject.GetComponent<Stats>().currentHP <=0 && !isKnockedBack)
        {
            PlayerDeath();
        }
    }

    [ContextMenu("TAKE A HIT")]
    public void TakeHit()
    {
        if(!isTakingHit && damageCooldownCtr<=0)
        {
            damageCooldownCtr = 1.5f;
            Debug.Log("TakeHit");
            isTakingHit = true;
            isDashing = false;
            KnockBack();
        }
        
    }

    public void HitOver()
    {
        Debug.Log("HitOver");
        KnockBackOver();
        ReleaseActionLock();
        canDash = true;
        canDoubleJump = true;
        isTakingHit = false;
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
            transform.position = new Vector3(transform.position.x + 0.55f, transform.position.y + 1.3f);
        }
        else
        {
            transform.position = new Vector3(transform.position.x - 0.55f, transform.position.y + 1.3f);
        }
        
    }

    public void PlayerDeath()
    {
        SwitchState(playerState.DEATH);
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
        Gizmos.DrawLine(transform.position + new Vector3(0, wallCheckSpacing*0.5f), transform.position + new Vector3(0, wallCheckSpacing*0.5f) + new Vector3(wallCheckDistance, 0));
        //Gizmos.DrawWireSphere(attackPointUp.position, attackRadius);
        //Gizmos.DrawWireSphere(attackPointDown.position, attackRadius);
    }


}
