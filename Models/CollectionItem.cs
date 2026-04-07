namespace CollectionManagementSystem.Models;

public sealed class CollectionItem {
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string Name { get; set; } = string.Empty;
	public ItemStatus Status { get; set; } = ItemStatus.Owned;
	public string StatusLabel => Status switch {
		ItemStatus.Owned => "Posiadane",
		ItemStatus.Used => "Uzywane",
		ItemStatus.New => "Nowe",
		ItemStatus.ForSale => "Na sprzedaz",
		ItemStatus.Sold => "Sprzedane",
		ItemStatus.WantToBuy => "Chce kupic",
		_ => "Posiadane"
	};
	public string RatingDisplayText => $"Ocena: {Math.Clamp(Rating, 1, 10)}/10";
	public decimal Price { get; set; }
	public int Rating { get; set; } = 1;
	public string Comment { get; set; } = string.Empty;
	public string ImagePath { get; set; } = string.Empty;
	public bool HasImage => ImageSourcePath != null; // Nie mozemy bezposrednio z ImagePath, poniewaz sciezka moze być nieprawidlowa lub nieistniejaca
	public string? ImageSourcePath {
		get {
			if (string.IsNullOrWhiteSpace(ImagePath)) return null;
			if (Path.IsPathRooted(ImagePath)) return ImagePath;

			var fullPath = Path.Combine(
				FileSystem.AppDataDirectory,
				"CollectionManagementSystem",
				ImagePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)
			);

			return File.Exists(fullPath) ? fullPath : null;
		}
	}
	public List<CustomFieldValue> CustomFields { get; set; } = [];
}
