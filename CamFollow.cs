using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace Com.MyCompany.Pacman
{
    public class CamFollow : MonoBehaviourPunCallbacks
    {
        public Transform target;
        public float damping = 1;
        public float lookAheadFactor = 3;
        public float lookAheadReturnSpeed = 0.5f;
        public float lookAheadMoveThreshold = 0.1f;

        private float m_OffsetZ;
        private Vector3 m_LastTargetPosition;
        private Vector3 m_CurrentVelocity;
        private Vector3 m_LookAheadPos;

        // Use this for initialization
        void Start()
        {
            
            m_LastTargetPosition = target.position;
            m_OffsetZ = (transform.position - target.position).z;
            transform.parent = null;

            target = Player.LocalPlayerInstance.transform; // setting the target to the local player instance in the scene

        }


        // Update is called once per frame
        private void Update()
        {
            //
            if (target == null)
            {
                if (Player.LocalPlayerInstance != null)
                {
                    target = Player.LocalPlayerInstance.transform;
                    Debug.Log("<Color=Red>Found player instance ! : CamFollow.cs.Update()");
                    m_LastTargetPosition = target.position;
                    m_OffsetZ = (transform.position - target.position).z;
                    transform.parent = null;
                }
            }
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
}

