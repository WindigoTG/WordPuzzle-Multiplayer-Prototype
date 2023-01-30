using System;
using UnityEngine;
using TMPro;
using System.Text;
using System.Collections;
using Object = UnityEngine.Object;
using System.Collections.Generic;

namespace WordPuzzle
{
    public class CurrentInputPreview : IUpdateableRegular
    {
        #region Fields

        private RectTransform _inputPreview;
        private TextMeshProUGUI _text;

        private const float PADDING = 50f;
        private const float ANIMATION_DURATION = 0.1f;

        private CrossWordGrid _grid;
        private Action _callback;
        private string _inputWord;
        private int _revealLetterIndex;

        private MonoBehaviour _monoBeh;

        private TextMeshProUGUI _animationLetter;
        List<Vector3> _animationPositions = new List<Vector3>();

        private bool _isAnimating;
        private float _currentAnimationDuration;

        #endregion


        #region ClassLifeCycles

        public CurrentInputPreview(RectTransform inputDisplay, TextMeshProUGUI animationLetterPrefab)
        {
            _inputPreview = inputDisplay;
            _text = inputDisplay.GetComponentInChildren<TextMeshProUGUI>();
            SetText("");

            _animationLetter = Object.Instantiate(animationLetterPrefab, _inputPreview);
            _animationLetter.transform.localPosition = Vector3.zero;
            _animationLetter.transform.localScale = Vector3.one;
            _animationLetter.gameObject.SetActive(false);
        }

        #endregion


        #region Method

        public void SetText(string text)
        {
            _text.text = text;
            _inputPreview.sizeDelta = new Vector2(_text.preferredWidth + PADDING, _inputPreview.sizeDelta.y);

            if (text.Length > 0)
            {
                _inputPreview.gameObject.SetActive(true);
            }
            else
                _inputPreview.gameObject.SetActive(false);
        }

        public void AnimatePreviewToRevealWordInGridWithCallback(CrossWordGrid grid, Action callback, MonoBehaviour monoBeh)
        {
            _grid = grid;
            _callback = callback;
            _monoBeh = monoBeh;

            _inputWord = _text.text;

            if (!_grid.DoesContainWord(_inputWord))
            {
                _callback?.Invoke();
                return;
            }

            _revealLetterIndex = _inputWord.Length;

            _monoBeh.StartCoroutine(TestCoroutine());
        }

        private void RevealNextLetter()
        {
            _currentAnimationDuration = 0;

            if (--_revealLetterIndex < 0)
            {
                _callback?.Invoke();
                SetText("");
                return;
            }

            if (_revealLetterIndex > 0)
            {
                StringBuilder newText = new StringBuilder();
                for (int i = 0; i < _revealLetterIndex; i++)
                {
                    newText.Append(_inputWord[i]);
                }
                SetText(newText.ToString());
            }
            else
                SetText(" ");

            if (!_grid.IsLetterRevealed(_inputWord, _revealLetterIndex))
            {
                _animationLetter.text = _inputWord[_revealLetterIndex].ToString();

                var startPosition = _inputPreview.transform.position;
                var endPosition = _grid.GetLetterPosition(_inputWord, _revealLetterIndex);
                var midpoint = GetMiddlePoint(startPosition, endPosition, -0.3f);

                Vector3[] keyPositions = { startPosition, midpoint, endPosition };

                _animationPositions = iTween.GetSmoothPoints(keyPositions, 16);
                
                _isAnimating = true;
                _animationLetter.gameObject.SetActive(true);

                //_grid.ShowLetter(_inputWord, _revealLetterIndex);
            }
            else
            {
                RevealNextLetter();
                return;
            }

            //_monoBeh.StartCoroutine(TestCoroutine());
        }

        public static Vector3 GetMiddlePoint(Vector3 begin, Vector3 end, float delta = 0)
        {
            Vector3 center = Vector3.Lerp(begin, end, 0.5f);
            Vector3 beginEnd = end - begin;
            Vector3 perpendicular = new Vector3(-beginEnd.y, beginEnd.x, 0).normalized;
            Vector3 middle = center + perpendicular * delta;
            return middle;
        }

        private IEnumerator TestCoroutine()
        {
            yield return new WaitForSeconds(ANIMATION_DURATION);

            RevealNextLetter();
        }

        #endregion


        #region IUpdateableRegular

        public void UpdateRegular()
        {
            if (!_isAnimating)
                return;

            if (_currentAnimationDuration < ANIMATION_DURATION)
                _currentAnimationDuration += Time.deltaTime;

            var percentage = _currentAnimationDuration / ANIMATION_DURATION;

            int positionIndex = (int)Mathf.Clamp((_animationPositions.Count - 1) * percentage, 0, _animationPositions.Count - 1);

            _animationLetter.transform.position = _animationPositions[positionIndex];

            if (_currentAnimationDuration >= ANIMATION_DURATION)
            {
                _isAnimating = false;
                _animationLetter.gameObject.SetActive(false);
                _grid.ShowLetter(_inputWord, _revealLetterIndex);

                RevealNextLetter();
            }
        }

        #endregion
    }
}