using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

namespace Com.MyCompany.fatman
{
    public class player_PUN : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields

        private float speed = 10.0f;
        private float step;
        //private float counter = 10.0f;

        private CircleCollider2D cc;
        //private Animator anim;
        private SpriteRenderer spr;
        private AudioSource src;

        private bool UpPress = false;
        private bool DownPress = false;
        private bool RightPress = false;
        private bool LeftPress = false;

        public bool canDoTask = false;

        private Touch playerTouch;
        private Vector2 touchStart, touchEnd;

        public GameObject curCloseClient = null;
        public int curCloseClientId = -1;

        private PhotonView myPV;

        private Animator myAnim;

        #endregion

        #region Public Fields

        public Vector2 target_pos;
        public float MaxRayDist;
        public float MoveDist;

        public bool isIntruder = false;
        public bool closeToClient = false;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        //public GameObject localPlayerShow = LocalPlayerInstance;

        
        #endregion

        #region Monobehaviour Callbacks

#if !UNITY_5_4_OR_NEWER
/// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
void OnLevelWasLoaded(int level)
{
    this.CalledOnLevelWasLoaded(level);
}
#endif


        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
        }


        private void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                player_PUN.LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {

            if (photonView.IsMine)
            {
                player_PUN.LocalPlayerInstance = this.gameObject;
            }

            myAnim = GetComponent<Animator>();
            myPV = GetComponent<PhotonView>();
            cc = GetComponent<CircleCollider2D>();
            spr = GetComponent<SpriteRenderer>();
            src = GetComponent<AudioSource>();

#if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

#endif
        }

        private void Update()
        {

            if(LocalPlayerInstance == null)
            {
                if (photonView.IsMine)
                {
                    player_PUN.LocalPlayerInstance = this.gameObject;
                }
            }

            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

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
                        //spr.flipX = false;
                        if (transform.rotation.y != 0)
                            transform.Rotate(new Vector3(0, -180, 0));
                    }
                }
                else if (LeftPress)
                {
                    if (CheckMove(Vector2.left))
                    {
                        target_pos = new Vector2(transform.position.x - MoveDist, transform.position.y);
                        step = speed * Time.deltaTime;
                        transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                        //spr.flipX = true;


                        if (transform.rotation.y != 180)
                            transform.Rotate(new Vector3(0, 180, 0));

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
                        //instead of flipping the sprite we rotate the player to ensure changes are reflected across the network by PhotonTransformView
                            if (x > 0)
                            {
                                if (CheckMove(Vector2.right))
                                {
                                    target_pos = new Vector2(transform.position.x + MoveDist, transform.position.y);
                                    step = speed * Time.deltaTime;
                                    transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                                    //spr.flipX = false;
                                    if (transform.rotation.y != 0)
                                        transform.Rotate(new Vector3(0, -180, 0));
                                }
                            }
                            else
                            {
                                if (CheckMove(Vector2.left))
                                {
                                    target_pos = new Vector2(transform.position.x - MoveDist, transform.position.y);
                                    step = speed * Time.deltaTime;
                                    transform.position = Vector2.MoveTowards(transform.position, target_pos, step);
                                    //spr.flipX = true;


                                    if (transform.rotation.y != 180)
                                        transform.Rotate(new Vector3(0, 180, 0));
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

        }

        void OnTriggerEnter2D(Collider2D other)
        {
            

            if (other.gameObject.CompareTag("Player"))
            {
                curCloseClient = other.gameObject;
                closeToClient = true;
            }
            else if(other.gameObject.CompareTag("TaskObj"))
            {
                canDoTask = true;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                closeToClient = false;
                curCloseClient = null;
            }
            else if (other.gameObject.CompareTag("TaskObj"))
            {
                canDoTask = false;
            }


        }

        #endregion

        #region Private Functions


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

#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
#endif

        #endregion

        #region Public Functions


        public void ExecuteKill()
        {
            myAnim.Play("player_Attack");
            //the attack animation is played when the kill button is pressed, even if we are not close to anyone!

            if (closeToClient && curCloseClient)
            {
                
                //PhotonNetwork.Instantiate("DeadBody", curCloseClient.transform.position, Quaternion.identity);
                curCloseClientId = curCloseClient.GetComponent<PhotonView>().ViewID;
                curCloseClient.GetComponent<player_PUN>().myPV.RPC("OnPlayerKilled", RpcTarget.All, curCloseClientId);

                //disconnect the other player immediately as the kill is executed thru and RPC after instantiating the dead body
            }
        }

        public void becomeIntruder(int intruderIndex)
        {
            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[intruderIndex])
            {
                Debug.Log("The index for intruder matched ! Index : " + intruderIndex);
                isIntruder = true;
                Debug.Log("set isIntruder boolean to true");
            }
            else
            {
                Debug.Log("The index for intruder did not match. Index : " + intruderIndex);

            }
        }

        public void DoTask()
        {
            if(canDoTask)
            {
                //code for the task
            }
        }

#if UNITY_EDITOR_WIN
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

#endif
#if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        
#endif
        /*
        public void OnPlayerKilled()
        {
            //do something when the player is killed
        }
        */
        

        #endregion

        #region Misc

        #endregion

        #region IPunObservable implementation


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(isIntruder);
            }
            else
            {
                isIntruder = (bool)stream.ReceiveNext();
            }
        }


        #endregion

        #region PUN RPC

        [PunRPC]
        void OnPlayerKilled(int idOfPlayer)
        {
            Debug.Log("called OnPlayerKilled RPC");

            Debug.Log(idOfPlayer);
            
            if(!myPV.IsMine)
            {
                return;
            }

            int curId = myPV.ViewID;
            Debug.Log(curId);

            if (idOfPlayer == curId)
            {
                Debug.Log("curCloseClient matched !");
                PhotonNetwork.Disconnect();
                Debug.Log("Disconnected !");
            }
        }

        #endregion

    }
}


/* =============================================================================
# Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
# Email:           aurav.tomar@gmail.com
# FileName:        Player.cs
# Updated On:      15/07/2021
============================================================================= */
