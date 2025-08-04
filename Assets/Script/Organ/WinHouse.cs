using Cysharp.Threading.Tasks;
using Event;
using System;
using UnityEngine;

namespace Assets.Script.Organ
{
    public class WinHouse : MonoBehaviour
    {
        public Collider2D Collider2;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag.Equals("Player"))
            {
                EventBus.Game.SendMessage(GameEventType.GAME_PRE_END);
                QuitGame();
            }
        }

        public async void QuitGame()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            Game.Instance.ExitGame();
        }
    }
}