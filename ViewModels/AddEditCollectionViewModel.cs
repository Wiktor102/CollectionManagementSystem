using System.Collections.ObjectModel;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.ViewModels;

public sealed partial class AddEditCollectionViewModel : BaseViewModel {
	private readonly ICollectionRepository _repository;
	private readonly INavigationService _navigationService;
	private string _collectionId = string.Empty;
	private string _collectionName = string.Empty;
	private string _collectionType = string.Empty;
	private bool _isEditMode;
	private string _newColumnName = string.Empty;
	private string _newAllowedValues = string.Empty;
	private int _newColumnTypeIndex = (int)CustomColumnType.Text;

	public AddEditCollectionViewModel(ICollectionRepository repository, INavigationService navigationService) {
		_repository = repository;
		_navigationService = navigationService;

		SaveCommand = TrackCommand(new Command(async () => await SaveAsync(), () => !IsBusy));
		CancelCommand = TrackCommand(new Command(async () => await _navigationService.GoBackAsync(), () => !IsBusy));
		AddColumnCommand = TrackCommand(new Command(async () => await AddColumnAsync(), () => !IsBusy));
		DeleteColumnCommand = TrackCommand(new Command<CustomColumn>(async column => await DeleteColumnAsync(column), _ => !IsBusy));
		MoveUpColumnCommand = TrackCommand(new Command<CustomColumn>(column => MoveColumn(column, -1), _ => !IsBusy));
		MoveDownColumnCommand = TrackCommand(new Command<CustomColumn>(column => MoveColumn(column, 1), _ => !IsBusy));
	}

	public ObservableCollection<CustomColumn> CustomColumns { get; } = [];

	public string CollectionId {
		get => _collectionId;
		set => SetProperty(ref _collectionId, value);
	}

	public string CollectionName {
		get => _collectionName;
		set => SetProperty(ref _collectionName, value);
	}

	public string CollectionType {
		get => _collectionType;
		set => SetProperty(ref _collectionType, value);
	}

	public bool IsEditMode {
		get => _isEditMode;
		set => SetProperty(ref _isEditMode, value);
	}

	public string NewColumnName {
		get => _newColumnName;
		set => SetProperty(ref _newColumnName, value);
	}

	public string NewAllowedValues {
		get => _newAllowedValues;
		set => SetProperty(ref _newAllowedValues, value);
	}

	public int NewColumnTypeIndex {
		get => _newColumnTypeIndex;
		set => SetProperty(ref _newColumnTypeIndex, value);
	}

	public IReadOnlyList<string> ColumnTypeNames { get; } = [
		"Tekst",
		"Liczba",
		"Zestaw wartości"
	];

	public Command SaveCommand { get; }
	public Command CancelCommand { get; }
	public Command AddColumnCommand { get; }
	public Command<CustomColumn> DeleteColumnCommand { get; }
	public Command<CustomColumn> MoveUpColumnCommand { get; }
	public Command<CustomColumn> MoveDownColumnCommand { get; }

	public async Task InitializeAsync(string? collectionId) {
		CollectionId = collectionId ?? string.Empty;
		CustomColumns.Clear();

		if (string.IsNullOrWhiteSpace(CollectionId)) {
			Title = "Nowa kolekcja";
			CollectionName = string.Empty;
			CollectionType = string.Empty;
			IsEditMode = false;
			return;
		}

		var collection = await _repository.GetCollectionAsync(CollectionId);
		if (collection is null) return; // Jesli kolekcja istnieje, whodzimy w tryb edycji.

		IsEditMode = true;
		Title = "Edycja kolekcji";
		CollectionName = collection.Name;
		CollectionType = collection.Type;

		foreach (var column in collection.CustomColumns) {
			CustomColumns.Add(new CustomColumn(column.Id, column.Name, column.Type, column.AllowedValues.ToList()));
		}
	}

	private async Task SaveAsync() {
		if (string.IsNullOrWhiteSpace(CollectionName)) {
			await Shell.Current.DisplayAlertAsync("Brak nazwy", "Podaj nazwę kolekcji.", "OK");
			return;
		}

		await RunBusyAsync(async () => {
			if (!IsEditMode) {
				var created = await _repository.CreateCollectionAsync(CollectionName, CollectionType);
				created.CustomColumns = CustomColumns.Select(CloneColumn).ToList();
				await _repository.SaveCollectionAsync(created);
			} else {
				var existing = await _repository.GetCollectionAsync(CollectionId);
				if (existing is null) return;

				existing.Name = CollectionName.Trim();
				existing.Type = CollectionType.Trim();
				existing.CustomColumns = CustomColumns.Select(CloneColumn).ToList();

				// Purge values for removed columns.
				var allowedColumnIds = existing.CustomColumns.Select(c => c.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
				foreach (var item in existing.Items) {
					item.CustomFields = item.CustomFields
						.Where(field => allowedColumnIds.Contains(field.ColumnId))
						.ToList();
				}

				await _repository.SaveCollectionAsync(existing);
			}

			await _navigationService.GoBackAsync();
		});
	}

	private async Task AddColumnAsync() {
		if (string.IsNullOrWhiteSpace(NewColumnName)) {
			await Shell.Current.DisplayAlertAsync("Brak nazwy", "Podaj nazwę kolumny.", "OK");
			return;
		}

		if (CustomColumns.Any(c => string.Equals(c.Name, NewColumnName.Trim(), StringComparison.OrdinalIgnoreCase))) {
			await Shell.Current.DisplayAlertAsync("Duplikat", "Kolumna o tej nazwie już istnieje.", "OK");
			return;
		}

		var selectedType = (CustomColumnType)NewColumnTypeIndex;
		var allowed = selectedType == CustomColumnType.ValueSet
			? NewAllowedValues.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(value => value.Trim())
				.Where(value => !string.IsNullOrWhiteSpace(value))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList()
			: [];

		var column = new CustomColumn(
			CustomColumn.BuildUniqueColumnId(NewColumnName.Trim(), CustomColumns),
			NewColumnName.Trim(),
			selectedType,
			allowed);

		CustomColumns.Add(column);
		NewColumnName = string.Empty;
		NewAllowedValues = string.Empty;
		NewColumnTypeIndex = (int)CustomColumnType.Text;
	}

	private async Task DeleteColumnAsync(CustomColumn? column) {
		if (column is null) return;

		var confirm = await Shell.Current.DisplayAlertAsync(
			"Usuń kolumnę",
			$"Czy usunąć kolumnę '{column.Name}' z całej kolekcji?",
			"Usuń",
			"Anuluj");

		if (!confirm) return;
		CustomColumns.Remove(column);
	}

	private void MoveColumn(CustomColumn? column, int offset) {
		if (column is null) return;

		var oldIndex = CustomColumns.IndexOf(column);
		if (oldIndex < 0) return;

		var newIndex = oldIndex + offset;
		if (newIndex < 0 || newIndex >= CustomColumns.Count) return;
		CustomColumns.Move(oldIndex, newIndex);
	}

	private static CustomColumn CloneColumn(CustomColumn column) {
		return new CustomColumn(column.Id, column.Name, column.Type, [.. column.AllowedValues]);
	}
}
