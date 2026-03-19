using System.Collections.ObjectModel;
using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Services;

namespace CollectionManagementSystem.ViewModels;

public sealed class ItemDetailViewModel : BaseViewModel {
	private readonly ICollectionRepository _repository;
	private readonly INavigationService _navigationService;
	private readonly FileStorageService _storageService;
	private string _collectionId = string.Empty;
	private string _itemId = string.Empty;
	private string _name = string.Empty;
	private string _status = string.Empty;
	private string _price = string.Empty;
	private string _rating = string.Empty;
	private string _comment = string.Empty;
	private string _imagePath = string.Empty;

	public ItemDetailViewModel(
		ICollectionRepository repository,
		INavigationService navigationService,
		FileStorageService storageService) {
		_repository = repository;
		_navigationService = navigationService;
		_storageService = storageService;

		EditCommand = TrackCommand(new Command(async () => await EditAsync(), () => !IsBusy));
	}

	public ObservableCollection<CustomFieldDisplay> CustomFields { get; } = new();

	public string Name {
		get => _name;
		private set => SetProperty(ref _name, value);
	}

	public string Status {
		get => _status;
		private set => SetProperty(ref _status, value);
	}

	public string Price {
		get => _price;
		private set => SetProperty(ref _price, value);
	}

	public string Rating {
		get => _rating;
		private set => SetProperty(ref _rating, value);
	}

	public string Comment {
		get => _comment;
		private set => SetProperty(ref _comment, value);
	}

	public string ImagePath {
		get => _imagePath;
		private set => SetProperty(ref _imagePath, value);
	}

	public Command EditCommand { get; }

	public async Task InitializeAsync(string? collectionId, string? itemId) {
		_collectionId = collectionId ?? string.Empty;
		_itemId = itemId ?? string.Empty;
		CustomFields.Clear();

		if (string.IsNullOrWhiteSpace(_collectionId) || string.IsNullOrWhiteSpace(_itemId)) {
			return;
		}

		await RunBusyAsync(async () => {
			var collection = await _repository.GetCollectionAsync(_collectionId);
			var item = collection?.Items.FirstOrDefault(i => string.Equals(i.Id, _itemId, StringComparison.OrdinalIgnoreCase));
			if (collection is null || item is null) {
				return;
			}

			Title = item.Name;
			Name = item.Name;
			Status = item.Status.ToPolish();
			Price = $"{item.Price:0.00} zł";
			Rating = $"{item.Rating}/10";
			Comment = string.IsNullOrWhiteSpace(item.Comment) ? "Brak" : item.Comment;
			ImagePath = string.IsNullOrWhiteSpace(item.ImagePath)
				? string.Empty
				: _storageService.ToAbsolutePath(item.ImagePath);

			foreach (var field in item.CustomFields) {
				var columnName = collection.CustomColumns.FirstOrDefault(column =>
					string.Equals(column.Id, field.ColumnId, StringComparison.OrdinalIgnoreCase))?.Name ?? field.ColumnId;

				CustomFields.Add(new CustomFieldDisplay {
					Name = columnName,
					Value = field.Value
				});
			}
		});
	}

	private async Task EditAsync() {
		await _navigationService.NavigateToAsync(nameof(Views.AddEditItemPage), new Dictionary<string, object> {
			[NavigationKeys.CollectionId] = _collectionId,
			[NavigationKeys.ItemId] = _itemId
		});
	}
}

public sealed class CustomFieldDisplay {
	public string Name { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
}
