using Event;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public Button _startGameButton;

        private void Awake()
        {
            _startGameButton.onClick.AddListener(OnStartGameClicked);
            EventBus.Game.AddListener(GameEventType.GAME_PRE_END, OnGameEnd);
        }

        private void OnDestroy()
        {
            _startGameButton.onClick.RemoveListener(OnStartGameClicked);
            EventBus.Game.RemoveListener(GameEventType.GAME_PRE_END, OnGameEnd);
        }

        private void OnStartGameClicked()
        {
            Game.Instance.UI.MainMenuCanvasGroup.alpha = 0;
            EventBus.Game.SendMessage(GameEventType.GAME_START);
        }

        private void OnGameEnd()
        {

        }
    }
}