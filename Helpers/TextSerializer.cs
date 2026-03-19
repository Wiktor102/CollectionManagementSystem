using System.Globalization;
using System.Text;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.Helpers;

public static class TextSerializer {
	public static string SerializeIndex(IEnumerable<Collection> collections) {
		var sb = new StringBuilder();
		sb.AppendLine("[INDEX]");

		foreach (var collection in collections) {
			sb.AppendLine(string.Join("|",
				"ENTRY",
				collection.Id,
				Clean(collection.Name),
				collection.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
		}

		sb.AppendLine("[/INDEX]");
		return sb.ToString();
	}

	public static string SerializeCollection(Collection collection) {
		var sb = new StringBuilder();
		sb.AppendLine("[COLLECTION_META]");
		sb.AppendLine($"NAME={Clean(collection.Name)}");
		sb.AppendLine($"TYPE={Clean(collection.Type)}");
		sb.AppendLine($"CREATED={collection.CreatedAt:yyyy-MM-dd}");
		sb.AppendLine("[/COLLECTION_META]");
		sb.AppendLine();
		sb.AppendLine("[CUSTOM_COLUMNS]");

		foreach (var column in collection.CustomColumns) {
			if (column.Type == CustomColumnType.ValueSet) {
				var values = string.Join("~", column.AllowedValues.Select(Clean));
				sb.AppendLine($"COLUMN|{Clean(column.Name)}|VALUES|{values}");
			}
			else {
				sb.AppendLine($"COLUMN|{Clean(column.Name)}|{column.Type.ToString().ToUpperInvariant()}");
			}
		}

		sb.AppendLine("[/CUSTOM_COLUMNS]");
		sb.AppendLine();
		sb.AppendLine("[ITEMS]");

		var columnNameById = collection.CustomColumns
			.Where(c => !string.IsNullOrWhiteSpace(c.Id))
			.GroupBy(c => c.Id, StringComparer.OrdinalIgnoreCase)
			.ToDictionary(g => g.Key, g => g.First().Name, StringComparer.OrdinalIgnoreCase);

		foreach (var item in collection.Items) {
			sb.AppendLine("[ITEM]");
			sb.AppendLine($"ID={item.Id}");
			sb.AppendLine($"NAME={Clean(item.Name)}");
			sb.AppendLine($"STATUS={ToStatusToken(item.Status)}");
			sb.AppendLine($"PRICE={item.Price.ToString("0.00", CultureInfo.InvariantCulture)}");
			sb.AppendLine($"RATING={Math.Clamp(item.Rating, 1, 10)}");
			sb.AppendLine($"COMMENT={Clean(item.Comment)}");
			sb.AppendLine($"IMAGE={Clean(item.ImagePath)}");

			foreach (var customField in item.CustomFields) {
				if (string.IsNullOrWhiteSpace(customField.Value)) {
					continue;
				}

				var columnName = columnNameById.TryGetValue(customField.ColumnId, out var foundName)
					? foundName
					: customField.ColumnId;

				sb.AppendLine($"CUSTOM|{Clean(columnName)}={Clean(customField.Value)}");
			}

			sb.AppendLine("[/ITEM]");
		}

		sb.AppendLine("[/ITEMS]");
		return sb.ToString();
	}

	private static string Clean(string? value) {
		if (string.IsNullOrEmpty(value)) {
			return string.Empty;
		}

		return value.Replace("\r", " ").Replace("\n", " ").Trim();
	}

	private static string ToStatusToken(ItemStatus status) {
		return status switch {
			ItemStatus.Owned => "OWNED",
			ItemStatus.Used => "USED",
			ItemStatus.New => "NEW",
			ItemStatus.ForSale => "FOR_SALE",
			ItemStatus.Sold => "SOLD",
			ItemStatus.WantToBuy => "WANT_TO_BUY",
			_ => "OWNED"
		};
	}
}
