namespace CollectionManagementSystem.Models;

public sealed class CustomColumn {
	public string Id { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public CustomColumnType Type { get; set; } = CustomColumnType.Text;
	public List<string> AllowedValues { get; set; } = new();
}
