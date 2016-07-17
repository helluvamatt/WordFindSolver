using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace WordFindSolver
{
	/// <summary>
	/// Interaction logic for NumericUpDown.xaml
	/// </summary>
	public partial class NumericUpDown : UserControl
	{
		public NumericUpDown()
		{
			InitializeComponent();
		}

		#region Event handlers

		private static object OnCoerceValueProperty(DependencyObject source, object data)
		{
			NumericUpDown control = source as NumericUpDown;
			int newValue = (int)data;
			if (newValue > control.MaxValue) newValue = control.MaxValue;
			if (newValue < control.MinValue) newValue = control.MinValue;
			return newValue;
		}

		private static void OnMinValuePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			NumericUpDown control = source as NumericUpDown;
			int newMinValue = (int)e.NewValue;
			if (control.Value < newMinValue) control.Value = newMinValue;
		}

		private static void OnMaxValuePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			NumericUpDown control = source as NumericUpDown;
			int newMaxValue = (int)e.NewValue;
			if (control.Value > newMaxValue) control.Value = newMaxValue;
		}

		private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
		{
			if (e.DataObject.GetDataPresent(typeof(string)))
			{
				string text = (string)e.DataObject.GetData(typeof(string));
				if (!IsNumeric(text))
				{
					e.CancelCommand();
				}
			}
			else
			{
				e.CancelCommand();
			}
		}

		private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = !IsNumeric(e.Text);
		}

		private void upButton_Click(object sender, RoutedEventArgs e)
		{
			Value++;
		}

		private void downButton_Click(object sender, RoutedEventArgs e)
		{
			Value--;
		}

		#endregion

		#region Utility methods

		private bool IsNumeric(string text)
		{
			long val;
			return IsNumeric(text, out val);
		}

		private bool IsNumeric(string text, out long val)
		{
			return long.TryParse(text, out val);
		}

		#endregion

		#region Dependency properties

		public static readonly DependencyProperty ValueDependencyProperty = DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(0, null, OnCoerceValueProperty) { BindsTwoWayByDefault = true });
		public static readonly DependencyProperty MinValueDependencyProperty = DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(int.MinValue, OnMinValuePropertyChanged));
		public static readonly DependencyProperty MaxValueDependencyProperty = DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(int.MaxValue, OnMaxValuePropertyChanged));

		public int Value
		{
			get
			{
				return (int)GetValue(ValueDependencyProperty);
			}
			set
			{
				SetValue(ValueDependencyProperty, value);
			}
		}

		public int MinValue
		{
			get
			{
				return (int)GetValue(MinValueDependencyProperty);
			}
			set
			{
				SetValue(MinValueDependencyProperty, value);
			}
		}

		public int MaxValue
		{
			get
			{
				return (int)GetValue(MaxValueDependencyProperty);
			}
			set
			{
				SetValue(MaxValueDependencyProperty, value);
			}
		}

		#endregion
	}
}
