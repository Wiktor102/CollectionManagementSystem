using System.Collections.ObjectModel;
using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.ViewModels;

public sealed class CollectionListViewModel : BaseViewModel {
	private readonly ICollectionRepository _repository;
	private readonly INavigationService _navigationService;
	private Collection? _currentCollection;

	public CollectionListViewModel(ICollectionRepository repository, INavigationService navigationService) {
		_repository = repository;
		_navigationService = navigationService;

		RefreshCommand = TrackCommand(new Command(async () => await RefreshAsync(), () => !IsBusy));
		OpenItemCommand = TrackCommand(new Command<CollectionItem>(async item => await OpenItemAsync(item), _ => !IsBusy));
		AddItemCommand = TrackCommand(new Command(async () => await AddItemAsync(), () => !IsBusy));
		OpenSummaryCommand = TrackCommand(new Command(async () => await OpenSummaryAsync(), () => !IsBusy));
		ExportCommand = TrackCommand(new Command(async () => await ExportAsync(), () => !IsBusy));
		ImportCommand = TrackCommand(new Command(async () => await ImportAsync(), () => !IsBusy));
		ManageColumnsCommand = TrackCommand(new Command(async () => await ManageColumnsAsync(), () => !IsBusy));
	}

	public ObservableCollection<CollectionItem> SortedItems { get; } = new();

	public Collection? CurrentCollection {
		get => _currentCollection;
		private set {
			if (SetProperty(ref _currentCollection, value)) {
				Title = value is null ? "Kolekcja" : $"{value.Name} ({value.Type})";
			}
		}
	}

	public string CollectionId { get; private set; } = string.Empty;

	public Command RefreshCommand { get; }
	public Command<CollectionItem> OpenItemCommand { get; }
	public Command AddItemCommand { get; }
	public Command OpenSummaryCommand { get; }
	public Command ExportCommand { get; }
	public Command ImportCommand { get; }
	public Command ManageColumnsCommand { get; }

	public async Task InitializeAsync(string? collectionId) {
		CollectionId = collectionId ?? string.Empty;
		await RefreshAsync();
	}

	public async Task RefreshAsync() {
		if (string.IsNullOrWhiteSpace(CollectionId)) {
			return;
		}

		await RunBusyAsync(async () => {
			CurrentCollection = await _repository.GetCollectionAsync(CollectionId);
			RebuildSortedItems();
		});
	}

	public void RebuildSortedItems() {
		SortedItems.Clear();
		if (CurrentCollection is null) {
			return;
		}

		var sorted = CurrentCollection.Items
			.OrderBy(item => item.Status == ItemStatus.Sold ? 1 : 0)
			.ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
			.ToList();

		foreach (var item in sorted) {
			SortedItems.Add(item);
		}
	}

	private async Task OpenItemAsync(CollectionItem? item) {
		if (item is null || CurrentCollection is null) {
			return;
		}

		await _navigationService.NavigateToAsync(nameof(Views.ItemDetailPage), new Dictionary<string, object> {
			[NavigationKeys.CollectionId] = CurrentCollection.Id,
			[NavigationKeys.ItemId] = item.Id
		});
	}

	private async Task AddItemAsync() {
		if (CurrentCollection is null) {
			return;
		}

		await _navigationService.NavigateToAsync(nameof(Views.AddEditItemPage), new Dictionary<string, object> {
			[NavigationKeys.CollectionId] = CurrentCollection.Id
		});
	}

	private async Task OpenSummaryAsync() {
		if (CurrentCollection is null) {
			return;
		}

		await _navigationService.NavigateToAsync(nameof(Views.SummaryPage), new Dictionary<string, object> {
			[NavigationKeys.CollectionId] = CurrentCollection.Id
		});
	}

	private async Task ManageColumnsAsync() {
		if (CurrentCollection is null) {
			return;
		}

		await _navigationService.NavigateToAsync(nameof(Views.AddEditCollectionPage), new Dictionary<string, object> {
			[NavigationKeys.CollectionId] = CurrentCollection.Id
		});
	}

	private async Task ExportAsync() {
		if (CurrentCollection is null) {
			return;
		}

		var selectedFolder = await Shell.Current.DisplayPromptAsync(
			"Eksport",
			"Podaj folder docelowy eksportu:",
			"OK",
			"Anuluj",
			initialValue: Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

		if (string.IsNullOrWhiteSpace(selectedFolder)) {
			return;
		}

		if (!Directory.Exists(selectedFolder)) {
			Directory.CreateDirectory(selectedFolder);
		}

		await RunBusyAsync(async () => {
			var exportPath = await _repository.ExportCollectionAsync(CurrentCollection.Id, selectedFolder);
			await Shell.Current.DisplayAlertAsync("Eksport zakończony", $"Wyeksportowano do: {exportPath}", "OK");
		});
	}

	private async Task ImportAsync() {
		if (CurrentCollection is null) {
			return;
		}

		var pickedFile = await FilePicker.Default.PickAsync(new PickOptions {
			PickerTitle = "Wybierz plik kolekcji do importu",
			FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> {
				[DevicePlatform.WinUI] = new[] { ".txt" }
			})
		});

		if (pickedFile is null) {
			return;
		}

		await RunBusyAsync(async () => {
			await _repository.ImportIntoCollectionAsync(CurrentCollection.Id, pickedFile.FullPath, async duplicateItemName => {
				return await Shell.Current.DisplayAlertAsync(
					"Duplikat podczas importu",
					$"Element '{duplicateItemName}' już istnieje. Nadpisać?",
					"Nadpisz",
					"Pomiń");
			});

			await RefreshAsync();
			await Shell.Current.DisplayAlertAsync("Import zakończony", "Import danych został wykonany.", "OK");
		});
	}
}
