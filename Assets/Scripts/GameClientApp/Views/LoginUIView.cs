using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WordPuzzle
{
    public class LoginUIView : MonoBehaviour
    {
        #region Fields

        [Header("Login")]
        [SerializeField] private RectTransform _loginUI;
        [SerializeField] private TMP_InputField _loginEmailInput;
        [SerializeField] private TMP_InputField _loginPasswordInput;
        [SerializeField] private TextMeshProUGUI _loginStatusMessage;
        [SerializeField] private Button _loginButton;
        [SerializeField] private Button _goToRegistrationButton;
        [Space]
        [Header("Registration")]
        [SerializeField] private RectTransform _registrationUI;
        [SerializeField] private TMP_InputField _registerFirstNameInput;
        [SerializeField] private TMP_InputField _registerLastNameInput;
        [SerializeField] private TMP_InputField _registerNicknameInput;
        [SerializeField] private TMP_InputField _registerEmailInput;
        [SerializeField] private TMP_InputField _registerPasswordInput;
        [SerializeField] private TMP_InputField _registerPasswordConfirmInput;
        [SerializeField] private TextMeshProUGUI _registerStatusMessage;
        [SerializeField] private Button _registerRegistrationButton;
        [SerializeField] private Button _backButton;

        #endregion


        #region Properties

        public Button LoginButton => _loginButton;
        public Button GoToRegistrationButton => _goToRegistrationButton;
        public Button RegisterRegistrationButton => _registerRegistrationButton;
        public Button BackButton => _backButton;

        #endregion


        #region UnityMethods

        private void Start()
        {
            VerifyLoginUI();
            VerifyRegistrationUI();

            _loginEmailInput.onValueChanged.AddListener(value => VerifyLoginUI());
            _loginPasswordInput.onValueChanged.AddListener(value => VerifyLoginUI());

            _registerEmailInput.onValueChanged.AddListener(value => VerifyRegistrationUI());
            _registerPasswordInput.onValueChanged.AddListener(value => VerifyRegistrationUI());
            _registerPasswordConfirmInput.onValueChanged.AddListener(value => VerifyRegistrationUI());
            _registerFirstNameInput.onValueChanged.AddListener(value => VerifyRegistrationUI());
            _registerLastNameInput.onValueChanged.AddListener(value => VerifyRegistrationUI());
            _registerNicknameInput.onValueChanged.AddListener(value => VerifyRegistrationUI());
        }

        #endregion


        #region Methods

        private void VerifyLoginUI()
        {
            _loginButton.interactable = !string.IsNullOrWhiteSpace(_loginEmailInput.text) &&
                !string.IsNullOrWhiteSpace(_loginPasswordInput.text);
        }

        private void VerifyRegistrationUI()
        {
            _registerRegistrationButton.interactable = !string.IsNullOrWhiteSpace(_registerEmailInput.text) &&
                !string.IsNullOrWhiteSpace(_registerPasswordInput.text) &&!string.IsNullOrWhiteSpace(_registerPasswordConfirmInput.text) 
                && !string.IsNullOrWhiteSpace(_registerFirstNameInput.text) && !string.IsNullOrWhiteSpace(_registerLastNameInput.text) &&
                !string.IsNullOrWhiteSpace(_registerNicknameInput.text);
        }

        public void ClearLoginStatusMessage() => _loginStatusMessage.text = "";

        public void ClearRegistrationStatusMessage() => _registerStatusMessage.text = "";

        public void SetLoginStatusMessage(string message) => _loginStatusMessage.text = message;

        public void SetRegistrationStatusMessage(string message) => _registerStatusMessage.text = message;

        public void ShowLoginUI() 
        {
            _loginEmailInput.text = "";
            _loginPasswordInput.text = "";
            VerifyLoginUI();
            _loginUI.gameObject.SetActive(true); 
        }

        public void HideLoginUI() => _loginUI.gameObject.SetActive(false);

        public void ShowRegistrationUI()
        {
            _registerEmailInput.text = "";
            _registerFirstNameInput.text = "";
            _registerLastNameInput.text = "";
            _registerPasswordInput.text = "";
            _registerPasswordConfirmInput.text = "";
            VerifyRegistrationUI();
            _registrationUI.gameObject.SetActive(true);
        }

        public void HideRegistrationUI() => _registrationUI.gameObject.SetActive(false);

        public LoginData GetLoginData()
        {
            return new LoginData { Email = _loginEmailInput.text, Password = _loginPasswordInput.text };
        }

        public RegistrationData GetRegistrationData()
        {
            return new RegistrationData
            {
                Email = _registerEmailInput.text,
                Password = _registerPasswordInput.text,
                PasswordConfirm = _registerPasswordConfirmInput.text,
                FirstName = _registerFirstNameInput.text,
                LastName = _registerLastNameInput.text,
                Nickname = _registerNicknameInput.text
            };
        }

        #endregion
    }
}