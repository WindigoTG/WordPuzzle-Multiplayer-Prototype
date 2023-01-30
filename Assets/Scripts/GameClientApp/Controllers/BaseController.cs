using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WordPuzzle
{
    public abstract class BaseController : IDisposable
    {
        #region Fields

        protected List<BaseController> _baseControllers = new List<BaseController>();
        protected List<GameObject> _gameObjects = new List<GameObject>();
        private bool _isDisposed;

        #endregion


        #region IDisposable

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            foreach (var baseController in _baseControllers)
                baseController?.Dispose();

            _baseControllers.Clear();

            foreach (var cachedGameObject in _gameObjects)
                Object.Destroy(cachedGameObject);

            _gameObjects.Clear();

            OnDispose();
        }

        #endregion


        #region Methods

        protected void AddController(BaseController baseController)
        {
            if (!_baseControllers.Contains(baseController))
                _baseControllers.Add(baseController);
        }

        protected void RemoveController(BaseController baseController)
        {
            if (_baseControllers.Contains(baseController))
            {
                _baseControllers.Remove(baseController);
                baseController.Dispose();
            }
        }

        protected void AddGameObject(GameObject gameObject)
        {
            _gameObjects.Add(gameObject);
        }

        protected void InstantiatePrefab<T>(T prefabToInstantiate, Transform spawnParent, Action<T> callback) where T : MonoBehaviour
        {
            var prefab = prefabToInstantiate.gameObject;
            var newObject = GameObject.Instantiate(prefab, spawnParent);
            _gameObjects.Add(newObject);

            callback.Invoke(newObject.GetComponent<T>());
        }

        protected virtual void OnDispose()
        {
        }

        #endregion
    }
}