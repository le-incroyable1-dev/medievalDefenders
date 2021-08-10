using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Com.MyCompany.fatman
{
    public class playerUIcontrol : MonoBehaviour
    {
        #region Private Fields

        private player_PUN target;

        [Tooltip("UI Text to display Player's Name")]
        [SerializeField]
        private TMP_Text playerNameText;


        [Tooltip("UI Slider to display Player's Suspect Level")]
        [SerializeField]
        private Slider playerSusSlider;

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

        public void SetTarget(player_PUN _target)
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
# FileName:        playerUIcontrol.cs
# Updated On:      10/08/2021
============================================================================= */
