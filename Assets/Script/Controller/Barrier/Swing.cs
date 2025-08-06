using System;
using UnityEngine;

namespace Script.Controller.Barrier
{
    public class Swing : MonoBehaviour
    {
        public float rotationSpeed;
        public float rotationTime;
        public float rotationAngle;
        public float currentAngle;
        public Transform twig;

        private PlayerController _currentPlayer; 

        private void FixedUpdate()
        {
            Rotate();

          
            if (_currentPlayer != null && Input.GetKey(KeyCode.Space))
            {
                Release(_currentPlayer);
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.SwitchState(player.hangOnState);
                    player.transform.SetParent(twig);
                    player.transform.SetPositionAndRotation(twig.position, Quaternion.identity);
                    player.SetRigidActive(false);
                    player.SetColliderActive(false);
                    player.Swing = this;

                    _currentPlayer = player; 
                }
            }
        }

        public void Rotate()
        {
            Vector3 angle = new Vector3(0f, 0f, currentAngle);
            if (transform.rotation.eulerAngles.z > rotationAngle || transform.rotation.eulerAngles.z < -rotationAngle)
            {
                currentAngle = -currentAngle;
                angle = new Vector3(0f, 0f, currentAngle);
            }

            transform.Rotate(angle / rotationTime * rotationSpeed * Time.deltaTime);
        }

        public void Release(PlayerController player)
        {
            if (player == null) return;

            player.SetRigidActive(true);
            player.SetColliderActive(true);
            player.transform.SetParent(null);
            player.SwitchState(player.fallState);
            _currentPlayer = null;
            player.Swing = null;
        }
    }
}