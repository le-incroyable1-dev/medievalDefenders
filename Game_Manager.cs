using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

namespace Com.MyCompany.Pacman
{
    public class Game_Manager : MonoBehaviourPunCallbacks
    {

        #region Public Fields

        private AudioSource game_audio;
        public GameObject WinStruct;
        public GameObject LoseStruct;
        public static Game_Manager Instance;

        [Tooltip("This is the player prefab that will be spawned.")]
        public GameObject playerPrefab;

        #endregion

        #region MonoBehaviour Callbacks
        #endregion

        #region Photon CallBacks


        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        public override void OnPlayerEnteredRoom(Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                LoadArena();
            }
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom


                LoadArena();
            }
        }

        #endregion


        #region Public Methods


        public void LeaveRoom()
        {
            //Destroy(GameObject.Find("Canvas"));
            //Destroy(PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>().PlayerUiPrefab);
            PhotonNetwork.LeaveRoom();
        }

        public void OnWin()
        {
            LoseStruct.SetActive(false);
            WinStruct.SetActive(true);
        }

        public void OnLose()
        {
            WinStruct.SetActive(false);
            LoseStruct.SetActive(true);
        }

        public void SetAudio()
        {
            game_audio.volume = 1;
        }


        #endregion


        #region Private Methods

        private void Start()
        {
            game_audio = GetComponent<AudioSource>();
            SetAudio();
            //LoseStruct.SetActive(false);
            //WinStruct.SetActive(false);
            //DontDestroyOnLoad(gameObject);

            Instance = this;
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (Player.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 12f, 0f), Quaternion.identity, 0);
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }

            }

        }

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for 1");
        }

        #endregion
    }
}
/* =============================================================================
#  Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
#  Email:           aurav.tomar@gmail.com
#  FileName:        Game_Manager.cs
#  Created On:      26/11/2020
#  Updated On:      14/05/2021
============================================================================= */
