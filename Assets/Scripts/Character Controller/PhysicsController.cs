using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsController : MonoBehaviour
{
	public float MaxSpeed = 5f;
	public float Acceleration = 500f;
	public float DeAcceleration = 500f;
	public float JumpForce = 2f;
	public LayerMask CollisionMask;

	private Rigidbody2D _rb;
	private BoxCollider2D _bc;

	private float _moveX;

	private Vector2 _RaycastOrigin;
	private float _slopeAngle;

	private bool isGrounded;


	// Use this for initialization
	void Start()
	{
		_rb = GetComponent<Rigidbody2D>();
		_bc = GetComponent<BoxCollider2D>();
	}

	// Update is called once per frame
	void Update()
	{
		_moveX = Input.GetAxisRaw("Horizontal");
		DetectSlopeAndRotatePlayer();
		Debug.Log(_rb.velocity.magnitude);
		Debug.Log(isGrounded);

		if (Input.GetButtonDown("Jump") && isGrounded)
		{
			Jump();
		}
	}

	private void FixedUpdate()
	{
		if (_moveX != 0)
		{
			Accelerate();
		}
		else
		{
			Breaking();
		}
	}

	void DetectSlopeAndRotatePlayer()
	{
		_RaycastOrigin = new Vector2(transform.position.x, transform.position.y);
		RaycastHit2D hit = Physics2D.Raycast(_RaycastOrigin + new Vector2(0, -0.4f), Vector2.down, .3f, CollisionMask);
		Debug.DrawRay(_RaycastOrigin + new Vector2(0, -0.4f), Vector2.down * 1f, Color.red, 1f);

		_slopeAngle = -Vector2.Angle(hit.normal, Vector2.up);

		isGrounded = hit;
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

	void Accelerate()
	{
		if (_moveX < 0)
		{
			if (Vector2.Dot(_rb.velocity, -transform.right) < MaxSpeed)
			{
				_rb.AddForce(transform.right * _moveX * Acceleration * 10f * Time.deltaTime);
			}
		}
		if (_moveX > 0)
		{
			if (Vector2.Dot(_rb.velocity, transform.right) < MaxSpeed)
			{
				_rb.AddForce(transform.right * _moveX * Acceleration * 10f * Time.deltaTime);
			}
		}
	}

	void Breaking()
	{
		_rb.AddForce(transform.right * Vector2.Dot(_rb.velocity, -transform.right) * -DeAcceleration);
	}

	void Jump()
	{
		_rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
	}
}
