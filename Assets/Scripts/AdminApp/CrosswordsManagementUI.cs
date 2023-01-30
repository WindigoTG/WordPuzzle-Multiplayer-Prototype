#if UNITY_STANDALONE || UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

namespace WordPuzzle.Administration
{
    public class CrosswordsManagementUI : MonoBehaviour
    {
        #region Fields

        [Header("Buttons")]
        [SerializeField] private Button _loadFromDBButton;
        [SerializeField] private Button _saveToDBButton;
        [SerializeField] private Button _cleadAllButton;
        [SerializeField] private Button _addThemeButton;
        [SerializeField] private Button _removeThemeButton;
        [SerializeField] private Button _addCrosswordButton;
        [SerializeField] private Button _removeCrosswordButton;
        [Space]
        [Header("UI elements")]
        [SerializeField] private RectTransform _themesScrollViewContent;
        [SerializeField] private RectTransform _crosswordsScrollViewContent;
        [SerializeField] private RectTransform _wordsScrollViewContent;
        [SerializeField] private TMP_InputField _themeNameInputField;
        [SerializeField] private Grid _crosswordPreview;
        [SerializeField] private TextMeshProUGUI _clueText;
        [SerializeField] private TextMeshProUGUI _statusMessageText;
        [Space]
        [SerializeField] private YesNoDialogue _yesNoDialogue;
        [Space]
        [SerializeField] private SelectableElementView _selectableElementPrefab;
        [Space]
        [SerializeField] private Context _context;

        private List<SelectableElementView> _themes = new List<SelectableElementView>();
        private List<SelectableElementView> _crosswords = new List<SelectableElementView>();
        private List<SelectableElementView> _words = new List<SelectableElementView>();

        private int _selectedThemeIndex = -1;
        private int _selectedCrosswordIndex = -1;
        private int _selectedWordIndex = -1;

        private ThemeHolder _themeHolder = new ThemeHolder();
        private CrosswordLoader _crosswordLoader = new CrosswordLoader();
        private CrossWordGrid _crosswordPreviewHandler;

        private bool _isThemeSelectionLocked;

        #endregion


        #region UnityMethods

        void Awake()
        {
            SubscribeListeners();
            _crosswordPreviewHandler = new CrossWordGrid(_crosswordPreview, _context);
        }

        private void OnEnable()
        {
            UpdateUI();
        }

        void OnDestroy()
        {
            UnsubscribeListeners();
        }

        #endregion


        #region Methods

        private void SubscribeListeners()
        {
            _loadFromDBButton.onClick.AddListener(LoadFromDB);
            _saveToDBButton.onClick.AddListener(SaveToDB);
            _cleadAllButton.onClick.AddListener(ClearAll);
            _addThemeButton.onClick.AddListener(AddTheme);
            _removeThemeButton.onClick.AddListener(RemoveTheme);
            _addCrosswordButton.onClick.AddListener(AddCrossword);
            _removeCrosswordButton.onClick.AddListener(RemoveCrossword);

            _themeNameInputField.onValueChanged.AddListener(OnThemeNameEdit);
        }

        private void UnsubscribeListeners()
        {
            _loadFromDBButton.onClick.RemoveAllListeners();
            _saveToDBButton.onClick.RemoveAllListeners();
            _cleadAllButton.onClick.RemoveAllListeners();
            _addThemeButton.onClick.RemoveAllListeners();
            _removeThemeButton.onClick.RemoveAllListeners();
            _addCrosswordButton.onClick.RemoveAllListeners();
            _removeCrosswordButton.onClick.RemoveAllListeners();

            _themeNameInputField.onValueChanged.RemoveAllListeners();
        }

        private async void LoadFromDB()
        {
            var task = _themeHolder.ReadThemesFromDataBase();

            while (!task.IsCompleted)
                await System.Threading.Tasks.Task.Yield();

            if (task.Result)
                BuildThemesList();
        }

        private async void SaveToDB()
        {
            await _themeHolder.UploadThemesToDataBase();
        }

        private void AddTheme()
        {
            _themeHolder.AddNewTheme();
            _themeHolder.SetThemeNameByIndex($"Тема {_themeHolder.ThemesCount}", _themeHolder.ThemesCount - 1);

            var newThemeIndex = _themeHolder.ThemesCount - 1;

            CreateSelectionItem(_themes, _themesScrollViewContent, () => SelectTheme(newThemeIndex), _themeHolder.GetThemeNameByIndex(newThemeIndex));

            SelectTheme(newThemeIndex);
        }

