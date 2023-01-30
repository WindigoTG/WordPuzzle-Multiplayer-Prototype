using System;
using UnityEngine;


namespace WordPuzzle
{
    [Serializable]
    public class Context
    {
        #region Fields

        [SerializeField] private Transform _variableUiHolder;
        [SerializeField] private Transform _commonUiHolder;
        [SerializeField] private Transform _fadeOverlayUiHolder;
        [Space]
        [SerializeField] private UiPrefabsData _uiPrefabsData;
        [Space]
        [SerializeField, Min(0)] private float _screenFadeTime;

        private PlayerProfile _playerProfile;
        private ICrosswordService _worldService;
        private ScreenFadeController _screenFadeController;
        private ProfileService _profileService;
        private FriendListController _friendListController;
        private ChatController _chatController;

        #endregion


        #region Properties

        public Transform VariableUiHolder => _variableUiHolder;
        public Transform CommonUiHolder => _commonUiHolder;
        public Transform FadeOverlayUiHolder => _fadeOverlayUiHolder;
        public UiPrefabsData UIPrefabsData => _uiPrefabsData;
        public PlayerProfile PlayerProfile => _playerProfile;
        public float ScreenFadeTime => _screenFadeTime;
        public ProfileService ProfileService => _profileService;
        public FriendListController FriendListController => _friendListController;
        public ChatController ChatController => _chatController;

        public ICrosswordService CrosswordService
        {
            get
            {
                if (_worldService == null)
                    _worldService = new CrosswordService();

                return _worldService;
            }
        }

        public ScreenFadeController ScreenFadeController
        {
            get
            {
                if (_screenFadeController == null)
                    _screenFadeController = new ScreenFadeController(this);

                return _screenFadeController;
            }
        }

        #endregion


        #region Methods

        public void SetPlayerProfile(PlayerProfile playerProfile)
        {
            _playerProfile = playerProfile;
        }

        public void SetrosswordService(ICrosswordService worldService)
        {
            _worldService = worldService;
        }

        public void SetScreenFadeController(ScreenFadeController screenFadeController)
        {
            _screenFadeController = screenFadeController;
        }

        public void SetProfileService(ProfileService profileService)
        {
            _profileService = profileService;
        }

        public void SetFriendlistController(FriendListController friendListController)
        {
            _friendListController = friendListController;
        }

        public void SetChatController(ChatController chatController)
        {
            _chatController = chatController;
        }

        #endregion
    }
}