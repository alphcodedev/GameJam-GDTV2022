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
    private float _moveInput;
    private float _verticalInput;

    public bool _canMove = true;
    public bool _facingRight = true;

    public bool FacingRight
    {
        get => _facingRight;
        set => _facingRight = value;
    }

    private Rigidbody2D _rb;
    private Animator _anim;

    public bool _verticalMovement = false;
    public bool _isGhost = false;

    public bool VerticalMovement
    {
        get => _verticalMovement;
        set => _verticalMovement = value;
    }

    // Jump Variables
    public bool _isOnGround;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;
    public int _remainingJumps;

    public float jumpHangTime = .1f;
    private float jumpHangCounter;

    public float jumpBufferLength = .1f;
    private float jumpBufferCount;

    // Wall Slide Variables
    public bool _isFacingWall;
    public bool _isBackWall;
    public Transform frontCheck;
    public Transform backCheck;
    public bool _wallSliding;
    public float wallSlideSpeed;

    // Used to know if player is wanting to grab wall
    public bool _wallGrab = false;

    // Used to know if player is actually climbing any wall (for anim purposes)
    public bool _isClimbing = false;

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
        ResetExtraJumps();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _isGhost = !_isGhost;
        }

        _moveInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");

        HandleJump();
        HandleWallMovement();
        // HandleWallJump();

        _anim.SetBool(s_WalkingHash, _moveInput != 0);
        _anim.SetBool(s_JumpingHash, !_isOnGround);
        _anim.SetBool(s_GhostHash, _isGhost);
        _anim.SetBool(s_PossessHash, _isClimbing);

    }
    
    private void FixedUpdate()
    {
        _isOnGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        _isFacingWall = Physics2D.OverlapCircle(frontCheck.position, checkRadius, whatIsGround);
        _isBackWall = Physics2D.OverlapCircle(backCheck.position, checkRadius, whatIsGround);
        
        PerformMovement();
        PerformJump();
        PerformWallMovement();
    }

    private void HandleJump()
    {
        // Player can't Jump when it's climbing
        if (Input.GetKeyDown(KeyCode.Space) && !_isClimbing)
        {
            // Handle Jump Buffer Time
            jumpBufferCount = jumpBufferLength;

            if((_isFacingWall || _isBackWall) && !_isOnGround && !_isGhost)
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
        if (_isOnGround)
        {
            jumpHangCounter = jumpHangTime;
        }
        else
        {
            jumpHangCounter -= Time.deltaTime;
        }

        if (limitJump)
        {
            _rb.velocity = new(_rb.velocity.x, _rb.velocity.y * .5f);
            jumpHangCounter = 0;
            limitJump = false;
        }
        

        // Perform the Jump
        if (_remainingJumps > 0 && jumpBufferCount > 0 && jumpHangCounter > 0 && !_isFacingWall && !_isBackWall)
        {
            _rb.velocity = Vector2.up * jumpForce * 2;
            // _rb.AddRelativeForce(Vector2.up * jumpForce * 100);
            jumpBufferCount = 0;
            _remainingJumps--;
        }

        // Gradually decrease jumpBufferCount over time
        if (jumpBufferCount >= 0)
            jumpBufferCount -= Time.deltaTime;

        // If we're falling
        if (!_isGhost && _rb.velocity.y < 0)
        {
            _rb.velocity += Vector2.down * 0.1f;
        }
    }


    // TODO Que cuando salte mirando para el otro lado funcione igual
    private void WallJump()
    {
        _wallSliding = false;
        StartCoroutine(DisableMovement(.08f));

        Vector2 wallDir;
        if (_isFacingWall && _facingRight || !_isFacingWall && !_facingRight || _isBackWall && !_facingRight)
        {
            wallDir = Vector2.left;
        }else
        {
            wallDir = Vector2.right;
        }
        
        Debug.Log(wallDir);

        _rb.velocity = new Vector2(0, 0);
        _rb.velocity += (Vector2.up / 3.2f + wallDir / 1.8f) * jumpForce * 5;
        
        wallJumped = true;
    }

    private void HandleFlip()
    {
        if (!_verticalMovement)
        {
            if (!_facingRight && _moveInput > 0 || _facingRight && _moveInput < 0)
            {
                Flip();
            }
        }
        else
        {
            if (!_facingRight && _verticalInput > 0 || _facingRight && _verticalInput < 0)
            {
                Flip();
            }
        }
    }

    private void Flip()
    {
        // transform.localScale = new(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        transform.Rotate(Vector3.up,180,Space.World);
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
        if (!_canMove || _isFacingWall && _isClimbing)
        {
            // Debug.Log("Movement Locked");
            return;
        }

        HandleFlip();
        if (!wallJumped)
        {
            _rb.velocity = new Vector2(_moveInput * speed, _rb.velocity.y);
        }
        else
        {
            _rb.velocity = Vector2.Lerp(_rb.velocity, (new Vector2(_moveInput * speed, _rb.velocity.y)), 10 * Time.deltaTime);
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
            if (_isOnGround && other.gameObject.name == "Ground")
            {
                ResetExtraJumps();
                wallJumped = false;
            }

            if (_isFacingWall || _isBackWall)
            {
                wallJumped = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & whatIsGround) != 0 && _isGhost && _wallGrab && !_isOnGround)
        {
            Unpossess();
        }
    }

    private void HandleWallMovement()
    {
        if (Input.GetKeyUp(KeyCode.LeftControl) && _wallGrab)
        {
            _wallGrab = false;
            if (_isGhost)
            {
                Unpossess();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            _wallGrab = true;

            if (_isGhost && !isColliderModified)
            {
                GetComponent<BoxCollider2D>().size += new Vector2(k_PossessColliderXOffset, 0);
                frontCheck.localPosition += new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
                isColliderModified = true;
            }
        }
    }

    private void PerformWallMovement()
    {
        if (_wallGrab)
        {
            if (_isGhost)
            {
                if (_isFacingWall)
                {
                    if (!inverseMovement)
                        transform.Rotate(Vector3.forward, _facingRight ? 90f : -90);
                    else
                        transform.Rotate(Vector3.forward, _facingRight ? -90f : 90);


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

                    _isFacingWall = false;
                    _verticalMovement = !_verticalMovement;

                    _isClimbing = true;
                }
                else if (_isOnGround)
                {
                    _isClimbing = true;
                }
            }
            else
            {
                if (_isFacingWall)
                {
                    _isClimbing = true;
                    
                }
            }
        }

        _wallSliding = _isFacingWall && !_isOnGround && _moveInput != 0 && !_wallGrab && !wallJumped;

        // Perform vertical movement
        if (_isOnGround && _wallGrab && _verticalMovement)
        {
            _rb.gravityScale = 0;
            _rb.velocity = new Vector2(0, _verticalInput * speed);
        }
        
        if (_wallSliding)
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
            frontCheck.localPosition -= new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
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

        _isClimbing = false;
    }

    private void SwapFacingRight()
    {
        _facingRight = !_facingRight;
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