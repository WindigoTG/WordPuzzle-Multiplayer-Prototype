using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace WordPuzzle
{
    public class InterestItemView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TMP_InputField _input;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _removeButton;

        private bool _isBlankMode;

        #endregion


        #region Properties

        public Button AddButton => _addButton;
        public Button RemoveButton => _removeButton;

        public string Text => _input.text;

        public bool IsBlankMode => _isBlankMode;

        #endregion


        #region Methods

        public void SetBlankMode()
        {
            _addButton.gameObject.SetActive(true);
            _removeButton.gameObject.SetActive(false);
            _input.gameObject.SetActive(false);
            _input.text = "";
            _isBlankMode = true;
        }

        public void SetInputMode()
        {
            _addButton.gameObject.SetActive(false);
            _removeButton.gameObject.SetActive(true);
            _input.gameObject.SetActive(true);
            _isBlankMode = false;
        }

        public void SetInputModeWithText(string text)
        {
            SetInputMode();
            _input.text = text;
        }

        #endregion
    }
}