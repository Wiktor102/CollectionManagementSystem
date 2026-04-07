namespace CollectionManagementSystem.Models;

public sealed class CustomColumn {
	public string Id { get; private set; } = string.Empty;
	public string Name { get; private set; } = string.Empty;
	public CustomColumnType Type { get; private set; } = CustomColumnType.Text;
	public List<string> AllowedValues { get; private set; } = [];

	public CustomColumn(string id, string name, CustomColumnType type = CustomColumnType.Text, IEnumerable<string>? allowedValues = null) {
		Id = id ?? string.Empty;
		Name = name ?? string.Empty;
		Type = type;
		AllowedValues = allowedValues is null ? [] : allowedValues.ToList();
	}

	public string DisplayTypeName => Type switch {
		CustomColumnType.Text => "Tekst",
		CustomColumnType.Number => "Liczba",
		CustomColumnType.ValueSet => "Zestaw wartości",
		_ => Type.ToString()
	};

	public string DisplayTypeAndValues {
		get {
			if (Type == CustomColumnType.ValueSet && AllowedValues != null && AllowedValues.Any()) {
				return $"Zestaw wartości: {string.Join(", ", AllowedValues)}";
			}

			return DisplayTypeName;
		}
	}

	public static string BuildUniqueColumnId(string sourceName, IEnumerable<CustomColumn> existingColumns) {
		var baseId = string.Concat(sourceName.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '_')).Trim('_');
		if (string.IsNullOrWhiteSpace(baseId)) {
			baseId = "column";
		}

		var existingIds = existingColumns
			.Select(column => column.Id)
			.Where(id => !string.IsNullOrWhiteSpace(id))
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

		var candidate = baseId;
		var index = 1;
		while (existingIds.Contains(candidate)) {
			index++;
			candidate = $"{baseId}_{index}";
		}

		return candidate;
	}
}
