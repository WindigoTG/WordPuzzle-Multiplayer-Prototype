using System;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace WordPuzzle
{
    public class Notificator
    {
        #region Fields

        private CanvasGroup _notification;
        private TextMeshProUGUI _text;
        private const string BASE_TEXT = "Другой игрок разгадал слово:\n";
        private const float DURATION = 2f;

        private Vector3 _startPosition;

        #endregion


        #region ClassLifeCycles

        public Notificator(CanvasGroup notification)
        {
            _notification = notification;
            _text = notification.GetComponentInChildren<TextMeshProUGUI>();
            _notification.gameObject.SetActive(false);
            _startPosition = _notification.transform.localPosition;
        }

        #endregion


        #region Methods

        public void ShowNotification(string text)
        {
            if (_text == null)
                return;

            ResetNotofication(text);

            _notification.transform.DOLocalMoveY(100, DURATION).SetEase(Ease.InOutQuad);
            _notification.DOFade(0, DURATION).SetEase(Ease.InExpo);
        }

        private void ResetNotofication(string text)
        {
            _text.text = BASE_TEXT + $"\"{text.ToUpper()}\"";

            _notification.transform.localPosition = _startPosition;
            _notification.alpha = 1;
            _notification.gameObject.SetActive(true);
        }

        public void ShowNotificationWithCallback(string text, Action callback)
        {
            if (_text == null)
                return;

            ResetNotofication(text);

            _notification.transform.DOLocalMoveY(100, DURATION).SetEase(Ease.InOutQuad);

            Sequence sequence = DOTween.Sequence();
            TweenCallback tweenCallback = new TweenCallback(callback);
            sequence.Append(_notification.DOFade(0, DURATION).SetEase(Ease.InExpo));
            sequence.AppendCallback(tweenCallback);
        }

        

        #endregion
    }
}