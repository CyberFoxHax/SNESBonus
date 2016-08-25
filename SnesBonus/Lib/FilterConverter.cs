using System;
using System.Globalization;
using System.Windows.Data;

/////
//  http://stackoverflow.com/a/24083259/1148434
/////
namespace SnesBonus.Lib{
	internal class IgnoreNewItemPlaceholderConverter : IValueConverter {
		public static readonly IgnoreNewItemPlaceholderConverter Instance = new IgnoreNewItemPlaceholderConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value != null && value.ToString() == "{NewItemPlaceholder}")
				return System.Windows.DependencyProperty.UnsetValue;
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}