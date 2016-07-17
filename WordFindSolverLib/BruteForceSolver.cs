using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WordFindSolverLib
{
	[Description("Brute Force")]
	public class BruteForceSolver : ISolver
	{
		private HashSet<string> _Words;

		private static readonly int[] moveX = new int[] { -1, 0, 1, -1, 1, -1, 0, 1 };
		private static readonly int[] moveY = new int[] { -1, -1, -1, 0, 0, 1, 1, 1 };

		public BruteForceSolver()
		{
			_Words = new HashSet<string>();
		}

		public int MinWordSize { get; set; }

		public int MaxWordSize { get; set; }

		public void AddWord(string word)
		{
			// Filter
			string newWord = string.Empty;
			foreach (char c in word)
			{
				if (!char.IsLetter(c)) continue;
				newWord += char.ToLower(c);
			}
			if (newWord.Length > 0) _Words.Add(newWord);
		}

		public Dictionary<string, List<Solution>> SolvePuzzle(Board board, CancellationToken cancelToken, IProgress<ProgressSolution> progress)
		{
			Dictionary<string, List<Solution>> solutions = new Dictionary<string, List<Solution>>();
			for (int y = 0; y < board.Height; y++)
			{
				for (int x = 0; x < board.Width; x++)
				{
					if (cancelToken.IsCancellationRequested) return solutions;
					FindSolution(solutions, cancelToken, progress, new TransientMove(board, x, y), x, y);
					progress.Report(new ProgressSolution { Progress = y * board.Width + x, Total = board.Height * board.Width });
				}
			}
			return solutions;
		}

		private void FindSolution(Dictionary<string, List<Solution>> solutions, CancellationToken token, IProgress<ProgressSolution> progress, TransientMove tMove, int curX, int curY)
		{
			if (token.IsCancellationRequested) return;

			tMove.BeginMove(curX, curY);
			
			if (tMove.GetCurrentStep() >= MinWordSize)
			{
				string word = tMove.GetCurrentWord();
				if (_Words.Contains(word))
				{
					// Yay! We found a word!
					if (!solutions.ContainsKey(word)) solutions.Add(word, new List<Solution>());
					var solution = tMove.GetSolution();
					var origin = tMove.GetOrigin();
					solutions[word].Add(solution);
					progress.Report(new ProgressSolution { Progress = origin.Y * tMove.Board.Width + origin.X, Total = tMove.Board.Width * tMove.Board.Height, LastSolution = solution });
				}
			}

			if (tMove.GetCurrentStep() < MaxWordSize)
			{
				for (int i = 0; i < moveX.Length && i < moveY.Length; i++)
				{
					if (token.IsCancellationRequested) return;

					int nextX = curX + moveX[i];
					int nextY = curY + moveY[i];
					if (tMove.CanMoveTo(nextX, nextY))
					{
						FindSolution(solutions, token, progress, tMove, nextX, nextY);
					}
				}
			}

			tMove.EndMove();
		}

		private class TransientMove
		{
			private Stack<Coord> _Coords;

			public TransientMove(Board board, int x, int y)
			{
				if (board == null) throw new ArgumentNullException("board");
				if (x < 0 || x >= board.Width) throw new ArgumentOutOfRangeException("x");
				if (y < 0 || y >= board.Height) throw new ArgumentOutOfRangeException("y");

				Board = board;
				_Coords = new Stack<Coord>();
			}

			public Board Board { get; private set; }

			public bool CanMoveTo(int x, int y)
			{
				if (x < 0 || x >= Board.Width) return false; // X-coord must be on board
				if (y < 0 || y >= Board.Height) return false; // Y-coord must be on board
				var lastMove = _Coords.Peek();
				if (x == lastMove.X && y == lastMove.Y) return false; // X and Y coords must be different than the last move
				if (Math.Abs(lastMove.X - x) > 1) return false; // Can only move one space away (X-coord check)
				if (Math.Abs(lastMove.Y - y) > 1) return false; // Can only move one space away (Y-coord check)
				var thisMove = new Coord(x, y);
				// Each space can only be visited once, walk the stack
				foreach (Coord c in _Coords)
				{
					if (c.Equals(thisMove)) return false;
				}
				return true;
			}

			public void BeginMove(int x, int y)
			{
				_Coords.Push(new Coord(x, y));
			}

			public void EndMove()
			{
				_Coords.Pop();
			}

			public string GetCurrentWord()
			{
				StringBuilder bldr = new StringBuilder();
				foreach (Coord c in _Coords)
				{
					bldr.Append(Board.Data[c.Y * Board.Width + c.X]);
				}
				return bldr.ToString().ToLowerInvariant();
			}

			public int GetCurrentStep()
			{
				return _Coords.Count;
			}

			public Solution GetSolution()
			{
				// Both of these method calls with create new "copies" or a snapshot of the TransientMove as it exists right now
				return new Solution(GetCurrentWord(), _Coords.ToList());
			}

			public Coord GetOrigin()
			{
				return _Coords.FirstOrDefault();
			}

		}
	}
}
