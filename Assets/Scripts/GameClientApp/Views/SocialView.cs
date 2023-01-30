using UnityEngine;
using UnityEngine.UI;

namespace WordPuzzle
{
    public class SocialView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Button _socialButton;

        #endregion


        #region Properties

        public Button SocialButton => _socialButton;

        #endregion
    }
}