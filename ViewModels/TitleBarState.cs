using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectionManagementSystem.ViewModels;

public sealed class TitleBarState : INotifyPropertyChanged {
	private string _subtitle = string.Empty;

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Subtitle {
		get => _subtitle;
		set => SetProperty(ref _subtitle, value);
	}

	public ObservableCollection<TitleBarAction> Actions { get; } = new();

	private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") {
		if (EqualityComparer<T>.Default.Equals(storage, value)) {
			return false;
		}

		storage = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		return true;
	}
}
