using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public event Action<ObstacleType> _was_collision;
    public event Action _add_score;
    public ObstacleType _type;

    private float _speed = 0;

   [HideInInspector] public bool _move = true;

    private void Start()
    {
        _move = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Player")
            _was_collision(_type);
        else if (collision.transform.tag == "Basket")
            Destroy(gameObject);
        else if (collision.transform.tag == "AddScore")
            _add_score();
    }

    public void SetSpeed(float speed) => _speed = speed;

    private void FixedUpdate()
    {
        if (_move)
            transform.position -= Vector3.right * _speed / 500;
    }
}
public enum ObstacleType : int
{
    Box,
    Lazer,
    EvilRobotUp,
    EvilRobotDown,
    EvilRobotUpx2,
}
