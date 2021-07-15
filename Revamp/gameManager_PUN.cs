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
        public int intruderPlayerIndex = -1;
        public player_PUN playerScript;

        #endregion

        #region Public Fields

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        public GameObject killButton;

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
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(Random.Range(-0.2f,0.2f), 0f, -1f), Quaternion.identity, 0);
            }

            playerScript = player_PUN.LocalPlayerInstance.GetComponent<player_PUN>();

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(PauseExecution(5));
                //Pauses execution here for 5 seconds, and then chooses the intruder client 
                chooseIntruder();

            }

            


            if(PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[intruderPlayerIndex])
            {
                StartCoroutine(PauseExecution(5));
                //Pauses execution here for 5 seconds, and then chooses the intruder client
                killButton.SetActive(true);
                //playerScript.isIntruder = true;
                //if the local player is the intruder, then we allow him to access the kill button
            }

        }

        private void Update()
        {
            
            if(!killButton.activeSelf)
            {
                StartCoroutine(PauseExecution(3));
                //pause execution for 3 seconds in order to avoid unnecessary load
                if (intruderPlayerIndex < PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[intruderPlayerIndex])
                    {
                        killButton.SetActive(true);
                        //if the local player is the intruder, then we allow him to access the kill button
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
        
        public void OnKillButton()
        {
            playerScript.ExecuteKill();
        }

        public void Leave()
        {
            PhotonNetwork.Disconnect();
        }

        #endregion

        #region Private Functions 

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

        IEnumerator PauseExecution(int seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        #endregion
    }
}


/* =============================================================================
# Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
# Email:           aurav.tomar@gmail.com
# FileName:        gameManager_PUN.cs
# Updated On:      15/07/2021
============================================================================= */
