using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SeniPhysicsController : MonoBehaviour
{
    // ------------------- DEBUG -------------------
    public bool Debug_Grounded { get { return _isGrounded; } }
    public bool Debug_Sliding { get { return _isSliding; } }
    public bool Debug_Crouching { get { return _isCrouching; } }
    public bool Debug_Blocked { get { return _movementBlocked; } }
    public float Debug_Velocity { get { return _rb.velocity.magnitude; } }
    // ------------------- DEBUG -------------------

    public float MaxWalkSpeed = 5f;
    public float MaxRunSpeed = 8f;
    public float MaxCrouchSpeed = 3f;
    public float MaxAirSpeed = 5f;
    public float Acceleration = 50f;
    public float Deacceleration = 50f;
    public float AirAcceleration = 10f;
    public float JumpForce = 10f;
    public float WalkableSlopeAngle = 45f;
    public float GroundRayTraceRange = 0.1f;
    public float AlignWithGroundSpeed = 10f;
    public LayerMask CollisionMask;
    public Transform BodySprite;

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private CharacterBodyRotation _bodyRotation;
    private float _currentMaxSpeed;
    private float _inputX;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _movementBlocked;
    private bool _isSliding;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _bodyRotation = GetComponentInChildren<CharacterBodyRotation>();
        _currentMaxSpeed = MaxWalkSpeed;
    }

    private void FixedUpdate()
    {
        DetectSlopeAndRotatePlayer();
    }

    void Update()
    {
        _inputX = Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.F))
        {
            Crouch(true);
        }
        else
        {
            Crouch(false);
        }

        if (_isGrounded && _isSliding == false)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }

            if (Input.GetKey(KeyCode.LeftShift) && _isCrouching == false)
            {
                Run();
            }
            else if (_isCrouching == false)
            {
                Walk();
            }

            if (_movementBlocked == false)
            {
                if (_inputX != 0)
                {
                    MoveOnGround();
                }
                else
                {
                    Breaking();
                }
            }
        }
        else
        {
            if (_inputX != 0 && _isSliding == false)
            {
                MoveAir();
            }
        }
    }

    void Run()
    {
        _currentMaxSpeed = MaxRunSpeed;
    }

    void Walk()
    {
        _currentMaxSpeed = MaxWalkSpeed;
    }

    void Crouch(bool crouch)
    {
        if (crouch)
        {
            if (_isCrouching == false)
            {
                _col.size = new Vector2(_col.size.x, 0.5f);
                _currentMaxSpeed = MaxCrouchSpeed;
                _isCrouching = true;
                transform.position = transform.position + -transform.up * 0.25f;
                BodySprite.localScale = new Vector3(0.5f, 0.5f, 1);
            }
        }
        else
        {
            if (_isCrouching)
            {
                Vector2 castOrigin = _col.transform.position + transform.up * 1;
                RaycastHit2D hit = Physics2D.CircleCast(castOrigin, 0.25f, transform.up, 0f, CollisionMask);

                if (hit == false)
                {
                    _col.size = new Vector2(_col.size.x, 1f);
                    Walk();
                    _isCrouching = false;
                    transform.position = transform.position + transform.up * 0.25f;
                    BodySprite.localScale = new Vector3(0.5f, 1, 1);
                }
            }
        }

    }

    void DetectSlopeAndRotatePlayer()
    {
        Vector3 colliderSizeOffset = _col.transform.position + transform.up * _col.offset.y + transform.up * -_col.size.y / 2;


        Vector2 raycastOriginLeft = colliderSizeOffset + transform.right * -0.2f + transform.up * 0.25f;
        Vector2 raycastOriginRight = colliderSizeOffset + transform.right * 0.2f + transform.up * 0.25f;
        RaycastHit2D hitLeft = Physics2D.Raycast(raycastOriginLeft, -transform.up, GroundRayTraceRange, CollisionMask);
        RaycastHit2D hitRight = Physics2D.Raycast(raycastOriginRight, -transform.up, GroundRayTraceRange, CollisionMask);
        Debug.DrawRay(raycastOriginLeft, -transform.up * GroundRayTraceRange, Color.red);
        Debug.DrawRay(raycastOriginRight, -transform.up * GroundRayTraceRange, Color.yellow);


        Vector2 perpendicularLine = hitLeft.point - hitRight.point;
        // rotate 90� Clockwise
        Vector2 normal = new Vector2(perpendicularLine.y, -perpendicularLine.x).normalized;

        if (hitLeft || hitRight)
        {
            bool leftRayOnSlope = false;
            bool rightRayOnSlope = false;
            float avarageDistance = Mathf.Infinity;
            if (hitLeft)
            {
                leftRayOnSlope = IsOnSlope(hitLeft.normal);
            }
            if (hitRight)
            {
                rightRayOnSlope = IsOnSlope(hitRight.normal);
            }


            if (hitLeft && !hitRight)
            {
                avarageDistance = hitLeft.distance;
                if (leftRayOnSlope == false)
                {
                    RotateTransformUp(hitLeft.normal);
                }
            }
            else if (!hitLeft && hitRight)
            {
                avarageDistance = hitRight.distance;
                if (rightRayOnSlope == false)
                {
                    RotateTransformUp(hitRight.normal);
                }
            }
            else if (hitLeft && hitRight)
            {
                avarageDistance = (hitLeft.distance + hitRight.distance) / 2;
                if (IsOnSlope(normal) == false)
                {
                    RotateTransformUp(normal);
                }
            }

            if (avarageDistance < 0.3f)
            {
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }

            if (leftRayOnSlope && rightRayOnSlope)
            {
                _isSliding = true;
            }
            else
            {
                _isSliding = false;
            }
        }
        else
        {
            _isGrounded = false;
            RotateTransformUp(Vector2.up);
        }
    }

    void MoveOnGround()
    {
        float dot = CalculateDotProduct();

        float moveSpeed = Mathf.MoveTowards(dot, _currentMaxSpeed * _inputX, Acceleration * Time.deltaTime);
        Vector2 myVel = new Vector2(transform.right.x, transform.right.y);
        _rb.velocity = myVel * moveSpeed;
        _bodyRotation.PlayerMoveDir = Mathf.RoundToInt(Mathf.Sign(_inputX));
    }

    void Breaking()
    {
        float dot = CalculateDotProduct();

        float moveSpeed = Mathf.MoveTowards(dot, 0, Deacceleration * Time.deltaTime);
        Vector2 myVel = new Vector2(transform.right.x, transform.right.y);
        _rb.velocity = myVel * moveSpeed + new Vector2(0, -Physics2D.gravity.y * _rb.mass * _rb.gravityScale * Time.deltaTime);
        _bodyRotation.PlayerMoveDir = 0;
    }

    void MoveAir()
    {
        float moveX = Mathf.MoveTowards(_rb.velocity.x, MaxAirSpeed * _inputX, AirAcceleration * Time.deltaTime);
        _rb.velocity = new Vector2(moveX, _rb.velocity.y);
        _bodyRotation.PlayerMoveDir = Mathf.RoundToInt(Mathf.Sign(_inputX));
    }


    void Jump()
    {
        _isGrounded = false;
        _rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
        StartCoroutine(Countdown(0.2f));
    }

    // ---------------------- HELPER METHODS START ----------------------

    bool IsOnSlope(Vector2 normal)
    {
        var slope = Mathf.Abs(Vector2.Angle(normal, Vector2.up));
        if (slope < WalkableSlopeAngle)
        {
            return false;
        }

        return true;
    }

    void RotateTransformUp(Vector2 normal)
    {
        transform.up = Vector2.Lerp(transform.up, normal, AlignWithGroundSpeed * Time.deltaTime);
    }

    IEnumerator Countdown(float seconds)
    {
        _movementBlocked = true;
        yield return new WaitForSeconds(seconds);

        _movementBlocked = false;
    }

    float CalculateDotProduct()
    {
        Vector2 currentVel = _rb.velocity;
        Vector2 currentDirection = transform.right;
        Vector2 currentDirectionNormal = currentDirection.normalized;

        return Vector2.Dot(currentVel, currentDirectionNormal);
    }
    // ---------------------- HELPER METHODS END----------------------
}
