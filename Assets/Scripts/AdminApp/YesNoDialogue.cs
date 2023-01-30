#if UNITY_STANDALONE || UNITY_EDITOR

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WordPuzzle.Administration
{
    public class YesNoDialogue : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirmCallback;

        #endregion


        #region UnityMethods

        private void Awake()
        {
            _confirmButton.onClick.AddListener(OnConfirmClick);
            _cancelButton.onClick.AddListener(OnCancelClick);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveAllListeners();
            _cancelButton.onClick.RemoveAllListeners();
        }

        #endregion


        #region Methods

        public void ShowDialogue(Action onConfirmCallback, string message)
        {
            _onConfirmCallback = onConfirmCallback;
            _messageText.text = message;
            gameObject.SetActive(true);
        }

        public void OnConfirmClick()
        {
            _onConfirmCallback?.Invoke();
            HideDialogue();
        }

        public void OnCancelClick()
        {
            HideDialogue();
        }

        private void HideDialogue()
        {
            _onConfirmCallback = null;
            gameObject.SetActive(false);
        }

        #endregion
    }
}

#endif