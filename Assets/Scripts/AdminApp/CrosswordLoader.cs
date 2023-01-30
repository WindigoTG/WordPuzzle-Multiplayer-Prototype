using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace WordPuzzle.Administration
{
    public class CrosswordLoader
    {
        #region Methods

        public Crossword[] LoadCrosswordsFromPaths(string[] paths)
        {
            var crosswords = new List<Crossword>();
            
            for (int i = 0; i < paths.Length; i++)
            {
                if (!File.Exists(paths[i]))
                    continue;

                var data = File.ReadAllText(paths[i]);
                var crossword =  JsonUtility.FromJson<Crossword>(data);

                if (crossword.length == 0)
                    continue;

                crossword.clues = crossword.clues.OrderBy(clue => clue.number).ToList();
                crossword.words = crossword.words.OrderBy(word => word.number).ToList();
                crosswords.Add(crossword);

                foreach (var word in crossword.words)
                    Debug.Log(word.value);

                foreach (var clue in crossword.clues)
                    Debug.Log(clue.value);
            }

            return crosswords.ToArray();
        }

        #endregion
    }
}