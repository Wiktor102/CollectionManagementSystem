namespace CollectionManagementSystem.Views.Controls;

public partial class EmptyView : ContentView {
	public EmptyView() {
		InitializeComponent();
	}

	public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyView), string.Empty);

	public string Title {
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public static readonly BindableProperty ButtonTextProperty = BindableProperty.Create(nameof(ButtonText), typeof(string), typeof(EmptyView), string.Empty);

	public string ButtonText {
		get => (string)GetValue(ButtonTextProperty);
		set => SetValue(ButtonTextProperty, value);
	}


	public static readonly BindableProperty ButtonCommandProperty = BindableProperty.Create(nameof(ButtonCommand), typeof(Command), typeof(EmptyView), null);

	public Command ButtonCommand {
		get => (Command)GetValue(ButtonCommandProperty);
		set => SetValue(ButtonCommandProperty, value);
	}
}