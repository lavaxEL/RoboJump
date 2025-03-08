using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public event Action _was_collision;

    public Animator Animator;

    private float _speed = 0;

    [HideInInspector] public bool _move = true;

    private void Start()
    {
        _move = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            Animator.SetInteger("cntrl", 1);
            Invoke("DestroyThis", 1f);
            _was_collision();
        }
        else if (collision.transform.tag == "Basket")
            Destroy(gameObject);

    }

    private void FixedUpdate()
    {
        if (_move) 
            transform.position -= Vector3.right * _speed / 500;
    }

    public void SetSpeed(float speed) => _speed = speed;
    private void DestroyThis() =>Destroy(gameObject);
}
