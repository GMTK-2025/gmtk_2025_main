using System.Collections;
using UnityEngine;

namespace Assets.Script.UI
{
    public class UIController : MonoBehaviour
    {
        [field: SerializeField] public GameplayUI GameplayUI { get; set; }
        [field: SerializeField] public CanvasGroup GameplayCanvasGroup { get; set; }
        [field: SerializeField] public CanvasGroup MainMenuCanvasGroup { get; set; }

        public void EnterMainMenu()
        {
            MainMenuCanvasGroup.alpha = 1;
        }
    }
}