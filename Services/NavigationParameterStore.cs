namespace CollectionManagementSystem.Services;

public sealed class NavigationParameterStore {
	private readonly Dictionary<string, object> _parameters = new(StringComparer.OrdinalIgnoreCase);

	public void SetParameters(IDictionary<string, object> parameters) {
		foreach (var pair in parameters) {
			_parameters[pair.Key] = pair.Value;
		}
	}

	public T? Consume<T>(string key) where T : class {
		if (!_parameters.TryGetValue(key, out var value)) {
			return null;
		}

		_parameters.Remove(key);
		return value as T;
	}

	public void Clear(string key) {
		_parameters.Remove(key);
	}
}
