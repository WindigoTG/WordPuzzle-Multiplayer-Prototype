using UnityEngine;
using UnityEngine.UI;

namespace WordPuzzle
{
    public class SettingsView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button _settingsButton;

        #endregion


        #region Properties

        public Button SettingsButton => _settingsButton;

        #endregion
    }
}