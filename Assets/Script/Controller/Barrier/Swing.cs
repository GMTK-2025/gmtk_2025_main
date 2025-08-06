using System;
using Cysharp.Threading.Tasks;
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
        public float _attractInterval = 3f;
        private bool _canAttract = true;

        private void FixedUpdate()
        {
            Rotate();
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!_canAttract) return;
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.SwitchState(player.hangOnState);
                    player.transform.SetParent(twig);
                    player.transform.SetPositionAndRotation(twig.position, Quaternion.identity);
                    player.Swing = this;
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

        public async UniTask Release(PlayerController player)
        {
            player.transform.SetParent(null);
            player.SwitchState(player.fallState);
            Vector2 angle = transform.rotation.eulerAngles;
            player.AddForce(new Vector2(angle.x, angle.y));
            _canAttract = false;
            await UniTask.WaitForSeconds(_attractInterval);
            _canAttract = true;
        }
    }
}