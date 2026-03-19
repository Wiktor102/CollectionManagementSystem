using System.Collections.ObjectModel;
using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Services;

namespace CollectionManagementSystem.ViewModels;

public sealed class AddEditItemViewModel : BaseViewModel {
	private readonly ICollectionRepository _repository;
	private readonly INavigationService _navigationService;
	private readonly FileStorageService _storageService;
	private string _collectionId = string.Empty;
	private string _itemId = string.Empty;
	private string _name = string.Empty;
	private string _priceInput = "0";
	private int _rating = 5;
	private string _comment = string.Empty;
	private string _imagePath = string.Empty;
	private string _imagePreviewPath = string.Empty;
	private bool _isEditMode;
	private StatusOption? _selectedStatus;
	private Collection? _collection;

	public AddEditItemViewModel(
		ICollectionRepository repository,
		INavigationService navigationService,
		FileStorageService storageService) {
		_repository = repository;
		_navigationService = navigationService;
		_storageService = storageService;

		StatusOptions = new ObservableCollection<StatusOption>(new[] {
			new StatusOption { Status = ItemStatus.Owned, Label = "Posiadane" },
			new StatusOption { Status = ItemStatus.Used, Label = "Używane" },
			new StatusOption { Status = ItemStatus.New, Label = "Nowe" },
			new StatusOption { Status = ItemStatus.ForSale, Label = "Na sprzedaż" },
			new StatusOption { Status = ItemStatus.Sold, Label = "Sprzedane" },
			new StatusOption { Status = ItemStatus.WantToBuy, Label = "Chcę kupić" }
		});
		SelectedStatus = StatusOptions.First();

		SaveCommand = TrackCommand(new Command(async () => await SaveAsync(), () => !IsBusy));
		CancelCommand = TrackCommand(new Command(async () => await _navigationService.GoBackAsync(), () => !IsBusy));
		PickImageCommand = TrackCommand(new Command(async () => await PickImageAsync(), () => !IsBusy));
	}

	public ObservableCollection<StatusOption> StatusOptions { get; }
	public ObservableCollection<CustomFieldEditorViewModel> CustomFieldEditors { get; } = new();

	public string Name {
		get => _name;
		set => SetProperty(ref _name, value);
	}

	public string PriceInput {
		get => _priceInput;
		set => SetProperty(ref _priceInput, value);
	}

	public int Rating {
		get => _rating;
		set => SetProperty(ref _rating, Math.Clamp(value, 1, 10));
	}

	public string Comment {
		get => _comment;
		set => SetProperty(ref _comment, value);
	}

	public string ImagePath {
		get => _imagePath;
		set {
			if (SetProperty(ref _imagePath, value)) {
				ImagePreviewPath = string.IsNullOrWhiteSpace(_imagePath)
					? string.Empty
					: _storageService.ToAbsolutePath(_imagePath);
			}
		}
	}

	public string ImagePreviewPath {
		get => _imagePreviewPath;
		private set => SetProperty(ref _imagePreviewPath, value);
	}

	public bool IsEditMode {
		get => _isEditMode;
		private set => SetProperty(ref _isEditMode, value);
	}

	public StatusOption? SelectedStatus {
		get => _selectedStatus;
		set => SetProperty(ref _selectedStatus, value);
	}

	public Command SaveCommand { get; }
	public Command CancelCommand { get; }
	public Command PickImageCommand { get; }

