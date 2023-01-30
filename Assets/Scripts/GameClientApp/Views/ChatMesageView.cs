using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatMesageView : MonoBehaviour
{
    #region Fields

    [SerializeField] private RectTransform _message;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private RectTransform _messageBackground;
    [SerializeField] private float _additionalLineHeight = 52.9f;
    [SerializeField] private float _smallMargin = 10;
    [SerializeField] private float _bigMargin = 100;
    [SerializeField] private Sprite _localUserBG;
    [SerializeField] private Sprite _otherUserBG;

    private float _defaultHeight;
    private bool _isLocalUserMessage;
    private bool _isAdjusted;

    #endregion


    #region Properties

    public float Height => _message.sizeDelta.y;
    public bool IsAdjusted => _isAdjusted;

    #endregion


    #region UnityMethods

    private void Start()
    {
        _defaultHeight = _message.sizeDelta.y;
    }

    private void Update()
    {
        if (!_isAdjusted)
        {
            AdjustMessageSize();
            _isAdjusted = true;
        }
    }

    #endregion


    #region Methods

    public void SetText(string text, bool isLocalUserMessage)
    {
        _text.text = text;
        _isLocalUserMessage = isLocalUserMessage;
        _messageBackground.GetComponent<Image>().sprite = isLocalUserMessage ? _localUserBG : _otherUserBG;
    }

    private void AdjustMessageSize()
    {
        float maxWidth = _message.sizeDelta.x - _smallMargin - _bigMargin;

        var newMargin = maxWidth - _text.preferredWidth + 50;

        if (newMargin < _bigMargin)
            newMargin = _bigMargin;


        if (_isLocalUserMessage)
        {
            _messageBackground.SetLeft(newMargin);
            _messageBackground.SetRight(_smallMargin);
        }
        else
        {
            _messageBackground.SetLeft(_smallMargin);
            _messageBackground.SetRight(newMargin);
        }

        var size = _message.sizeDelta;
        size.y = _defaultHeight + _additionalLineHeight * (_text.textInfo.lineCount - 1);
        _message.sizeDelta = size;
    }

    #endregion
}
