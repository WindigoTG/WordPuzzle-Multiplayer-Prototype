#if UNITY_STANDALONE || UNITY_EDITOR

using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

namespace WordPuzzle.Administration
{
    public class SelectableElementView : MonoBehaviour, IPointerClickHandler
    {
#region Fields

        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _backgroungImage;
        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _selectedColor;
        private Action _onClickCallback;

#endregion


#region IPointerClickHandler

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClickCallback?.Invoke();
        }

#endregion


#region UnityMethods

        void Awake()
        {
            if (_text == null)
                _text = GetComponentInChildren<TextMeshProUGUI>();

            if (_backgroungImage == null)
                _backgroungImage = GetComponentInChildren<Image>();

            SetDefaultColor();
        }

#endregion


#region Methods

        public void SetText(string text) => _text.text = text;

        public void RegisterOnClickCallback(Action callback) => _onClickCallback = callback;

        public void SetDefaultColor() => _backgroungImage.color = _defaultColor;

        public void SetSelectedColor() => _backgroungImage.color = _selectedColor;

#endregion
    }
}

#endif