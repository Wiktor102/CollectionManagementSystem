using CollectionManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CollectionManagementSystem.Views;

public partial class MainPage : ContentPage {
	private readonly MainPageViewModel _viewModel;

	public MainPage() {
		InitializeComponent();
		_viewModel = App.Services.GetRequiredService<MainPageViewModel>();
		BindingContext = _viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);
		await _viewModel.InitializeAsync();
	}
}
