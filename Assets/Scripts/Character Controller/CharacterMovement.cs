using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
	const float skinWidth = 0.015f;

	private Rigidbody2D _rb;
	private BoxCollider2D _bc;
	RaycastOrigins raycastOrigins;

	public float Acceleration;
	public float DeAcceleration;
	public float MaxSpeed;

	public float JumpVelocity;
	public LayerMask CollisionMask;

	private float _moveX;
	private Vector2 _surfaceAngle;
	float slopeAngle;

	void Start()
	{
		_rb = GetComponent<Rigidbody2D>();
		_bc = GetComponent<BoxCollider2D>();
	}

	void Update()
	{
		UpdateRaycastorigins();
		_moveX = Input.GetAxisRaw("Horizontal");

		Debug.DrawRay(raycastOrigins.bottomLeft, Vector2.up * -2, Color.red);
		Debug.DrawRay(raycastOrigins.bottomRight, Vector2.up * -2, Color.red);
		Debug.DrawRay(raycastOrigins.bottomLeft - new Vector2(0, skinWidth), Vector2.right * -0.1f, Color.blue);
		Debug.DrawRay(raycastOrigins.bottomRight - new Vector2(0, skinWidth), Vector2.right * 0.1f, Color.blue);

		if (Input.GetButtonDown("Jump"))
		{
			Jump();
		}
		//print(_rb.velocity.normalized.x - _surfaceAngle.x);
	}

	void FixedUpdate()
	{
		DetectSlope();
		EvaluateMovement();
		//print(_rb.velocity.magnitude);
	}

	void DetectSlope()
	{
		Vector2 originLeft = raycastOrigins.bottomLeft - new Vector2(0, skinWidth);
		Vector2 originRight = raycastOrigins.bottomRight - new Vector2(0, skinWidth);

		RaycastHit2D hitLeft = Physics2D.Raycast(originLeft, Vector2.right * -1f, 0.5f, CollisionMask);
		RaycastHit2D hitRight = Physics2D.Raycast(originRight, Vector2.right * 1f, 0.5f, CollisionMask);
		if (hitLeft)
		{
			slopeAngle = -Vector2.Angle(hitLeft.normal, Vector2.up);
			float y = Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
			float x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
			_surfaceAngle = new Vector2(x, y).normalized;
		}
		else if (hitRight)
		{
			slopeAngle = Vector2.Angle(hitRight.normal, Vector2.up);
			float y = Mathf.Sin(slopeAngle * Mathf.Deg2Rad);
			float x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
			_surfaceAngle = new Vector2(x, y).normalized;
		}
		else
		{
			slopeAngle = 0f;
			_surfaceAngle = Vector2.right;
		}

		Debug.DrawRay(transform.position, _surfaceAngle * 2f, Color.green);
		Debug.DrawRay(transform.position, _surfaceAngle * -2f, Color.green);
	}

	void Jump()
	{
		_rb.velocity = new Vector2(_rb.velocity.x, JumpVelocity);
	}


	void EvaluateMovement()
	{
		//print(Mathf.Sign(slopeAngle));

		float dot = Vector2.Dot(_surfaceAngle, _rb.velocity);
		Vector2 vel = Vector3.RotateTowards(Vector2.right, _surfaceAngle, 10f, 0f);
		Vector2 velNormal = vel.normalized;
		//print(dot);

		if (_moveX < 0)
		{
			if (Mathf.Sign(slopeAngle) < 0)
			{
				if (_rb.velocity.x > MaxSpeed * velNormal.x)
				{
					float velX = Mathf.Clamp(MaxSpeed * velNormal.x - _rb.velocity.x, 0, Acceleration * Time.deltaTime);

					Accelerate(velX, 0);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.blue, .1f);
				}
				if (_rb.velocity.y < MaxSpeed * velNormal.y)
				{
					float velY = Mathf.Clamp(MaxSpeed * velNormal.y - _rb.velocity.y, 0, Acceleration * Time.deltaTime);

					Accelerate(0, velY);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.blue, .1f);
				}
			}
			else if (Mathf.Sign(slopeAngle) > 0)
			{
				if (_rb.velocity.x > MaxSpeed * velNormal.x)
				{
					float velX = -Mathf.Clamp(MaxSpeed * velNormal.x - _rb.velocity.x, 0, Acceleration * Time.deltaTime);

					Accelerate(velX, 0);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.blue, .1f);
				}
				if (_rb.velocity.y < MaxSpeed * velNormal.y)
				{
					float velY = Mathf.Clamp(MaxSpeed * velNormal.y - _rb.velocity.y, 0, Acceleration * Time.deltaTime);

					Accelerate(0, velY);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.blue, .1f);
				}
			}
			else
			{
				Accelerate(-Acceleration * Time.deltaTime, 0);
			}

		}
		if (_moveX > 0)
		{

			if (Mathf.Sign(slopeAngle) > 0)
			{
				if (_rb.velocity.x < MaxSpeed * velNormal.x)
				{
					float velX = Mathf.Clamp(MaxSpeed * velNormal.x - _rb.velocity.x, 0, Acceleration * Time.deltaTime);

					Accelerate(velX, 0);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.blue, .1f);
				}
				if (_rb.velocity.y < MaxSpeed * velNormal.y)
				{
					float velY = Mathf.Clamp(MaxSpeed * velNormal.y - _rb.velocity.y, 0, Acceleration * Time.deltaTime);

					Accelerate(0, velY);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.blue, .1f);
				}

			}
			else if (Mathf.Sign(slopeAngle) < 0)
			{
				Vector2 calculatedSpeed = new Vector2(0, 0);

				if (_rb.velocity.x < MaxSpeed * velNormal.x)
				{
					float velX = Mathf.Clamp(MaxSpeed * velNormal.x - _rb.velocity.x, 0, Acceleration * Time.deltaTime);

					calculatedSpeed =  new Vector2(velX, calculatedSpeed.y);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.cyan, .1f);
				}
				if (_rb.velocity.y < MaxSpeed * velNormal.y)
				{
					float velY = Mathf.Clamp(MaxSpeed * velNormal.y - _rb.velocity.y, 0, Acceleration * Time.deltaTime);

					calculatedSpeed = new Vector2(calculatedSpeed.x, velY);
					Debug.DrawRay(transform.position, velNormal * 10f, Color.yellow, 1f);
				}

				Accelerate(calculatedSpeed.x, calculatedSpeed.y);
				//vel = vel * -Mathf.Sign(slopeAngle) * _inputX * Acceleration * Time.deltaTime;
				//Debug.DrawRay(transform.position, vel * 10f, Color.blue, .1f);
				//Accelerate(vel.x, vel.y);
			}
			else
			{
				Accelerate(Acceleration * Time.deltaTime, 0);
			}

		}


		//Breaking();
	}
	void EvaluateMovementOld()
	{
		//if (_inputX < 0)
		//{
		//	if (_rb.velocity.x > MaxSpeed * Mathf.Sign(_inputX))
		//	{
		//		Accelerate(-Mathf.Clamp(MaxSpeed + _rb.velocity.x, 0, Acceleration * Time.deltaTime));
		//	}
		//}
		//if (_inputX > 0)
		//{
		//	if (_rb.velocity.x < MaxSpeed * Mathf.Sign(_inputX))
		//	{
		//		Accelerate(Mathf.Clamp(MaxSpeed + -_rb.velocity.x, 0, Acceleration * Time.deltaTime));
		//	}
		//}

		Breaking();

	}

	void Accelerate(float x, float y)
	{
		Debug.Log(x + " " + y);
		_rb.velocity = _rb.velocity + new Vector2(x, y);
		//_rb.velocity = _rb.velocity + new Vector2(x, 0);
	}

	void Breaking()
	{
		if (_rb.velocity.x < -0.1)
		{
			_rb.velocity = _rb.velocity + new Vector2(DeAcceleration * Time.deltaTime, 0);
		}
		if (_rb.velocity.x > 0.1)
		{
			_rb.velocity = _rb.velocity + new Vector2(-DeAcceleration * Time.deltaTime, 0);
		}
	}

	void UpdateRaycastorigins()
	{
		Bounds bounds = _bc.bounds;
		bounds.Expand(skinWidth * -2);

		raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
	}

	struct RaycastOrigins
	{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
}
