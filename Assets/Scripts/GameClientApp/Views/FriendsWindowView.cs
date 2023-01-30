using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WordPuzzle
{
    public class FriendsWindowView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private ScrollRect _scrollView;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _friendsButton;
        [SerializeField] private Button _pendingButton;
        [SerializeField] private Button _requestButton;
        [SerializeField] private Button _uncategorizedButton;
        [SerializeField] private TextMeshProUGUI _listEmptyMessage;
        [Space]
        [Header("Search")]
        [SerializeField] private RectTransform _searchPanel;
        [SerializeField] private Button _buttonSearchOpen;
        [SerializeField] private Button _buttonSearchStart;
        [SerializeField] private Button _buttonSearchCancel;
        [SerializeField] private TMP_InputField _searchNicknameInput;

        #endregion


        #region Properties

        public ScrollRect ScrolRect => _scrollView;
        public Button BackButton => _backButton;
        public Button FriendsButton => _friendsButton;
        public Button PendingButton => _pendingButton;
        public Button RequestButton => _requestButton;
        public Button UncategorizedButton => _uncategorizedButton;
        public TextMeshProUGUI ListEmptyMessage => _listEmptyMessage;

        public RectTransform SearchPanel => _searchPanel;
        public Button SearchOpenButton => _buttonSearchOpen;
        public Button SearchStartButton =>_buttonSearchStart;
        public Button ButtonSearchCancel => _buttonSearchCancel;
        public TMP_InputField SearchNicknameInput => _searchNicknameInput;

        #endregion
    }
}