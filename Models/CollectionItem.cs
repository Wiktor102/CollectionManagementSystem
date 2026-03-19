namespace CollectionManagementSystem.Models;

public sealed class CollectionItem {
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string Name { get; set; } = string.Empty;
	public ItemStatus Status { get; set; } = ItemStatus.Owned;
	public decimal Price { get; set; }
	public int Rating { get; set; } = 1;
	public string Comment { get; set; } = string.Empty;
	public string ImagePath { get; set; } = string.Empty;
	public List<CustomFieldValue> CustomFields { get; set; } = new();
}
