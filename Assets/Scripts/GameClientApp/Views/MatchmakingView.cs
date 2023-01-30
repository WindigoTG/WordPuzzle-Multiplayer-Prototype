using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WordPuzzle
{
    public class MatchmakingView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button _backButton;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private RectTransform _loadingCircle;
        [SerializeField] private float _rotation;

        #endregion


        #region Properties

        public Button BackButton => _backButton;
        public TextMeshProUGUI Text => _text;

        #endregion


        #region UnityMethods

        private void Start()
        {
            StartCoroutine(AnimateText());
        }

        private void Update()
        {
            _loadingCircle.Rotate(new Vector3(0, 0, _rotation * Time.deltaTime));
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        #endregion


        #region Methods

        private IEnumerator AnimateText()
        {
            var i = 0;
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                i = ++i % 4;
                _text.text = "Поиск второго игрока" + new string('.', i);
            }
        }

        #endregion
    }
}