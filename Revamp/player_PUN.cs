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

        public float speed = 5.0f;
        private float step;
        //private float counter = 10.0f;

        private CircleCollider2D cc;
        private Animator myAnim;
        private SpriteRenderer spr;
        private AudioSource src;

        public bool UpPress = false;
        public bool DownPress = false;
        public bool RightPress = false;
        public bool LeftPress = false;

        public bool canDoTask = false;

        private Touch playerTouch;
        private Vector2 touchStart, touchEnd;

        public GameObject curCloseClient = null;
        public int curCloseClientId = -1;

        private PhotonView myPV;

        public GameObject curTaskObj = null;
        public int taskCompletion = 0;

        public Slider susSlider;

        #endregion

        #region Public Fields

        public Vector2 target_pos;
        public float MaxRayDist;
        public float MoveDist;

        public bool isIntruder = false;
        public bool closeToClient = false;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The Player's UI GameObject Prefab")]
        public GameObject PlayerUiPrefab;

        public GameObject PlayerSusScreenPrefab;

        public GameObject uiGO;

        public GameObject playerSuspectScreen = null;
        public GameObject loseScreen = null;
        public GameObject winScreen = null;

        public GameObject curSusPlayer = null;

        public GameObject[] curPlayersList = null;

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
            uiGO = Instantiate(PlayerUiPrefab);
            uiGO.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
           
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

            MaxRayDist = 0.15f;
            MoveDist = 0.15f;

            myAnim = GetComponent<Animator>();
            myPV = GetComponent<PhotonView>();
            cc = GetComponent<CircleCollider2D>();
            spr = GetComponent<SpriteRenderer>();
            src = GetComponent<AudioSource>();

            //loseScreen.SetActive(false);
            //winScreen.SetActive(true);

#if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

#endif

            if (PlayerUiPrefab != null)
            {
                uiGO = Instantiate(PlayerUiPrefab);
                uiGO.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

            susSlider = uiGO.GetComponentInChildren<Slider>();
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

            //use UNITY_STANDALONE_WIN for .exe build



            if (myPV.IsMine)
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

            if(Input.GetMouseButtonDown(0) && myPV.IsMine)
            {
                //Debug.Log("Mouse(0) clicked !");
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0);
                if (hit)
                {
                    //Debug.Log("Object hit : " + hit.collider.gameObject.name);
                    if (hit.collider.CompareTag("Player") && !isIntruder &&!hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
                    {
                        //if i clicked on a player, given that i am not the intruder and that player is not me !
                        Debug.Log("Clicked on a player !!");
                        curSusPlayer = hit.collider.gameObject;
                        playerSuspectScreen.SetActive(true);
                    }
                }
            }



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
                curTaskObj = other.gameObject;
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
                taskCompletion = 0;
                curTaskObj = null;
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
                if (rch && (rch.collider.gameObject.CompareTag("Wall") /*|| rch.collider.gameObject.CompareTag("Player"))*/))
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

        public void OnReportKillButton()
        {
            if (curSusPlayer.GetComponent<player_PUN>().isIntruder)
            {
                int curSusPlayerId = curSusPlayer.GetComponent<PhotonView>().ViewID;
                Debug.Log("intruderID being kicked : " + curSusPlayerId);
                curSusPlayer.GetComponent<player_PUN>().myPV.RPC("OnPlayerKilled", RpcTarget.All, curSusPlayerId);
                myPV.RPC("TurnOnWinScreenForMembers", RpcTarget.All);
            }
            else
            {
                loseScreen.SetActive(true);
                StartCoroutine(PauseExecutionForDisconnect(3));
            }
        }

        public void OnSuspectButton()
        {
            int curSusPlayerId = curSusPlayer.GetComponent<PhotonView>().ViewID;
            Debug.Log("intruderID being sused : " + curSusPlayerId);
            curSusPlayer.GetComponent<player_PUN>().myPV.RPC("OnSusedPlayer", RpcTarget.All, curSusPlayerId);
        }

        public void CloseSusScreen()
        {
            playerSuspectScreen.SetActive(false);
        }

        public void ExecuteKill()
        {
            myAnim.Play("player_Attack");
            //the attack animation is played when the kill button is pressed, even if we are not close to anyone!

            if (closeToClient && curCloseClient && photonView.IsMine)
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

        public void performTask(int increment)
        {
            if (photonView.IsMine)
            {

                if (canDoTask)
                {
                    myAnim.Play("player_Attack");
                    taskCompletion += increment;
                }

                if (taskCompletion >= 100)
                {
                    Debug.Log("Task completed !");
                    
                    if(curTaskObj)
                    {
                        curTaskObj.GetComponent<PhotonView>().RequestOwnership();
                        Debug.Log("Ownership Requested !");
                        //request ownership 
                        
                        PhotonNetwork.Destroy(curTaskObj);
                        //take ownership of the object and network destroy it
                    }


                    curTaskObj = null;
                    canDoTask = false;
                    taskCompletion = 0;
                }

            }
        }


        /*
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
        */

#if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        
#endif
        
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

            if (!myPV.IsMine)
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
        

        [PunRPC]
        void TurnOnWinScreenForMembers()
        {
            if (!myPV.IsMine)
                return;

            if(!isIntruder)
                winScreen.SetActive(true);
        }

        [PunRPC]
        void OnSusedPlayer(int idOfPlayer)
        {
            Debug.Log("called OnSusedPlayer RPC");

            Debug.Log(idOfPlayer);

            /*
            if (!myPV.IsMine)
            {
                return;
            }
            */

            int curId = myPV.ViewID;
            Debug.Log(curId);

           curPlayersList = GameObject.FindGameObjectsWithTag("Player");

            for(int i = 0; i < curPlayersList.Length; i++)
            {
                Debug.Log("Current id being checked for sus : " + curPlayersList[i].GetComponent<PhotonView>().ViewID);

                if(curPlayersList[i].GetComponent<PhotonView>().ViewID == idOfPlayer)
                {
                    Debug.Log("Sus id matched : " + idOfPlayer);
                    curPlayersList[i].GetComponent<player_PUN>().susSlider.value += 0.1f;
                }
            }
        }

        #endregion

        #region Coroutines

        IEnumerator PauseExecutionForDisconnect(int seconds)
        {
            yield return new WaitForSeconds(seconds);
            PhotonNetwork.Disconnect();
        }

        #endregion

    }
}



/* =============================================================================
# Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
# Email:           aurav.tomar@gmail.com
# FileName:        player_PUN.cs
# Updated On:      10/08/2021
============================================================================= */
