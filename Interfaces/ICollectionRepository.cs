using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.Interfaces;

public interface ICollectionRepository {
	Task<IReadOnlyList<Collection>> GetCollectionsAsync();
	Task<Collection?> GetCollectionAsync(string collectionId);
	Task<Collection> CreateCollectionAsync(string name, string type);
	Task SaveCollectionAsync(Collection collection);
	Task DeleteCollectionAsync(string collectionId);
	Task<bool> HasDuplicateItemNameAsync(string collectionId, string itemName, string? ignoredItemId = null);
	Task UpsertItemAsync(string collectionId, CollectionItem item);
	Task DeleteItemAsync(string collectionId, string itemId);
	Task<string> ExportCollectionAsync(string collectionId, string destinationFolder);
	Task ImportIntoCollectionAsync(string collectionId, string importFilePath, Func<string, Task<bool>> shouldOverwriteAsync);
}
