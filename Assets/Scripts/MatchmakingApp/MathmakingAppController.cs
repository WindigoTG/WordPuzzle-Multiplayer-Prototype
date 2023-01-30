#if UNITY_STANDALONE || UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using TMPro;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WordPuzzle.Matchmaking
{
    public class MathmakingAppController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _stopButton;

        private bool _isMessagePending;
        private string _message;

        private MatchmakingHandler _matchmakingHandler = new MatchmakingHandler();
        private LikesHelper _likesHelper = new LikesHelper();
        private UserSearchHandler _searchHandler = new UserSearchHandler();

        #endregion


        #region UnityMethods

        private void Awake()
        {
            Application.targetFrameRate = 60;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                string message;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    message = "Firebase depencencies resolved successfully";
                    //StartServices();
                }
                else
                {
                    message = "Could not resolve all Firebase dependencies: " + dependencyStatus;
                }

                Firebase.Database.FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);

                Debug.Log(message);
                SetMessage(message);
            });
        }

        private void Start()
        {
            _startButton.onClick.AddListener(OnStartButtonClick);
            _stopButton.onClick.AddListener(OnStopButtonClick);
            _matchmakingHandler.StatusMessage += SetMessage;
        }

        void Update()
        {
            if (_isMessagePending)
            {
                _statusText.text = _message;
                _isMessagePending = false;
            }

            _matchmakingHandler?.UpdateRegular();
            _searchHandler?.UpdateRegular();
        }

        private void OnDestroy()
        {
            StopServices();
            _startButton.onClick.RemoveAllListeners();
            _stopButton.onClick.RemoveAllListeners();
            _matchmakingHandler.StatusMessage -= SetMessage;
        }

        #endregion


        #region Methods

        private void SetMessage(string message)
        {
            _message = message;
            _isMessagePending = true;
        }

        private void StartServices()
        {
            _matchmakingHandler.StartMatchmaking(10f);
            _likesHelper.StartListening();
            _searchHandler.StartSearchHandler(10f);
        }

        private void StopServices()
        {
            _matchmakingHandler.StopMatchmaking();
            _likesHelper.StopListening();
            _searchHandler.StopSearchHandler();
        }

        private void OnStartButtonClick()
        {
            StartServices();
        }

        private void OnStopButtonClick()
        {
            StopServices();

#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

#endregion
    }
}

#endif