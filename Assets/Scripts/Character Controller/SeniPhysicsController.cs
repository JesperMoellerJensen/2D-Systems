using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SeniPhysicsController : MonoBehaviour
{
    // ------------------- DEBUG -------------------
    public bool Debug_Ground { get { return _isGrounded; } }
    public bool Debug_Sliding { get { return _isSliding; } }
    public bool Debug_Blocked { get { return _movementBlocked; } }
    public float Debug_Slope { get { return _slopeAngle; } }
    public float Debug_Velocity { get { return _rb.velocity.magnitude; } }
    // ------------------- DEBUG -------------------

    public float MaxSpeed = 10f;
	public float Acceleration = 50f;
	public float Deacceleration = 50f;
	public float AirAcceleration = 10f;
	public float JumpForce = 10f;
	public float WalkableSlopeAngle = 45f;
	public float GroundRayTraceRange = 0.1f;
	public LayerMask CollisionMask;

	private Rigidbody2D _rb;
	private CharacterBodyRotation _bodyRotation;
	private float _inputX;
    private float _slopeAngle;
    private bool _isGrounded;
	private bool _movementBlocked;
	private bool _isSliding;

	void Start()
	{
		_rb = GetComponent<Rigidbody2D>();
		_bodyRotation = GetComponentInChildren<CharacterBodyRotation>();
	}

	void Update()
	{
		DetectSlopeAndRotatePlayer();
		_inputX = Input.GetAxisRaw("Horizontal");


		if (_isGrounded)
		{
			if (Input.GetButtonDown("Jump"))
			{
				Jump();
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

		//Debug.Log(_rb.velocity.magnitude);
	}

	bool DetectWalls()
	{
		bool check = false;
		Vector2 raycastOrigin1 = transform.position + transform.right * 0.2f;
		RaycastHit2D hit1 = Physics2D.Raycast(raycastOrigin1, transform.right, 0.5f, CollisionMask);
		Debug.DrawRay(raycastOrigin1, transform.right * 0.5f, Color.blue, 0.01f);

		Vector2 raycastOrigin2 = transform.position + -transform.right * 0.2f;
		RaycastHit2D hit2 = Physics2D.Raycast(raycastOrigin2, -transform.right, 0.5f, CollisionMask);
		Debug.DrawRay(raycastOrigin2, -transform.right * 0.5f, Color.yellow, 0.01f);

		if (hit1 || hit2)
		{
			check = true;
		}

		return check;
	}

	void DetectSlopeAndRotatePlayer()
	{
		Vector2 raycastOrigin = transform.position + transform.up * -0.48f;
		RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, -transform.up, 1f, CollisionMask);
		Debug.DrawRay(raycastOrigin, -transform.up * 0.2f, Color.red, 0.01f);

		_slopeAngle = -Vector2.Angle(hit.normal, Vector2.up);

		if (hit.distance <= GroundRayTraceRange)
		{
			if (Mathf.Abs(_slopeAngle) < WalkableSlopeAngle)
			{
				_isSliding = false;
				_isGrounded = hit;

				//rotates player to surface normal
				if (hit)
				{
					transform.up = hit.normal;
				}
				else
				{
					transform.up = Vector2.up;
				}
			}
			else
			{
			    if (DetectWalls())
			    {
			        _isSliding = true;
			    }
			    else
			    {
			        _isSliding = false;
			    }
                _isGrounded = false;
				transform.up = Vector2.up;
			}
		}
		else
		{
			_isGrounded = false;
			transform.up = Vector2.up;

			if (DetectWalls())
			{
				_isSliding = true;
			}
			else
			{
				_isSliding = false;
			}
		}
	}

	void MoveOnGround()
	{
		float dot = CalculateDotProduct();

		float moveSpeed = Mathf.MoveTowards(dot, MaxSpeed * _inputX, Acceleration * Time.deltaTime);
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
		float moveX = Mathf.MoveTowards(_rb.velocity.x, MaxSpeed * _inputX, AirAcceleration * Time.deltaTime);
		_rb.velocity = new Vector2(moveX, _rb.velocity.y);
		_bodyRotation.PlayerMoveDir = Mathf.RoundToInt(Mathf.Sign(_inputX));
	}

	float CalculateDotProduct()
	{
		Vector2 currentVel = _rb.velocity;
		Vector2 currentDirection = transform.right;
		Vector2 currentDirectionNormal = currentDirection.normalized;

		//Vector2 vP = CurrentDirectionNormal * dot;

		return Vector2.Dot(currentVel, currentDirectionNormal);
	}

	void Jump()
	{
		_isGrounded = false;
		_rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
		StartCoroutine(Countdown(0.2f));
	}

	IEnumerator Countdown(float seconds)
	{
		_movementBlocked = true;
		yield return new WaitForSeconds(seconds);

		_movementBlocked = false;
	}
}
