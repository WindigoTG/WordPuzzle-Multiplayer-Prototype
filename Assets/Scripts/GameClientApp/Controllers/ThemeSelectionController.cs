using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WordPuzzle
{
    public class ThemeSelectionController : BaseController
    {
        #region Fields

        private ThemeSelectionView _view;
        private Context _context;
        private PlayerProfile _playerProfile;

        private bool _isFadedOut;
        private bool _isLevelReceived;

        #endregion


        #region ClassLifeCycles

        public ThemeSelectionController(Context context)
        {
            _context = context;
            _playerProfile = _context.PlayerProfile;

            InstantiatePrefab<ThemeSelectionView>(_context.UIPrefabsData.WorldSelectionView, _context.VariableUiHolder, InitView);
        }

        #endregion


        #region Methods

        private void InitView(ThemeSelectionView view)
        {
            _view = view;
            _view.BackButton.onClick.AddListener(BackToMenu);
            SetThemes();
            _view.Init();
            _view.SetWorld(_playerProfile.SelectedTheme);
        }

        private void SetThemes()
        {
            if (_context.CrosswordService.ThemesCount > 0)
            {
                for (int i = 0; i < _context.CrosswordService.ThemesCount; i++)
                {
                    AddTheme(i);
                }
            }
            else
            {
                AddTheme(-1);
            }
        }

        private void AddTheme(int themeIndex)
        {
            var themeItem = Object.Instantiate(_context.UIPrefabsData.ThemeSelectionItem);

            var text = themeItem.GetComponentInChildren<TextMeshProUGUI>();
            if (text)
                text.text = _context.CrosswordService.GetThemeNameByIndex(themeIndex);

            var button = themeItem.GetComponentInChildren<Button>();

            if (button)
            {
                button.onClick.AddListener(() => SelectTheme(themeIndex));
                _view.AddWorld(themeItem.transform);
            }
            else
            {
                Object.Destroy(themeItem);
            }
        }

        private void BackToMenu()
        {
            _context.ScreenFadeController.FadeOutWithCallback(() => {
                _playerProfile.CurrentState.Value = GameState.Menu;
                _context.ScreenFadeController.FadeInWithCallback(() => { });
            });
        }

        private void SelectTheme(int index)
        {
            _context.ScreenFadeController.FadeOutWithCallback(() =>
            {
                _playerProfile.SelectedTheme = index;
                _playerProfile.CurrentState.Value = GameState.Matchmaking;
                _context.ScreenFadeController.FadeInWithCallback(() => { });
            });
        }

        private void OnFadeOutComplete()
        {
            _isFadedOut = true;

            if (_isFadedOut && _isLevelReceived)
                BeginGame();
        }

        private void OnLevelReceived(Crossword crossword)
        {
            _playerProfile.CrosswordToPlay = crossword;
            _playerProfile.IsLevelPreSet = true;
            _isLevelReceived = true;

            if (_isFadedOut && _isLevelReceived)
                BeginGame();
        }

        private void BeginGame()
        {
            _playerProfile.CurrentState.Value = GameState.Game;
            _context.ScreenFadeController.FadeInWithCallback(() => { });
        }

        #endregion


        #region IDisposable

        protected override void OnDispose()
        {
            _view.BackButton.onClick.RemoveAllListeners();
            base.OnDispose();
        }

        #endregion
    }
}