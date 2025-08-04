using System.Collections.Generic;
using Event;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.UI
{
    public class GameplayUI : MonoBehaviour
    {
        public Button _mainMenuButton;
        public List<GameObject> _life;
        public CanvasGroup _gameplayCanvasGroup;
        public CanvasGroup _settingCanvasGroup;
        public CanvasGroup _wincanvasGroup;

        private void Awake()
        {
            _gameplayCanvasGroup.alpha = 0;
            _mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            EventBus.Character.AddListener(CharacterEventType.CHARACTER_HURT, OnCharacterHurt);
            EventBus.Game.AddListener(GameEventType.GAME_START, OnStartGame);
            EventBus.Game.AddListener(GameEventType.GAME_PRE_END, OnGameEnd);
        }

        private void OnDestroy()
        {
            EventBus.Character.RemoveListener(CharacterEventType.CHARACTER_HURT, OnCharacterHurt);
            EventBus.Game.RemoveListener(GameEventType.GAME_START, OnStartGame);
            EventBus.Game.RemoveListener(GameEventType.GAME_PRE_END, OnGameEnd);
        }

        private void OnCharacterHurt()
        {
            int life = Game.Instance.Player.GetGhostSystem().currentLives;
            if (life < 0) return;
            for (int i = _life.Count - 1; i >= life; i--)
            {
                _life[i].SetActive(false);
            }
        }

        private void OnMainMenuButtonClicked()
        {
            Game.Instance.UI.EnterMainMenu();
        }

        private void OnStartGame()
        {
            _settingCanvasGroup.alpha = 0;
            _gameplayCanvasGroup.alpha = 1;
            for (int i = 0; i < _life.Count; i++)
            {
                _life[i].SetActive(true);
            }
        }

        private void OnGameEnd()
        {
            _wincanvasGroup.alpha = 1;
        }
    }
}