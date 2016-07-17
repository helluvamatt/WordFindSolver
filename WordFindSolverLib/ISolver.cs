using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WordFindSolverLib
{
	public interface ISolver
	{
		int MinWordSize { get; set; }
		int MaxWordSize { get; set; }
		void AddWord(string word);
		Dictionary<string, List<Solution>> SolvePuzzle(Board board, CancellationToken cancelToken, IProgress<ProgressSolution> progress);
	}

	public class Solution
	{
		public Solution(string word, List<Coord> coords)
		{
			Word = word;
			Coords = coords;
		}

		public string Word { get; private set; }

		public List<Coord> Coords { get; private set; }
	}

	public class Board
	{
		public Board(int width, int height, char[] data)
		{
			Width = width;
			Height = height;
			Data = data;
		}

		public int Width { get; private set; }
		public int Height { get; private set; }
		public char[] Data { get; private set; }

		public void ResizeTo(int w, int h)
		{
			int oldW = Width;
			int oldH = Height;
			char[] oldData = Data;

			var temp = new List<char>(w * h);
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					if (x < oldW && y < oldH)
					{
						temp.Add(oldData[y * oldW + x]);
					}
					else
					{
						temp.Add('A');
					}
				}
			}
			Data = temp.ToArray();
			Height = h;
			Width = w;
		}
	}

	public struct Coord
	{
		public Coord(int x, int y)
		{
			X = x;
			Y = y;
		}

		public int X { get; set; }
		public int Y { get; set; }

		public bool Equals(Coord other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			if (obj is Coord) return Equals((Coord)obj);
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 23;
			hash = hash * 31 + X.GetHashCode();
			hash = hash * 31 + Y.GetHashCode();
			return hash;
		}

		public override string ToString()
		{
			return string.Format("({0},{1})", X, Y);
		}
	}

	public class ProgressSolution
	{
		public int Progress { get; set; }
		public int Total { get; set; }
		public bool IsIndeterminate { get; set; }
		public Solution LastSolution { get; set; }
	}
}
