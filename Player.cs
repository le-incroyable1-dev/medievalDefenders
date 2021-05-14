using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;


namespace Com.MyCompany.Pacman
{
    public class Player : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region Private Fields

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

        private Touch playerTouch;
        private Vector2 touchStart, touchEnd;

        #endregion

        #region Public Fields

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
        public GameObject[] GlowLights;
        public AudioSource intense_src;

        public TMP_Text score_Text;
        public TMP_Text counter_Text;

        public Vector2 target_pos;

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;

        //public int Health = 80;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion


        //TEST
        //public bool test_CanMove = true;

        // Start is called before the first frame update
        #region MonoBehaviour Callbacks

        void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                Player.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            if (photonView.IsMine)
            {
                Player.LocalPlayerInstance = this.gameObject;
            }

            game_Manager = GameObject.Find("GameManager").GetComponent<Game_Manager>();


#if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo = Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
#endif
            //Detect required stuff
            //player_Light = GetComponentInChildren<Light>();
            score_Text = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<TMP_Text>();
            counter_Text = GameObject.FindGameObjectWithTag("CounterText").GetComponent<TMP_Text>();
            intense_src = GameObject.Find("IntenseMusic").GetComponent<AudioSource>();
            counterShow = GameObject.FindGameObjectWithTag("Counter_0");

            //anim = GetComponent<Animator>();
            cc = GetComponent<CircleCollider2D>();
            spr = GetComponent<SpriteRenderer>();
            src = GetComponent<AudioSource>();

            intense_src.volume = 0;

            score = 0;
            score_Text.text = score.ToString();
            target_pos = transform.position;
        }

#if !UNITY_5_4_OR_NEWER
/// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
void OnLevelWasLoaded(int level)
{
    this.CalledOnLevelWasLoaded(level);
}
#endif


        void CalledOnLevelWasLoaded(int level)
        {
            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);

            /*
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            */

        }

#if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
#endif


        private void Update()
        {

#if UNITY_EDITOR_WIN

            if (photonView.IsMine)
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
            }

#endif


#if UNITY_ANDROID

            if (photonView.IsMine)
            {
                if (Input.touchCount > 0)
                {
                    playerTouch = Input.GetTouch(0);

                    if (playerTouch.phase == TouchPhase.Began)
                    {
                        touchStart = playerTouch.position;
                    }

                    else if (playerTouch.phase == TouchPhase.Moved || playerTouch.phase == TouchPhase.Ended)
                    {
                        touchEnd = playerTouch.position;

                        float x = touchEnd.x - touchStart.x;
                        float y = touchEnd.y - touchStart.y;

                        if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0)
                        {
                            //SHE JUST TAPPED, NO MOTION OF THE TOUCH
                        }

                        else if (Mathf.Abs(x) > Mathf.Abs(y))
                        {
                            if (x > 0)
                            {
                                if (CheckMove(Vector2.right))
                                {
                                    target_pos = new Vector2(transform.position.x + MoveDist, transform.position.y);
                                    step = speed * Time.deltaTime;
                                    transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                                    spr.flipX = false;
                                }
                            }
                            else
                            {
                                if (CheckMove(Vector2.left))
                                {
                                    target_pos = new Vector2(transform.position.x - MoveDist, transform.position.y);
                                    step = speed * Time.deltaTime;
                                    transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                                    spr.flipX = true;
                                }
                            }
                        }

                        else
                        {
                            if (y > 0)
                            {
                                if (CheckMove(Vector2.up))
                                {
                                    target_pos = new Vector2(transform.position.x, transform.position.y + MoveDist);
                                    step = speed * Time.deltaTime;
                                    transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                                }
                            }
                            else
                            {
                                if (CheckMove(Vector2.down))
                                {
                                    target_pos = new Vector2(transform.position.x, transform.position.y - MoveDist);
                                    step = speed * Time.deltaTime;
                                    transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                                }
                            }
                        }
                    }

                }
            }

#endif

            //
            if (photonView.IsMine)
            {
                if (canKill)
                {
                    if (((int)counter) > 0)
                    {
                        counter_Text.text = ((int)counter).ToString();
                        counter -= Time.deltaTime;
                    }
                    else
                    {
                        //GlowLights[0].SetActive(false);
                        player_Light.intensity = 3.5f;
                        counterShow.SetActive(false);
                        src.clip = pickupClip;
                        intense_src.volume = 0;
                        game_Manager.SetAudio();
                        counter = 10.0f;
                        canKill = false;
                    }
                }
            }


        }

        #endregion

        #region Private Methods

#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
#endif

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
            if (collision.gameObject.CompareTag("kp") && (!canKill) && photonView.IsMine)
            {
                collision.gameObject.SetActive(false);
                canKill = true;
                intense_src.volume = 1;
                //GlowLights[0].SetActive(true);
                player_Light.intensity = 5f;
                game_Manager.SetAudio();
                counterShow.SetActive(true);

            }
            else if (collision.gameObject.CompareTag("Pickup") && photonView.IsMine)
            {
                score += 1;
                score_Text.text = score.ToString();
                src.Play();
                collision.gameObject.SetActive(false);
            }

        }

        #endregion


#if UNITY_EDITOR_WIN

        #region Public Methods

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

        #endregion

#endif

        #region IPunObservable implementation


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }


        #endregion

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

}

/* =============================================================================
# Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
# Email:           aurav.tomar@gmail.com
# FileName:        Player.cs
# Updated On:      14/05/2021
============================================================================= */

