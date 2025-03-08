using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject[] Skins;
    [SerializeField] private Rigidbody2D Rigidbody;

    [HideInInspector] public Animator _animator;
    public bool _dead = true;
    public bool _enable = false;
    [HideInInspector] public event Action _on_ground;

    private Vector2 _velocity_temp;
    private RigidbodyConstraints2D _constraints_temp;

    public void EnableCurrentSkin(string name)
    {
        for (int i = 0; i < Skins.Length; i++)
        {
            Skins[i].SetActive(false);
            if (Skins[i].transform.name == name)
            {
                Skins[i].SetActive(true);
                _animator = Skins[i].GetComponent<Animator>();
                _animator.SetInteger("cntrl", 0);
            }
        }
    }


    public void SetStartPosition()
    {
        transform.position = new Vector3(Camera.main.ScreenToWorldPoint(new Vector2(300, 0)).x, transform.position.y, 0);
    }

    public void Run()
    {
        _dead = false;
        _animator.SetInteger("cntrl", 1);
    }

    public void Jump()
    {
        Rigidbody.velocity = new Vector2(0, 25);
        _animator.SetInteger("cntrl", 2);
    }
    public void Dead()
    {
        _dead = true;
        _animator.SetInteger("cntrl", 3);
    }

    public void DoubleJump()
    {
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y+15);
    }

    public void Pause()
    {
        _velocity_temp = Rigidbody.velocity;
        _constraints_temp = Rigidbody.constraints;
        Rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        Rigidbody.isKinematic = true;
    }

    public void UnPause()
    {
        Rigidbody.isKinematic = false;
        Rigidbody.constraints = _constraints_temp;
        Rigidbody.velocity = _velocity_temp;
    }

    private void Update()
    {
        if (Rigidbody.velocity.y == 0 && _animator.GetInteger("cntrl")!=1 && !_dead && _enable)
        {
            Run();
            _on_ground();
           
        }
        
    }
}
