#if UNITY_STANDALONE || UNITY_EDITOR

using UnityEngine;
using UnityEngine.UI;
using Firebase;

namespace WordPuzzle.Administration
{
    public class AdminAppController : MonoBehaviour
    {
        #region Fields

        [Header("SelectionButtons")]
        [SerializeField] private Button _crosswordsButton;

        [Space]
        [Header("Category UI Holders")]
        [SerializeField] private GameObject _crosswordsHolder;

        #endregion


        #region UnityMethods

        void Awake()
        {
            DisableUI();
            SubscribeButtonsOnClick();

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    Debug.Log("Firebase depencencies resolved successfully");
                }
                else
                {
                    Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
                }

                Firebase.Database.FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            });
        }

        private void OnDestroy()
        {
            UnsubscribeButtonsOnClick();
        }

        #endregion


        #region UnityMethods

        private void SubscribeButtonsOnClick()
        {
            _crosswordsButton.onClick.AddListener(EnableCrosswords);
        }

        private void UnsubscribeButtonsOnClick()
        {
            _crosswordsButton.onClick.RemoveAllListeners();
        }

        private void DisableUI()
        {
            _crosswordsHolder.SetActive(false);
        }

        private void DisableSelectionButtons()
        {
            _crosswordsButton.interactable = false;
        }

        private void EnableSelectionButtons()
        {
            _crosswordsButton.interactable = true;
        }

        private void EnableCrosswords()
        {
            EnableSelectionButtons();
            _crosswordsButton.interactable = false;
            _crosswordsHolder.SetActive(true);
        }



        #endregion
    }
}

#endif