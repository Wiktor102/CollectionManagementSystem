using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.Helpers;

public static class StatusTextHelper {
	public static string ToPolish(this ItemStatus status) {
		return status switch {
			ItemStatus.Owned => "Posiadane",
			ItemStatus.Used => "Uzywane",
			ItemStatus.New => "Nowe",
			ItemStatus.ForSale => "Na sprzedaz",
			ItemStatus.Sold => "Sprzedane",
			ItemStatus.WantToBuy => "Chce kupic",
			_ => "Posiadane"
		};
	}
}
