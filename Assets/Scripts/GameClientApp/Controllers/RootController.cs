using Firebase;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace WordPuzzle
{
    public class RootController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Context _context;
        [SerializeField] private GameState _startState = GameState.Menu;

        [SerializeField] TMPro.TextMeshProUGUI _debugText;

        private MainController _mainController;
        private PlayerProfile _playerProfile;
        private CrosswordService _crosswordService;

        private bool _isReadyToBegin;
        private string _debugMessage;
        private bool _isDebugMessageReady;

        #endregion


        #region UnityMethods

        private void Awake()
        {
            Application.targetFrameRate = 60;


            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _isDebugMessageReady = true;
                    _debugMessage = "Firebase is OK";
                    InitCrosswordServiceAsync();
                    Debug.Log("Firebase is OK");
                }
                else
                {
                    _isDebugMessageReady = true;
                    _debugMessage = "Could not resolve all Firebase dependencies: " + dependencyStatus;
                }

                Firebase.Database.FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            });
        }

        private void Update()
        {
            _mainController?.UpdateRegular();

            if (_isReadyToBegin)
                Begin();

            if (_isDebugMessageReady)
            {
                _debugText.text = _debugMessage;
                _isDebugMessageReady = false;
            }
        }
        
        public void OnDestroy()
        {
            _mainController?.Dispose();
        }

        #endregion


        #region Methods

        private async void InitCrosswordServiceAsync()
        {
            _crosswordService = new CrosswordService();
            Task<(bool isSuccessful, string message)> task; 

            do
            {
                task = _crosswordService.ReadThemesFromDataBase();
                await task;
            } while (task.IsFaulted);

            if (!task.Result.isSuccessful)
            {
                _debugMessage = task.Result.message;
                _isDebugMessageReady = true;
                return;
            }

            _debugMessage = task.Result.message;
            _isDebugMessageReady = true;
            _isReadyToBegin = true;
        }

        private void Begin()
        {
            _playerProfile = new PlayerProfile();
            _playerProfile.CurrentState.Value = _startState;
            _context.SetPlayerProfile(_playerProfile);
            _context.SetrosswordService(_crosswordService);
            _mainController = new MainController(_context);
            _isReadyToBegin = false;
            _debugText.gameObject.SetActive(false);
        }

        #endregion

    }
}