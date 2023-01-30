using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Firebase.Database;
using System.Threading.Tasks;

namespace WordPuzzle
{
    public class CrosswordService : ICrosswordService
    {
        #region Fields

        private List<Theme> _themes;
        private Theme _defaultTheme;

        private const string DEFAULT_CROSSWORDS_PATH = "Crosswords";
        private const string DEFAULT_THEME_NAME = "Без темы";

        private FirebaseDatabase _dbInstance;
        private DatabaseReference _dbReference;

        #endregion


        #region Properties

        public int ThemesCount => _themes != null ? _themes.Count : 0;

        #endregion


        #region ClassLifeCycles

        public CrosswordService()
        {
            _dbInstance = FirebaseDatabase.DefaultInstance;
            _dbInstance.SetPersistenceEnabled(false);
            _dbReference = _dbInstance.RootReference;
            //LoadDefaultCrosswordsData(DEFAULT_CROSSWORDS_PATH);
        }

        public CrosswordService(string worldPath)
        {
            LoadDefaultCrosswordsData(worldPath);
        }

        #endregion


        #region Methods

        private void LoadDefaultCrosswordsData(string path)
        {
            var crosswordsData = Resources.LoadAll<TextAsset>(path);

            var levels = new List<Crossword>(crosswordsData.Length);

            for (int i = 0; i < crosswordsData.Length; i++)
                levels.Add(CreateCrossword(crosswordsData[i]));

            _defaultTheme = new Theme() { Name = DEFAULT_THEME_NAME, Crosswords = levels };
        }

        public void LoadAdditionalThemes()
        {
            LoadAdditionalThemes(DEFAULT_CROSSWORDS_PATH);
        }

        public void LoadAdditionalThemes(string folderPath)
        {
            var path = Path.Combine(Application.dataPath, folderPath);

            if (!Directory.Exists(path))
                return;

            List<Theme> themes = new List<Theme>();

            var folders = Directory.GetDirectories(path);

            foreach (var folder in folders)
            {
                var folderName = folder.Split('\\')[folder.Split('\\').Length - 1];

                var files = Directory.GetFiles(folder);

                if (files.Length == 0)
                    continue;

                var crosswords = new List<Crossword>();

                foreach(var file in files)
                {
                    if (file.EndsWith(".json"))
                    {
                        var str = File.ReadAllText(file);

                        if (str.Length > 0)
                            crosswords.Add(CreateCrossword(str));
                    }
                }

                if (crosswords.Count == 0)
                    continue;

                themes.Add(new Theme() { Name = folderName, Crosswords = crosswords});
            }

            if (themes.Count > 0)
                _themes = themes;
        }

        public Theme GetThemeByIndex(int index)
        {
            if (index >= 0 && index < ThemesCount)
            {
                return _themes[index];
            }

            return _defaultTheme;
        }

        public string GetThemeNameByIndex(int index)
        {
            if (index >= 0 && index < ThemesCount)
            {
                return _themes[index].Name;
            }

            return _defaultTheme.Name;
        }

        private Crossword CreateCrossword(TextAsset levelJson)
        {
            return JsonUtility.FromJson<Crossword>(levelJson.text);
        }

        private Crossword CreateCrossword(string jsonString)
        {
            return JsonUtility.FromJson<Crossword>(jsonString);
        }

        public async Task<(bool isSuccessful, string message)> ReadThemesFromDataBase()
        {
            List<Theme> newThemes = new List<Theme>();
            bool isSuccsessful;
            do
            {
                try
                {
                    var DBTask = _dbReference.Child(References.CROSSWORDS_BRANCH).GetValueAsync();
                    while (!DBTask.IsCompleted)
                        await Task.Yield();

                    if (DBTask.Exception != null)
                    {
                        //Debug.LogWarning($"Failed to register task with {DBTask.Exception}");
                        return (false, $"Failed to register task with {DBTask.Exception}");
                    }
                    else if (DBTask.Result.Value == null)
                    {
                        //Debug.LogWarning("No data retrieved");
                        return (false, "No data retrieved");
                    }

                    foreach (var child in DBTask.Result.Children)
                    {
                        var json = child.Value.ToString();
                        var newTheme = JsonUtility.FromJson<Theme>(json);
                        newThemes.Add(newTheme);
                    }
                    isSuccsessful = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e.Message);
                    Debug.LogWarning(e.GetType());
                    newThemes.Clear();
                    isSuccsessful = false;
                }
            } while (!isSuccsessful);

            if (newThemes.Count > 0)
            {
                _themes?.Clear();
                _themes = newThemes;
                return (true, $"{newThemes.Count} crosswords reseived");
            }
            else
                return (false, "No crosswords");
            
        }

        #endregion
    }
}