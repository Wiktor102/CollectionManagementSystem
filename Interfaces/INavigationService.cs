namespace CollectionManagementSystem.Interfaces;

public interface INavigationService {
	Task NavigateToAsync(string route);
	Task NavigateToAsync(string route, IDictionary<string, object> parameters);
	Task GoBackAsync();
	T? ConsumeParameter<T>(string key) where T : class;
}
