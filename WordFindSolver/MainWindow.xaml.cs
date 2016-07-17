using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WordFindSolverLib;

namespace WordFindSolver
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			_Board = new Board(3, 3, new char[] { 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A', 'A' });
			BoardData = new ObservableCollection<char>(_Board.Data);
			Solutions = new ObservableCollection<SolutionEntry>();
		}

		private Board _Board;
		private CancellationTokenSource _CancelTokenSource;

		#region Event handlers

		private static void OnBoardWidthPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			MainWindow w = (MainWindow)source;
			w.BoardDimensionsChanged();
		}

		private static void OnBoardHeightPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			MainWindow w = (MainWindow)source;
			w.BoardDimensionsChanged();
		}

		private static void OnBoardDataPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			// This feels very... JavaScript
			MainWindow w = (MainWindow)source;
			var action = new NotifyCollectionChangedEventHandler((object o, NotifyCollectionChangedEventArgs args) => {
				var data = (ObservableCollection<char>)o;
				data.CopyTo(w._Board.Data, 0);
			});

			if (e.OldValue != null)
				((ObservableCollection<char>)e.OldValue).CollectionChanged -= action;
			if (e.NewValue != null)
				((ObservableCollection<char>)e.NewValue).CollectionChanged += action;

		}

		private static void OnSelectedSolutionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			// TODO UI for specific words showing paths through the word search
			MainWindow w = (MainWindow)source;
			if (e.NewValue != null)
			{

			}
			else
			{
				
			}
		}

		private void listBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Text))
			{
				// TODO Validation rules?
				char c = e.Text.Last();
				if (char.IsLower(c))
				{
					int selIndex = listBox.SelectedIndex;
					if (selIndex > -1)
					{
						BoardData[selIndex] = char.ToUpper(c);
						e.Handled = true;
						listBox.SelectedIndex = selIndex;
					}
				}
			}
		}

		private void browseButton_Click(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Filter = "All files (*.*)|*.*";
			fileDialog.Multiselect = false;
			fileDialog.Title = "Select a word list file...";
			bool? result = fileDialog.ShowDialog(this);
			if (result.HasValue && result.Value)
			{
				WordsFile = fileDialog.FileName;
			}
			else
			{
				WordsFile = null;
			}
		}

		private async void solveButton_Click(object sender, RoutedEventArgs e)
		{
			if (IsBusy && _CancelTokenSource != null)
			{
				_CancelTokenSource.Cancel();
				return;
			} 

			// Validation
			if (string.IsNullOrEmpty(WordsFile))
			{
				browseButtonPopup.IsOpen = true;
				return;
			}

			ResetSolutions();
			IsBusy = true;
			var solver = new SolverService(new BruteForceSolver() { MinWordSize = 3, MaxWordSize = 10 }, _Board, WordsFile);
			var progress = new Progress<ProgressSolution>(OnProgressReported);
			_CancelTokenSource = new CancellationTokenSource();
			var solutions = await solver.SolveAsync(_CancelTokenSource.Token, progress);
			IsBusy = false;
			LastProgress = null;

			if (!_CancelTokenSource.IsCancellationRequested)
			{
				Solutions.AddAll(solutions.Select(s => new SolutionEntry(s.Key, s.Value)));
			}

			_CancelTokenSource = null;
		}

		private void OnProgressReported(ProgressSolution progress)
		{
			LastProgress = progress;
			progressBar.IsIndeterminate = progress.IsIndeterminate;
			progressBar.Value = progress.Progress;
			progressBar.Maximum = progress.Total;
			// TODO May do something else with the progress object, log it etc...
		}

		#endregion

		#region Utility methods

		private void BoardDimensionsChanged()
		{
			_Board.ResizeTo(BoardWidth, BoardHeight);
			BoardData = new ObservableCollection<char>(_Board.Data);
			ResetSolutions();
		}

		private void ResetSolutions()
		{
			SelectedSolution = null;
			Solutions.Clear();
		}

		#endregion

		#region Dependency properties

		private static readonly DependencyProperty BoardWidthProperty = DependencyProperty.Register("BoardWidth", typeof(int), typeof(MainWindow), new FrameworkPropertyMetadata(3, OnBoardWidthPropertyChanged));
		private static readonly DependencyProperty BoardHeightProperty = DependencyProperty.Register("BoardHeight", typeof(int), typeof(MainWindow), new FrameworkPropertyMetadata(3, OnBoardHeightPropertyChanged));
		private static readonly DependencyProperty BoardDataProperty = DependencyProperty.Register("BoardData", typeof(ObservableCollection<char>), typeof(MainWindow), new FrameworkPropertyMetadata(OnBoardDataPropertyChanged));
		private static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register("IsBusy", typeof(bool), typeof(MainWindow));
		private static readonly DependencyProperty LastProgressProperty = DependencyProperty.Register("LastProgress", typeof(ProgressSolution), typeof(MainWindow));
		private static readonly DependencyProperty WordsFileProperty = DependencyProperty.Register("WordsFile", typeof(string), typeof(MainWindow));
		private static readonly DependencyProperty SolutionsProperty = DependencyProperty.Register("Solutions", typeof(ObservableCollection<SolutionEntry>), typeof(MainWindow));
		private static readonly DependencyProperty SelectedSolutionProperty = DependencyProperty.Register("SelectedSolution", typeof(SolutionEntry), typeof(MainWindow), new FrameworkPropertyMetadata(OnSelectedSolutionPropertyChanged));

		private int BoardWidth
		{
			get
			{
				return (int)GetValue(BoardWidthProperty);
			}
			set
			{
				SetValue(BoardWidthProperty, value);
			}
		}

		private int BoardHeight
		{
			get
			{
				return (int)GetValue(BoardHeightProperty);
			}
			set
			{
				SetValue(BoardHeightProperty, value);
			}
		}

		private ObservableCollection<char> BoardData
		{
			get
			{
				return (ObservableCollection<char>)GetValue(BoardDataProperty);
			}
			set
			{

				SetValue(BoardDataProperty, value);
			}
		}

		private bool IsBusy
		{
			get
			{
				return (bool)GetValue(IsBusyProperty);
			}
			set
			{
				SetValue(IsBusyProperty, value);
			}
		}

		private ProgressSolution LastProgress
		{
			get
			{
				return (ProgressSolution)GetValue(LastProgressProperty);
			}
			set
			{
				SetValue(LastProgressProperty, value);
			}
		}

		private string WordsFile
		{
			get
			{
				return (string)GetValue(WordsFileProperty);
			}
			set
			{
				SetValue(WordsFileProperty, value);
			}
		}

		private ObservableCollection<SolutionEntry> Solutions
		{
			get
			{
				return (ObservableCollection<SolutionEntry>)GetValue(SolutionsProperty);
			}
			set
			{
				SetValue(SolutionsProperty, value);
			}
		}

		private SolutionEntry SelectedSolution
		{
			get
			{
				return (SolutionEntry)GetValue(SelectedSolutionProperty);
			}
			set
			{
				SetValue(SelectedSolutionProperty, value);
			}
		}

		#endregion

		#region Utility classes

		internal class SolutionEntry
		{
			public SolutionEntry(string word, List<Solution> solutions)
			{
				Word = word;
				Solutions = solutions;
			}

			public string Word { get; private set; }

			public List<Solution> Solutions { get; private set; }

			public override string ToString()
			{
				return Word;
			}
		}

		internal class SolutionPath
		{
			public SolutionPath(List<bool> cells, Color color)
			{
				Cells = cells;
				Color = color;
			}

			public Color Color { get; private set; }

			public List<bool> Cells { get; private set; }
		}

		#endregion
	}
}
