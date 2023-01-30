using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace WordPuzzle
{
    public class CrossWordGrid
    {
        #region Fields

        private Grid _grid;
        private Crossword _crossword;
        private Context _context;

        private int _crosswordSizeX;
        private int _crosswordSizeY;
        private float _cellSize;

        private TextMeshProUGUI[,] _crosswordLetters;
        private Vector2Int _arrayOffset;
        private Vector2Int _gridOffset;

        private Dictionary<Word, List<Vector2Int>> _letterIndicesByWord;

        #endregion


        #region ClassLifeCycles

        public CrossWordGrid(Grid grid, Context context)
        {
            _grid = grid;
            _context = context;
        }

        #endregion


        #region Methods

        public void InitiateGridForCrossword(Crossword crossword)
        {
            _crossword = crossword;

            SetGridSize();
            BuildCrossword();
        }

        private void SetGridSize()
        {
            _crosswordSizeX = 0;
            _crosswordSizeY = 0;

            if (_letterIndicesByWord != null)
                _letterIndicesByWord.Clear();
            else
                _letterIndicesByWord = new Dictionary<Word, List<Vector2Int>>();

            for (int i = _crossword.minXIndex; i <= _crossword.maxXIndex; i++)
                _crosswordSizeX++;

            for (int i = _crossword.minYIndex; i <= _crossword.maxYIndex; i++)
                _crosswordSizeY++;

            var cellSizeByX = (_grid.transform as RectTransform).rect.width /( _crosswordSizeX - 1);
            var cellSizeByY = (_grid.transform as RectTransform).rect.height /( _crosswordSizeY - 1);

            _cellSize = Mathf.Min(cellSizeByX, cellSizeByY);

            _grid.cellSize = new Vector3(_cellSize, _cellSize, _cellSize);

            _arrayOffset = new Vector2Int(-_crossword.minXIndex, -_crossword.minYIndex);
            _gridOffset = new Vector2Int(_crossword.maxXIndex + _crossword.minXIndex + 1, _crossword.maxYIndex + _crossword.minYIndex + 1);
        }

        private void BuildCrossword()
        {
            ClearCrossword();

            _crosswordLetters = new TextMeshProUGUI[_crosswordSizeX, _crosswordSizeY];

            foreach (var word in _crossword.words)
            {
                int x = word.start.x;
                int y = word.start.y;

                if (!_letterIndicesByWord.ContainsKey(word))
                    _letterIndicesByWord.Add(word, new List<Vector2Int>());

                for (int i = 0; i < word.value.Length; i++)
                {
                    var arrayX = x + _arrayOffset.x;
                    var arrayY = y + _arrayOffset.y;

                    if (_crosswordLetters[arrayX, arrayY] == null)
                    {

                        var cell = Object.Instantiate(_context.UIPrefabsData.CrosswordCell);

                        cell.transform.SetParent(_grid.transform);
                        cell.transform.localScale = new Vector3(0.95f, 0.95f, 1);
                        (cell.transform as RectTransform).sizeDelta = new Vector2(_cellSize, _cellSize);

                        (cell.transform as RectTransform).localPosition = GetCellPosition(x, y);

                        var text = cell.GetComponentInChildren<TextMeshProUGUI>();

                        text.text = word.value[i].ToString();
                        text.gameObject.SetActive(false);

                        _crosswordLetters[arrayX, arrayY] = text;

                        
                    }

                    _letterIndicesByWord[word].Add(new Vector2Int(arrayX, arrayY));

                    if (word.orientation < 0)
                        x++;
                    else
                        y++;
                }
            }
        }

        public void ClearCrossword()
        {
            if (_crosswordLetters != null)
                foreach (var letter in _crosswordLetters)
                    if (letter != null)
                        Object.Destroy(letter.transform.parent.gameObject);
        }

        private Vector3 GetCellPosition(int x, int y)
        {
            var offsetX = _gridOffset.x;
            var offsetY = _gridOffset.y;

            offsetX /= 2;

            offsetY -= offsetY / 2;

            if (offsetY < 0)
                offsetY++;

            var position = _grid.CellToLocal(new Vector3Int(x - offsetX, -(y - offsetY), 0));

            if (_crosswordSizeX % 2 != 0)
                position.x -= _cellSize / 2;

            if (_crosswordSizeY % 2 != 0)
                position.y -= _cellSize / 2;

            return position;
        }

        public void ShowWord(int wordIndex)
        {
            var desiredWord = GetWord(wordIndex);

            ShowWord(desiredWord);
        }

        public void ShowWord(string word)
        {
            var desiredWord = GetWord(word);
            ShowWord(desiredWord);
        }

        public void ShowWord(Word word)
        {
            if (!_letterIndicesByWord.ContainsKey(word))
                return;

            foreach (var coordinate in _letterIndicesByWord[word])
                _crosswordLetters[coordinate.x, coordinate.y].gameObject.SetActive(true);
        }

        public void ShowAllWords()
        {
            foreach (var word in _crossword.words)
                ShowWord(word);
        }

        private Word GetWord(int wordIndex)
        {
            return _crossword.words.Find(x => x.number == wordIndex + 1);
        }

        private Word GetWord(string word)
        {
            return _crossword.words.Find(x => x.value.Equals(word));
        }


        public bool DoesContainWord(string word)
        {
            foreach (var kvp in _letterIndicesByWord)
                if (kvp.Key.value.Equals(word))
                    return true;

            return false;
        }

        public Vector3 GetLetterPosition(string word, int letterIndex)
        {
            var desiredWord = GetWord(word);

            if (!_letterIndicesByWord.ContainsKey(desiredWord))
                return Vector3.zero;

            if (letterIndex < 0 || letterIndex >= _letterIndicesByWord[desiredWord].Count)
                return Vector3.zero;

            var letterArrayIndex = _letterIndicesByWord[desiredWord][letterIndex];

            return _crosswordLetters[letterArrayIndex.x, letterArrayIndex.y].rectTransform.position;
        }

        public void ShowLetter(string word, int letterIndex)
        {
            var desiredWord = GetWord(word);

            if (!_letterIndicesByWord.ContainsKey(desiredWord))
                return;

            if (letterIndex < 0 || letterIndex >= _letterIndicesByWord[desiredWord].Count)
                return;

            var letterArrayIndex = _letterIndicesByWord[desiredWord][letterIndex];

            _crosswordLetters[letterArrayIndex.x, letterArrayIndex.y].gameObject.SetActive(true);
        }

        public bool IsLetterRevealed(string word, int letterIndex)
        {
            var desiredWord = GetWord(word);

            if (!_letterIndicesByWord.ContainsKey(desiredWord))
                return false;

            if (letterIndex < 0 || letterIndex >= _letterIndicesByWord[desiredWord].Count)
                return false;

            var letterArrayIndex = _letterIndicesByWord[desiredWord][letterIndex];

            return _crosswordLetters[letterArrayIndex.x, letterArrayIndex.y].gameObject.activeSelf;
        }

        #endregion
    }
}