using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    private float speed = 10.0f;
    private float step;
    private float counter = 10.0f;

    private CircleCollider2D cc;
    //private Animator anim;
    private SpriteRenderer spr;
    private AudioSource src; 

    private bool UpPress = false;
    private bool DownPress = false;
    private bool RightPress = false;
    private bool LeftPress = false;

    public Game_Manager game_Manager;

    public static bool canKill = false;

    public float MaxRayDist;
    public float MoveDist;
    public float score;

    public AudioClip pickupClip;
    public AudioClip musicGen;
    public AudioClip musicActiv;
    //public GameObject playerFace;
    public GameObject counterShow;
    public Light player_Light;
    public AudioSource intense_src;

    public TextMeshProUGUI score_Text;
    public TextMeshProUGUI counter_Text;

    public Vector2 target_pos;


    //TEST
    //public bool test_CanMove = true;

    // Start is called before the first frame update
    void Start()
    { 
        //anim = GetComponent<Animator>();
        cc = GetComponent<CircleCollider2D>();
        spr = GetComponent<SpriteRenderer>();
        src = GetComponent<AudioSource>();
        
        intense_src.volume = 0;

        score = 0;
        score_Text.text = score.ToString();
        target_pos = transform.position;
    }

    private void Update()
    {

        
        if (UpPress)
        {
            if (CheckMove(Vector2.up))
            {
                target_pos = new Vector2(transform.position.x, transform.position.y + MoveDist);
                step = speed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
            }
        }
        else if (DownPress)
        {
            if (CheckMove(Vector2.down))
            {

                target_pos = new Vector2(transform.position.x, transform.position.y - MoveDist);
                step = speed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
            }
        }
        else if (RightPress)
        {
            if (CheckMove(Vector2.right))
            {
                target_pos = new Vector2(transform.position.x + MoveDist, transform.position.y);
                step = speed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                spr.flipX = false;
            }
        }
        else if (LeftPress)
        {
            if (CheckMove(Vector2.left))
            {
                target_pos = new Vector2(transform.position.x - MoveDist, transform.position.y);
                step = speed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                spr.flipX = true;

            }
        }

        //
       if(canKill)
        {
            if(((int)counter) > 0)
            {
                counter_Text.text = ((int)counter).ToString();
                counter -= Time.deltaTime;
            }
            else
            {
                counterShow.SetActive(false);
                src.clip = pickupClip;
                intense_src.volume = 0;
                game_Manager.SetAudio();
                counter = 10.0f;
                canKill = false;
            }
        }


    }

    private bool CheckMove(Vector2 dir)
    {
        RaycastHit2D[] rchObj = new RaycastHit2D[10];

        cc.Cast(dir, rchObj, MaxRayDist, true);
        foreach (RaycastHit2D rch in rchObj)
        {
            if (rch && rch.collider.gameObject.tag == "Wall")
            {
                return false;
            }
        }
        return true;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("kp") && (!canKill))
        {
            collision.gameObject.SetActive(false);
            canKill = true;
            intense_src.volume = 1;
            player_Light.intensity = 1.1f;
            game_Manager.SetAudio();
            counterShow.SetActive(true);
              
        }
        else if (collision.gameObject.CompareTag("Pickup"))
        {
            score += 1;
            score_Text.text = score.ToString();
            src.Play();
            collision.gameObject.SetActive(false);
        }

    }

    //
    public void MoveUp()
    {
        UpPress = true;
        
    }
    public void MoveDown()
    {
        DownPress = true;
        
    }
    public void MoveRight()
    {
        RightPress = true;
    }
    public void MoveLeft()
    {
        LeftPress = true;
        
    }
    public void MoveUpRelease()
    {
        UpPress = false;

    }
    public void MoveDownRelease()
    {
        DownPress = false;
    }
    public void MoveRightRelease()
    {
        RightPress = false;
    }
    public void MoveLeftRelease()
    {
        LeftPress = false;
    }
    //

    //TEST
    /*bool CheckMove(Vector2 dir)
   {
       RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, MaxRayDist);

       if (hit.collider == null || hit.collider.gameObject.tag == "Player")
           return true;
       else
           return false;

   }*/


}



/* =============================================================================
#  Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
#  Email:           aurav.tomar@gmail.com
#  FileName:        Player.cs
#  Updated On:      23/12/2020
#  Created On:      24/11/2020
============================================================================= */

