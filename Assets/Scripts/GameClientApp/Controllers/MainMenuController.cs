namespace WordPuzzle
{
    public class MainMenuController : BaseController
    {
        #region Fields

        protected readonly PlayerProfile _playerProfile;
        protected MainMenuView _view;
        protected Context _context;

        #endregion


        #region ClassLifeCycles

        public MainMenuController(Context context)
        {
            _context = context;
            _playerProfile = _context.PlayerProfile;

            _context.ScreenFadeController.FadeOutWithCallback(Init);
        }

        #endregion


        #region Methods

        private async void Init()
        {
            InstantiatePrefab<MainMenuView>(_context.UIPrefabsData.MainMenu, _context.VariableUiHolder, InitView);

            await _context.ProfileService.RetrievePlayerData();
            await _context.FriendListController.RetrieveFriendList();
            await _context.ChatController.RetrieveMessages();

            _context.ScreenFadeController.FadeInWithCallback(() => { });
        }

        protected void InitView(MainMenuView view)
        {
            _view = view;
            _view.PlayButton.onClick.AddListener(_view.ShowModeSelectionPopup);
            _view.CancelButton.onClick.AddListener(_view.HideModeSelectionPopup);
            _view.DebugLoadLevelsButton.onClick.AddListener(DebugLoadLevels);
            _view.SoloButton.onClick.AddListener(() => StartGame(false));
            _view.OnlineButton.onClick.AddListener(() => StartGame(true));
            _view.ProfileButton.onClick.AddListener(OnProfileButtonClick);
            _view.MessagesButton.onClick.AddListener(OnMessagesButtonClick);
            _view.ShopButton.onClick.AddListener(OnShopButtonClick);
            _view.SettingsButton.onClick.AddListener(OnSettingsButtonClick);

            _view.HideModeSelectionPopup();
        }

        protected void StartGame(bool isOnlineSelected)
        {
            _context.ScreenFadeController.FadeOutWithCallback(() => {
                _playerProfile.CurrentState.Value = GameState.ThemeSelect;
                _playerProfile.IsOnlinePlaySelected = isOnlineSelected;
                _context.ScreenFadeController.FadeInWithCallback(() => { });
            });
            
        }

        protected void DebugLoadLevels()
        {
            if (_context.CrosswordService is CrosswordService)
            (_context.CrosswordService as CrosswordService).LoadAdditionalThemes();
        }

        private void OnProfileButtonClick()
        {
            _context.ProfileService.ShowPlayerProfileUI();
        }

        private void OnMessagesButtonClick()
        {
            _context.FriendListController.ShowFriendsWindow();
        }

        private void OnShopButtonClick()
        {
            
        }

        private void OnSettingsButtonClick()
        {
            
        }

        #endregion


        #region IDisposeable

        protected override void OnDispose()
        {
            _view.DebugLoadLevelsButton.onClick.RemoveAllListeners();
            _view.PlayButton.onClick.RemoveAllListeners();
            _view.CancelButton.onClick.RemoveAllListeners();
            _view.SoloButton.onClick.RemoveAllListeners();
            _view.OnlineButton.onClick.RemoveAllListeners();
            _view.ProfileButton.onClick.RemoveAllListeners();
            _view.MessagesButton.onClick.RemoveAllListeners();
            _view.ShopButton.onClick.RemoveAllListeners();
            _view.SettingsButton.onClick.RemoveAllListeners();
            base.OnDispose();
        }

        #endregion
    }
}