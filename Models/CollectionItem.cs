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
	public decimal Price { get; set; }
	public int Rating { get; set; } = 1;
	public string Comment { get; set; } = string.Empty;
	public string ImagePath { get; set; } = string.Empty;
	public List<CustomFieldValue> CustomFields { get; set; } = new();
}
