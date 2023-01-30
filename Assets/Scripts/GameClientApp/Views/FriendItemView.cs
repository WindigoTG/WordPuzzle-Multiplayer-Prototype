using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace WordPuzzle
{
    public class FriendItemView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button _messageButton;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _crosswordsAmountText;
        [SerializeField] private Image _photoBG;
        [SerializeField] private RawImage _photo;
        [SerializeField] private Button _profileButton;
        [SerializeField] private Button _declineButton;

        #endregion


        #region Profile

        public Button MessageButton => _messageButton;
        public TextMeshProUGUI NameText => _nameText;
        public TextMeshProUGUI CrosswordsAmountText => _crosswordsAmountText;
        public Image PhotoBG => _photoBG;
        public RawImage Photo => _photo;
        public Button ProfileButton => _profileButton;
        public Button DeclineButton => _declineButton;

        #endregion


        #region Methods

        public void FillUserData(UserProfileData userData)
        {
            _nameText.text = TextFormatter.GetFormattedName(userData.FirstName, userData.LastName, userData.Nickname);
            _crosswordsAmountText.text = userData.CrosswordsSolved.ToString();

            _photoBG.gameObject.SetActive(userData.Photo == null);
            _photo.gameObject.SetActive(userData.Photo != null);
            if (userData.Photo != null)
                _photo.texture = userData.Photo;
        }

        public void SetMessage(string message)
        {

        }

        #endregion
    }
}