namespace WordPuzzle
{
    public class SettingsController : BaseController
    {
        #region Fields

        private Context _context;
        private SettingsView _view;

        #endregion


        #region ClassLifeCycles

        public SettingsController(Context context)
        {
            _context = context;
            InstantiatePrefab<SettingsView>(_context.UIPrefabsData.SettingsPrefab, _context.CommonUiHolder, InitView);
        }

        #endregion


        #region Methods

        private void InitView(SettingsView view)
        {
            _view = view;
        }

        #endregion
    }
}