        private void RemoveTheme()
        {
            System.Action callback = () => 
            { 
                _themeHolder.RemoveThemeByIndex(_selectedThemeIndex);
                BuildThemesList();
            };

            var message = "Удалить выбранную тему и содержащиеся в ней кроссворды?";
        
            _yesNoDialogue.ShowDialogue(callback, message);
        }

        private void AddCrossword()
        {
            var paths = StandaloneFileBrowser.OpenFilePanel("Select crossword", "", "json", true);
            if (paths.Length == 0)
                return;

            var crosswords = _crosswordLoader.LoadCrosswordsFromPaths(paths);

            foreach(var crossword in crosswords)
            {
                _themeHolder.AddCrosswordToThemeByIndex(crossword, _selectedThemeIndex);

                var newCrosswordIndex = _themeHolder.GetThemeCrosswordCountByIndex(_selectedThemeIndex) - 1;

                CreateSelectionItem(_crosswords, _crosswordsScrollViewContent, () => SelectCrossword(newCrosswordIndex), $"Кроссворд {newCrosswordIndex + 1}");

                SelectCrossword(newCrosswordIndex);
            }
        }

        private void RemoveCrossword()
        {
            System.Action callback = () =>
            {
                _themeHolder.RemoveCrosswordFromThemeByIndex(_selectedThemeIndex, _selectedCrosswordIndex);
                BuildCrosswordsList();
            };

            var message = "Удалить выбранный кроссворд?";

            _yesNoDialogue.ShowDialogue(callback, message);
        }

        private void ClearAll()
        {
            _themeHolder.ClearThemes();
            ClearThemesList();
        }

        private void ClearThemesList()
        {
            _selectedThemeIndex = -1;

            for (int i = _themes.Count - 1; i >= 0; i--)
                Destroy(_themes[i].gameObject);
            _themes.Clear();

            _themeNameInputField.text = "";
            
            ClearCrosswordsList();
        }

        private void ClearCrosswordsList()
        {
            _selectedCrosswordIndex = -1;

            for (int i = _crosswords.Count - 1; i >= 0; i--)
                Destroy(_crosswords[i].gameObject);

            _crosswords.Clear();
            _crosswordPreviewHandler.ClearCrossword();

            ClearWordsList();
        }

        private void ClearWordsList()
        {
            _selectedWordIndex = -1;

            for (int i = _words.Count - 1; i >= 0; i--)
                Destroy(_words[i].gameObject);

            _words.Clear();

            _clueText.text = "";
        }
        private void BuildThemesList()
        {
            ClearThemesList();

            for (int i = 0; i < _themeHolder.ThemesCount; i++)
            {
                var newThemeIndex = i;
                var themeName = _themeHolder.GetThemeNameByIndex(newThemeIndex);

                CreateSelectionItem(_themes, _themesScrollViewContent, () => SelectTheme(newThemeIndex), themeName);
            }
        }

        private void BuildCrosswordsList()
        {
            ClearCrosswordsList();

            for (int i = 0; i < _themeHolder.GetThemeCrosswordCountByIndex(_selectedThemeIndex); i++)
            {
                var newCrosswordIndex = i;

                CreateSelectionItem(_crosswords, _crosswordsScrollViewContent, () => SelectCrossword(newCrosswordIndex), $"Кроссворд {newCrosswordIndex + 1}");
            }
        }

        private void BuildWordsList()
        {
            ClearWordsList();

            Debug.Log($"Word count: { _themeHolder.GetWordCountByIndex(_selectedThemeIndex, _selectedCrosswordIndex)}");

            for (int i = 0; i < _themeHolder.GetWordCountByIndex(_selectedThemeIndex, _selectedCrosswordIndex); i++)
            {
                var newWordIndex = i;
                var word = _themeHolder.GetWordByIndex(_selectedThemeIndex, _selectedCrosswordIndex, newWordIndex);

                CreateSelectionItem(_words, _wordsScrollViewContent, () => SelectWord(newWordIndex), word);
            }
        }

        private void UpdateTheme()
        {
            foreach (var theme in _themes)
                theme.SetDefaultColor();

            if (_selectedThemeIndex < 0)
                return;

            _themes[_selectedThemeIndex].SetSelectedColor();
            _themeNameInputField.text = _themeHolder.GetThemeNameByIndex(_selectedThemeIndex);
            BuildCrosswordsList();
            UpdateUI();
        }

