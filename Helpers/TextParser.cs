using System.Globalization;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.Helpers;

public static class TextParser {
	private enum ParserState {
		Idle,
		ReadingMeta,
		ReadingColumns,
		ReadingItems,
		ReadingItem
	}

	public static List<CollectionIndexEntry> ParseIndex(string text) {
		var entries = new List<CollectionIndexEntry>();
		var inIndex = false;

		foreach (var rawLine in SplitLines(text)) {
			var line = rawLine.Trim();
			if (string.IsNullOrWhiteSpace(line)) {
				continue;
			}

			if (line.Equals("[INDEX]", StringComparison.OrdinalIgnoreCase)) {
				inIndex = true;
				continue;
			}

			if (line.Equals("[/INDEX]", StringComparison.OrdinalIgnoreCase)) {
				break;
			}

			if (!inIndex || !line.StartsWith("ENTRY|", StringComparison.OrdinalIgnoreCase)) {
				continue;
			}

			var parts = line.Split('|');
			if (parts.Length < 4) {
				continue;
			}

			entries.Add(new CollectionIndexEntry {
				Id = parts[1].Trim(),
				Name = parts[2].Trim(),
				CreatedAt = ParseDate(parts[3])
			});
		}

		return entries;
	}

	public static Collection ParseCollection(string text, string collectionId) {
		var collection = new Collection { Id = collectionId };
		var state = ParserState.Idle;
		CollectionItem? currentItem = null;

		foreach (var rawLine in SplitLines(text)) {
			var line = rawLine.Trim();
			if (string.IsNullOrWhiteSpace(line)) {
				continue;
			}

			if (line.StartsWith('[') && line.EndsWith(']')) {
				switch (line.ToUpperInvariant()) {
					case "[COLLECTION_META]":
						state = ParserState.ReadingMeta;
						break;
					case "[/COLLECTION_META]":
						state = ParserState.Idle;
						break;
					case "[CUSTOM_COLUMNS]":
						state = ParserState.ReadingColumns;
						break;
					case "[/CUSTOM_COLUMNS]":
						state = ParserState.Idle;
						break;
					case "[ITEMS]":
						state = ParserState.ReadingItems;
						break;
					case "[/ITEMS]":
						state = ParserState.Idle;
						break;
					case "[ITEM]":
						currentItem = new CollectionItem();
						state = ParserState.ReadingItem;
						break;
					case "[/ITEM]":
						if (currentItem is not null) {
							if (string.IsNullOrWhiteSpace(currentItem.Id)) {
								currentItem.Id = Guid.NewGuid().ToString();
							}

							collection.Items.Add(currentItem);
						}

						currentItem = null;
						state = ParserState.ReadingItems;
						break;
				}

				continue;
			}

			if (state == ParserState.ReadingMeta && line.Contains('=') && !line.StartsWith('|')) {
				ReadMetaKeyValue(collection, line);
				continue;
			}

			if (state == ParserState.ReadingColumns && line.StartsWith("COLUMN|", StringComparison.OrdinalIgnoreCase)) {
				var column = ParseColumn(line, collection.CustomColumns);
				if (column is not null) {
					collection.CustomColumns.Add(column);
				}

				continue;
			}

			if (state == ParserState.ReadingItem && currentItem is not null) {
				if (line.StartsWith("CUSTOM|", StringComparison.OrdinalIgnoreCase)) {
					ReadCustomField(collection, currentItem, line);
				}
				else if (line.Contains('=') && !line.StartsWith('|')) {
					ReadItemKeyValue(currentItem, line);
				}
			}
		}

		if (collection.CreatedAt == default) {
			collection.CreatedAt = DateTime.Today;
		}

		return collection;
	}

	private static IEnumerable<string> SplitLines(string text) {
		return text.Replace("\r", string.Empty).Split('\n', StringSplitOptions.None);
	}

	private static DateTime ParseDate(string value) {
		if (DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) {
			return parsed;
		}

		return DateTime.Today;
	}

