using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace Com.MyCompany.Pacman
{
    public class player_UI : MonoBehaviour
    {
        #region Private Fields

        private Player target;

        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private TMP_Text playerNameText;


        //[Tooltip("UI Slider to display Player's Health")]
        //[SerializeField]
        //private Slider playerHealthSlider;

        [SerializeField]
        private float targetOffsetforUi = 0f;

        Transform targetTransform;

        #endregion


        #region MonoBehaviour Callbacks

        void Awake()
        {
            this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
        }

        void Update()
        {
            // Reflect the Player Health
            /*
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = target.Health;
            }
            */

            // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
            if (target == null)
            {
                Destroy(this.gameObject);
                return;
            }
        }

        void LateUpdate()
        {


            // #Critical
            // Follow the Target GameObject on screen.
            if (targetTransform != null)
            {
                Vector3 uiPos = Camera.main.WorldToScreenPoint(targetTransform.position);
                uiPos.y += targetOffsetforUi;
                transform.position = uiPos;
            }
        }

        #endregion


        #region Public Methods

        public void SetTarget(Player _target)
        {
            if (_target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            // Cache references for efficiency
            target = _target;
            if (playerNameText != null)
            {
                playerNameText.text = target.photonView.Owner.NickName;
            }

            targetTransform = this.target.GetComponent<Transform>();
        }

        #endregion
    }
}

/* =============================================================================
# Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
# Email:           aurav.tomar@gmail.com
# FileName:        player_UI.cs
# Updated On:      14/05/2021
============================================================================= */
