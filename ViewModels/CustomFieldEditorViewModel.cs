using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.ViewModels;

public sealed class CustomFieldEditorViewModel : BaseViewModel {
	private string _value = string.Empty;

	public required CustomColumn Column { get; init; }

	public string Value {
		get => _value;
		set => SetProperty(ref _value, value);
	}

	public string ColumnId => Column.Id;
	public string ColumnName => Column.Name;
	public bool IsText => Column.Type == CustomColumnType.Text;
	public bool IsNumber => Column.Type == CustomColumnType.Number;
	public bool IsValueSet => Column.Type == CustomColumnType.ValueSet;
	public List<string> AllowedValues => Column.AllowedValues;
}