	private static void ReadMetaKeyValue(Collection collection, string line) {
		var split = line.Split('=', 2);
		if (split.Length != 2) {
			return;
		}

		var key = split[0].Trim().ToUpperInvariant();
		var value = split[1].Trim();

		switch (key) {
			case "NAME":
				collection.Name = value;
				break;
			case "TYPE":
				collection.Type = value;
				break;
			case "CREATED":
				collection.CreatedAt = ParseDate(value);
				break;
		}
	}

	private static CustomColumn? ParseColumn(string line, IReadOnlyCollection<CustomColumn> existingColumns) {
		var parts = line.Split('|');
		if (parts.Length < 3) {
			return null;
		}

		var name = parts[1].Trim();
		if (string.IsNullOrWhiteSpace(name)) {
			return null;
		}

		var typeToken = parts[2].Trim().ToUpperInvariant();
		var type = typeToken switch {
			"TEXT" => CustomColumnType.Text,
			"NUMBER" => CustomColumnType.Number,
			"VALUESET" => CustomColumnType.ValueSet,
			"VALUES" => CustomColumnType.ValueSet,
			_ => CustomColumnType.Text
		};

		var allowedValues = new List<string>();
		if (type == CustomColumnType.ValueSet && parts.Length > 3) {
			allowedValues = parts[3]
				.Split('~', StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToList();
		}

		return new CustomColumn {
			Id = BuildUniqueColumnId(name, existingColumns),
			Name = name,
			Type = type,
			AllowedValues = allowedValues
		};
	}

	private static void ReadItemKeyValue(CollectionItem item, string line) {
		var split = line.Split('=', 2);
		if (split.Length != 2) {
			return;
		}

		var key = split[0].Trim().ToUpperInvariant();
		var value = split[1].Trim();

		switch (key) {
			case "ID":
				item.Id = value;
				break;
			case "NAME":
				item.Name = value;
				break;
			case "STATUS":
				item.Status = ParseStatus(value);
				break;
			case "PRICE":
				if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var price)) {
					item.Price = price;
				}
				break;
			case "RATING":
				if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rating)) {
					item.Rating = Math.Clamp(rating, 1, 10);
				}
				break;
			case "COMMENT":
				item.Comment = value;
				break;
			case "IMAGE":
				item.ImagePath = value;
				break;
		}
	}

	private static void ReadCustomField(Collection collection, CollectionItem item, string line) {
		var payload = line["CUSTOM|".Length..];
		var split = payload.Split('=', 2);
		if (split.Length != 2) {
			return;
		}

		var columnName = split[0].Trim();
		var value = split[1].Trim();

		var column = collection.CustomColumns.FirstOrDefault(c =>
			string.Equals(c.Name, columnName, StringComparison.OrdinalIgnoreCase));

		if (column is null) {
			column = new CustomColumn {
				Id = BuildUniqueColumnId(columnName, collection.CustomColumns),
				Name = columnName,
				Type = CustomColumnType.Text
			};
			collection.CustomColumns.Add(column);
		}

		item.CustomFields.Add(new CustomFieldValue {
			ColumnId = column.Id,
			Value = value
		});
	}

	private static ItemStatus ParseStatus(string token) {
		return token.Trim().ToUpperInvariant() switch {
			"OWNED" => ItemStatus.Owned,
			"USED" => ItemStatus.Used,
			"NEW" => ItemStatus.New,
			"FOR_SALE" => ItemStatus.ForSale,
			"SOLD" => ItemStatus.Sold,
			"WANT_TO_BUY" => ItemStatus.WantToBuy,
			_ => ItemStatus.Owned
		};
	}

	private static string BuildUniqueColumnId(string name, IReadOnlyCollection<CustomColumn> existingColumns) {
		var baseId = string.Concat(name.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '_')).Trim('_');
		if (string.IsNullOrWhiteSpace(baseId)) {
			baseId = "column";
		}

		var candidate = baseId;
		var index = 1;

		while (existingColumns.Any(c => string.Equals(c.Id, candidate, StringComparison.OrdinalIgnoreCase))) {
			index++;
			candidate = $"{baseId}_{index}";
		}

		return candidate;
	}
}
