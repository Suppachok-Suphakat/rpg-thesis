using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLockShooter : MonoBehaviour
{
    public float speed;
    public float lifeTime;
    private Rigidbody2D bulletRB;

    void Start()
    {
        bulletRB = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }
}
