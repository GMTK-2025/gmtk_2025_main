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

        private void FixedUpdate()
        {
            Rotate();
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                player.SwitchState(player.hangOnState);
                player.gameObject.transform.SetParent(twig);
                player.gameObject.transform.SetPositionAndRotation(twig.transform.position, Quaternion.Euler(new Vector3(0,0,0)));
                player.SetRigidActive(false);
                player.SetColliderActive(false);
                player.Swing = this;
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
        }
    }
}