using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace Com.MyCompany.Pacman
{
    public class Ghost : MonoBehaviour
    {

        #region Public Fields

        public float speed = 1.0f;
        public float WaitDur;
        public GameObject player;
        public Light player_Light;
        public float dist;
        public AudioClip player_Kill;

        public string attackClipName;

        #endregion


        #region Private Fields

        private Vector2 direction = Vector2.up;
        private float directionChangeTime = 0.0f; //the next time the ghost may change direction

        private Rigidbody2D rb;
        private CircleCollider2D cc;
        private SpriteRenderer spr;
        private Animator anim;
        private Animator player_Anim;
        private AudioSource player_Audio;

        private bool isEnemy2 = false;

        #endregion


        #region Monobehaviour Callbacks

        private void Awake()
        {
            if (gameObject.tag == "Enemy_2")
                isEnemy2 = true;
        }

        // Use this for initialization
        void Start()
        {
            
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            cc = GetComponent<CircleCollider2D>();
            spr = GetComponent<SpriteRenderer>();

            player = Player.LocalPlayerInstance;

            player_Anim = player.GetComponent<Animator>();
            player_Audio = player.GetComponent<AudioSource>();
            player_Light = player.GetComponentInChildren<Light>();

        }

        // Update is called once per frame
        void Update()
        {

            //
            if(player == null)
            {
                if(Player.LocalPlayerInstance != null)
                {
                    player = Player.LocalPlayerInstance;
                    Debug.Log("<Color=Red> Found player instance ! : Ghost.cs.Update()");
                    player_Anim = player.GetComponent<Animator>();
                    player_Audio = player.GetComponent<AudioSource>();
                    player_Light = player.GetComponentInChildren<Light>();
                }    
            }
            //
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
                transform.position = new Vector2((transform.position.x), transform.position.y);
            }
            if (rb.velocity.y == 0)
            {
                transform.position = new Vector2(transform.position.x, (transform.position.y));
            }
        }

        #endregion


        #region Private Methods
        //for perpendilular up/down/left/right
        private bool canChangeDirection()
        {
            Vector2 perpRight = Extra.PerpendicularRight(direction);
            bool openRight = openDirection(perpRight);
            Vector2 perpLeft = Extra.PerpendicularLeft(direction);
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
            Vector2 perRight = Extra.PerpendicularRight(direction);
            bool openRight = openDirection(perRight);
            Vector2 perLeft = Extra.PerpendicularLeft(direction);
            bool openLeft = openDirection(perLeft);
            if (openRight || openLeft)
            {
                int choice = Random.Range(0, 2);
                if (!openLeft || (choice == 0 && openRight))             //meeting points pe
                {
                    direction = perRight;
                    spr.flipX = true;

                }
                else
                {
                    direction = perLeft;
                    spr.flipX = false;
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

            cc.Cast(dir, rchObj, dist, true);
            foreach (RaycastHit2D rch in rchObj)
            {
                if (rch && rch.collider.gameObject.tag == "Wall")
                {
                    return false;
                }
            }
            return true;
        }


        void OnTriggerEnter2D(Collider2D target)
        {
            if (target.gameObject.CompareTag("Player"))
            {
                //back to origin
                StartCoroutine(OnPlayer());
            }
        }

        private IEnumerator OnPlayer()
        {
            if (!Player.canKill)
            {
                anim.Play(attackClipName);
                player_Anim.Play("player_Scared");
                yield return new WaitForSecondsRealtime(WaitDur);
                if (isEnemy2)
                {
                    player_Light.intensity = 0.1f;
                }
                else
                    player.transform.position = new Vector2(0, 12);
            }
            else
            {
                player_Anim.Play("player_Attack");
                player_Audio.clip = player_Kill;
                player_Audio.Play();
                yield return new WaitForSecondsRealtime(0.1f);
                Destroy(gameObject);
            }

        }
        #endregion
    }
}


/* =============================================================================
#  Author:          Divya Gandhi - https://github.com/divya16-bit
#  Email:           16gandhi.hemani@gmail.com
#  FileName:        ghost.cs
#  Created On:      26/11/2020
#  Updated On :     14/05/2021 by Aurav
============================================================================= */
