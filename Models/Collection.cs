namespace CollectionManagementSystem.Models;

public sealed class Collection {
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string Name { get; set; } = string.Empty;
	public string Type { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.Today;
	public List<CollectionItem> Items { get; set; } = new();
	public List<CustomColumn> CustomColumns { get; set; } = new();
}
