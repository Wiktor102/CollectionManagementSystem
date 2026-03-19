namespace CollectionManagementSystem.Models;

public sealed class CollectionIndexEntry {
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.Today;
}
