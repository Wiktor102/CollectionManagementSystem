using Microsoft.Extensions.DependencyInjection;

namespace CollectionManagementSystem {
	public partial class App : Application {
		public static IServiceProvider Services { get; set; } = default!;

		public App() {
			InitializeComponent();
		}

		protected override Window CreateWindow(IActivationState? activationState) {
			var shell = Services.GetRequiredService<AppShell>();
			return new Window(shell);
		}
	}
}