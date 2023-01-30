using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;

namespace WordPuzzle
{
    public class OnlineGameplayController : GameplayController
    {
        #region Fields

        private Notificator _notificator;

        private string _matchID;
        private string _thisPlayerID;
        private string _otherPlayerID;
        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;
        private DatabaseReference _otherPlayerSolvedWords;
        private DatabaseReference _otherPlayerIsFinished;
        private DatabaseReference _otherPlayerIsReady;
        private DatabaseReference _otherPlayerIsRequestingContinue;
        private DatabaseReference _otherPlayerHasLeftGame;

        private bool _isMasterClient;

        private System.Action _onOtherPlayerReady;

        private bool _isWaitingToContinue;
        private bool _isOtherPlayerRequestingContinue;
        private bool _hasOtherPlayerLeftGame;
        private bool _isOtherPlayerFinished;

        private GameplayPhase _currentPhase;

        private const float _timestampInterval = 60f;
        private float _timestampTimer;
        private bool _isActive;

        private bool _isFirstGameInMatch;

        private int _localSolvedWords;
        private int _otherSolvedWords;

        #endregion


        #region Properties

        private bool IsCrosswordSolved
        {
            get
            {
                foreach (var isWordSolved in _solvedWords)
                    if (!isWordSolved)
                        return false;

                return true;
            }
        }

        #endregion


        #region ClassLifeCycles

        public OnlineGameplayController(Context context) : base(context)
        {
            _thisPlayerID = _playerProfile.PlayerID;
            _matchID = _playerProfile.MatchID;

            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;

            InstantiatePrefab<GameplayUIView>(_context.UIPrefabsData.GameplayUiView, _context.VariableUiHolder, InitView);

            _crosswoedGrid = new CrossWordGrid(_view.CrosswordGrid, _context);

            _inputPreview = new CurrentInputPreview(_view.CurrentInputPreview, _context.UIPrefabsData.AnimationLetter);
            _notificator = new Notificator(_view.Notification);

            RetreiveMatchData();

            _isFirstGameInMatch = true;
        }

        #endregion


        #region Methods

        private async void RetreiveMatchData()
        {
            var matchDataTask = _dbReference.Child(References.ACTIVE_MATCHES_BRANCH).Child(_matchID).Child(References.PLAYERS).GetValueAsync();

            await matchDataTask;

            Debug.Log(matchDataTask.Result.ChildrenCount);

            if (matchDataTask.Result.ChildrenCount != 2)
            {
                Debug.Log("Incorrect Match Info : incorrect player count");
                //TODO: return to matchmaking;
                return;
            }

            var players = new List<string>(2);

            foreach(var player in matchDataTask.Result.Children)
            {
                players.Add(player.Key);
            }

            if (!players.Contains(_thisPlayerID))
            {
                Debug.Log("Incorrect Match Info : this player isn't registered into this match");
                //TODO: return to matchmaking;
                return;
            }

            if (players[0].Equals(_thisPlayerID))
                _isMasterClient = true;

            Debug.Log("Is Master Client: " + _isMasterClient);

            foreach (var player in players)
                if (!player.Equals(_thisPlayerID))
                    _otherPlayerID = player;

            _context.ProfileService.RetrieveOtherPlayerData(_otherPlayerID);

            InitMatch();
        }

        private void InitMatch()
        {
            _currentPhase = GameplayPhase.Setup;
            _isWaitingToContinue = false;
            _isOtherPlayerRequestingContinue = false;

            GetReferences();

            _otherPlayerIsReady.ValueChanged += OnOtherPlayerReady;
            _otherPlayerHasLeftGame.ValueChanged += OnOtherPlayerLeftGame;

            if (_isMasterClient)
            {
                _onOtherPlayerReady = BeginGame;
                SelectCrosswordForMatch();
            }
            else
            {
                _onOtherPlayerReady = RetrieveCrossword;
            }

            _isActive = true;
        }

