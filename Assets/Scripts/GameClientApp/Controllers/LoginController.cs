using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;

namespace WordPuzzle
{
    public class LoginController : BaseController
    {
        #region Fields

        private Context _context;
        private PlayerProfile _playerProfile;
        private LoginUIView _view;

        private FirebaseAuth _auth;

        private FirebaseUser _user;

        #endregion


        #region ClassLifeCycles

        public LoginController(Context context)
        {
            _context = context;
            _playerProfile = _context.PlayerProfile;

            InstantiatePrefab<LoginUIView>(_context.UIPrefabsData.LoginMenuPrefab, _context.VariableUiHolder, InitView);

            _auth = FirebaseAuth.DefaultInstance;
        }

        #endregion


        #region Methods

        private void InitView(LoginUIView view)
        {
            _view = view;

            _view.HideRegistrationUI();
            _view.ShowLoginUI();

            _view.LoginButton.onClick.AddListener(OnLoginButtonClick);
            _view.GoToRegistrationButton.onClick.AddListener(OnGoToRegistrationButtonClick);
            _view.RegisterRegistrationButton.onClick.AddListener(OnRegisterButtonClick);
            _view.BackButton.onClick.AddListener(OnBackToLoginButtonClick);
        }

        private async void OnLoginButtonClick()
        {
            var loginData = _view.GetLoginData();
            var loginTask = Login(loginData);
            await loginTask;

            if (loginTask.Result)
            {
                OnSuccessfulLogin();
            }
        }

        private void OnGoToRegistrationButtonClick()
        {
            _view.HideLoginUI();
            _view.ShowRegistrationUI();
        }

        private async void OnRegisterButtonClick()
        {
            var registrationData = _view.GetRegistrationData();
            var registrationTask = RegisterNewUser(registrationData);
            await registrationTask;

            if (registrationTask.Result)
            {
                OnSuccessfulRegistration();
            }
        }

        private void OnBackToLoginButtonClick()
        {
            _view.HideRegistrationUI();
            _view.ShowLoginUI();
        }

        private async Task<bool> Login(LoginData loginData)
        {
            var loginTask = _auth.SignInWithEmailAndPasswordAsync(loginData.Email, loginData.Password);

            while (!loginTask.IsCompleted)
            await Task.Yield();

            if (loginTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {loginTask.Exception}");
                FirebaseException firebaseEx = loginTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Вход не удался";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Требуется E-mail";
                        break;
                    case AuthError.MissingPassword:
                        message = "Требуется пароль";
                        break;
                    case AuthError.WrongPassword:
                        message = "Неверный пароль";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Неверный E-mail";
                        break;
                    case AuthError.UserNotFound:
                        message = "Учетная запись не найдена";
                        break;
                }
                _view.SetLoginStatusMessage(message);

                return false;
            }
            else
            {
                _user = loginTask.Result;

                return true;
            }
        }

        private void OnSuccessfulLogin()
        {
            _view.SetLoginStatusMessage("Succsess");
            ProceedToGame();
        }

        private async Task<bool> RegisterNewUser(RegistrationData regData)
        {
            if (string.IsNullOrWhiteSpace(regData.FirstName))
            {
                _view.SetRegistrationStatusMessage("Введите имя");
                return false;
            }

            if (string.IsNullOrWhiteSpace(regData.LastName))
            {
                _view.SetRegistrationStatusMessage("Введите фамилию");
                return false;
            }

            if (!regData.Password.Equals(regData.PasswordConfirm))
            {
                _view.SetRegistrationStatusMessage("Пароль и подтверждение пароля не совпадают");
                return false;
            }

            var RegisterTask = _auth.CreateUserWithEmailAndPasswordAsync(regData.Email, regData.Password);

            while (!RegisterTask.IsCompleted)
                await Task.Yield();

            if (RegisterTask.Exception != null)
            {
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Регистрация не удалась";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Требуется E-mail";
                        break;
                    case AuthError.MissingPassword:
                        message = "Требуется пароль";
                        break;
                    case AuthError.WeakPassword:
                        message = "Ненадёжный пароль";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "E-mail уже используется";
                        break;
                }
                _view.SetRegistrationStatusMessage(message);

                return false;
            }

            _user = RegisterTask.Result;
            
            UserProfile profile = new UserProfile { DisplayName = regData.Nickname };

            Task profileTask;
            do
            {
                profileTask = _user.UpdateUserProfileAsync(profile);

                while (profileTask.IsCompleted)
                    await Task.Yield();

            } while (profileTask.Exception != null);

            await _context.ProfileService.CreateUserInDB(_user.UserId, regData.FirstName, regData.LastName, regData.Nickname);

            return true;
        }

        private void OnSuccessfulRegistration()
        {
            _view.SetRegistrationStatusMessage("Succsess");
            ProceedToGame();
        }

        private void ProceedToGame()
        {
            _playerProfile.PlayerID = _user.UserId;
            _playerProfile.CurrentState.Value = GameState.Menu;
        }

        #endregion


        #region IDisposable

        protected override void OnDispose()
        {
            _view.LoginButton.onClick.RemoveAllListeners();
            _view.GoToRegistrationButton.onClick.RemoveAllListeners();
            _view.RegisterRegistrationButton.onClick.RemoveAllListeners();
            _view.BackButton.onClick.RemoveAllListeners();

            base.OnDispose();
        }

        #endregion
    }
}