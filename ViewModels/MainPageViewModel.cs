using System.Collections.ObjectModel;
using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.ViewModels;

public sealed class MainPageViewModel : BaseViewModel {
	private readonly ICollectionRepository _repository;
	private readonly INavigationService _navigationService;

	public MainPageViewModel(ICollectionRepository repository, INavigationService navigationService) {
		_repository = repository;
		_navigationService = navigationService;
		Title = "Moje kolekcje";
		TitleBar.Subtitle = "Zarządzaj kolekcjami i elementami w jednym miejscu.";

		RefreshCommand = TrackCommand(new Command(async () => await InitializeAsync(), () => !IsBusy));
		OpenCollectionCommand = TrackCommand(new Command<Collection>(async collection => await OpenCollectionAsync(collection), _ => !IsBusy));
		AddCollectionCommand = TrackCommand(new Command(async () => await AddCollectionAsync(), () => !IsBusy));
		EditCollectionCommand = TrackCommand(new Command<Collection>(async collection => await EditCollectionAsync(collection), _ => !IsBusy));
		DeleteCollectionCommand = TrackCommand(new Command<Collection>(async collection => await DeleteCollectionAsync(collection), _ => !IsBusy));
		TitleBar.Actions.Add(new TitleBarAction("Dodaj kolekcję", AddCollectionCommand));
	}

	public ObservableCollection<Collection> Collections { get; } = new();

	public Command RefreshCommand { get; }
	public Command<Collection> OpenCollectionCommand { get; }
	public Command AddCollectionCommand { get; }
	public Command<Collection> EditCollectionCommand { get; }
	public Command<Collection> DeleteCollectionCommand { get; }

	public async Task InitializeAsync() {
		await RunBusyAsync(async () => {
			Collections.Clear();
			var collections = await _repository.GetCollectionsAsync();
			foreach (var collection in collections) {
				Collections.Add(collection);
			}
		});
	}

	private async Task OpenCollectionAsync(Collection? collection) {
		if (collection is null) {
			return;
		}

		await _navigationService.NavigateToAsync(nameof(Views.CollectionListPage), new Dictionary<string, object> {
			[NavigationKeys.CollectionId] = collection.Id
		});
	}

	private async Task AddCollectionAsync() {
		await _navigationService.NavigateToAsync(nameof(Views.AddEditCollectionPage));
	}

	private async Task EditCollectionAsync(Collection? collection) {
		if (collection is null) {
			return;
		}

		await _navigationService.NavigateToAsync(nameof(Views.AddEditCollectionPage), new Dictionary<string, object> {
			[NavigationKeys.CollectionId] = collection.Id
		});
	}

	private async Task DeleteCollectionAsync(Collection? collection) {
		if (collection is null) {
			return;
		}

		var confirm = await Shell.Current.DisplayAlertAsync(
			"Usuwanie kolekcji",
			$"Czy na pewno chcesz usunąć kolekcję '{collection.Name}'?",
			"Usuń",
			"Anuluj");

		if (!confirm) {
			return;
		}

		await RunBusyAsync(async () => {
			await _repository.DeleteCollectionAsync(collection.Id);
			Collections.Remove(collection);
		});
	}
}
