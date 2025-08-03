using Event;
using Script.Controller.Character;
using Script.Controller.Mechanism;
using Tools;

public class Game : Singleton<Game>
{
    private RespawnController _respawn;
    private GCharacterController _gCharacter;
    private 
    public RespawnController Respawn => _respawn;
    public GCharacterController GCharacter => _gCharacter;

    void Start()
    {
        _respawn = new RespawnController();
        _gCharacter = new GCharacterController();
    }

    void Update()
    {
    }

    public void StartGame()
    {
        EventBus.Game.SendMessage(GameEventType.GAME_START);
    }
    
}