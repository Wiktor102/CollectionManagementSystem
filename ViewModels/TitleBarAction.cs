using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectionManagementSystem.ViewModels;

public sealed class TitleBarAction(string text, ICommand command) : INotifyPropertyChanged {
	public string Text { get; } = text;
	public ICommand Command { get; } = command;
	public event PropertyChangedEventHandler? PropertyChanged;

	private bool _isVisible = true;
	public bool IsVisible {
		get => _isVisible;
		set => SetProperty(ref _isVisible, value);
	}

	private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") {
		if (EqualityComparer<T>.Default.Equals(storage, value)) {
			return false;
		}

		storage = value;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		return true;
	}
}
