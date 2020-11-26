using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{

    public float speed = 1.0f;

    private Vector2 direction = Vector2.up;
    private float directionChangeTime = 0.0f; //the next time the ghost may change direction

    private Rigidbody2D rb;
    private CircleCollider2D cc;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Direction  ki Change
        if (!openDirection(direction))
        {
            if (canChangeDirection())
            {
                changeDirection();
            }
            else if (rb.velocity.magnitude < speed)
            {
                changeDirectionAtRandom();
            }
        }
        else if (canChangeDirection() && Time.time > directionChangeTime)
        {
            changeDirectionAtRandom();
        }
        else if (rb.velocity.magnitude < speed)
        {
            changeDirectionAtRandom();
        }

        //Movement basic
        rb.velocity = direction * speed;
        if (rb.velocity.x == 0)
        {
            transform.position = new Vector2(Mathf.Round(transform.position.x), transform.position.y);
        }
        if (rb.velocity.y == 0)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Round(transform.position.y));
        }
    }
    //for perpendilular up/down/left/right
    private bool canChangeDirection()
    {
        Vector2 perpRight =extra.PerpendicularRight(direction);
        bool openRight = openDirection(perpRight);
        Vector2 perpLeft =extra.PerpendicularLeft(direction);
        bool openLeft = openDirection(perpLeft);
        return openRight || openLeft;
    }

    private void changeDirectionAtRandom()
    {
        directionChangeTime = Time.time + 1;//can't change directions for a second
        if (Random.Range(0, 2) > 0)
        {
            changeDirection();
        }
    }

    private void changeDirection()
    {
        directionChangeTime = Time.time + 1; //can't change directions for within that second/or maybe in a sec not sure yet
        Vector2 perRight = extra.PerpendicularRight(direction);
        bool openRight = openDirection(perRight);
        Vector2 perLeft = extra.PerpendicularLeft(direction);
        bool openLeft = openDirection(perLeft);
        if (openRight || openLeft)
        {
            int choice = Random.Range(0, 2);
            if (!openLeft || (choice == 0 && openRight))             //meeting points pe
            {
                direction = perRight;
            }
            else
            {
                direction = perLeft;
            }
        }
        else
        {
            direction = -direction;   //up/down
        }
    }
    private bool openDirection(Vector2 dir)
    {
        RaycastHit2D[] rchObj = new RaycastHit2D[10];
        float dist = 1;
        cc.Cast(dir, rchObj, dist, true);
        foreach (RaycastHit2D rch in rchObj)
        {
            if (rch && rch.collider.gameObject.tag == "Walls")
            {
                return false;
            }
        }
        return true;
    }
    void OnCollisionEnter2D(Collision2D target)
    {
        if (target.gameObject.tag == "Player")
        {
            target.transform.position = new Vector2(0, 0);            //back to origin
        }
    }
}


/* =============================================================================
#  Author:          Divya Gandhi - https://github.com/divya16-bit
#  Email:           16gandhi.hemani@gmail.com
#  FileName:        ghost.cs
#  Created On:      26/11/2020
============================================================================= */
