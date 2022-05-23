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

    public bool _facingRight = true;

    public bool FacingRight
    {
        get => _facingRight;
        set => _facingRight = value;
    }

    private Rigidbody2D _rb;
    private Animator _anim;

    public bool _verticalMovement = false;
    private bool _isGhost = false;

    public bool VerticalMovement
    {
        get => _verticalMovement;
        set => _verticalMovement = value;
    }

    // Jump Variables
    public bool _isGrounded;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;
    private int _remainingJumps;

    public float jumpHangTime = .1f;
    private float jumpHangCounter;

    public float jumpBufferLength = .1f;
    private float jumpBufferCount;

    // Wall Slide Variables
    public bool _isOnWall;
    public Transform frontCheck;
    public bool _wallSliding;
    public float wallSlidingSpeed;

    // private bool _wallGrab = false;
    private bool _possess = false;

    public Transform topCheck;


    // Wall Jump Variables
    private bool wallJumping;
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

    void ResetExtraJumps()
    {
        _remainingJumps = k_RemainingJumpsAmount;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        ResetExtraJumps();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _isGhost = !_isGhost;
        }

        _moveInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");

        if (_isGrounded)
        {
            ResetExtraJumps();
        }

        PerformMovement();
        HandleJump();
        HandleWallSlide();
        // HandleWallJump();

        _anim.SetBool(s_WalkingHash, _moveInput != 0);
        _anim.SetBool(s_JumpingHash, !_isGrounded);
        _anim.SetBool(s_GhostHash, _isGhost);
        _anim.SetBool(s_PossessHash, _possess);
    }

    private void HandleJump()
    {
        // Handle Jump Hang Time
        if (_isGrounded)
        {
            jumpHangCounter = jumpHangTime;
        }
        else
        {
            jumpHangCounter -= Time.deltaTime;
        }

        // Player can't Jump when it's Possessing
        if (Input.GetKeyDown(KeyCode.Space) && !_possess)
        {
            // Handle Jump Buffer Time
            jumpBufferCount = jumpBufferLength;

            // // Handle Wall Grab
            // if (_wallSliding)
            // {
            //     wallJumping = true;
            //     Invoke(nameof(ResetWallJumping), wallJumpTime);
            // }
        }

        // Perform the Jump
        if (jumpBufferCount > 0 && _remainingJumps > 0 && jumpHangCounter > 0)
        {
            // _rb.velocity = Vector2.up * jumpForce;
            _rb.AddRelativeForce(Vector2.up * jumpForce);
            jumpBufferCount = 0;
            _remainingJumps--;
        }

        // Gradually decrease jumpBufferCount over time
        if (jumpBufferCount >= 0)
            jumpBufferCount -= Time.deltaTime;

        // If we're moving upwards when we release the jump button
        if (Input.GetKeyUp(KeyCode.Space) && _rb.velocity.y > 0)
        {
            _rb.velocity = new(_rb.velocity.x, _rb.velocity.y * .5f);
            jumpHangCounter = 0;
        }
    }

    public void HandleFlip()
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

    void FixedUpdate()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        _isOnWall = Physics2D.OverlapCircle(frontCheck.position, checkRadius, whatIsGround);
    }

    void Flip()
    {
        transform.localScale = new(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        SwapFacingRight();
    }

    private void PerformMovement()
    {
        // Player can not move horizontally if it's on wall and not grounded
        if (_isOnWall && !_isGrounded && _possess)
        {
            // Debug.Log("Movement Locked");
            return;
        }

        HandleFlip();
        _rb.velocity = new Vector2(_moveInput * speed, _rb.velocity.y);
        // if(!_verticalMovement)
        // _rb.velocity = transform.right * speed * _moveInput;
        // else
        //     _rb.velocity = new Vector2(_rb.velocity.x, _moveInput * speed) ;
    }

    private void HandleWallSlide()
    {
        // if (_isOnWall && !_isGrounded && _moveInput != 0)
        if (Input.GetKeyDown(KeyCode.LeftControl) && _isGhost)
        {
            _possess = true;
            GetComponent<BoxCollider2D>().size += new Vector2(k_PossessColliderXOffset, 0);
            frontCheck.localPosition += new Vector3(k_PossessFrontCheckerXOffset, 0, 0);

            // _wallSliding = true;
            // if(_isOnWall)
            //     _wallGrab = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            // _wallGrab = false;
            _possess = false;
            GetComponent<BoxCollider2D>().size -= new Vector2(k_PossessColliderXOffset, 0);
            frontCheck.localPosition -= new Vector3(k_PossessFrontCheckerXOffset, 0, 0);

            transform.rotation = Quaternion.identity;
            _verticalMovement = false;
            _rb.gravityScale = k_GravityScale;

            if (inverseMovement)
            {
                SwapFacingRight();
                inverseMovement = false;
            }
        }

        if (_possess && _isOnWall)
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
            // This is the code to fix incorrect behaviour when walking on left wall
            // if (!_facingRight)
            // {
            //     SwapFacingRight()
            // }

            _isOnWall = false;
            _verticalMovement = !_verticalMovement;
        }
        else
        {
        }

        // Handle Vertical Movement
        if (_isGrounded && _possess && _verticalMovement)
        {
            _rb.gravityScale = 0;
            _rb.velocity = new Vector2(0, _verticalInput * speed);
        }

        // if (_wallSliding)
        // {
        //     _rb.velocity = new Vector2(_rb.velocity.x,
        //         Mathf.Clamp(_rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        // }
    }

    private void SwapFacingRight()
    {
        _facingRight = !_facingRight;
    }

    private void HandleWallJump()
    {
        if (wallJumping)
        {
            _rb.velocity = new Vector2(xWallForce * -_moveInput, yWallForce);
        }
    }

    void ResetWallJumping()
    {
        wallJumping = false;
    }
}