	public async Task InitializeAsync(string? collectionId, string? itemId) {
		_collectionId = collectionId ?? string.Empty;
		_itemId = itemId ?? string.Empty;

		_collection = await _repository.GetCollectionAsync(_collectionId);
		if (_collection is null) {
			return;
		}

		CustomFieldEditors.Clear();
		foreach (var column in _collection.CustomColumns) {
			CustomFieldEditors.Add(new CustomFieldEditorViewModel {
				Column = new CustomColumn {
					Id = column.Id,
					Name = column.Name,
					Type = column.Type,
					AllowedValues = column.AllowedValues.ToList()
				}
			});
		}

		if (string.IsNullOrWhiteSpace(_itemId)) {
			IsEditMode = false;
			Title = "Dodaj element";
			Name = string.Empty;
			PriceInput = "0";
			Rating = 5;
			Comment = string.Empty;
			ImagePath = string.Empty;
			SelectedStatus = StatusOptions.First();
			return;
		}

		IsEditMode = true;
		Title = "Edytuj element";

		var item = _collection.Items.FirstOrDefault(i => string.Equals(i.Id, _itemId, StringComparison.OrdinalIgnoreCase));
		if (item is null) {
			return;
		}

		Name = item.Name;
		PriceInput = item.Price.ToString("0.00");
		Rating = item.Rating;
		Comment = item.Comment;
		ImagePath = item.ImagePath;
		SelectedStatus = StatusOptions.FirstOrDefault(option => option.Status == item.Status) ?? StatusOptions.First();

		foreach (var editor in CustomFieldEditors) {
			var value = item.CustomFields.FirstOrDefault(field =>
				string.Equals(field.ColumnId, editor.ColumnId, StringComparison.OrdinalIgnoreCase));
			editor.Value = value?.Value ?? string.Empty;
		}
	}

	private async Task PickImageAsync() {
		var itemIdForImage = string.IsNullOrWhiteSpace(_itemId) ? Guid.NewGuid().ToString() : _itemId;
		var savedRelativePath = await ImageHelper.PickAndStoreImageAsync(_storageService, itemIdForImage);
		if (!string.IsNullOrWhiteSpace(savedRelativePath)) {
			ImagePath = savedRelativePath;
			_itemId = itemIdForImage;
		}
	}

	private async Task SaveAsync() {
		if (_collection is null) {
			return;
		}

		if (string.IsNullOrWhiteSpace(Name)) {
			await Shell.Current.DisplayAlertAsync("Brak nazwy", "Nazwa elementu jest wymagana.", "OK");
			return;
		}

		if (!decimal.TryParse(PriceInput.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price)) {
			await Shell.Current.DisplayAlertAsync("Błędna cena", "Podaj poprawną wartość ceny.", "OK");
			return;
		}

		foreach (var editor in CustomFieldEditors.Where(e => e.IsNumber && !string.IsNullOrWhiteSpace(e.Value))) {
			if (!decimal.TryParse(editor.Value.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _)) {
				await Shell.Current.DisplayAlertAsync("Błędna wartość", $"Pole '{editor.ColumnName}' wymaga liczby.", "OK");
				return;
			}
		}

		var duplicateExists = await _repository.HasDuplicateItemNameAsync(_collectionId, Name, IsEditMode ? _itemId : null);
		if (duplicateExists) {
			var proceed = await Shell.Current.DisplayAlertAsync(
				"Duplikat nazwy",
				"Element o tej nazwie już istnieje. Czy chcesz zapisać mimo to?",
				"Kontynuuj",
				"Anuluj");

			if (!proceed) {
				return;
			}
		}

		await RunBusyAsync(async () => {
			var item = new CollectionItem {
				Id = string.IsNullOrWhiteSpace(_itemId) ? Guid.NewGuid().ToString() : _itemId,
				Name = Name.Trim(),
				Status = SelectedStatus?.Status ?? ItemStatus.Owned,
				Price = price,
				Rating = Math.Clamp(Rating, 1, 10),
				Comment = Comment.Trim(),
				ImagePath = ImagePath,
				CustomFields = CustomFieldEditors
					.Where(editor => !string.IsNullOrWhiteSpace(editor.Value))
					.Select(editor => new CustomFieldValue {
						ColumnId = editor.ColumnId,
						Value = editor.Value.Trim()
					})
					.ToList()
			};

			await _repository.UpsertItemAsync(_collectionId, item);
			await _navigationService.GoBackAsync();
		});
	}
}
