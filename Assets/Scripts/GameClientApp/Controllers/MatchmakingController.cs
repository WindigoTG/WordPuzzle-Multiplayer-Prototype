using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;
using System;

namespace WordPuzzle
{
    public class MatchmakingController : BaseController, IUpdateableRegular
    {
        #region Fields

        private Context _context;
        private PlayerProfile _playerProfile;
        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _mathmakingQueue;

        private MatchmakingView _view;

        private string _playerID;
        private string _theme;

        private const float _timestampInterval = 30f;
        private float _timestampTimer;
        private bool _isInQueue;

        #endregion


        #region ClassLifeCycles

        public MatchmakingController(Context context)
        {
            _context = context;
            _playerProfile = _context.PlayerProfile;
            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;
            _playerID = _playerProfile.PlayerID;
            _theme = _context.CrosswordService.GetThemeNameByIndex(_playerProfile.SelectedTheme);

            InstantiatePrefab<MatchmakingView>(_context.UIPrefabsData.MatchmakingView, _context.VariableUiHolder, InitView);

            InitMatchmaking();
        }

        #endregion


        #region Methods

        private void InitView(MatchmakingView view)
        {
            _view = view;

            _view.BackButton.onClick.AddListener(LeaveMathmaking);
        }

        private async void InitMatchmaking()
        {
            await Task.Yield();

            if (_playerProfile.IsOnlinePlaySelected)
            {
                Debug.Log("Online game");
                StartMatchmaking();
            }
            else
            {
                Debug.Log("Solo game");
                _playerProfile.CurrentState.Value = GameState.Game;
            }
        }

        private async void StartMatchmaking()
        {
            await Enqueue();
        }

        private async Task Enqueue()
        {
            var EnqueueTask = _dbReference.Child(References.MATHMAKING_QUEUE_BRANCH).Child(_theme)
                .Child(_playerID).Child(References.MATCH_ID).SetValueAsync(References.PENDING);
            await EnqueueTask;

            _mathmakingQueue = _dbInstance.GetReference($"{References.MATHMAKING_QUEUE_BRANCH}/{_theme}/{_playerID}");
            _mathmakingQueue.ChildChanged += OnMatchIDReceived;
            _isInQueue = true;
        }

        private async Task Dequeue()
        {
            _isInQueue = false;
            _mathmakingQueue.ChildChanged -= OnMatchIDReceived;

            var DequeueTask = _dbReference.Child(References.MATHMAKING_QUEUE_BRANCH).Child(_theme).Child(_playerID).RemoveValueAsync();
            await DequeueTask;
        }

        private void OnMatchIDReceived(object sender, ChildChangedEventArgs e)
        {
            Debug.Log("Received data");
            if (!e.Snapshot.Key.Equals(References.MATCH_ID))
                return;

            _playerProfile.MatchID = e.Snapshot.Value.ToString();
            Debug.Log("Received match ID");
            StartGame();
        }

        private async void LeaveMathmaking()
        {
            await Dequeue();

            _context.ScreenFadeController.FadeOutWithCallback(() => {
                _playerProfile.CurrentState.Value = GameState.Menu;
                _context.ScreenFadeController.FadeInWithCallback(() => { });
            });
        }

        private async void StartGame()
        {
            Debug.Log("Dequeueing started");
            await Dequeue();
            Debug.Log("Left queue");
            _context.ScreenFadeController.FadeOutWithCallback(() => {
                _playerProfile.CurrentState.Value = GameState.Game;
                //_context.ScreenFadeController.FadeInWithCallback(() => { });
            });
        }

        private async void PostActivityTimestamp()
        {
            await _mathmakingQueue.Child(References.TIMESTAMP).SetValueAsync(System.DateTime.UtcNow.ToBinary().ToString());
        }

        #endregion


        #region IUpdateableRegular

        public void UpdateRegular()
        {
            if (!_isInQueue)
                return;

            if (_timestampTimer > 0)
                _timestampTimer -= Time.deltaTime;
            else
            {
                PostActivityTimestamp();
                _timestampTimer = _timestampInterval;
            }
        }

        #endregion


        #region IDisposeable

        protected override void OnDispose()
        {
            _view.BackButton.onClick.RemoveAllListeners();
            base.OnDispose();
        }

        #endregion
    }
}