using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bullet : MonoBehaviour
{
    private Rigidbody2D _rb;
    public float BulletSpeed;
    public float LifeTime;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = transform.right * BulletSpeed;
        Destroy(gameObject, LifeTime);
    }
}
