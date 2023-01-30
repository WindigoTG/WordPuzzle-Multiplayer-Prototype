namespace WordPuzzle
{
    public class MainController : BaseController, IUpdateableRegular
    {
        #region Fields

        private Context _context;
        private PlayerProfile _playerProfile;

        private MainMenuController _mainMenuController;
        private SocialController _socialController;
        private SettingsController _settingsController;
        private ThemeSelectionController _themeSelectionController;
        private GameplayController _gameplayController;
        private ScreenFadeController _screenFadeController;
        private MatchmakingController _matchmakingController;
        private LoginController _loginController;

        #endregion


        #region ClassLifeCycles

        public MainController(Context context)
        {
            _context = context;
            _playerProfile = _context.PlayerProfile;
            _playerProfile.CurrentState.SubscribeOnChange(OnChangeGameState);

            //_socialController = new SocialController(_context);
            //_settingsController = new SettingsController(_context);
            _screenFadeController = new ScreenFadeController(_context);
            //AddController(_socialController);
            //AddController(_settingsController);
            AddController(_screenFadeController);
            _context.SetScreenFadeController(_screenFadeController);
            AddController(new FriendListController(_context));
            AddController(new ProfileService(_context));
            AddController(new ChatController(_context));

            OnChangeGameState(_playerProfile.CurrentState.Value);
        }

        #endregion


        #region Methods

        private void OnChangeGameState(GameState state)
        {
            switch (state)
            {
                case GameState.Menu:
                    {
                        RemoveControllers();
                        _mainMenuController = new MainMenuController(_context);
                        AddController(_mainMenuController);
                        break;
                    }

                case GameState.ThemeSelect:
                    {
                        RemoveControllers();
                        _themeSelectionController = new ThemeSelectionController(_context);
                        AddController(_themeSelectionController);
                        break;
                    }

                case GameState.Game:
                    {
                        RemoveControllers();
                        if (_playerProfile.IsOnlinePlaySelected)
                            _gameplayController = new OnlineGameplayController(_context);
                        else
                            _gameplayController = new SoloGameplayController(_context);

                        AddController(_gameplayController);
                        break;
                    }

                case GameState.Matchmaking:
                    {
                        RemoveControllers();
                        _matchmakingController = new MatchmakingController(_context);
                        AddController(_matchmakingController);
                        break;
                    }

                case GameState.Login:
                    RemoveControllers();
                    _loginController = new LoginController(_context);
                    AddController(_loginController);
                    break;

                default:
                    {
                        RemoveControllers();
                    }
                    break;
            }
        }

        private void RemoveControllers()
        {
            RemoveController(_mainMenuController);
            RemoveController(_themeSelectionController);
            RemoveController(_gameplayController);
            RemoveController(_matchmakingController);
            RemoveController(_loginController);
        }

        #endregion


        #region IUpdateableRegular

        public void UpdateRegular()
        {
            for (int i = 0; i < _baseControllers.Count; i++)
            {
                if (_baseControllers[i] is IUpdateableRegular)
                    (_baseControllers[i] as IUpdateableRegular).UpdateRegular();
            }
        }

        #endregion


        #region IDisposeable

        protected override void OnDispose()
        {
            _playerProfile.CurrentState.UnSubscribeOnChange(OnChangeGameState);
            base.OnDispose();
        }

        #endregion
    }
}