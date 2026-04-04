using CollectionManagementSystem.ViewModels;
using System.Collections.Specialized;

namespace CollectionManagementSystem;

public partial class MainWindow : Window {
	private Page? _trackedPage;
	private TitleBarState? _trackedTitleBarState;

	public MainWindow() {
		InitializeComponent();
		BindingContext = App.Services.GetRequiredService<MainPageViewModel>();
		ShellRoot.Navigated += OnShellNavigated;
		SyncBindingContext(Shell.Current?.CurrentPage ?? ShellRoot.CurrentPage);
	}

	protected override void OnHandlerChanged() {
		base.OnHandlerChanged();

#if WINDOWS
		var platformWindow = Handler?.PlatformView as Microsoft.UI.Xaml.Window;
		if (platformWindow == null) return;
		var titleBar = platformWindow.AppWindow.TitleBar;
		titleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
		titleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(255, 0, 105, 137);

		titleBar.ButtonHoverBackgroundColor = Windows.UI.Color.FromArgb(255, 0, 105, 137);
		titleBar.ButtonHoverForegroundColor = Windows.UI.Color.FromArgb(255, 194, 241, 255);
#endif
	}

	private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e) {
		SyncBindingContext(Shell.Current?.CurrentPage ?? ShellRoot.CurrentPage);
	}

	private void SyncBindingContext(Page? page) {
		if (_trackedPage is not null) {
			_trackedPage.BindingContextChanged -= OnTrackedPageBindingContextChanged;
		}

		_trackedPage = page;
		if (_trackedPage is not null) {
			_trackedPage.BindingContextChanged += OnTrackedPageBindingContextChanged;
		}

		if (page?.BindingContext is not null) {
			BindingContext = page.BindingContext;
		}

		TrackTitleBarState(BindingContext);
	}

	private void OnTrackedPageBindingContextChanged(object? sender, EventArgs e) {
		if (sender is BindableObject bindable) {
			BindingContext = bindable.BindingContext;
			TrackTitleBarState(BindingContext);
		}
	}

	private void TrackTitleBarState(object? bindingContext) {
		if (_trackedTitleBarState is not null) {
			_trackedTitleBarState.Actions.CollectionChanged -= OnTitleBarActionsChanged;
		}

		_trackedTitleBarState = (bindingContext as BaseViewModel)?.TitleBar;
		if (_trackedTitleBarState is not null) {
			_trackedTitleBarState.Actions.CollectionChanged += OnTitleBarActionsChanged;
		}

		RebuildTitleBarActions();
	}

	private void OnTitleBarActionsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		RebuildTitleBarActions();
	}

	private void RebuildTitleBarActions() {
		TitleBarActionsHost.Children.Clear();
		if (_trackedTitleBarState is null) return;

		object? styleResource = null;
		Application.Current?.Resources.TryGetValue("TitleBarActionButtonStyle", out styleResource);
		var buttonStyle = styleResource as Style;

		foreach (var action in _trackedTitleBarState.Actions) {
			var button = new Button {
				BindingContext = action,
				Style = buttonStyle
			};
			button.SetBinding(Button.TextProperty, nameof(TitleBarAction.Text));
			button.SetBinding(Button.CommandProperty, nameof(TitleBarAction.Command));
			button.SetBinding(VisualElement.IsVisibleProperty, nameof(TitleBarAction.IsVisible));
			TitleBarActionsHost.Children.Add(button);
		}
	}
}
