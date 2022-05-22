using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    private float _moveInput;

    private bool _facingRight = true;

    private Rigidbody2D _rb;
    private Animator _anim;

    // Jump Variables
    private bool _isGrounded;
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

    // Wall Jump Variables
    private bool wallJumping;
    public float xWallForce;
    public float yWallForce;
    public float wallJumpTime;

    // Animator Hashes
    private static readonly int s_WalkingHash = Animator.StringToHash("walking");
    private static readonly int s_JumpingHash = Animator.StringToHash("jumping");

    // Const
    private const int k_RemainingJumpsAmount = 1;

    void ResetExtraJumps()
    {
        _remainingJumps = k_RemainingJumpsAmount;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        ResetExtraJumps();
    }

    void Update()
    {
        _moveInput = Input.GetAxis("Horizontal");

        HandleFlip();

        if (_isGrounded)
        {
            ResetExtraJumps();
        }

        HandleMovement();
        HandleJump();
        HandleWallSlide();
        HandleWallJump();

        _anim.SetBool(s_WalkingHash, _moveInput != 0);
        _anim.SetBool(s_JumpingHash, !_isGrounded);
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


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // Handle Jump Buffer Time
            jumpBufferCount = jumpBufferLength;

            // Handle Wall Slide
            if (_wallSliding)
            {
                wallJumping = true;
                Invoke(nameof(ResetWallJumping), wallJumpTime);
            }
        }

        // Perform the Jump
        if (jumpBufferCount > 0 && _remainingJumps > 0 && jumpHangCounter > 0)
        {
            _rb.velocity = Vector2.up * jumpForce;
            jumpBufferCount = 0;
            _remainingJumps--;
        }

        // Gradually decrease jumpBufferCount over time
        if (jumpBufferCount >= 0)
            jumpBufferCount -= Time.deltaTime;

        // If we're moving upwards when we release the jump button
        if (Input.GetKeyUp(KeyCode.UpArrow) && _rb.velocity.y > 0)
        {
            _rb.velocity = new(_rb.velocity.x, _rb.velocity.y * .5f);
            jumpHangCounter = 0;
        }
    }

    private void HandleFlip()
    {
        if (!_facingRight && _moveInput > 0)
        {
            Flip();
        }
        else if (_facingRight && _moveInput < 0)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        _isOnWall = Physics2D.OverlapCircle(frontCheck.position, checkRadius, whatIsGround);
    }

    private void HandleWallSlide()
    {
        if (_isOnWall && !_isGrounded && _moveInput != 0)
        {
            _wallSliding = true;
        }
        else
        {
            _wallSliding = false;
        }

        if (_wallSliding)
        {
            _rb.velocity = new Vector2(_rb.velocity.x,
                Mathf.Clamp(_rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
    }

    private void HandleWallJump()
    {
        if (wallJumping)
        {
            _rb.velocity = new Vector2(xWallForce * -_moveInput, yWallForce);
        }
    }

    private void HandleMovement()
    {
        _rb.velocity = new Vector2(_moveInput * speed, _rb.velocity.y);
    }

    void Flip()
    {
        transform.localScale = new(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        _facingRight = !_facingRight;
    }

    void ResetWallJumping()
    {
        wallJumping = false;
    }
}