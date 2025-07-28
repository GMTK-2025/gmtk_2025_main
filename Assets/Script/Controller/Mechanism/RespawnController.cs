using System.Collections.Generic;

namespace Script.Controller.Mechanism
{
    public class RespawnController
    {
        private readonly List<RespawnPoint> _respawnPoints = new();

        public void FrameUpdate()
        {
        }

        public void PhysicsUpdate()
        {
        }

        public void Respawn(PlayerController player)
        {
            int idx = -1;
            for (int i = 0; i < _respawnPoints.Count; i++)
            {
                if (idx < _respawnPoints[i].index)
                {
                    idx = _respawnPoints[i].index;
                }
            }
            if (idx != -1) _respawnPoints[idx].transform.position = player.transform.position;
        }

        public void AddRespawnPoint(RespawnPoint respawnPoint)
        {
            if (_respawnPoints.Contains(respawnPoint)) return;
            if (_respawnPoints.Count == 0) _respawnPoints.Add(respawnPoint);
        }
    }
}