        private void GetReferences()
        {
            if (_otherPlayerIsReady == null)
                _otherPlayerIsReady = _dbInstance
                    .GetReference($"{References.ACTIVE_MATCHES_BRANCH}/{_matchID}/{References.PLAYERS}/{_otherPlayerID}/{References.IS_READY}");

            if (_otherPlayerIsFinished == null)
                _otherPlayerIsFinished = _dbInstance
                    .GetReference($"{References.ACTIVE_MATCHES_BRANCH}/{_matchID}/{References.PLAYERS}/{_otherPlayerID}/{References.IS_FINISHED}");

            if (_otherPlayerSolvedWords == null)
                _otherPlayerSolvedWords = _dbInstance
                    .GetReference($"{References.ACTIVE_MATCHES_BRANCH}/{_matchID}/{References.PLAYERS}/{_otherPlayerID}/{References.SOLVED_WORDS}");

            if (_otherPlayerIsRequestingContinue == null)
                _otherPlayerIsRequestingContinue = _dbInstance
                    .GetReference($"{References.ACTIVE_MATCHES_BRANCH}/{_matchID}/{References.PLAYERS}/{_otherPlayerID}/{References.IS_REQUESTING_CONTINUE}");

            if (_otherPlayerHasLeftGame == null)
                _otherPlayerHasLeftGame = _dbInstance
                    .GetReference($"{References.ACTIVE_MATCHES_BRANCH}/{_matchID}/{References.PLAYERS}/{_otherPlayerID}/{References.HAS_LEFT_GAME}");
        }

        private async Task SetUpDefaultValues()
        {
            var tasks = new List<Task>();

            tasks.Add(PostIsReady(false));
            tasks.Add(PostIsFinished(false));
            tasks.Add(PostIsRequestingContinue(false));

            await Task.WhenAll(tasks.ToArray());
        }

        private void OnOtherPlayerReady(object sender, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Value == null || !bool.Parse(e.Snapshot.Value.ToString()))
                return;

            _onOtherPlayerReady?.Invoke();
            _onOtherPlayerReady = null;

            _otherPlayerIsReady.ValueChanged -= OnOtherPlayerReady;

            _otherPlayerIsFinished.ValueChanged += OnOtherPlayerFinished;
            _otherPlayerSolvedWords.ChildChanged += OnOtherPlayerSolvedWord;

        }

        private void OnOtherPlayerFinished(object sender, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Value == null || !bool.Parse(e.Snapshot.Value.ToString()))
                return;

            _isOtherPlayerFinished = true;

