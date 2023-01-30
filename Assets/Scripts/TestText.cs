using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestText : MonoBehaviour
{
    [SerializeField] RectTransform _message;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] RectTransform _messageBackground;

    float _defaultHeight;

    // Start is called before the first frame update
    void Start()
    {
        _defaultHeight = _message.sizeDelta.y;
    }

    // Update is called once per frame
    void Update()
    {
        var size = _message.sizeDelta;
        size.y = _defaultHeight + 52.9f * (_text.textInfo.lineCount - 1);
        _message.sizeDelta = size;

        float width = _message.sizeDelta.x - 110;
        
        var right =  width - _text.preferredWidth + 25;

        if (right < 100)
            right = 100;

        _messageBackground.SetRight(right);
    }
}
