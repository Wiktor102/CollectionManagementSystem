using System.Globalization;

namespace CollectionManagementSystem.Converters;

public sealed class RatingToDisplayConverter : IValueConverter {
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is int rating) {
			return $"Ocena: {Math.Clamp(rating, 1, 10)}/10";
		}

		if (int.TryParse(value?.ToString(), out var parsedRating)) {
			return $"Ocena: {Math.Clamp(parsedRating, 1, 10)}/10";
		}

		return "Ocena: -";
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}
