using System.Collections.Generic;
using System;

namespace WordPuzzle
{
    [Serializable]
    public struct Theme
    {
        public string Name;
        public List<Crossword> Crosswords;
    }
}