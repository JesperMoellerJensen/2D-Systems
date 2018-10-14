using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private BoxCollider2D _bc;

    public float Acceleration;
    public float DeAcceleration;
    public float MaxSpeed;

    private float _moveX;
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
    }

    void FixedUpdate()
    {
        EvaluateMovement();
    }

    void EvaluateMovement()
    {
        if (_moveX < 0)
        {
            if (_rb.velocity.x > MaxSpeed * Mathf.Sign(_moveX))
            {
                Accelerate(-Mathf.Clamp(MaxSpeed + _rb.velocity.x,0,Acceleration*Time.deltaTime));
            }
        }
        if (_moveX > 0)
        {
            if (_rb.velocity.x < MaxSpeed * Mathf.Sign(_moveX))
            {
                Accelerate(Mathf.Clamp(MaxSpeed + -_rb.velocity.x, 0, Acceleration*Time.deltaTime));
            }
        }

        Breaking();

    }

    void Breaking()
    {
        if (_rb.velocity.x < -0.1)
        {
            _rb.velocity = _rb.velocity + new Vector2(DeAcceleration*Time.deltaTime, 0);
        }
        if(_rb.velocity.x > 0.1)
        {
            _rb.velocity = _rb.velocity + new Vector2(-DeAcceleration * Time.deltaTime, 0);
        }
    }
    void Accelerate(float xAcc)
    {
        _rb.velocity = _rb.velocity + new Vector2(xAcc, 0);

        Debug.Log(string.Format("V = {0} A = {1}", _rb.velocity.x,xAcc));
    }


}
