namespace CollectionManagementSystem {
	public partial class AppShell : Shell {
		public AppShell() {
			InitializeComponent();
			Routing.RegisterRoute(nameof(Views.CollectionListPage), typeof(Views.CollectionListPage));
			Routing.RegisterRoute(nameof(Views.AddEditCollectionPage), typeof(Views.AddEditCollectionPage));
			Routing.RegisterRoute(nameof(Views.ItemDetailPage), typeof(Views.ItemDetailPage));
			Routing.RegisterRoute(nameof(Views.AddEditItemPage), typeof(Views.AddEditItemPage));
			Routing.RegisterRoute(nameof(Views.SummaryPage), typeof(Views.SummaryPage));
		}
	}
}
