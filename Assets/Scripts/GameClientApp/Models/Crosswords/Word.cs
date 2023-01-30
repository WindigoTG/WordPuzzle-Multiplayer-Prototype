using System;
using UnityEngine;

namespace WordPuzzle
{
	[Serializable]
	public struct Word
	{
		public int number;
		public string value;
		public int length;
		public Vector2Int start;
		public int orientation;
	}
}