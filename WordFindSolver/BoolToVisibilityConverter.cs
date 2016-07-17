using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WordFindSolver
{
	[ValueConversion(typeof(bool), typeof(Visibility))]
	internal class BoolToVisibilityConverter : IValueConverter
	{
		public BoolToVisibilityConverter()
		{
			TrueVisibility = Visibility.Visible;
			FalseVisibility = Visibility.Hidden;
		}

		public Visibility TrueVisibility { get; set; }
		public Visibility FalseVisibility { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
			{
				return (bool)value ? TrueVisibility : FalseVisibility;
			}
			return FalseVisibility;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is Visibility && ((Visibility)value) == TrueVisibility;
		}
	}
}
