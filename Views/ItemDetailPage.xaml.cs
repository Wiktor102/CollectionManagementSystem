using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CollectionManagementSystem.Views;

public partial class ItemDetailPage : ContentPage {
	private readonly ItemDetailViewModel _viewModel;
	private readonly INavigationService _navigationService;
	private string _lastCollectionId = string.Empty;
	private string _lastItemId = string.Empty;

	public ItemDetailPage() {
		InitializeComponent();
		_viewModel = App.Services.GetRequiredService<ItemDetailViewModel>();
		_navigationService = App.Services.GetRequiredService<INavigationService>();
		BindingContext = _viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);
		var collectionId = _navigationService.ConsumeParameter<string>(NavigationKeys.CollectionId) ?? _lastCollectionId;
		var itemId = _navigationService.ConsumeParameter<string>(NavigationKeys.ItemId) ?? _lastItemId;
		_lastCollectionId = collectionId;
		_lastItemId = itemId;
		await _viewModel.InitializeAsync(collectionId, itemId);
	}
}
