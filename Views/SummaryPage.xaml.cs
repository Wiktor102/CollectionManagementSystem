using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CollectionManagementSystem.Views;

public partial class SummaryPage : ContentPage {
	private readonly SummaryViewModel _viewModel;
	private readonly INavigationService _navigationService;
	private string _lastCollectionId = string.Empty;

	public SummaryPage() {
		InitializeComponent();
		_viewModel = App.Services.GetRequiredService<SummaryViewModel>();
		_navigationService = App.Services.GetRequiredService<INavigationService>();
		BindingContext = _viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);
		var collectionId = _navigationService.ConsumeParameter<string>(NavigationKeys.CollectionId) ?? _lastCollectionId;
		_lastCollectionId = collectionId;
		await _viewModel.InitializeAsync(collectionId);
	}
}
