using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.ViewModels;

public sealed class StatusOption {
	public required ItemStatus Status { get; init; }
	public required string Label { get; init; }
}
