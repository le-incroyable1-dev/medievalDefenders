using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private float speed = 10.0f;
    private Animator anim;
    private Transform faceTransform;
    private float step;
    private Vector2 target_pos;


    public float MaxRayDist;
    public float MoveDist;
    //public GameObject playerFace;
    public Text score_Text;
    public float score;


    // Start is called before the first frame update
    void Start()
    { 
        anim = GetComponent<Animator>();
        //faceTransform = playerFace.GetComponent<Transform>();
        score = 0;
        score_Text.text = score.ToString();
        target_pos = transform.position;
    }

    private void Update()
    {
        
        step = speed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
    }

    bool CheckMove(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, MaxRayDist);

        if (hit.collider == null)
            return true;
        else
            return false;
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Pickup"))
        {
            score += 1;
            score_Text.text = score.ToString();
        }
    }
    public void MoveUp()
    {
        
        if (CheckMove(Vector2.up))
        {
           target_pos = new Vector2(transform.position.x, transform.position.y + MoveDist);
        }
    }
    public void MoveDown()
    {
        
        if (CheckMove(Vector2.down))
        {

            target_pos = new Vector2(transform.position.x, transform.position.y - MoveDist);
        }
    }
    public void MoveRight()
    {
        if (CheckMove(Vector2.right))
        {
            target_pos = new Vector2(transform.position.x + MoveDist, transform.position.y);
        }
    }
    public void MoveLeft()
    {
        
        if (CheckMove(Vector2.left))
        {
            target_pos = new Vector2(transform.position.x - MoveDist, transform.position.y);
        }
    }
}

/* =============================================================================
#  Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
#  Email:           aurav.tomar@gmail.com
#  FileName:        Player.cs
#  Created On:      24/11/2020
============================================================================= */

