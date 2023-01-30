using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WordPuzzle
{
    public class ScreenFadeController : BaseController, IUpdateableRegular
    {
        private Context _context;
        private Image _screenCover;

        private Color _opaqueColor;
        private Color _transparentColor;

        private Action _callback;

        private Color _colorFrom;
        private Color _colorTo;
        private float _fadeProgress;
        private bool _isFading;

        #region ClassLifeCycles

        public ScreenFadeController(Context context)
        {
            _context = context;

            _screenCover = Object.Instantiate(_context.UIPrefabsData.ScreenCoverPrefab, _context.FadeOverlayUiHolder);
            AddGameObject(_screenCover.gameObject);

            _opaqueColor = _screenCover.color;
            _transparentColor = _screenCover.color;
            _opaqueColor.a = 1;
            _transparentColor.a = 0;

            _screenCover.gameObject.SetActive(false);
        }

        #endregion


        #region IUpdateableRegular

        public void UpdateRegular()
        {
            if (!_isFading)
                return;

            _screenCover.color = Color.Lerp(_colorFrom, _colorTo, _fadeProgress);

            if (_fadeProgress < _context.ScreenFadeTime)
                _fadeProgress += Time.deltaTime;
            else
            {
                _screenCover.gameObject.SetActive(_colorTo == _opaqueColor ? true : false);
                _fadeProgress = 0;
                _isFading = false;
                _callback?.Invoke();
            }


        }

        #endregion


        #region Methods

        public void FadeOutWithCallback(Action callback)
        {
            _callback = callback;
            _colorTo = _opaqueColor;
            _colorFrom = _transparentColor;
            _isFading = true;
            _screenCover.gameObject.SetActive(true);
        }

        public void FadeInWithCallback(Action callback)
        {
            _callback = callback;
            _colorTo = _transparentColor;
            _colorFrom = _opaqueColor;
            _isFading = true;
            _screenCover.gameObject.SetActive(true);
        }

        #endregion
    }
}