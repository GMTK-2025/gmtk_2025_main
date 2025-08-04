using Assets.Script.UI;
using Event;
using Script.Controller.Character;
using Script.Controller.Mechanism;
using Tools;
using UnityEngine;

public class Game : Singleton<Game>
{
    private RespawnController _respawn;
    private GCharacterController _gCharacter;
    [field: SerializeField] public PlayerController Player { get; set; }
    public RespawnController Respawn => _respawn;
    public GCharacterController GCharacter => _gCharacter;
    [field: SerializeField] public UIController UI { get; set; }


    private void Start()
    {
        _respawn = new RespawnController();
        _gCharacter = new GCharacterController();
    }

    private void Update()
    {
    }

    public void StartGame()
    {
        EventBus.Game.SendMessage(GameEventType.GAME_START);
    }

    public void EndGame()
    {
        EventBus.Game.SendMessage(GameEventType.GAME_END);
    }

    public void PauseGame()
    {
    }

    public void ResumeGame()
    {
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}