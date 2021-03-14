using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teleporter : MonoBehaviour
{
    public Transform tPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform collisionTransform = collision.gameObject.GetComponent<Transform>();

        //if (collision.gameObject.CompareTag("Player"))
            collisionTransform.position = Vector3.Lerp(collisionTransform.position, tPoint.position, 1.5f);
    }
}

/* =============================================================================
#  Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
#  Email:           aurav.tomar@gmail.com
#  FileName:        teleporter.cs
============================================================================= */

