using System.Globalization;

namespace CollectionManagementSystem.Converters;

public sealed class ImagePathToSourceConverter : IValueConverter {
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		var path = value?.ToString();
		if (string.IsNullOrWhiteSpace(path)) {
			return "dotnet_bot.png";
		}

		if (Path.IsPathRooted(path)) {
			return path;
		}

		var fullPath = Path.Combine(
			FileSystem.AppDataDirectory,
			"CollectionManagementSystem",
			path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar));

		return File.Exists(fullPath) ? fullPath : "dotnet_bot.png";
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}
