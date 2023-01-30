using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace WordPuzzle
{
    public class ChatWindowView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private RectTransform _chatWindow;
        [Header("Top")]
        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _photoBG;
        [SerializeField] private RawImage _photo;
        [Space]
        [Header("Viewport")]
        [SerializeField] private ScrollRect _scrollRect;
        [Space]
        [Header("Bottom")]
        [SerializeField] private Button _sendButton;
        [SerializeField] private TMP_InputField _messageInput;
        [SerializeField] private RectTransform _inputBgPanel;
        [SerializeField] private float _additionalLineHeight = 64.4f;
        [SerializeField, Min(1)] private float _inputExpansionRatio = 4f;
        [Space]
        [SerializeField] private RectTransform _notification;
        [SerializeField] private TextMeshProUGUI _notificationText;
        [SerializeField] private Image _notificationPhotoBG;
        [SerializeField] private RawImage _notificationPhoto;
        [SerializeField, Min(0)] private float _notificationSlideInOutTime;
        [SerializeField, Min(0)] private float _notificationStayTime;

        private float _defaultInputPanelHeight;
        private float _maxInputPanelHeight;

        private bool _isNotificationShowing;

        #endregion


        #region Properties

        public Button BackButton => _backButton;
        public Button SendButton => _sendButton;
        public string InputText => _messageInput.text;
        public ScrollRect ScrollRect => _scrollRect;
        public bool ChatWindowActiveSelf => _chatWindow.gameObject.activeSelf;

        #endregion


        #region UnityMethods

        private void Awake()
        {
            _defaultInputPanelHeight = _inputBgPanel.sizeDelta.y;
            _maxInputPanelHeight = _defaultInputPanelHeight * _inputExpansionRatio;
            _messageInput.onValueChanged.AddListener(OnInputChange);
        }

        #endregion


        #region Methods

        private void OnInputChange(string text)
        {
            var lineCount = _messageInput.textComponent.textInfo.lineCount;
            var size = _inputBgPanel.sizeDelta;
            size.y = Mathf.Min(_defaultInputPanelHeight + _additionalLineHeight * (lineCount - 1), _maxInputPanelHeight);
            _inputBgPanel.sizeDelta = size;
        }

        public void ResetInput() => _messageInput.text = "";

        public void ShowChatWindow() => _chatWindow.gameObject.SetActive(true);

        public void HideChatWindow() => _chatWindow.gameObject.SetActive(false);

        public void ShowChatWindowForUser(UserProfileData user)
        {
            _nameText.text = TextFormatter.GetFormattedName(user.FirstName, user.LastName, user.Nickname);

            if (user.Photo != null)
            {
                _photo.texture = user.Photo;
                _photo.gameObject.SetActive(true);
                _photoBG.gameObject.SetActive(false);
            }
            else
            {
                _photo.gameObject.SetActive(false);
                _photoBG.gameObject.SetActive(true);
            }

            ShowChatWindow();
        }

        public void ShowNewMessageNotification(string senderName, Texture2D senderPhoto = null)
        {
            if (_isNotificationShowing)
                return;

            _notificationText.text = $"Новое сообщение от\n{senderName}";

            if (senderPhoto != null)
            {
                _notificationPhoto.texture = senderPhoto;
                _notificationPhotoBG.gameObject.SetActive(false);
                _notificationPhoto.gameObject.SetActive(true);
            }
            else
            {
                _notificationPhotoBG.gameObject.SetActive(true);
                _notificationPhoto.gameObject.SetActive(false);
            }

            _isNotificationShowing = true;

            var originalYPos = _notification.localPosition.y;
            var targetYPos = originalYPos - _notification.sizeDelta.y;

            TweenCallback tweenCallback = new TweenCallback(() => _isNotificationShowing = false);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_notification.DOLocalMoveY(targetYPos, _notificationSlideInOutTime).SetEase(Ease.OutQuad));
            sequence.Append(_notification.DOLocalMoveY(targetYPos, _notificationStayTime).SetEase(Ease.InOutQuad));
            sequence.Append(_notification.DOLocalMoveY(originalYPos, _notificationSlideInOutTime).SetEase(Ease.InQuad));
            sequence.AppendCallback(tweenCallback);
        }

        public void ShowNewMessageNotificationForUser(UserProfileData user)
        {
            ShowNewMessageNotification(TextFormatter.GetFormattedName(user.FirstName, user.LastName, user.Nickname), user.Photo);
        }

        #endregion
    }
}