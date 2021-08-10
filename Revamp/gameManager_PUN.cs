using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

namespace Com.MyCompany.fatman
{
    public class gameManager_PUN : MonoBehaviourPunCallbacks
    {
        #region Private Fields

        PhotonView pv;
        int intruderPlayerIndex = -1;
        player_PUN playerScript;

        int callCount = 0;
        bool check = false;

        #endregion

        #region Public Fields

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        public GameObject killButton;

        public GameObject susScreen = null;


        #endregion

        #region Monobehaviour Callbacks

        private void Start()
        {
            pv = GetComponent<PhotonView>();

            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(Random.Range(-7f,7f), 0f, -1f), Quaternion.identity, 0);
            }

            

            
            playerScript = player_PUN.LocalPlayerInstance.GetComponent<player_PUN>();
            Debug.Log("Found player_PUN for local player !");
            //setup the local player script reference correctly

            playerScript.playerSuspectScreen = susScreen;


            if (PhotonNetwork.IsMasterClient)
            {
                //ensure these functions are called only ONCE


                SetupTasks();
                //setup the required task objects 

                chooseIntruder();
                //choose the Intruder

            }


            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[intruderPlayerIndex])
            {
                killButton.SetActive(true);
                //playerScript.isIntruder = true;
                //if the local player is the intruder, then we allow him to access the kill button
                callCount = 1;
            }





        }

        private void Update()
        {
            if(!playerScript && player_PUN.LocalPlayerInstance)
            {
                playerScript = player_PUN.LocalPlayerInstance.GetComponent<player_PUN>();
                Debug.Log("Found player_PUN for local player !");
                //setup the local player script reference correctly
            }


            if(!killButton.activeSelf && callCount == 0) 
            {
                //check callCount to ensure we don't call these more than ONCE

                if (PhotonNetwork.IsMasterClient)
                {
                    chooseIntruder();
                }


                if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[intruderPlayerIndex])
                {
                    killButton.SetActive(true);
                    //playerScript.isIntruder = true;
                    //if the local player is the intruder, then we allow him to access the kill button
                    callCount = 1;
                }

                
            }

            if(playerScript)
            {
                if(playerScript.isIntruder && !check)
                {
                    if(! (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[intruderPlayerIndex]))
                    {
                        playerScript.isIntruder = false;
                        check = true;
                    }
                }
            }




        }

        #endregion

        #region PUN CALLBACKS

        public override void OnDisconnected(DisconnectCause cause)
        {
            SceneManager.LoadScene(0);
        }

        public override void OnLeftRoom()
        {
            PhotonNetwork.Disconnect();
        }

        #endregion

        #region Public Functions
        
        public void susScreenOnKillReportButton()
        {
            if (playerScript)
                playerScript.OnReportKillButton();
        }

        public void susScreenOnReportSusButton()
        {
            if (playerScript)
                playerScript.OnSuspectButton();
        }

        public void DoTask()
        {
            if(playerScript)
                playerScript.performTask(20);
        }

        public void OnKillButton()
        {
            playerScript.ExecuteKill();
        }

        public void Leave()
        {
            PhotonNetwork.Disconnect();
        }

        public void CloseSusScreen()
        {
            if (playerScript)
                playerScript.CloseSusScreen();
        }

#if UNITY_STANDALONE_WIN
        public void MoveUp()
        {
            if(playerScript)
                playerScript.UpPress = true;

        }
        public void MoveDown()
        {
            if(playerScript)
                playerScript.DownPress = true;

        }
        public void MoveRight()
        {
            if(playerScript)
                playerScript.RightPress = true;
        }
        public void MoveLeft()
        {
            if(playerScript)
                playerScript.LeftPress = true;

        }
        public void MoveUpRelease()
        {
            if(playerScript)
                playerScript.UpPress = false;

        }
        public void MoveDownRelease()
        {
            if(playerScript)
                playerScript.DownPress = false;
        }
        public void MoveRightRelease()
        {
            if(playerScript)
                playerScript.RightPress = false;
        }
        public void MoveLeftRelease()
        {
            if(playerScript)
                playerScript.LeftPress = false;
        }

#endif

        #endregion

        #region Private Functions 


        void SetupTasks()
        {
            PhotonNetwork.Instantiate("tempTaskObject", new Vector3(0, 0, 1), Quaternion.identity);

        }

        void chooseIntruder()
        {
            intruderPlayerIndex = UnityEngine.Random.Range(0, PhotonNetwork.CurrentRoom.PlayerCount);
            pv.RPC("RPC_syncIntruder", RpcTarget.All, intruderPlayerIndex);
        }

        [PunRPC]
        void RPC_syncIntruder(int intruderIndex)
        {
            intruderPlayerIndex = intruderIndex;
            if (player_PUN.LocalPlayerInstance)
            {
                playerScript.becomeIntruder(intruderPlayerIndex);
            }
            else
            {
                Debug.Log("The local player instance is currently null !!");
            }
        }

        #endregion

        #region Coroutines

        IEnumerator DisconnectAndLoad()
        {
            PhotonNetwork.LeaveRoom();

            while (PhotonNetwork.InRoom)
                yield return null;

            SceneManager.LoadScene(0);
        }

        /*
        IEnumerator PauseExecutionForKillButton(int seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (PhotonNetwork.IsMasterClient)
            {
                chooseIntruder();

            }


            if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[intruderPlayerIndex])
            {
                killButton.SetActive(true);
                //playerScript.isIntruder = true;
                //if the local player is the intruder, then we allow him to access the kill button
            }

            
        }
        */

        #endregion
    }
}


/* =============================================================================
# Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
# Email:           aurav.tomar@gmail.com
# FileName:        gameManager_PUN.cs
# Updated On:      10/08/2021
============================================================================= */
