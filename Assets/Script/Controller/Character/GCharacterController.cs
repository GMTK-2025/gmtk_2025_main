namespace Script.Controller.Character
{
    public class GCharacterController
    {
        private PlayerController _player;

        public PlayerController Player
        {
            get
            {
                if (_player == null)
                {
                    return null;
                }

                return _player;
            }
            set => _player = value;
        }
    }
}