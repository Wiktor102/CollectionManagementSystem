using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Views;

public partial class AddEditItemPage : ContentPage {
	private readonly AddEditItemViewModel _viewModel;
	private readonly INavigationService _navigationService;
	private string _lastCollectionId = string.Empty;

	public AddEditItemPage() {
		InitializeComponent();
		_viewModel = App.Services.GetRequiredService<AddEditItemViewModel>();
		_navigationService = App.Services.GetRequiredService<INavigationService>();
		BindingContext = _viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);
		var collectionId = _navigationService.ConsumeParameter<string>(NavigationKeys.CollectionId) ?? _lastCollectionId;
		var itemId = _navigationService.ConsumeParameter<string>(NavigationKeys.ItemId);
		_lastCollectionId = collectionId;

		await _viewModel.InitializeAsync(collectionId, itemId);
	}
}
