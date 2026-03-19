using Microsoft.Extensions.Logging;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Services;
using CollectionManagementSystem.ViewModels;
using CollectionManagementSystem.Views;

namespace CollectionManagementSystem {
	public static class MauiProgram {
		public static MauiApp CreateMauiApp() {
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts => {
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

			builder.Services.AddSingleton<FileStorageService>();
			builder.Services.AddSingleton<NavigationParameterStore>();
			builder.Services.AddSingleton<ICollectionRepository, CollectionRepository>();
			builder.Services.AddSingleton<INavigationService, NavigationService>();

			builder.Services.AddTransient<MainPageViewModel>();
			builder.Services.AddTransient<CollectionListViewModel>();
			builder.Services.AddTransient<AddEditCollectionViewModel>();
			builder.Services.AddTransient<ItemDetailViewModel>();
			builder.Services.AddTransient<AddEditItemViewModel>();
			builder.Services.AddTransient<SummaryViewModel>();

			builder.Services.AddTransient<MainPage>();
			builder.Services.AddTransient<CollectionListPage>();
			builder.Services.AddTransient<AddEditCollectionPage>();
			builder.Services.AddTransient<ItemDetailPage>();
			builder.Services.AddTransient<AddEditItemPage>();
			builder.Services.AddTransient<SummaryPage>();
			builder.Services.AddTransient<AppShell>();

			var app = builder.Build();
			App.Services = app.Services;
			return app;
		}
	}
}