        private void UpdateCrossword()
        {
            foreach (var crossword in _crosswords)
                crossword.SetDefaultColor();

            if (_selectedCrosswordIndex < 0)
                return;

            _crosswords[_selectedCrosswordIndex].SetSelectedColor();
            BuildWordsList();
            UpdateUI();

            var crosswordToPreview = _themeHolder.GetCrosswordByIndex(_selectedThemeIndex, _selectedCrosswordIndex);
            if (crosswordToPreview.length > 0)
            {
                _crosswordPreviewHandler.InitiateGridForCrossword(crosswordToPreview);
                _crosswordPreviewHandler.ShowAllWords();
            }
        }

        private void UpdateWord()
        {
            foreach (var word in _words)
                word.SetDefaultColor();

            if (_selectedWordIndex < 0)
                return;

            _words[_selectedWordIndex].SetSelectedColor();
            var clue = _themeHolder.GetClueForWordByIndex(_selectedThemeIndex, _selectedCrosswordIndex, _selectedWordIndex);
            _clueText.text = clue;
        }

        private void UpdateUI()
        {
            bool doesSelectedThemeHaveName = 
                (_selectedThemeIndex >= 0 && !string.IsNullOrWhiteSpace(_themeNameInputField.text) && !string.IsNullOrEmpty(_themeNameInputField.text)) 
                || _selectedThemeIndex < 0;
            bool doesSelectedThemeHaveCrosswords = _themeHolder.GetThemeCrosswordCountByIndex(_selectedThemeIndex) > 0;

            _addThemeButton.interactable = (doesSelectedThemeHaveName && doesSelectedThemeHaveCrosswords) || _themeHolder.ThemesCount == 0;
            _removeThemeButton.interactable = _themeHolder.ThemesCount > 0;

            _addCrosswordButton.interactable = _selectedThemeIndex >= 0 && doesSelectedThemeHaveName;
            _removeCrosswordButton.interactable = doesSelectedThemeHaveName && _themeHolder.GetThemeCrosswordCountByIndex(_selectedThemeIndex) > 0 && _selectedCrosswordIndex >= 0;

            _saveToDBButton.interactable = _themeHolder.ThemesCount > 0 &&  doesSelectedThemeHaveName;

            _cleadAllButton.interactable = _themeHolder.ThemesCount > 0;

            _themeNameInputField.interactable = _selectedThemeIndex >= 0;


            _isThemeSelectionLocked = _selectedThemeIndex >= 0 && string.IsNullOrWhiteSpace(_themeNameInputField.text);
        }

        private void SelectTheme(int themeIndex)
        {
            if (themeIndex < 0 || themeIndex >= _themeHolder.ThemesCount || _isThemeSelectionLocked ||
                themeIndex == _selectedThemeIndex)
                return;

            Debug.Log("Theme " + themeIndex);

            _selectedThemeIndex = themeIndex;
            UpdateTheme();
        }

        private void SelectCrossword(int crosswordIndex)
        {
            if (crosswordIndex < 0 || crosswordIndex >= _themeHolder.GetThemeCrosswordCountByIndex(_selectedThemeIndex) ||
                crosswordIndex == _selectedCrosswordIndex)
                return;

            Debug.Log("Crossword " + crosswordIndex);

            _selectedCrosswordIndex = crosswordIndex;
            UpdateCrossword();
        }

        private void SelectWord(int wordIndex)
        {
            if (wordIndex < 0 || wordIndex >= _themeHolder.GetWordCountByIndex(_selectedThemeIndex, _selectedCrosswordIndex) ||
                wordIndex == _selectedWordIndex)
                return;

            Debug.Log("Word " + wordIndex);

            _selectedWordIndex = wordIndex;
            UpdateWord();
        }

        

        private void OnThemeNameEdit(string text)
        {
            if (_selectedThemeIndex >= 0)
                _themes[_selectedThemeIndex].SetText(text);

            if (!string.IsNullOrWhiteSpace(text))
                _themeHolder.SetThemeNameByIndex(text, _selectedThemeIndex);

            UpdateUI();
        }

        private void CreateSelectionItem(List<SelectableElementView> collection, RectTransform parentTransform, System.Action callback, string name)
        {
            var selectionItem = Instantiate(_selectableElementPrefab, parentTransform);
            selectionItem.RegisterOnClickCallback(callback);
            selectionItem.SetText(name);
            collection.Add(selectionItem);
        }

        #endregion
    }
}

#endif