namespace WordPuzzle
{
    public class SocialController : BaseController
    {
        #region Fields

        private Context _context;
        private SocialView _view;

        #endregion


        #region ClassLifeCycles

        public SocialController(Context context)
        {
            _context = context;
            InstantiatePrefab<SocialView>(_context.UIPrefabsData.SocialPrefab, _context.CommonUiHolder, InitView);
        }

        #endregion


        #region Methods

        private void InitView(SocialView view)
        {
            _view = view;
        }

        #endregion
    }
}