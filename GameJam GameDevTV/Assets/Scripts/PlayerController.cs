using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    private float xInput;
    private float yInput;

    public bool _canMove = true;
    public bool facingRight = true;

    public bool FacingRight
    {
        get => facingRight;
        set => facingRight = value;
    }

    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _sprite;

    public bool _verticalMovement = false;
    public bool _isGhost = false;

    public bool VerticalMovement
    {
        get => _verticalMovement;
        set => _verticalMovement = value;
    }

    // Jump Variables
    public bool onGround;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;
    public int _remainingJumps;

    public float jumpHangTime = .1f;
    private float jumpHangCounter;

    public float jumpBufferLength = .1f;
    private float jumpBufferCount;

    // Wall Slide Variables
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public Transform rightCheck;
    public Transform leftCheck;
    public bool wallSliding;
    public float wallSlideSpeed;

    // Used to know if player is wanting to grab wall
    public bool wallGrab = false;

    // Used to know if player is actually climbing any wall (for anim purposes)
    public bool isClimbing = false;

    public Transform topCheck;


    // Wall Jump Variables
    private bool wallJumped;
    public float xWallForce;
    public float yWallForce;
    public float wallJumpTime;

    // Animator Hashes
    private static readonly int s_WalkingHash = Animator.StringToHash("walking");
    private static readonly int s_JumpingHash = Animator.StringToHash("jumping");
    private static readonly int s_GhostHash = Animator.StringToHash("ghost");
    private static readonly int s_PossessHash = Animator.StringToHash("possess");

    // Constants
    private const int k_RemainingJumpsAmount = 1;
    private const int k_GravityScale = 3;
    private const float k_PossessColliderXOffset = 0.88f;
    private const float k_PossessFrontCheckerXOffset = k_PossessColliderXOffset / 2;

    // This is set to true when player is on left or top wall
    public bool inverseMovement = false;
    public bool isColliderModified = false;
    public bool limitJump = false;

    private void ResetExtraJumps()
    {
        _remainingJumps = k_RemainingJumpsAmount;
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        ResetExtraJumps();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _isGhost = !_isGhost;
        }

        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");

        HandleJump();
        HandleWallMovement();
        // HandleWallJump();

        _anim.SetBool(s_WalkingHash, xInput != 0);
        _anim.SetBool(s_JumpingHash, !onGround);
        _anim.SetBool(s_GhostHash, _isGhost);
        _anim.SetBool(s_PossessHash, isClimbing);

    }
    
    private void FixedUpdate()
    {
        onGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        onRightWall = Physics2D.OverlapCircle(rightCheck.position, checkRadius, whatIsGround);
        onLeftWall = Physics2D.OverlapCircle(leftCheck.position, checkRadius, whatIsGround);
        onWall = onLeftWall || onRightWall;

        PerformMovement();
        PerformJump();
        PerformWallMovement();
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Player can't normal Jump when it's climbing
            if (!isClimbing)
            {
                // Handle Jump Buffer Time
                jumpBufferCount = jumpBufferLength;
            }

            if(onWall && !onGround && !_isGhost)
                WallJump();
                
            // // Handle Wall Grab
            // if (_wallSliding)
            // {
            //     wallJumping = true;
            //     Invoke(nameof(ResetWallJumping), wallJumpTime);
            // }
        }
        
        // If we're moving upwards when we release the jump button (Sensitive Jump)
        if (Input.GetKeyUp(KeyCode.Space) && _rb.velocity.y > 0 && (!wallJumped))
        {
            limitJump = true;
        }
    }
    

    private void PerformJump()
    {
        // Handle Jump Hang Time
        if (onGround)
        {
            jumpHangCounter = jumpHangTime;
        }
        else
        {
            jumpHangCounter -= Time.deltaTime;
        }

        // Sensitive Jump
        if (limitJump)
        {
            _rb.velocity = new(_rb.velocity.x, _rb.velocity.y * .5f);
            jumpHangCounter = 0;
            limitJump = false;
        }
        
        // Perform the Jump
        if (_remainingJumps > 0 && jumpBufferCount > 0 && jumpHangCounter > 0)   // && !onRightWall && !onLeftWall
        {
            _rb.velocity = Vector2.up * jumpForce * 2;
            // _rb.AddRelativeForce(Vector2.up * jumpForce * 100);
            jumpBufferCount = 0;
            _remainingJumps--;
        }

        // Gradually decrease jumpBufferCount over time
        if (jumpBufferCount >= 0)
            jumpBufferCount -= Time.deltaTime;

        // If we're falling, fall faster.
        if (!_isGhost && _rb.velocity.y < 0)
        {
            _rb.velocity += Vector2.down * 0.1f;
        }
    }

    private void WallJump()
    {
        wallSliding = false;
        StartCoroutine(DisableMovement(.08f));

        Vector2 wallDir = onRightWall ? Vector2.left : Vector2.right;
        
        _rb.velocity = new Vector2(_rb.velocity.x, 0);
        _rb.velocity += (Vector2.up / 3f + wallDir / 1.8f) * (jumpForce * 4);
        
        wallJumped = true;
    }

    private void HandleFlip()
    {
        if (!facingRight && xInput > 0 || facingRight && xInput < 0)
        {
            Flip();
        }
        // if (!_verticalMovement)
        // {
        //     if (!_facingRight && xInput > 0 || _facingRight && xInput < 0)
        //     {
        //         Flip();
        //     }
        // }
        // else
        // {
        // if (!_facingRight && yInput > 0 || _facingRight && yInput < 0)
        //     {
        //         Flip();
        //     }
        // }
    }

    private void Flip()
    {
        _sprite.flipX = !_sprite.flipX;
        SwapFacingRight();
    }

    private IEnumerator DisableMovement
        (float time = 0)
    {
        _canMove = false;
        yield return new WaitForSeconds(time);
        _canMove = true;
    }

    private void PerformMovement()
    {
        // Player can not move horizontally if it's climbing
        if (!_canMove || onRightWall && isClimbing)
        {
            // Debug.Log("Movement Locked");
            return;
        }

        HandleFlip();
        if (!wallJumped)
        {
            _rb.velocity = new Vector2(xInput * speed, _rb.velocity.y);
        }
        else
        {
            _rb.velocity = Vector2.Lerp(_rb.velocity, (new Vector2(xInput * speed, _rb.velocity.y)), 10 * Time.deltaTime);
        }
        // if(!_verticalMovement)
        // _rb.velocity = transform.right * speed * _moveInput;
        // else
        //     _rb.velocity = new Vector2(_rb.velocity.x, _moveInput * speed) ;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & whatIsGround) != 0)
        {
            if (onGround)
            {
                ResetExtraJumps();
                wallJumped = false;
            }

            if (onWall)
            {
                wallJumped = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & whatIsGround) != 0 && _isGhost && wallGrab && !onGround)
        {
            Unpossess();
        }
    }

    private void HandleWallMovement()
    {
        if (Input.GetKeyUp(KeyCode.LeftControl) && wallGrab)
        {
            wallGrab = false;
            isClimbing = false;
            
            if (_isGhost)
            {
                Unpossess();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            wallGrab = true;

            if (_isGhost && !isColliderModified)
            {
                GetComponent<BoxCollider2D>().size += new Vector2(k_PossessColliderXOffset, 0);
                rightCheck.localPosition += new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
                isColliderModified = true;
            }
        }
    }

    private void PerformWallMovement()
    {
        if (wallGrab)
        {
            if (_isGhost)
            {
                if (onRightWall)
                {
                    if (!inverseMovement)
                        transform.Rotate(Vector3.forward, facingRight ? 90f : -90);
                    else
                        transform.Rotate(Vector3.forward, facingRight ? -90f : 90);


                    if (inverseMovement && transform.rotation.eulerAngles.z is 0 or 90)
                    {
                        SwapFacingRight();
                        inverseMovement = false;
                    }
                    else if (!inverseMovement && transform.rotation.eulerAngles.z is 270 or 180)
                    {
                        SwapFacingRight();
                        inverseMovement = true;
                    }

                    onRightWall = false;
                    _verticalMovement = !_verticalMovement;

                    // isClimbing = true;
                }
                else if (onGround)
                {
                    // isClimbing = true;
                }
            }
            else
            {
                if (onWall)
                {
                    // isClimbing = true;
                    //_verticalMovement = true;
                }
            }
        }

        // !WallJumped is also needed so wall jump can be performed as expected (it avoid player from receiving Y slide velocity)
        wallSliding = onWall && !onGround && xInput != 0 && !wallGrab && !wallJumped;

        isClimbing = wallGrab && onWall;

        // Perform vertical movement
        // if ((onGround || isClimbing) && wallGrab && _verticalMovement)
        if(isClimbing && !wallJumped)
        {
            _rb.gravityScale = 0;
            _rb.velocity = new Vector2(0, yInput * speed);
        }
        else
        {
            _rb.gravityScale = k_GravityScale;
        }
        
        if (wallSliding)
        {
            // If player is moving against the wall dont move horizontally, otherwise move as normal
            //float push = (_isOnWall && _moveInput != 0) ? 0 : _rb.velocity.x;

            _rb.velocity = new Vector2(_rb.velocity.x, -wallSlideSpeed);
        }
    }

    private void Unpossess()
    {
        if (isColliderModified)
        {
            GetComponent<BoxCollider2D>().size -= new Vector2(k_PossessColliderXOffset, 0);
            rightCheck.localPosition -= new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
            isColliderModified = false;
        }

        transform.rotation = Quaternion.identity;
        _verticalMovement = false;
        _rb.gravityScale = k_GravityScale;

        if (inverseMovement)
        {
            SwapFacingRight();
            inverseMovement = false;
        }

        isClimbing = false;
    }

    private void SwapFacingRight()
    {
        facingRight = !facingRight;
    }

    // private void HandleWallJump()
    // {
    //     if (wallJumped)
    //     {
    //         _rb.velocity = new Vector2(xWallForce * -_moveInput, yWallForce);
    //     }
    // }
    //
    // void ResetWallJumping()
    // {
    //     wallJumped = false;
    // }
}