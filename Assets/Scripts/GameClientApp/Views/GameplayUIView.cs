using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using DG.Tweening;

namespace WordPuzzle
{
    public class GameplayUIView : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        #region Fields

        [Header("Gameplay")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private TextMeshProUGUI _clueText;
        [SerializeField] private Button _clueButton;
        [SerializeField] private Button _shuffleButton;
        [SerializeField] private Grid _crosswordGrid;
        [SerializeField] private RectTransform _lettersRoot;
        [SerializeField] private RectTransform _currentInputPreview;
        [SerializeField] private CanvasGroup _notification;
        [SerializeField] private float _inputLettersPositionRadius = 200;
        [SerializeField] private float _inputSensitivityRadius = 0.2f;
        [SerializeField] private Button _switchWordButton;
        [Space]
        [Header("End Screen")]
        [SerializeField] private RectTransform _endscreen;
        [SerializeField] private RectTransform _resultsPanel;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _backButton;
        [Header("Local Player")]
        [SerializeField] private RectTransform _localPlayerResults;
        [SerializeField] private TextMeshProUGUI _localPlayerName;
        [SerializeField] private TextMeshProUGUI _localPlayerAge;
        [SerializeField] private TextMeshProUGUI _localPlayerSolvedWords;
        [SerializeField] private TextMeshProUGUI _localPlayerTime;
        [SerializeField] private Image _localPlayerPhotoBG;
        [SerializeField] private RawImage _localPlayerPhoto;
        [Header("Other Player")]
        [SerializeField] private RectTransform _otherPlayerResults;
        [SerializeField] private TextMeshProUGUI _otherPlayerName;
        [SerializeField] private TextMeshProUGUI _otherPlayerAge;
        [SerializeField] private TextMeshProUGUI _otherPlayerSolvedWords;
        [SerializeField] private TextMeshProUGUI _otherPlayerTime;
        [SerializeField] private Image _otherPlayerPhotoBG;
        [SerializeField] private RawImage _otherPlayerPhoto;
        [SerializeField] private Button _otherPlayerProfileButton;



        public event Action<List<int>> FinalInput;
        public event Action<List<int>> CurrentInput;

        private int _letterCount;

        private List<Vector3> _letterPositions = new List<Vector3>();
        private List<Vector3> _letterLocalPositions = new List<Vector3>();
        private List<RectTransform> _inputLetters = new List<RectTransform>();

        private List<int> _currentIndices = new List<int>();
        private List<Vector3> _points = new List<Vector3>();
        private List<Vector3> _positions = new List<Vector3>();
        private Vector3 _inputPosition;

        private bool _isDragging;

        private GameObject _inputLetterPrefab;

        private bool _isReceivingInput;

        #endregion


        #region Properties

        public Button ClueButton => _clueButton;
        public Button ContinueButton => _continueButton;
        public Button BackButton => _backButton;
        public Grid CrosswordGrid => _crosswordGrid;
        public RectTransform CurrentInputPreview => _currentInputPreview;
        public CanvasGroup Notification => _notification;
        public Button OtherPlayerProfileButton => _otherPlayerProfileButton;
        public Button SwitchWordButton => _switchWordButton;

        #endregion


        #region Methods

        public void SetReceivingInput(bool _isReceivingInput)
        {
            this._isReceivingInput = _isReceivingInput;
        }

        public void HideClue()
        {
            _clueText.text = "";
            _clueText.gameObject.SetActive(false);
        }

        public void ShowClue(string clueText)
        {
            _clueText.text = clueText;
            _clueText.gameObject.SetActive(true);
        }

        public void SetLetterPrefab(GameObject prefab)
        {
            _inputLetterPrefab = prefab;
        }

        public void ShowEndscreen()
        {
            _endscreen.gameObject.SetActive(true);
        }

        public void ShowResultsPanel()
        {
            _resultsPanel.gameObject.SetActive(true);
            SetContinueButtonInteractable(true);
        }

        public void SetContinueButtonInteractable(bool isInteractable)
        {
            _continueButton.interactable = isInteractable;
        }

        public void HideEndscreen()
        {
            _endscreen.gameObject.SetActive(false);
            _resultsPanel.gameObject.SetActive(false);
            _otherPlayerResults.gameObject.SetActive(false);
        }

        public void ResetInput()
        {
            _isDragging = false;
            _currentIndices.Clear();
            BuildPoints();
        }

        #endregion


        #region IBeginDragHandler

        public void OnBeginDrag(PointerEventData eventData)
        {

            _isDragging = true;
        }

        #endregion


        #region IDragHandler

        public void OnDrag(PointerEventData data)
        {
            if (!_isReceivingInput || !_isDragging)
                return;

            _inputPosition = Camera.main.ScreenToWorldPoint(data.position);
            _inputPosition.z = 90;

            int nearest = GetNearestPosition(_inputPosition, _letterPositions);
            Vector3 letterPosition = _letterPositions[nearest];

            if (Vector3.Distance(letterPosition, _inputPosition) < _inputSensitivityRadius)
            {
                if (_currentIndices.Count >= 2 && _currentIndices[_currentIndices.Count - 2] == nearest)
                {
                    _currentIndices.RemoveAt(_currentIndices.Count - 1);
                }
                else if (!_currentIndices.Contains(nearest))
                {
                    _currentIndices.Add(nearest);
                }
                CurrentInput?.Invoke(_currentIndices);
            }

            BuildPoints();
        }

        #endregion


        #region IEndDragHandler

        public void OnEndDrag(PointerEventData data)
        {
            if (!_isReceivingInput)
                return;

            FinalInput?.Invoke(_currentIndices);

            _isDragging = false;
            _currentIndices.Clear();
            lineRenderer.positionCount = 0;
        }

        #endregion


        #region UnityMethods

        private void Start()
        {
            _shuffleButton.onClick.AddListener(ShuffleLetters);
        }

        private void Update()
        {

            if (_points.Count >= 2 && _isDragging)
            {
                DrawLine();
            }
        }

        private void OnDestroy()
        {
            _shuffleButton.onClick.RemoveAllListeners();
        }

        #endregion


        #region Methods
        public void LoadWord(string word)
        {
            if (_inputLetters.Count > 0)
            {
                foreach (var lettetText in _inputLetters)
                {
                    Destroy(lettetText.gameObject);
                }
                _inputLetters.Clear();
                _letterLocalPositions.Clear();
                _letterPositions.Clear();
            }

            _letterCount = word.Length;

            float delta = 360f / _letterCount;

            float angle = 90;
            for (int i = 0; i < _letterCount; i++)
            {
                float angleRadian = angle * Mathf.PI / 180f;
                float x = Mathf.Cos(angleRadian);
                float y = Mathf.Sin(angleRadian);
                Vector3 position = _inputLettersPositionRadius * new Vector3(x, y, 0);

                _letterLocalPositions.Add(position);
                _letterPositions.Add(_lettersRoot.TransformPoint(position));

                angle += delta;
            }

            for (int i = 0; i < _letterCount; i++)
            {
                var letter = Object.Instantiate(_inputLetterPrefab).transform as RectTransform;

                letter.SetParent(_lettersRoot);
                letter.localScale = Vector3.one;

                letter.GetComponentInChildren<TextMeshProUGUI>().text = word[i].ToString().ToUpper();

                _inputLetters.Add(letter);
            }

            ShufflePositions();

            for (int i = 0; i < _letterCount; i++)
            {
                _inputLetters[i].localPosition = _letterLocalPositions[i];
            }
        }

        private void DrawLine()
        {
            _positions = iTween.GetSmoothPoints(_points.ToArray(), 8);
            lineRenderer.positionCount = _positions.Count;
            lineRenderer.SetPositions(_positions.ToArray());
        }

        private int GetNearestPosition(Vector3 point, List<Vector3> letters)
        {
            float min = float.MaxValue;
            int index = -1;
            for (int i = 0; i < letters.Count; i++)
            {
                float distant = Vector3.Distance(point, letters[i]);
                if (distant < min)
                {
                    min = distant;
                    index = i;
                }
            }
            return index;
        }

        private void BuildPoints()
        {
            _points.Clear();
            foreach (var i in _currentIndices) _points.Add(_letterPositions[i]);

            if (_currentIndices.Count == 1 || _points.Count >= 1 && Vector3.Distance(_inputPosition, _points[_points.Count - 1]) >= _inputSensitivityRadius)
            {
                _points.Add(_inputPosition);
            }
        }

        private void ShufflePositions()
        {
            var seed = Random.Range(0, 10000);
            _letterLocalPositions.Shuffle(seed);
            _letterPositions.Shuffle(seed);
        }

        private void ShuffleLetters()
        {
            ShufflePositions();

            for (int i = 0; i < _letterCount; i++)
            {
                Sequence sequence = DOTween.Sequence();
                sequence.Append(_inputLetters[i].DOLocalMove(Vector3.zero, .5f));
                sequence.Append(_inputLetters[i].DOLocalMove(_letterLocalPositions[i], .5f));
            }
        }

        public void ShowOtherPlayerResults() => _otherPlayerResults.gameObject.SetActive(true);

        public void SetLocalPlayerName(string firstName, string lastName, string nickname) => _localPlayerName.text = TextFormatter.GetFormattedNameTwoLines(firstName, lastName, nickname);

        public void SetLocalPlayerAge(string age) => _localPlayerAge.text = TextFormatter.GetFormattedAge(age);

        public void SetLocalPlayerSolvedWords(int amount) => _localPlayerSolvedWords.text = TextFormatter.GetFormattedSolvedWordsAmount(amount);

        public void SetLocalPlayerTime(TimeSpan time) => _localPlayerTime.text = TextFormatter.GetFormattedTime(time);

        public void SetLocalPlayerTime(double time) => _localPlayerTime.text = TextFormatter.GetFormattedTime(time);

        public void SetLocalPlayerTime(string time) => _localPlayerTime.text = TextFormatter.GetFormattedTime(time);

        public void SetLocalPlayerPhoto(Texture2D photo)
        {
            if (photo == null)
            {
                _localPlayerPhoto.gameObject.SetActive(false);
                _localPlayerPhotoBG.gameObject.SetActive(true);
            }
            else
            {
                _localPlayerPhoto.texture = photo;
                _localPlayerPhoto.gameObject.SetActive(true);
                _localPlayerPhotoBG.gameObject.SetActive(false);
            }
        }

        public void SetOtherPlayerName(string firstName, string lastName, string nickname) => _otherPlayerName.text = TextFormatter.GetFormattedName(firstName, lastName, nickname);

        public void SetOtherPlayerAge(string age) => _otherPlayerAge.text = TextFormatter.GetFormattedAge(age);

        public void SetOtherPlayerSolvedWords(int amount) => _otherPlayerSolvedWords.text = TextFormatter.GetFormattedSolvedWordsAmount(amount);

        public void SetOtherPlayerTime(TimeSpan time) => _otherPlayerTime.text = TextFormatter.GetFormattedTime(time);

        public void SetOtherPlayerTime(double time) => _otherPlayerTime.text = TextFormatter.GetFormattedTime(time);

        public void SetOtherPlayerTime(string time) => _otherPlayerTime.text = TextFormatter.GetFormattedTime(time);

        public void SetOtherPlayerPhoto(Texture2D photo)
        {
            if (photo == null)
            {
                _otherPlayerPhoto.gameObject.SetActive(false);
                _otherPlayerPhotoBG.gameObject.SetActive(true);
            }
            else
            {
                _otherPlayerPhoto.texture = photo;
                _otherPlayerPhoto.gameObject.SetActive(true);
                _otherPlayerPhotoBG.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}