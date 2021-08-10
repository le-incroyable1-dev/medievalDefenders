using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace Com.MyCompany.fatman
{
    public class camFollowLocal : MonoBehaviour
    {

        #region Public Fields

        public Transform target;
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;

        #endregion

        #region Private Fields

        private float m_OffsetZ;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_CurrentVelocity;
        private Vector3 m_LookAheadPos;

        #endregion

        #region Monobehaviour Callbacks

        // Use this for initialization
        void Start()
        { 

            if (player_PUN.LocalPlayerInstance)
            {
                target = player_PUN.LocalPlayerInstance.transform; // setting the target to the local player instance in the scene
            }

            if(target)
            {

                m_LastTargetPosition = target.position;
                m_OffsetZ = (transform.position - target.position).z;
                transform.parent = null;
            }
        }


        // Update is called once per frame
        private void Update()
        {
            //
            if (target == null)
            {
                if (player_PUN.LocalPlayerInstance != null)
                {
                    target = player_PUN.LocalPlayerInstance.transform;
                    Debug.Log("Found player instance ! : CamFollow.cs.Update()");
                    m_LastTargetPosition = target.position;
                    m_OffsetZ = (transform.position - target.position).z;
                    transform.parent = null;
                }
            }

            if (target)
            {
                //
                // only update lookahead pos if accelerating or changed direction
                float xMoveDelta = (target.position - m_LastTargetPosition).x;

                bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

                if (updateLookAheadTarget)
                {
                    m_LookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
                }
                else
                {
                    m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
                }

                Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
                Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

                transform.position = newPos;

                m_LastTargetPosition = target.position;
            }
        }

        #endregion
    }
}

/* =============================================================================
# Author:          Aurav S Tomar - https://github.com/le-incroyable1-dev
# Email:           aurav.tomar@gmail.com
# FileName:        camFollowLocal.cs
# Updated On:      10/08/2021
============================================================================= */
