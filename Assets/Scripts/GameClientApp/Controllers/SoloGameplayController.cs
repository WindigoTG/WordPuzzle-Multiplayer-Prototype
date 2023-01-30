using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordPuzzle
{
    public class SoloGameplayController : GameplayController
    {
        #region ClassLifeCycles

        public SoloGameplayController(Context context) : base(context) 
        {
            SetInitialLevel();

            InstantiatePrefab<GameplayUIView>(_context.UIPrefabsData.GameplayUiView, _context.VariableUiHolder, InitView);

            _crosswoedGrid = new CrossWordGrid(_view.CrosswordGrid, _context);

            _inputPreview = new CurrentInputPreview(_view.CurrentInputPreview, _context.UIPrefabsData.AnimationLetter);

            NextWord();
            _crosswoedGrid.InitiateGridForCrossword(_crossword);
        }

        #endregion


        #region Methods

        protected override void OnContinueButtonClick()
        {
            PlayNewCrossword(GetRandomCrossword());
        }

        #endregion
    }
}