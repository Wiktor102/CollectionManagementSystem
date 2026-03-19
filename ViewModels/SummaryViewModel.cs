using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.ViewModels;

public sealed class SummaryViewModel : BaseViewModel {
	private readonly ICollectionRepository _repository;
	private int _totalItems;
	private int _ownedItems;
	private int _soldItems;
	private int _forSaleItems;
	private int _wantToBuyItems;
	private decimal _averageRating;
	private decimal _estimatedValue;

	public SummaryViewModel(ICollectionRepository repository) {
		_repository = repository;
	}

	public int TotalItems {
		get => _totalItems;
		private set => SetProperty(ref _totalItems, value);
	}

	public int OwnedItems {
		get => _ownedItems;
		private set => SetProperty(ref _ownedItems, value);
	}

	public int SoldItems {
		get => _soldItems;
		private set => SetProperty(ref _soldItems, value);
	}

	public int ForSaleItems {
		get => _forSaleItems;
		private set => SetProperty(ref _forSaleItems, value);
	}

	public int WantToBuyItems {
		get => _wantToBuyItems;
		private set => SetProperty(ref _wantToBuyItems, value);
	}

	public decimal AverageRating {
		get => _averageRating;
		private set => SetProperty(ref _averageRating, value);
	}

	public decimal EstimatedValue {
		get => _estimatedValue;
		private set => SetProperty(ref _estimatedValue, value);
	}

	public async Task InitializeAsync(string? collectionId) {
		if (string.IsNullOrWhiteSpace(collectionId)) {
			return;
		}

		await RunBusyAsync(async () => {
			var collection = await _repository.GetCollectionAsync(collectionId);
			if (collection is null) {
				return;
			}

			Title = $"Podsumowanie: {collection.Name}";
			var items = collection.Items;
			TotalItems = items.Count;
			OwnedItems = items.Count(item => item.Status is ItemStatus.Owned or ItemStatus.Used or ItemStatus.New);
			SoldItems = items.Count(item => item.Status == ItemStatus.Sold);
			ForSaleItems = items.Count(item => item.Status == ItemStatus.ForSale);
			WantToBuyItems = items.Count(item => item.Status == ItemStatus.WantToBuy);

			AverageRating = items.Count == 0
				? 0
				: Math.Round(items.Average(item => (decimal)item.Rating), 1, MidpointRounding.AwayFromZero);

			EstimatedValue = items
				.Where(item => item.Status is not ItemStatus.WantToBuy and not ItemStatus.Sold)
				.Sum(item => item.Price);
		});
	}
}
