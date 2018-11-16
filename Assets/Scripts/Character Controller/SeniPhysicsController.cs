using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SeniPhysicsController : MonoBehaviour
{
	public float MaxSpeed = 10f;
	public float Acceleration = 50f;
	public float Deacceleration = 50f;
	public float AirAcceleration = 10f;
	public float JumpForce = 10f;
	public float WalkableSlopeAngle = 45f;
	public float GroundRayTraceRange = 0.1f;
	public LayerMask CollisionMask;

	private Rigidbody2D _rb;
	private BoxCollider2D _bc;
	private CharacterBodyRotation _bodyRotation;
	private float _inputX;
	public bool _isGrounded;
	public bool _MovementBlocked;
	public bool _isSliding;

	void Start()
	{
		_rb = GetComponent<Rigidbody2D>();
		_bc = GetComponent<BoxCollider2D>();
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

			if (_MovementBlocked == false)
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
		Vector2 RaycastOrigin1 = transform.position + transform.right * 0.2f;
		RaycastHit2D hit1 = Physics2D.Raycast(RaycastOrigin1, transform.right, 0.5f, CollisionMask);
		Debug.DrawRay(RaycastOrigin1, transform.right * 0.5f, Color.blue, 0.01f);

		Vector2 RaycastOrigin2 = transform.position + -transform.right * 0.2f;
		RaycastHit2D hit2 = Physics2D.Raycast(RaycastOrigin2, -transform.right, 0.5f, CollisionMask);
		Debug.DrawRay(RaycastOrigin2, -transform.right * 0.5f, Color.yellow, 0.01f);

		if (hit1 || hit2)
		{
			check = true;
		}
		else
		{
			check = false;
		}

		return check;
	}

	void DetectSlopeAndRotatePlayer()
	{
		Vector2 RaycastOrigin = transform.position + transform.up * -0.48f;
		RaycastHit2D hit = Physics2D.Raycast(RaycastOrigin, -transform.up, 1f, CollisionMask);
		Debug.DrawRay(RaycastOrigin, -transform.up * 0.2f, Color.red, 0.01f);

		float slopeAngle = -Vector2.Angle(hit.normal, Vector2.up);

		if (hit.distance <= GroundRayTraceRange)
		{
			if (Mathf.Abs(slopeAngle) < WalkableSlopeAngle)
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
				_isSliding = true;
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
		_rb.velocity = myVel * moveSpeed + new Vector2(0, -Physics2D.gravity.y * (_rb.mass + 1.5f) * _rb.gravityScale * Time.deltaTime);
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
		Vector2 CurrentDirectionNormal = currentDirection.normalized;

		//Vector2 vP = CurrentDirectionNormal * dot;

		return Vector2.Dot(currentVel, CurrentDirectionNormal);
	}

	void Jump()
	{
		_isGrounded = false;
		_rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
		StartCoroutine(Countdown(0.2f));
	}

	IEnumerator Countdown(float seconds)
	{
		_MovementBlocked = true;
		yield return new WaitForSeconds(seconds);

		_MovementBlocked = false;
	}
}
