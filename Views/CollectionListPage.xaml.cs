using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CollectionManagementSystem.Views;

public partial class CollectionListPage : ContentPage {
	private readonly CollectionListViewModel _viewModel;
	private readonly INavigationService _navigationService;

	public CollectionListPage() {
		InitializeComponent();
		_viewModel = App.Services.GetRequiredService<CollectionListViewModel>();
		_navigationService = App.Services.GetRequiredService<INavigationService>();
		BindingContext = _viewModel;
	}

	protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
		base.OnNavigatedTo(args);
		var collectionId = _navigationService.ConsumeParameter<string>(NavigationKeys.CollectionId) ?? _viewModel.CollectionId;
		await _viewModel.InitializeAsync(collectionId);
	}

	protected override void OnAppearing() {
		base.OnAppearing();
		_viewModel.RebuildSortedItems();
	}

	private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
		if (e.CurrentSelection.FirstOrDefault() is not CollectionItem selectedItem) {
			return;
		}

		if (_viewModel.OpenItemCommand.CanExecute(selectedItem)) {
			_viewModel.OpenItemCommand.Execute(selectedItem);
		}

		ItemsCollectionView.SelectedItem = null;
	}
}
