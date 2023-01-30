using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

namespace WordPuzzle
{
    public class PlayerProfileUIView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private ProfileView _profileUIview;
        [SerializeField] private EditProfileView _editProfileUIview;

        [Header("PhotoEditing")]
        [SerializeField] private RectTransform _photoSourceDialogue;
        [SerializeField] private Button _cameraButton;
        [SerializeField] private Button _galleryButton;
        [SerializeField] private Button _closeDialogueButton;
        [SerializeField] private Button _sendMessageButton;

        #endregion


        #region Properties

        public Button EditProfileButton => _profileUIview.EditProfileButton; 
        public Button CloseButton => _profileUIview.CloseButton; 
        public Button EditPhotoButton => _editProfileUIview.EditPhotoButton;
        public Button SaveButton => _editProfileUIview.SaveButton; 
        public Button CancelButton => _editProfileUIview.CancelButton;
        public Button GalleryButton => _galleryButton;
        public Button CameraButton => _cameraButton;
        public Button AddFriendButton => _profileUIview.AddFriendButton;
        public Button LikeButton => _profileUIview.HeartButton;
        public Button SendMessageButton => _sendMessageButton;

        public string EditFirstNameText => _editProfileUIview.FirstNameInput.text;
        public string EditLastNameText => _editProfileUIview.LastNameInput.text;
        public string EditNicknameText => _editProfileUIview.NicknameInput.text;
        public string EditCountryText => _editProfileUIview.CountryInput.text; 
        public string EditCityText => _editProfileUIview.CityInput.text; 
        public string EditAgeText => _editProfileUIview.AgeInput.text;

        public Texture2D EditedPhoto => (Texture2D)_editProfileUIview.EditPhotoImage.texture;

        public RectTransform InterestsParent => _editProfileUIview.InterestsParent;

        #endregion


        #region UnityMethods

        private void Start()
        {
            _closeDialogueButton.onClick.AddListener(HidePhotoSourceDialogue);
            HidePhotoSourceDialogue();
        }

        #endregion


        #region Methods

        public void ShowPhotoSourceDialogue() => _photoSourceDialogue.gameObject.SetActive(true);

        public void HidePhotoSourceDialogue() => _photoSourceDialogue.gameObject.SetActive(false);

        public void ShowProfileUI(bool isLocalPlayerProfile) 
        {
            if (isLocalPlayerProfile)
                _profileUIview.SetLocalUserProfileView();
            else
                _profileUIview.SetOtherUserProfileView();
            _profileUIview.gameObject.SetActive(true); 
        }

        public void HideProfileUI() => _profileUIview.gameObject.SetActive(false);

        public void ShowProfileEditUI() => _editProfileUIview.gameObject.SetActive(true);

        public void HideProfileEditUI() => _editProfileUIview.gameObject.SetActive(false);

        public void SetUserName(string firstName, string lastName, string nickname) => _profileUIview.NameText.text = TextFormatter.GetFormattedName(firstName, lastName, nickname);

        public void SetUserLocation(string country, string city) => _profileUIview.LocationText.text = $"{country}\n{city}";

        public void SetUserAge(string age) => _profileUIview.AgeText.text =  TextFormatter.GetFormattedAge(age);

        public void SetUserLikes(string amount) => _profileUIview.HeartCountText.text = amount;

        public void SetUserInterests(string[] interests)
        {
            StringBuilder text = new StringBuilder();
            text.Append("<b>Интересы:</b> ");

            if (interests != null)
            {
                for (int i = 0; i < interests.Length; i++)
                {
                    text.Append(interests[i]);

                    if (i != interests.Length - 1)
                        text.Append(", ");
                }
            }
            _profileUIview.InterestsText.text = text.ToString();
        }

        public void SetUserRegistrationDate(string regDate) => _profileUIview.RegDateText.text = $"Дата регистрации\n{regDate}";

        public void SetUserEditPhoto(Texture2D photo)
        {
            _editProfileUIview.EditPhotoImage.texture = photo;
            _editProfileUIview.EditPhotoImage.gameObject.SetActive(true);
            _editProfileUIview.EditPhotoBackground.gameObject.SetActive(false);
        }

        public void SetUserPhoto(Texture2D photo)
        {
            _profileUIview.PhotoImage.texture = photo;
            _profileUIview.PhotoImage.gameObject.SetActive(true);
            _profileUIview.PhotoBackground.gameObject.SetActive(false);
        }

        public void SetBlankUserPhoto()
        {
            _profileUIview.PhotoImage.gameObject.SetActive(false);
            _profileUIview.PhotoBackground.gameObject.SetActive(true);
        }

        public void FillProfileEditInputFields(UserProfileData profileData)
        {
            _editProfileUIview.FirstNameInput.text = profileData.FirstName;
            _editProfileUIview.LastNameInput.text = profileData.LastName;
            _editProfileUIview.CountryInput.text = profileData.Country;
            _editProfileUIview.CityInput.text = profileData.City;
            _editProfileUIview.AgeInput.text = profileData.Age;
            _editProfileUIview.NicknameInput.text = profileData.Nickname;
        }

        #endregion
    }
}