using UnityEngine;
using UnityEngine.UI;

namespace WordPuzzle
{
    public class MainMenuView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button _playButton;
        [SerializeField] private Button _tasksButton;
        [SerializeField] private Button _profileButton;
        [SerializeField] private Button _messagesButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _debugLoadLevelsButton;
        [SerializeField] private RectTransform _modeSelectionPopup;
        [SerializeField] private Button _soloButton;
        [SerializeField] private Button _onlineButton;
        [SerializeField] private Button _cancelButton;

        #endregion


        #region Properties

        public Button PlayButton => _playButton;
        public Button TasksButton => _tasksButton;
        public Button DebugLoadLevelsButton => _debugLoadLevelsButton;
        public Button SoloButton => _soloButton;
        public Button OnlineButton => _onlineButton;
        public Button CancelButton => _cancelButton;
        public Button ProfileButton => _profileButton;
        public Button MessagesButton => _messagesButton;
        public Button ShopButton => _shopButton;
        public Button SettingsButton => _settingsButton;

        #endregion


        #region UnityMethods

        private void Awake()
        {
            #if !UNITY_EDITOR && !UNITY_STANDALONE_WIN
            _debugLoadLevelsButton.gameObject.SetActive(false);
            #endif
        }

        #endregion


        #region Methods

        public void HideModeSelectionPopup() => _modeSelectionPopup.gameObject.SetActive(false);

        public void ShowModeSelectionPopup() => _modeSelectionPopup.gameObject.SetActive(true);

        #endregion
    }
}