#if UNITY_STANDALONE || UNITY_EDITOR

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace WordPuzzle.Administration
{
    public class ThemeHolder
    {
        #region Fields

        private List<Theme> _themes = new List<Theme>();

        #endregion


        #region Properties

        public int ThemesCount => _themes.Count;

        #endregion


        #region Methods

        public async Task UploadThemesToDataBase()
        {
            await DataBaseCleanUp();

            foreach (var theme in _themes)
            {

                var DBTask = Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child(References.CROSSWORDS_BRANCH).Child(theme.Name).SetValueAsync(JsonUtility.ToJson(theme));

                while (!DBTask.IsCompleted)
                    await Task.Yield();

                if (DBTask.Exception != null)
                {
                    Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
                }
                else
                {
                    Debug.Log($"{theme} uploaded successfully");
                }
            }
        }

        public async Task DataBaseCleanUp()
        {
            var DBTask = Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child(References.CROSSWORDS_BRANCH).GetValueAsync();

            while (!DBTask.IsCompleted)
                await Task.Yield();

            if (DBTask.Exception != null)
            {
                Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value == null)
            {
                Debug.LogWarning("No data retrieved");
            }

            foreach (var child in DBTask.Result.Children)
            {
                if (_themes.Exists(Theme => Theme.Name.Equals(child.Key)))
                    continue;

                var removeTask = Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child(References.CROSSWORDS_BRANCH).Child(child.Key).RemoveValueAsync();

                while (!removeTask.IsCompleted)
                    await Task.Yield();
            }
        }

        public async Task<bool> ReadThemesFromDataBase()
        {
            var DBTask = Firebase.Database.FirebaseDatabase.DefaultInstance.RootReference.Child(References.CROSSWORDS_BRANCH).GetValueAsync();

            while (!DBTask.IsCompleted)
                await Task.Yield();

            if (DBTask.Exception != null)
            {
                Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
                return false;
            }
            else if (DBTask.Result.Value == null)
            {
                Debug.LogWarning("No data retrieved");
                return false;
            }

            var newThemes = new List<Theme>();

            foreach (var child in DBTask.Result.Children)
            {
                var newTheme = JsonUtility.FromJson<Theme>(child.GetValue(false).ToString());
                newThemes.Add(newTheme);
            }

            if (newThemes.Count > 0)
            {
                _themes.Clear();
                _themes = newThemes;
                return true;
            }
            else
                return false;
        }

        public void AddNewTheme()
        {
            _themes.Add(new Theme { Crosswords = new List<Crossword>() });
        }

        public void RemoveThemeByIndex(int themeIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount)
                return;

            _themes.RemoveAt(themeIndex);
        }

        public void ClearThemes() => _themes.Clear();

        public void SetThemeNameByIndex(string themeName, int themeIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount)
                return;

            var theme = _themes[themeIndex];
            theme.Name = themeName;
            _themes[themeIndex] = theme;
        }

        public void AddCrosswordToThemeByIndex(Crossword crossword, int themeIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount)
                return;

            _themes[themeIndex].Crosswords.Add(crossword);
        }

        public void RemoveCrosswordFromThemeByIndex(int themeIndex, int crosswordIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount ||
                crosswordIndex < 0 || crosswordIndex >= _themes[themeIndex].Crosswords.Count)
                return;

            _themes[themeIndex].Crosswords.RemoveAt(crosswordIndex);
        }

        public int GetThemeCrosswordCountByIndex(int themeIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount)
                return -1;

            return _themes[themeIndex].Crosswords.Count;
        }

        public string GetThemeNameByIndex(int themeIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount)
                return string.Empty;

            return _themes[themeIndex].Name;
        }

        public int GetWordCountByIndex(int themeIndex, int crosswordIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount ||
                crosswordIndex < 0 || crosswordIndex >= _themes[themeIndex].Crosswords.Count)
                return - 1;

            return _themes[themeIndex].Crosswords[crosswordIndex].words.Count;
        }

        public string GetWordByIndex(int themeIndex, int crosswordIndex, int wordIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount ||
                crosswordIndex < 0 || crosswordIndex >= _themes[themeIndex].Crosswords.Count ||
                wordIndex < 0 || wordIndex >= _themes[themeIndex].Crosswords[crosswordIndex].words.Count)
                return string.Empty;

            return _themes[themeIndex].Crosswords[crosswordIndex].words[wordIndex].value;
        }

        public Crossword GetCrosswordByIndex(int themeIndex, int crosswordIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount ||
                crosswordIndex < 0 || crosswordIndex >= _themes[themeIndex].Crosswords.Count)
                return new Crossword();

            return _themes[themeIndex].Crosswords[crosswordIndex];
        }

        public string GetClueForWordByIndex(int themeIndex, int crosswordIndex, int wordIndex)
        {
            if (themeIndex < 0 || themeIndex >= ThemesCount ||
                crosswordIndex < 0 || crosswordIndex >= _themes[themeIndex].Crosswords.Count ||
                wordIndex < 0 || wordIndex >= _themes[themeIndex].Crosswords[crosswordIndex].words.Count)
                return "";

            return _themes[themeIndex].Crosswords[crosswordIndex].clues[wordIndex].value;
        }

        #endregion
    }
}

#endif