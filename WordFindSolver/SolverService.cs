using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WordFindSolverLib;

namespace WordFindSolver
{
	internal class SolverService
	{
		private ISolver _Solver;
		private Board _Board;
		private string _WordsFile;

		public SolverService(ISolver instance, Board board, string wordsFile)
		{
			_Solver = instance;
			_Board = board;
			_WordsFile = wordsFile;
		}

		public async Task<Dictionary<string, List<Solution>>> SolveAsync(CancellationToken cancelToken, IProgress<ProgressSolution> progress)
		{
			progress.Report(new ProgressSolution { IsIndeterminate = true });
			using (FileStream wordsFileStream = new FileStream(_WordsFile, FileMode.Open, FileAccess.Read))
			{
				using (StreamReader wordsFileStreamReader = new StreamReader(wordsFileStream))
				{
					string word;
					while ((word = await wordsFileStreamReader.ReadLineAsync()) != null && !cancelToken.IsCancellationRequested)
					{
						_Solver.AddWord(word);
					}
				}
			}

			if (cancelToken.IsCancellationRequested) return null;

			return await Task.Run(() => _Solver.SolvePuzzle(_Board, cancelToken, progress));
		}
	}
}
