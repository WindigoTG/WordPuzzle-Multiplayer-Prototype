using System;
using System.Collections.Generic;

namespace WordPuzzle
{
    [Serializable]
    public struct Crossword
    {
        public List<Word> words;
        public List<Clue> clues;
		public int length;

        public int minXIndex;
        public int maxXIndex;
        public int minYIndex;
        public int maxYIndex;
    }
}