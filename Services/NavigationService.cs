using CollectionManagementSystem.Interfaces;

namespace CollectionManagementSystem.Services;

public sealed class NavigationService(NavigationParameterStore parameterStore) : INavigationService {
	private readonly NavigationParameterStore _parameterStore = parameterStore;

	public Task NavigateToAsync(string route) {
		return Shell.Current.GoToAsync(route);
	}

	public Task NavigateToAsync(string route, IDictionary<string, object> parameters) {
		_parameterStore.SetParameters(parameters);
		return Shell.Current.GoToAsync(route);
	}

	public Task GoBackAsync() {
		return Shell.Current.GoToAsync("..");
	}

	public T? ConsumeParameter<T>(string key) where T : class {
		return _parameterStore.Consume<T>(key);
	}
}
