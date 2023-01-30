using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WordPuzzle
{
    [CreateAssetMenu(fileName = "UIPrefabsData", menuName = "Data/UIPrefabsData")]
    public class UiPrefabsData : ScriptableObject
    {

        #region Fields

        [SerializeField] private MainMenuView _mainMenuPrefab;
        [SerializeField] private SettingsView _settingsPrefab;
        [SerializeField] private Image _screenCoverPrefab;
        [SerializeField] private SocialView _socialPrefab;
        [SerializeField] private ThemeSelectionView _themeSelectionView;
        [SerializeField] private GameObject _themeSelectionItem;
        [SerializeField] private GameplayUIView _gameplayUiView;
        [SerializeField] private MatchmakingView _matchmakingView;
        [SerializeField] private LoginUIView _loginMenuPrefab;
        [SerializeField] private PlayerProfileUIView _playerProfileUi;
        [SerializeField] private InterestItemView _interestItemPrefab;
        [Space]
        [SerializeField] private GameObject _inputLetter;
        [SerializeField] private GameObject _crosswordCell;
        [SerializeField] private TextMeshProUGUI _animationLetter;
        [Space]
        [SerializeField] private FriendsWindowView _friendsWindowPrefab;
        [SerializeField] private FriendItemView _friendItemPrefab;
        [SerializeField] private ChatWindowView _chatWindowPrefab;
        [SerializeField] private ChatMesageView _textMessageItemPrefab;

        #endregion


        #region Properties

        public MainMenuView MainMenu => _mainMenuPrefab;
        public SettingsView SettingsPrefab => _settingsPrefab;
        public Image ScreenCoverPrefab => _screenCoverPrefab;
        public SocialView SocialPrefab => _socialPrefab;
        public ThemeSelectionView WorldSelectionView => _themeSelectionView;
        public GameObject ThemeSelectionItem => _themeSelectionItem;
        public GameplayUIView GameplayUiView => _gameplayUiView;
        public GameObject InputLetter => _inputLetter;
        public GameObject CrosswordCell => _crosswordCell;
        public TextMeshProUGUI AnimationLetter => _animationLetter;
        public MatchmakingView MatchmakingView => _matchmakingView;
        public LoginUIView LoginMenuPrefab => _loginMenuPrefab;
        public PlayerProfileUIView PlayerProfileUi => _playerProfileUi;
        public InterestItemView InterestItemPrefab => _interestItemPrefab;
        public FriendsWindowView FriendsWindowPrefab => _friendsWindowPrefab;
        public FriendItemView FriendItemPrefab => _friendItemPrefab;
        public ChatWindowView ChatWindowPrefab => _chatWindowPrefab;
        public ChatMesageView ChatMessageItemPrefab => _textMessageItemPrefab;

        #endregion
    }
}