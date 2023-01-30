using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WordPuzzle
{
    public class ThemeSelectionView : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        #region Fields

        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private int _scrollSpeed = 10;
        [SerializeField] private Button _backButton;

        private int _worldsCount;
        private bool _isLerping;
        private float[] _worldPositions;
        private float _stepSize;
        private int _currentWorldIndex = 0;
        private float _targetPosition;

        private RectTransform _rootCanvasTransform;

        #endregion


        #region Properties

        public Button BackButton => _backButton;

        #endregion


        #region UnityMethods

        void Update()
        {
            if (_isLerping == false) return;
            if (_scroll.horizontal)
            {
                _scroll.horizontalNormalizedPosition = Mathf.Lerp(_scroll.horizontalNormalizedPosition, _targetPosition, _scrollSpeed * _scroll.elasticity * Time.deltaTime);
                if (Mathf.Approximately(_scroll.horizontalNormalizedPosition, _targetPosition))
                    _isLerping = false;
            }
            else
            {
                _scroll.verticalNormalizedPosition = Mathf.Lerp(_scroll.verticalNormalizedPosition, _targetPosition, _scrollSpeed * _scroll.elasticity * Time.deltaTime);
                if (Mathf.Approximately(_scroll.verticalNormalizedPosition, _targetPosition))
                    _isLerping = false;
            }
        }

        #endregion


        #region IBeginDragHandler

        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        #endregion


        #region IEndDragHandler

        public void OnEndDrag(PointerEventData data)
        {
            _currentWorldIndex = _scroll.horizontal ? FindNearest(_scroll.horizontalNormalizedPosition) :
                        _scroll.vertical ? FindNearest(_scroll.verticalNormalizedPosition) : _currentWorldIndex;
            MoveToWorld(_currentWorldIndex);
        }

        #endregion


        #region IDragHandler

        public void OnDrag(PointerEventData data)
        {
            _isLerping = false;
        }

        #endregion


        #region Methods

        public void Init()
        {
            _worldsCount = _scroll.content.childCount;
            if (_worldsCount != 0)
                InitPoints(_worldsCount);

            var rectTransforms = GetComponentsInParent<RectTransform>();
            _rootCanvasTransform = rectTransforms[rectTransforms.Length - 1];
            UpdateUI();
        }

        public void InitPoints(int worldCount)
        {
            _worldsCount = worldCount;
            _worldPositions = new float[_worldsCount];

            if (_worldsCount > 1)
            {
                _stepSize = 1 / (float)(_worldsCount - 1);

                for (int i = 0; i < _worldsCount; i++)
                {
                    _worldPositions[i] = i * _stepSize;
                }
            }
            else
            {
                _worldPositions[0] = 0;
            }
        }

        int FindNearest(float currentPosition)
        {
            float distance = Mathf.Infinity;
            int output = 0;
            for (int i = 0; i < _worldPositions.Length; i++)
            {
                if (Mathf.Abs(_worldPositions[i] - currentPosition) < distance)
                {
                    distance = Mathf.Abs(_worldPositions[i] - currentPosition);
                    output = i;
                }
            }
            return output;
        }

        public void NextWorld()
        {
            MoveToWorld(_currentWorldIndex + 1);
        }

        public void PreviousWorld()
        {
            MoveToWorld(_currentWorldIndex - 1);
        }

        public void MoveToWorld(int worldIndex)
        {
            _currentWorldIndex = Mathf.Clamp(worldIndex, 0, _worldsCount - 1);
            _targetPosition = _worldPositions[_currentWorldIndex];
            _isLerping = true;
        }

        public void SetWorld(int worldIndex)
        {
            _currentWorldIndex = Mathf.Clamp(worldIndex, 0, _worldsCount - 1);
            _targetPosition = _worldPositions[_currentWorldIndex];

            if (_scroll.horizontal)
                _scroll.horizontalNormalizedPosition = _targetPosition;
            else
                _scroll.verticalNormalizedPosition = _targetPosition;
        }

        private void UpdateUI()
        {
            _scroll.content.sizeDelta = new Vector2(_rootCanvasTransform.rect.width * _scroll.content.childCount, _scroll.content.sizeDelta.y);
            _scroll.content.GetComponent<HorizontalLayoutGroup>().spacing = _rootCanvasTransform.rect.width;
        }

        public void AddWorld(Transform world)
        {
            world.SetParent(_scroll.content);
            world.localScale = Vector3.one;
        }

        #endregion


    }
}