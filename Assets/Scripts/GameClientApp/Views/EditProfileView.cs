using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditProfileView : MonoBehaviour
{
    #region Fields

    [SerializeField] private RawImage _editPhotoImage;
    [SerializeField] private Image _editPhotoBackground;
    [SerializeField] private Button _editPhotoButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TMP_InputField _firstNameInput;
    [SerializeField] private TMP_InputField _lastNameInput;
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private TMP_InputField _countryInput;
    [SerializeField] private TMP_InputField _cityInput;
    [SerializeField] private TMP_InputField _ageInput;
    [SerializeField] private RectTransform _interestsParent;

    #endregion


    #region Properties

    public RawImage EditPhotoImage => _editPhotoImage;
    public Image EditPhotoBackground => _editPhotoBackground;
    public Button EditPhotoButton => _editPhotoButton;
    public Button SaveButton => _saveButton;
    public Button CancelButton => _cancelButton;
    public TMP_InputField FirstNameInput => _firstNameInput;
    public TMP_InputField LastNameInput => _lastNameInput;
    public TMP_InputField NicknameInput => _nicknameInput;
    public TMP_InputField CountryInput => _countryInput;
    public TMP_InputField CityInput => _cityInput;
    public TMP_InputField AgeInput => _ageInput;
    public RectTransform InterestsParent => _interestsParent;

    #endregion
}
