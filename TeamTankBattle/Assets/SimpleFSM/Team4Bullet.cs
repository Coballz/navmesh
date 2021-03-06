﻿using UnityEngine;
using System.Collections;
using Team4;

public class Team4Bullet : MonoBehaviour
{
    //Explosion Effect

    public float Speed = 600.0f;
    public float LifeTime = 3.0f;
    public int damage = 10;

    void Start()
    {
        Destroy(gameObject, LifeTime);
    }

    void Update()
    {
        transform.position += 
			transform.forward * Speed * Time.deltaTime;       
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}