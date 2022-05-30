
using System.Collections;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    private float xInput;
    private float yInput;

    public bool canMove = true;
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
    public bool isGhost = false;

    public bool VerticalMovement
    {
        get => _verticalMovement;
        set => _verticalMovement = value;
    }

    // Jump Variables
    private bool collidedWithGround;
    public bool onGround;
    public Transform groundCheck;
    public float checkRadius;
    public LayerMask whatIsGround;
    public int _remainingJumps;

    public float jumpHangTime = .1f;
    private float jumpHangCounter;

    public float jumpBufferLength = .1f;
    private float jumpBufferCount;

    public bool onTop;
    public bool onCorner;

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

    public ParticleSystem jumpParticles;


    // Animator Hashes
    private static readonly int s_WalkingHash = Animator.StringToHash("walking");
    private static readonly int s_JumpingHash = Animator.StringToHash("jumping");
    private static readonly int s_GhostHash = Animator.StringToHash("ghost");
    private static readonly int s_DeathHash = Animator.StringToHash("death");
    private static readonly int s_ReviveHash = Animator.StringToHash("revive");
    private static readonly int s_GrabHash = Animator.StringToHash("grab");
    private static readonly int s_XInputHash = Animator.StringToHash("xInput");
    private static readonly int s_YInputHash = Animator.StringToHash("yInput");

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

    public void Die()
    {
        AudioManager.instance.PlaySound("DeathSound");
        StartCoroutine(DisableMovement(.6f));
        _rb.velocity = Vector2.zero;
        _anim.SetTrigger(s_DeathHash);
    }

    public void Revive()
    {
        AudioManager.instance.PlaySound("DeathSound");
        StartCoroutine(DisableMovement(.5f));
        _rb.velocity = Vector2.zero;
        isGhost = false;
        _anim.SetTrigger(s_ReviveHash);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        ResetExtraJumps();
        if (isGhost)
        {
            Die();
            AudioManager.instance.PauseSound("AliveTheme");
        }
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     isGhost = !isGhost;
        //     _anim.SetTrigger(isGhost ? s_GhostHash : s_ReviveHash);
        // }

        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");

        HandleJump();
        HandleWallMovement();
        // HandleWallJump();

        _anim.SetBool(s_WalkingHash, xInput != 0 && !onWall);
        _anim.SetBool(s_JumpingHash, !onGround);
        _anim.SetBool(s_GrabHash, isClimbing || wallGrab && onGround && isGhost);
        _anim.SetFloat(s_XInputHash, xInput);
        _anim.SetFloat(s_YInputHash, yInput);
    }

    private void FixedUpdate()
    {
        HandleCheckers();
        HandleFlip();
        PerformMovement();
        PerformJump();
        PerformWallMovement();
    }

    private void HandleCheckers()
    {
        onGround = Physics2D.OverlapBox(groundCheck.position, new Vector2(.05f,.2f), 0, whatIsGround);
        onTop = Physics2D.OverlapCircle(topCheck.position, checkRadius, whatIsGround);
        onLeftWall = Physics2D.OverlapBox(leftCheck.position, new Vector2(.2f,.8f), 0,whatIsGround,2f);
        onRightWall = Physics2D.OverlapBox(rightCheck.position, new Vector2(.2f,.8f),0, whatIsGround,2f);
        onWall = onLeftWall || onRightWall;
        onCorner = onWall && (onGround || onTop);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(leftCheck.position,new Vector2(.2f,.8f));
        Gizmos.DrawWireCube(rightCheck.position,new Vector2(.2f,.8f));
        Gizmos.DrawWireCube(groundCheck.position,new Vector2(.05f,.2f));
        Gizmos.DrawWireSphere(topCheck.position,checkRadius);
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

            if (onWall && !onGround && !isGhost)
                WallJump();
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
        if (jumpBufferCount > 0 && jumpHangCounter > 0) // && !onWall && _remainingJumps > 0
        {
            _rb.velocity = Vector2.up * jumpForce * 2;
            // _rb.AddRelativeForce(Vector2.up * jumpForce * 100);
            jumpBufferCount = 0;
            // _remainingJumps--;

            if (!isGhost)
            {
                jumpParticles.Play();
                AudioManager.instance.PlaySound("Jump1");
            }
        }

        // Gradually decrease jumpBufferCount over time
        if (jumpBufferCount >= 0)
            jumpBufferCount -= Time.deltaTime;

        // If we're falling, fall faster.
        if (!isGhost && _rb.velocity.y < 0)
        {
            _rb.velocity += Vector2.down * 0.1f;
        }

        if ((onGround && collidedWithGround) || onCorner)
        {
            ResetExtraJumps();
            collidedWithGround = false;
        }
    }

    private void WallJump()
    {
        wallSliding = false;
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallJumpDir = onRightWall ? Vector2.left : Vector2.right;

        _sprite.flipX = onRightWall;
        facingRight = !onRightWall;

        _rb.velocity = new Vector2(_rb.velocity.x, 0);
        _rb.velocity += (Vector2.up / 2.6f + wallJumpDir / 1.6f) * (jumpForce * 4);

        wallJumped = true;
    }

    private void HandleFlip()
    {
        if (!_verticalMovement)
        {
            if (!facingRight && xInput > 0 || facingRight && xInput < 0)
            {
                Flip();
            }
        }
        else
        {
            if (onRightWall && (!facingRight && yInput > 0 || facingRight && yInput < 0))
            {
                Flip();
            }else if (onLeftWall && (facingRight && yInput > 0 || !facingRight && yInput < 0))
            {
                Flip();
            }

        }
    }

    private void Flip()
    {
        _sprite.flipX = !_sprite.flipX;
        SwapFacingRight();
    }

    private IEnumerator DisableMovement
        (float time = 0)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    private void PerformMovement()
    {
        // Player can not move horizontally if it's climbing
        if (!canMove || isClimbing && onWall && !onCorner)
        {
            // Debug.Log("Movement Locked");
            return;
        }

        if (!wallJumped)
        {
            _rb.velocity = new Vector2(xInput * speed, _rb.velocity.y);
        }
        else
        {
            _rb.velocity = Vector2.Lerp(_rb.velocity, (new Vector2(xInput * speed, _rb.velocity.y)),
                10 * Time.deltaTime);
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
            if (wallGrab && isGhost)
            {
                ModifyCollider(true);
            }

            if (!onWall)
                collidedWithGround = true;

            if (onWall || onGround)
            {
                wallJumped = false;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & whatIsGround) != 0 && isGhost && wallGrab && !onGround && !onTop)
        {
            Unpossess();
        }
    }

    private void ModifyCollider(bool increase)
    {
        if (increase)
        {
            if (isColliderModified)
                return;

            GetComponent<BoxCollider2D>().size += new Vector2(k_PossessColliderXOffset, 0);
            rightCheck.localPosition += new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
            leftCheck.localPosition -= new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
            isColliderModified = true;
        }
        else
        {
            if (!isColliderModified)
                return;

            GetComponent<BoxCollider2D>().size -= new Vector2(k_PossessColliderXOffset, 0);
            rightCheck.localPosition -= new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
            leftCheck.localPosition += new Vector3(k_PossessFrontCheckerXOffset, 0, 0);
            isColliderModified = false;
        }
    }


    private void HandleWallMovement()
    {
        if (Input.GetKeyUp(KeyCode.LeftControl) && wallGrab)
        {
            wallGrab = false;
            isClimbing = false;

            if (isGhost)
            {
                Unpossess();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            wallGrab = true;
            if (isGhost)
            {
                ModifyCollider(true);
            }
        }
    }

    private void PerformWallMovement()
    {
        // !WallJumped is also needed so wall jump can be performed as expected (it avoid player from receiving Y slide velocity)
        wallSliding = onWall && !onGround && xInput != 0 && !wallGrab && !wallJumped;

        isClimbing = wallGrab && (isGhost ? onWall || onTop || onGround : onWall);

        if (isGhost)
        {
            // if(!_verticalMovement)
            _sprite.flipY = isClimbing && onTop && !onCorner;

            if (isClimbing)
            {
                if (onWall)
                {
                    var rot = _sprite.transform.localRotation;
                    rot.eulerAngles = new Vector3(0, 0, onRightWall ? 90 : -90);
                    _sprite.transform.localRotation = rot;

                    _verticalMovement = true;
                }
                else
                {
                    _sprite.transform.localRotation = Quaternion.identity;
                    _verticalMovement = false;
                }
            }
        }

        PerformClimbMovement();
        PerformWallSlide();
    }

    private void PerformClimbMovement()
    {
        // Perform vertical movement when climbing
        // if ((onGround || isClimbing) && wallGrab && _verticalMovement)
        if (isClimbing && !wallJumped)
        {
            _rb.gravityScale = 0;

            // Player can move horizontally while climbing when it's on ground, top or corner
            var xVel = onTop || onGround ? _rb.velocity.x : 0;
            // Ghost cannot move vertically when possessing ground or top
            var yVel = isGhost && !onCorner && (onTop || onGround) ? 0 : yInput * speed;
            _rb.velocity = new Vector2(xVel, yVel * .5f);
        }
        else
        {
            _rb.gravityScale = k_GravityScale;
        }
    }

    private void PerformWallSlide()
    {
        // Perform Wall Slide
        if (wallSliding && !isGhost)
        {
            // If player is moving against the wall dont move horizontally, otherwise move as normal
            //float push = (_isOnWall && _moveInput != 0) ? 0 : _rb.velocity.x;

            _rb.velocity = new Vector2(_rb.velocity.x, -wallSlideSpeed);
        }
    }

    private void Unpossess()
    {
        // This is done to keep modified collider when jumping when possessing wall
        if (!wallGrab)
            ModifyCollider(false);

        _sprite.transform.rotation = Quaternion.identity;
        _verticalMovement = false;
        _rb.gravityScale = k_GravityScale;

        // if (inverseMovement)
        // {
        //     SwapFacingRight();
        //     inverseMovement = false;
        // }

        isClimbing = false;
    }

    private void SwapFacingRight()
    {
        facingRight = !facingRight;
    }
}