            if (_currentPhase == GameplayPhase.Finished)
                ShowResults();
        }

        private void OnOtherPlayerSolvedWord(object sender, ChildChangedEventArgs e)
        {
            if (!bool.Parse(e.Snapshot.Value.ToString()))
                return;

            OnWordSolvedNotification(int.Parse(e.Snapshot.Key));
        }

        private async void BeginGame()
        {
            await SetUpDefaultValues();
            _solvedWords = new bool[_crossword.words.Count];
             NextWord();
            _crosswoedGrid.InitiateGridForCrossword(_crossword);
            _view.HideEndscreen();
            _currentPhase = GameplayPhase.InProgress;

            if (_isFirstGameInMatch)
            {
                _context.ScreenFadeController.FadeInWithCallback(() => { });
                _isFirstGameInMatch = false;
            }

            _startTime = DateTime.Now;
            _localSolvedWords = 0;
            _otherSolvedWords = 0;
            _isOtherPlayerFinished = false;
        }

        private async void RetrieveCrossword()
        {
            var crosswordTask = _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.CROSSWORD).GetValueAsync();

            await crosswordTask;
            _crossword = JsonUtility.FromJson<Crossword>(crosswordTask.Result.Value.ToString());

            await SetUpSolvedWords();

            await PostIsReady(true);

            BeginGame();
        }

        private async void SelectCrosswordForMatch()
        {
            var crossword = GetRandomCrossword();

            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.CROSSWORD).SetValueAsync(JsonUtility.ToJson(crossword));

            _crossword = crossword;

            await SetUpSolvedWords();

            await PostIsReady(true);
        }

        private async Task PostIsReady(bool isRedy)
        {
            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.PLAYERS).Child(_thisPlayerID).Child(References.IS_READY).SetValueAsync(isRedy.ToString());
        }

        private async Task PostIsFinished(bool isFinished)
        {
            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.PLAYERS).Child(_thisPlayerID).Child(References.IS_FINISHED).SetValueAsync(isFinished.ToString());
        }

        private async Task PostTime(TimeSpan time)
        {
            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.PLAYERS).Child(_thisPlayerID).Child(References.TIME).SetValueAsync(time.TotalSeconds);
        }

        private async Task SetUpSolvedWords()
        {
            await CleanUpSolvedWords();

            for (int i = 0; i < _crossword.length; i++)
                await PostSolvedWordStatus(i, false);
        }

        private async Task CleanUpSolvedWords()
        {
            var solvedWordsTask =  _dbReference.Child(References.ACTIVE_MATCHES_BRANCH).Child(_matchID)
                .Child(References.PLAYERS).Child(_thisPlayerID).Child(References.SOLVED_WORDS).GetValueAsync();

            await solvedWordsTask;

            foreach (var child in solvedWordsTask.Result.Children)
            {
                var removeTask = _dbReference.Child(References.ACTIVE_MATCHES_BRANCH).Child(_matchID)
                .Child(References.PLAYERS).Child(_thisPlayerID).Child(References.SOLVED_WORDS).Child(child.Key).RemoveValueAsync();

                await Task.Yield();
            }
        }

        private async Task PostSolvedWordStatus(int wordIndex, bool isSolved)
        {
            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH).Child(_matchID)
                .Child(References.PLAYERS).Child(_thisPlayerID).Child(References.SOLVED_WORDS)
                .Child(wordIndex.ToString()).SetValueAsync(isSolved.ToString());
        }

        protected override async void OnCrosswordSolved()
        {
            _context.ProfileService.IncrementSolvedCrosswordsCount();
            _finishTime = DateTime.Now;
            await PostTime(_finishTime.Subtract(_startTime));
            await PostIsFinished(true);
            _currentPhase = GameplayPhase.Finished;
            _otherPlayerIsRequestingContinue.ValueChanged += OnOtherPlayerContinueRequest;
            _view.ShowEndscreen();
            if (_hasOtherPlayerLeftGame || _isOtherPlayerFinished)
                ShowResults();
        }

        private async void ShowResults()
        {
            _view.SetLocalPlayerName(_context.ProfileService.LocalPlayerInfo.FirstName, _context.ProfileService.LocalPlayerInfo.LastName,
                _context.ProfileService.LocalPlayerInfo.Nickname);
            _view.SetLocalPlayerAge(_context.ProfileService.LocalPlayerInfo.Age);
            _view.SetLocalPlayerSolvedWords(_localSolvedWords);
            _view.SetLocalPlayerTime(_finishTime.Subtract(_startTime));
            _view.SetLocalPlayerPhoto(_context.ProfileService.LocalPlayerInfo.Photo);

            if (!_hasOtherPlayerLeftGame)
            {
                var otherTimeTask = _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                    .Child(_matchID).Child(References.PLAYERS).Child(_otherPlayerID).Child(References.TIME).GetValueAsync();
                await otherTimeTask;

                _view.SetOtherPlayerTime(otherTimeTask.Result.Value.ToString());
            }
            else
                _view.SetOtherPlayerTime(0d);

            _view.SetOtherPlayerName(_context.ProfileService.OtherPlayerInfo.FirstName, _context.ProfileService.OtherPlayerInfo.LastName,
                _context.ProfileService.OtherPlayerInfo.Nickname);
            _view.SetOtherPlayerAge(_context.ProfileService.OtherPlayerInfo.Age);
            _view.SetOtherPlayerSolvedWords(_otherSolvedWords);
            _view.SetOtherPlayerPhoto(_context.ProfileService.OtherPlayerInfo.Photo);

            _view.ShowOtherPlayerResults();
            _view.ShowResultsPanel();
        }

        private async void RequestContinueMatch()
        {
            _view.SetContinueButtonInteractable(false);
            await PostIsRequestingContinue(true);
        }

        private async Task PostIsRequestingContinue(bool isRedy)
        {
            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.PLAYERS).Child(_thisPlayerID).Child(References.IS_REQUESTING_CONTINUE).SetValueAsync(isRedy.ToString());
        }

        private void OnOtherPlayerContinueRequest(object sender, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Value == null || !bool.Parse(e.Snapshot.Value.ToString()))
                return;

            _isOtherPlayerRequestingContinue = true;

            ContinueMatch();
        }

        private void OnOtherPlayerLeftGame(object sender, ValueChangedEventArgs e)
        {
            if (e.Snapshot.Value == null || !bool.Parse(e.Snapshot.Value.ToString()))
                return;

            _hasOtherPlayerLeftGame = true;

            switch (_currentPhase)
            {
                case GameplayPhase.Finished:
                    ShowResults();
                    if (_isWaitingToContinue)
                        GoToMatchmaking();
                    break;

                case GameplayPhase.InProgress:
                    _hasOtherPlayerLeftGame = true;
                    break;

                case GameplayPhase.Setup:
                    GoToMatchmaking();
                    break;
            }
        }

        private void ContinueMatch()
        {
            if (!(_isWaitingToContinue && _isOtherPlayerRequestingContinue))
                return;

            _otherPlayerIsRequestingContinue.ValueChanged -= OnOtherPlayerContinueRequest;
            InitMatch();
        }

        private async void PostHasLeftGame(bool hasLeft)
        {
            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.PLAYERS).Child(_thisPlayerID).Child(References.HAS_LEFT_GAME).SetValueAsync(hasLeft.ToString());
        }

        private void OnWordSolvedNotification(int solvedWordIndex)
        {
            if (solvedWordIndex < 0 || solvedWordIndex >= _crossword.words.Count)
                return;

            var word = _crossword.words.Find(x => x.number == solvedWordIndex + 1).value;

            Debug.Log(word + "  solved");

            _solvedWords[solvedWordIndex] = true;
            _otherSolvedWords++;

            if (solvedWordIndex == _curentWordIndex)
            {
                _inputPreview.SetText(word);
                if (IsCrosswordSolved)
                {
                    _inputPreview.AnimatePreviewToRevealWordInGridWithCallback(_crosswoedGrid, () => { }, _view);
                    _notificator.ShowNotificationWithCallback(word, OnCrosswordSolved);
                }
                else
                {
                    _inputPreview.AnimatePreviewToRevealWordInGridWithCallback(_crosswoedGrid, NextWord, _view);
                    _notificator.ShowNotification(word);
                }
            }
            else
            {
                _crosswoedGrid.ShowWord(solvedWordIndex);
                _notificator.ShowNotification(word);
            }
        }

        protected override async void CheckFinalInput(List<int> inputIndices)
        {
            if (inputIndices.Count != _currentWord.value.Length)
            {
                _inputPreview.SetText("");
                return;
            }

            var inputWord = BuildWordFromInput(inputIndices);

            if (inputWord.ToString().Equals(_currentWord.value))
            {
                _view.SetReceivingInput(false);

                _solvedWords[_curentWordIndex] = true;
                _localSolvedWords++;
                await PostSolvedWordStatus(_curentWordIndex, true);

                _inputPreview.AnimatePreviewToRevealWordInGridWithCallback(_crosswoedGrid, NextWord, _view);

            }
            else
                _inputPreview.SetText("");
        }

        protected override void BackToMenu()
        {
            _context.ScreenFadeController.FadeOutWithCallback(() => {
                PostHasLeftGame(true);
                _playerProfile.CurrentState.Value = GameState.Menu;
                _context.ScreenFadeController.FadeInWithCallback(() => { });
            });
        }

        private void GoToMatchmaking()
        {
            _context.ScreenFadeController.FadeOutWithCallback(() => {
                PostHasLeftGame(true);
                _playerProfile.CurrentState.Value = GameState.Matchmaking;
                _context.ScreenFadeController.FadeInWithCallback(() => { });
            });
        }

        protected override void OnContinueButtonClick()
        {
            if (_hasOtherPlayerLeftGame)
            {
                GoToMatchmaking();
                return;
            }

            _isWaitingToContinue = true;
            RequestContinueMatch();
            ContinueMatch();
        }

        private async void PostActivityTimestamp()
        {
            await _dbReference.Child(References.ACTIVE_MATCHES_BRANCH)
                .Child(_matchID).Child(References.PLAYERS).Child(_thisPlayerID).Child(References.TIMESTAMP).SetValueAsync(System.DateTime.UtcNow.ToBinary().ToString());
        }

        #endregion


        #region IUpdateableRegular

        public override void UpdateRegular()
        {
            base.UpdateRegular();

            if (!_isActive)
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


        #region IDisposable

        protected override void OnDispose()
        {
            _otherPlayerIsReady.ValueChanged -= OnOtherPlayerReady;
            _otherPlayerIsFinished.ValueChanged -= OnOtherPlayerFinished;
            _otherPlayerSolvedWords.ChildChanged -= OnOtherPlayerSolvedWord;
            _otherPlayerIsRequestingContinue.ValueChanged -= OnOtherPlayerContinueRequest;
            _otherPlayerHasLeftGame.ValueChanged -= OnOtherPlayerLeftGame;

            base.OnDispose();
        }

        #endregion
    }
}