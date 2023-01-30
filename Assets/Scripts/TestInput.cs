using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class TestInput : MonoBehaviour
{
    [SerializeField] TMP_InputField _input;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] RectTransform _panel;

    float _defaultHeight;

    // Start is called before the first frame update
    void Start()
    {
        _input.onValueChanged.AddListener(OnInputChange);
        _defaultHeight = _panel.sizeDelta.y;
    }

    private void OnInputChange(string text)
    {
        
        var lineCount = _input.textComponent.textInfo.lineCount;
        Debug.Log(lineCount);
        var size = _panel.sizeDelta;
        size.y = _defaultHeight + 64.4f * (lineCount - 1);
        _panel.sizeDelta = size;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
