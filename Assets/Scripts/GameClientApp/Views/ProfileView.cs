using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WordPuzzle
{
    public class ProfileView : MonoBehaviour
    {
        #region Fields

        [Header("Common Elements")]
        [SerializeField] private RawImage _photoImage;
        [SerializeField] private Image _photoBackground;
        [SerializeField] private Button _editProfileButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _locationText;
        [SerializeField] private TextMeshProUGUI _ageText;
        [SerializeField] private TextMeshProUGUI _regDateText;
        [SerializeField] private TextMeshProUGUI _interestsText;
        [SerializeField] private TextMeshProUGUI _heartCountText;
        [SerializeField] private TextMeshProUGUI _starCountText;

        [Header("Other Player Elements")]
        [SerializeField] private Button _messageButton;
        [SerializeField] private Button _addFriendButton;
        [SerializeField] private Button _heartButton;
        [SerializeField] private Button _starButton;
        [Space]
        [SerializeField] private float _defaultHeight;
        [SerializeField] private float _additionalHeight;


        #endregion


        #region Properties

        public RawImage PhotoImage => _photoImage;
        public Image PhotoBackground => _photoBackground;
        public Button EditProfileButton => _editProfileButton;
        public Button CloseButton => _closeButton;
        public TextMeshProUGUI NameText => _nameText;
        public TextMeshProUGUI LocationText => _locationText;
        public TextMeshProUGUI AgeText => _ageText;
        public TextMeshProUGUI RegDateText => _regDateText;
        public TextMeshProUGUI InterestsText => _interestsText;
        public TextMeshProUGUI HeartCountText => _heartCountText;
        public TextMeshProUGUI StarCountText => _starCountText;
        public Button MessageButton => _messageButton;
        public Button AddFriendButton => _addFriendButton;
        public Button HeartButton => _heartButton;
        public Button StarButton => _starButton;

        #endregion


        #region Methods

        public void SetLocalUserProfileView()
        {
            var size = (transform as RectTransform).sizeDelta;
            size.y = _defaultHeight;
            (transform as RectTransform).sizeDelta = size;

            _editProfileButton.gameObject.SetActive(true);
            _messageButton.gameObject.SetActive(false);
            _addFriendButton.gameObject.SetActive(false);
            _heartButton.gameObject.SetActive(false);
            _starButton.gameObject.SetActive(false);
        }

        public void SetOtherUserProfileView()
        {
            var size = (transform as RectTransform).sizeDelta;
            size.y = _defaultHeight + _additionalHeight;
            (transform as RectTransform).sizeDelta = size;

            _editProfileButton.gameObject.SetActive(false);
            _messageButton.gameObject.SetActive(true);
            _addFriendButton.gameObject.SetActive(true);
            _heartButton.gameObject.SetActive(true);
            _starButton.gameObject.SetActive(true);
        }

        #endregion
    }
}