using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Lumin;

public class Movement : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb2d;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    private float walkSpeed = 5f;
    private float jumpPower = 20f;
    private bool isFacingRight = true;

    private Animator anim;
    float horizontalMovement;

    private bool isWallSliding;
    private float wallSlidingSpeed = 1f;

    private float coyoteTime = .2f;
    private float coyoteTimeCounter;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(9f, 9f);


    
    [Header("Dashing")]
    [SerializeField] private float dashVelocity = 8f;
    [SerializeField] private float dashTime = 0.5f;
    private Vector2 dashDirection;
    private bool isDashing;
    private bool canDash = true;
    private TrailRenderer trailRenderer;


    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        trailRenderer = gameObject.GetComponent<TrailRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isWallJumping)
        {
            Move();
        }
        if (isGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }



        if (coyoteTimeCounter > 0f && Input.GetButtonDown("Jump"))
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpPower);


        }

        if (Input.GetButtonDown("Jump") && rb2d.velocity.y > 0f)
        {
            rb2d.velocity = new Vector2(rb2d.velocity.x, rb2d.velocity.y * 0.5f);

            coyoteTimeCounter = 0f;

        }


        Dash();
        WallSlide();
        WallJump();

        

    }
    void FixedUpdate()
    {
        //Debug.Log(jumpRemain);
        Debug.Log(horizontalMovement);
        Debug.Log(isWallSliding);
        Debug.Log(isOnWall());



    }




    private void Move()
    {
        float horizontalMovement = Input.GetAxis("Horizontal") * walkSpeed;
        Vector2 newVelocity;
        newVelocity.x = horizontalMovement;
        newVelocity.y = rb2d.velocity.y;
        rb2d.velocity = newVelocity;
        if (!isFacingRight && horizontalMovement > 0f)
        {
            Flip();
        }
        if (isFacingRight && horizontalMovement < 0f)
        {
            Flip();
        }
    }
    private bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }
    private void Flip()
    {

        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private bool isOnWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }
    private void WallSlide()
    {
        if (isOnWall() && !isGrounded())
        {
            isWallSliding = true;
            rb2d.velocity = new Vector2(rb2d.velocity.x, Mathf.Clamp(rb2d.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke("StopWallJump");
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb2d.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
        Invoke("StopWallJump", wallJumpingDuration);
    }


    private void StopWallJump()
    {
        isWallJumping = false;
    }
    private void Dash()
    {
        var dashInput = Input.GetButtonDown("Dash");
        if (dashInput && canDash)
        {
            isDashing = true;
            canDash = false;
            trailRenderer.emitting = true;
            dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (dashDirection == Vector2.zero)
            {
                dashDirection = new Vector2(transform.localScale.x, 0);
            }
            StartCoroutine(StopDashing());

        }
        if (isDashing)
        {
            rb2d.velocity = dashDirection.normalized * dashVelocity;
            return;
        }
        if (isGrounded())
        {
            canDash = true;

        }
    }
    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashTime);
        trailRenderer.emitting = false;
        isDashing = false;
    }


}
   
 