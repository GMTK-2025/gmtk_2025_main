using UnityEngine;

namespace Script.Controller.Mechanism
{
    public class RespawnPoint : MonoBehaviour
    {
        public int index;
        private Collider2D _collider;
        private bool _isActive;

        public RespawnPoint(bool isActive)
        {
            _isActive = isActive;
        }

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _isActive = false;
            Game.Instance.Respawn.AddRespawnPoint(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            other.gameObject.TryGetComponent(out PlayerController player);
            _isActive = true;
            Debug.Log("Invoke player save");
        }
    }
}