using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WordPuzzle
{
    public abstract class GameplayController : BaseController, IUpdateableRegular
    {
        #region Fields

        protected Context _context;
        protected PlayerProfile _playerProfile;
        protected GameplayUIView _view;

        protected Crossword _crossword;
        protected int _curentWordIndex;
        protected Word _currentWord;
        protected bool[] _solvedWords;

        protected CrossWordGrid _crosswoedGrid;
        protected CurrentInputPreview _inputPreview;

        protected DateTime _startTime;
        protected DateTime _finishTime;

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

        public GameplayController(Context context)
        {
            _context = context;
            _playerProfile = _context.PlayerProfile;
        }

        #endregion


        #region Methods

        protected virtual void InitView(GameplayUIView view)
        {
            _view = view;
            _view.SetLetterPrefab(_context.UIPrefabsData.InputLetter);

            _view.HideEndscreen();

            _view.ClueButton.onClick.AddListener(ShowClue);
            _view.ContinueButton.onClick.AddListener(OnContinueButtonClick);
            _view.BackButton.onClick.AddListener(BackToMenu);
            _view.FinalInput += CheckFinalInput;
            _view.CurrentInput += ProcessInput;

            _view.OtherPlayerProfileButton.onClick.AddListener(_context.ProfileService.ShowOtherPlayerInMatchProfile);
            _view.SwitchWordButton.onClick.AddListener(NextWord);
        }

        protected virtual void SetInitialLevel()
        {
            _crossword = GetRandomCrossword();
            
            _solvedWords = new bool[_crossword.words.Count];

            _startTime = DateTime.Now;
            _finishTime = default;
        }

        protected Crossword GetRandomCrossword()
        {
            var randomLevelIndex = Random.Range(0, _context.CrosswordService.GetThemeByIndex(_playerProfile.SelectedTheme).Crosswords.Count);
            return _context.CrosswordService.GetThemeByIndex(_playerProfile.SelectedTheme).Crosswords[randomLevelIndex];
        }

        protected void NextWord()
        {
            if (IsCrosswordSolved)
            {
                OnCrosswordSolved();
                return;
            }

            _view.HideClue();
            _view.SetReceivingInput(true);
            _view.HideEndscreen();

            do
            {
                _curentWordIndex = Random.Range(0, _crossword.words.Count);
            } while (_solvedWords[_curentWordIndex] != false);

            _currentWord = _crossword.words.Find(x => x.number == _curentWordIndex + 1);

            _view.LoadWord(_currentWord.value);
        }

        protected virtual void OnCrosswordSolved()
        {
            _context.ProfileService.IncrementSolvedCrosswordsCount();
            _finishTime = DateTime.Now;
            FillPLayerResults();
            _view.ShowEndscreen();
            _view.ShowResultsPanel();
        }

        protected virtual void FillPLayerResults()
        {
            _view.SetLocalPlayerName(_context.ProfileService.LocalPlayerInfo.FirstName, _context.ProfileService.LocalPlayerInfo.LastName,
                _context.ProfileService.LocalPlayerInfo.Nickname);
            _view.SetLocalPlayerAge(_context.ProfileService.LocalPlayerInfo.Age);
            _view.SetLocalPlayerSolvedWords(_solvedWords.Length);
            _view.SetLocalPlayerTime(_finishTime.Subtract(_startTime));
            _view.SetLocalPlayerPhoto(_context.ProfileService.LocalPlayerInfo.Photo);
        }

        private void ShowClue()
        {
            var clueText = _crossword.clues.Find(x => x.number == _curentWordIndex + 1).value;
            _view.ShowClue(clueText);
        }

        protected virtual void CheckFinalInput(List<int> inputIndices)
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

                _inputPreview.AnimatePreviewToRevealWordInGridWithCallback(_crosswoedGrid, NextWord, _view);
            }
            else
                _inputPreview.SetText("");
        }

        protected void ProcessInput(List<int> inputIndices)
        {
            var inputWord = BuildWordFromInput(inputIndices);

            _inputPreview.SetText(inputWord);
        }

        protected string BuildWordFromInput(List<int> inputIndices)
        {
            StringBuilder inputWord = new StringBuilder();

            foreach (var index in inputIndices)
                inputWord.Append(_currentWord.value[index]);

            return inputWord.ToString();
        }

        protected abstract void OnContinueButtonClick();

        protected virtual void BackToMenu()
        {
            _context.ScreenFadeController.FadeOutWithCallback(() => {
                _playerProfile.CurrentState.Value = GameState.Menu;
                _context.ScreenFadeController.FadeInWithCallback(() => { });
            });
        }

        protected void PlayNewCrossword(Crossword crossword)
        {
            _crossword = crossword;
            _solvedWords = new bool[_crossword.words.Count];
            NextWord();
            _crosswoedGrid.InitiateGridForCrossword(_crossword);
            _startTime = DateTime.Now;
        }

        #endregion


        #region IUpdateableRegular

        public virtual void UpdateRegular()
        {
            _inputPreview.UpdateRegular();
        }

        #endregion


        #region IDisposable

        protected override void OnDispose()
        {
            _view.ClueButton.onClick.RemoveAllListeners();
            _view.ContinueButton.onClick.RemoveAllListeners();
            _view.BackButton.onClick.RemoveAllListeners();
            _view.FinalInput -= CheckFinalInput;
            _view.CurrentInput -= ProcessInput;
            _view.OtherPlayerProfileButton.onClick.RemoveAllListeners();
            _view.SwitchWordButton.onClick.RemoveAllListeners();
            base.OnDispose();
        }

        #endregion
    }
}