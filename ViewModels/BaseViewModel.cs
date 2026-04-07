using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectionManagementSystem.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged {
	private bool _isBusy;
	private string _title = string.Empty;
	private readonly List<Command> _trackedCommands = [];

	public event PropertyChangedEventHandler? PropertyChanged;

	public bool IsBusy {
		get => _isBusy;
		set {
			if (SetProperty(ref _isBusy, value)) {
				RefreshCommandStates();
			}
		}
	}

	public string Title {
		get => _title;
		set => SetProperty(ref _title, value);
	}

	public TitleBarState TitleBar { get; } = new();

	protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") {
		if (EqualityComparer<T>.Default.Equals(storage, value)) {
			return false;
		}

		storage = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = "") {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected Command TrackCommand(Command command) {
		_trackedCommands.Add(command);
		return command;
	}

	protected Command<T> TrackCommand<T>(Command<T> command) {
		_trackedCommands.Add(command);
		return command;
	}

	protected void RefreshCommandStates() {
		foreach (var command in _trackedCommands) {
			command.ChangeCanExecute();
		}
	}

	protected async Task RunBusyAsync(Func<Task> action) {
		if (IsBusy) {
			return;
		}

		try {
			IsBusy = true;
			await action();
		}
		finally {
			IsBusy = false;
		}
	}
}
