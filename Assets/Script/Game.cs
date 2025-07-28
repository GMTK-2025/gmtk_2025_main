using Event;
using Script.Controller.Mechanism;
using Tools;

public class Game : Singleton<Game>
{
    private RespawnController _respawn;

    public RespawnController Respawn => _respawn;

    void Start()
    {
        _respawn = new RespawnController();
    }

    void Update()
    {
    }

    public void StartGame()
    {
        EventBus.Game.SendMessage(GameEventType.GAME_START);
    }
    
}