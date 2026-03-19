using System.Globalization;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.Converters;

public sealed class StatusToOpacityConverter : IValueConverter {
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is ItemStatus status && status == ItemStatus.Sold) {
			return 0.4;
		}

		return 1.0;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}
