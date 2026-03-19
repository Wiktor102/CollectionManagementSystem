using System.Globalization;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.Converters;

public sealed class StatusToColorConverter : IValueConverter {
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not ItemStatus status) {
			return Colors.Gray;
		}

		return status switch {
			ItemStatus.Owned => ResolveColor("StatusOwnedColor", "#1A6B6B"),
			ItemStatus.Used => ResolveColor("StatusUsedColor", "#D97B29"),
			ItemStatus.New => ResolveColor("StatusNewColor", "#2B7A4B"),
			ItemStatus.ForSale => ResolveColor("StatusForSaleColor", "#E85D4A"),
			ItemStatus.Sold => ResolveColor("StatusSoldColor", "#6B6B6B"),
			ItemStatus.WantToBuy => ResolveColor("StatusWantToBuyColor", "#3B4C8F"),
			_ => Colors.Gray
		};
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}

	private static Color ResolveColor(string key, string fallback) {
		if (Application.Current?.Resources.TryGetValue(key, out var resource) == true && resource is Color color) {
			return color;
		}

		return Color.FromArgb(fallback);
	}
}
