using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CollectionManagementSystem.Views;

public partial class AddEditCollectionPage : ContentPage {
	private readonly AddEditCollectionViewModel _viewModel;
	private readonly INavigationService _navigationService;

	public AddEditCollectionPage() {
		InitializeComponent();
		_viewModel = App.Services.GetRequiredService<AddEditCollectionViewModel>();
		_navigationService = App.Services.GetRequiredService<INavigationService>();
		BindingContext = _viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);
		var collectionId = _navigationService.ConsumeParameter<string>(NavigationKeys.CollectionId);
		await _viewModel.InitializeAsync(collectionId);
	}
}
