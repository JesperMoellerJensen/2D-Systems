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
	public float AlignWithGroundSpeed = 10f;
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

	private void FixedUpdate()
	{
		DetectSlopeAndRotatePlayer();
	}

	void Update()
	{
		_inputX = Input.GetAxisRaw("Horizontal");


		if (_isGrounded && _isSliding == false)
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

	void DetectSlopeAndRotatePlayer()
	{
		Vector2 raycastOriginLeft = transform.position + transform.right * -0.2f + transform.up * 0.25f;
		Vector2 raycastOriginRight = transform.position + transform.right * 0.2f + transform.up * 0.25f;
		RaycastHit2D hitLeft = Physics2D.Raycast(raycastOriginLeft, -transform.up, GroundRayTraceRange, CollisionMask);
		RaycastHit2D hitRight = Physics2D.Raycast(raycastOriginRight, -transform.up, GroundRayTraceRange, CollisionMask);
		Debug.DrawRay(raycastOriginLeft, -transform.up * GroundRayTraceRange, Color.red);
		Debug.DrawRay(raycastOriginRight, -transform.up * GroundRayTraceRange, Color.yellow);


		Vector2 perpendicularLine = hitLeft.point - hitRight.point;
		// rotate 90� Clockwise
		Vector2 normal = new Vector2(perpendicularLine.y, -perpendicularLine.x).normalized;

		if (hitLeft || hitRight)
		{
			bool LeftRayOnSlope = false;
			bool RightRayOnSlope = false;
			float avarageDistance = Mathf.Infinity;
			if (hitLeft)
			{
				LeftRayOnSlope = IsOnSlope(hitLeft.normal);
			}
			if (hitRight)
			{
				RightRayOnSlope = IsOnSlope(hitRight.normal);
			}


			if (hitLeft && !hitRight)
			{
				avarageDistance = hitLeft.distance;
				if (LeftRayOnSlope == false)
				{
					RotateTransformUp(hitLeft.normal);
				}
			}
			else if (!hitLeft && hitRight)
			{
				avarageDistance = hitRight.distance;
				if (RightRayOnSlope == false)
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

			if(avarageDistance < 0.3f)
			{
				_isGrounded = true;
				Debug.Log("Yes " + avarageDistance);
			}
			else
			{
				_isGrounded = false;
				Debug.Log("No " + avarageDistance);
			}

			if (LeftRayOnSlope && RightRayOnSlope)
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
		else { return true; }
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


	// ---------------------- OLD CODE START ----------------------

	void DetectSlopeAndRotatePlayerOld()
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

	bool DetectWalls()
	{
		bool check = false;
		Vector2 raycastOrigin1 = transform.position + transform.right * 0.2f + transform.up * 0.5f;
		RaycastHit2D hit1 = Physics2D.Raycast(raycastOrigin1, transform.right, 0.5f, CollisionMask);
		Debug.DrawRay(raycastOrigin1, transform.right * 0.5f, Color.blue);

		Vector2 raycastOrigin2 = transform.position + -transform.right * 0.2f + transform.up * 0.5f;
		RaycastHit2D hit2 = Physics2D.Raycast(raycastOrigin2, -transform.right, 0.5f, CollisionMask);
		Debug.DrawRay(raycastOrigin2, -transform.right * 0.5f, Color.yellow);

		if (hit1 || hit2)
		{
			check = true;
		}

		return check;
	}

	// ---------------------- OLD CODE END ----------------------
}
