using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Interfaces;
using CollectionManagementSystem.Models;

namespace CollectionManagementSystem.Services;

public sealed class CollectionRepository(FileStorageService storageService) : ICollectionRepository {
	private readonly FileStorageService _storageService = storageService;
	private readonly SemaphoreSlim _mutex = new(1, 1);
	private readonly List<Collection> _cache = [];
	private bool _loaded;

	public async Task<IReadOnlyList<Collection>> GetCollectionsAsync() {
		await EnsureLoadedAsync();
		return _cache;
	}

	public async Task<Collection?> GetCollectionAsync(string collectionId) {
		await EnsureLoadedAsync();
		return _cache.FirstOrDefault(c => string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
	}

	public async Task<Collection> CreateCollectionAsync(string name, string type) {
		await EnsureLoadedAsync();

		var collection = new Collection {
			Id = Guid.NewGuid().ToString(),
			Name = name.Trim(),
			Type = type.Trim(),
			CreatedAt = DateTime.Today
		};

		_cache.Add(collection);
		await PersistCollectionAsync(collection);
		await SaveIndexAsync();

		return collection;
	}

	public async Task SaveCollectionAsync(Collection collection) {
		await EnsureLoadedAsync();
		var existing = _cache.FirstOrDefault(c => string.Equals(c.Id, collection.Id, StringComparison.OrdinalIgnoreCase));

		if (existing is null) {
			_cache.Add(collection);
		} else if (!ReferenceEquals(existing, collection)) {
			existing.Name = collection.Name;
			existing.Type = collection.Type;
			existing.CreatedAt = collection.CreatedAt;
			existing.CustomColumns = collection.CustomColumns;
			existing.Items = collection.Items;
			collection = existing;
		}

		await PersistCollectionAsync(collection);
		await SaveIndexAsync();
	}

	public async Task DeleteCollectionAsync(string collectionId) {
		await EnsureLoadedAsync();
		var existing = _cache.FirstOrDefault(c => string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
		if (existing is null) {
			return;
		}

		_cache.Remove(existing);
		FileStorageService.DeleteFile(_storageService.GetCollectionPath(collectionId));
		await SaveIndexAsync();
	}

	public async Task<bool> HasDuplicateItemNameAsync(string collectionId, string itemName, string? ignoredItemId = null) {
		await EnsureLoadedAsync();
		var collection = _cache.FirstOrDefault(c => string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
		if (collection is null) {
			return false;
		}

		var normalized = itemName.Trim();
		return collection.Items.Any(item =>
			string.Equals(item.Name.Trim(), normalized, StringComparison.OrdinalIgnoreCase)
			&& !string.Equals(item.Id, ignoredItemId, StringComparison.OrdinalIgnoreCase));
	}

	public async Task UpsertItemAsync(string collectionId, CollectionItem item) {
		await EnsureLoadedAsync();
		var collection = _cache.FirstOrDefault(c => string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
		if (collection is null) {
			return;
		}

		var existing = collection.Items.FirstOrDefault(i => string.Equals(i.Id, item.Id, StringComparison.OrdinalIgnoreCase));
		if (existing is null) {
			if (string.IsNullOrWhiteSpace(item.Id)) {
				item.Id = Guid.NewGuid().ToString();
			}

			collection.Items.Add(item);
		} else if (!ReferenceEquals(existing, item)) {
			existing.Name = item.Name;
			existing.Status = item.Status;
			existing.Price = item.Price;
			existing.Rating = item.Rating;
			existing.Comment = item.Comment;
			existing.ImagePath = item.ImagePath;
			existing.CustomFields = item.CustomFields;
		}

		await SaveCollectionAsync(collection);
	}

	public async Task DeleteItemAsync(string collectionId, string itemId) {
		await EnsureLoadedAsync();
		var collection = _cache.FirstOrDefault(c => string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
		if (collection is null) return;

		var item = collection.Items.FirstOrDefault(i => string.Equals(i.Id, itemId, StringComparison.OrdinalIgnoreCase));
		if (item is null) return;

		collection.Items.Remove(item);
		await SaveCollectionAsync(collection);
	}

	public async Task<string> ExportCollectionAsync(string collectionId, string destinationFolder, bool includeImages = false) {
		await EnsureLoadedAsync();
		var collection = _cache.FirstOrDefault(c => string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
		if (collection is null) {
			throw new InvalidOperationException("Nie znaleziono kolekcji do eksportu.");
		}

		var exportBaseName = BuildExportBaseName(collection.Name);

		var sourceCollectionPath = _storageService.GetCollectionPath(collection.Id);
		if (!includeImages) {
			var destinationTxtPath = Path.Combine(destinationFolder, $"{exportBaseName}.txt");
			await FileStorageService.CopyFileAsync(sourceCollectionPath, destinationTxtPath, true);
			return destinationTxtPath;
		}

		var exportFolderPath = Path.Combine(destinationFolder, exportBaseName);
		Directory.CreateDirectory(exportFolderPath);

		var destinationCollectionPath = Path.Combine(exportFolderPath, $"{exportBaseName}.txt");
		await FileStorageService.CopyFileAsync(sourceCollectionPath, destinationCollectionPath, true);

		var imagesFolder = Path.Combine(exportFolderPath, "images");
		Directory.CreateDirectory(imagesFolder);

		foreach (var item in collection.Items.Where(i => !string.IsNullOrWhiteSpace(i.ImagePath))) {
			var sourceImage = _storageService.ToAbsolutePath(item.ImagePath);
			if (!File.Exists(sourceImage)) {
				continue;
			}

			var fileName = Path.GetFileName(sourceImage);
			var destinationImage = Path.Combine(imagesFolder, fileName);
			await FileStorageService.CopyFileAsync(sourceImage, destinationImage, true);
		}

		return exportFolderPath;
	}

	public async Task ImportIntoCollectionAsync(string collectionId, string importFilePath, Func<string, Task<bool>> shouldOverwriteAsync) {
		await EnsureLoadedAsync();
		var target = _cache.FirstOrDefault(c => string.Equals(c.Id, collectionId, StringComparison.OrdinalIgnoreCase));
		if (target is null) {
			throw new InvalidOperationException("Nie znaleziono kolekcji docelowej.");
		}

		var importedText = await FileStorageService.ReadTextAsync(importFilePath);
		if (string.IsNullOrWhiteSpace(importedText)) {
			return;
		}

		var importedCollection = TextParser.ParseCollection(importedText, Guid.NewGuid().ToString());
		MergeColumns(target, importedCollection);
		var importDirectory = Path.GetDirectoryName(importFilePath) ?? string.Empty;

		foreach (var incomingItem in importedCollection.Items) {
			var existing = target.Items.FirstOrDefault(item =>
				string.Equals(item.Name.Trim(), incomingItem.Name.Trim(), StringComparison.OrdinalIgnoreCase));

			var mappedItem = MapImportedItem(target, importedCollection, incomingItem, importDirectory);

			if (existing is null) {
				target.Items.Add(mappedItem);
				continue;
			}

			var overwrite = await shouldOverwriteAsync(incomingItem.Name);
			if (!overwrite) {
				continue;
			}

			mappedItem.Id = existing.Id;
			target.Items[target.Items.IndexOf(existing)] = mappedItem;
		}

		await SaveCollectionAsync(target);
	}

	private async Task EnsureLoadedAsync() {
		if (_loaded) {
			return;
		}

		await _mutex.WaitAsync();
		try {
			if (_loaded) {
				return;
			}

			_cache.Clear();
			var indexText = await FileStorageService.ReadTextAsync(_storageService.IndexPath);
			var indexEntries = TextParser.ParseIndex(indexText);

			foreach (var entry in indexEntries) {
				var collectionPath = _storageService.GetCollectionPath(entry.Id);
				Collection collection;

				if (FileStorageService.FileExists(collectionPath)) {
					var collectionText = await FileStorageService.ReadTextAsync(collectionPath);
					collection = TextParser.ParseCollection(collectionText, entry.Id);
				} else {
					collection = new Collection {
						Id = entry.Id,
						Name = entry.Name,
						CreatedAt = entry.CreatedAt
					};
				}

				if (string.IsNullOrWhiteSpace(collection.Name)) {
					collection.Name = entry.Name;
				}

				if (collection.CreatedAt == default) {
					collection.CreatedAt = entry.CreatedAt;
				}

				_cache.Add(collection);
			}

			_loaded = true;
		} finally {
			_mutex.Release();
		}
	}

	private async Task PersistCollectionAsync(Collection collection) {
		var content = TextSerializer.SerializeCollection(collection);
		var path = _storageService.GetCollectionPath(collection.Id);
		await FileStorageService.WriteTextAsync(path, content);
	}

	private async Task SaveIndexAsync() {
		var content = TextSerializer.SerializeIndex(_cache);
		await FileStorageService.WriteTextAsync(_storageService.IndexPath, content);
	}

	private static void MergeColumns(Collection target, Collection importedCollection) {
		foreach (var importedColumn in importedCollection.CustomColumns) {
			var existing = target.CustomColumns.FirstOrDefault(column =>
				string.Equals(column.Name, importedColumn.Name, StringComparison.OrdinalIgnoreCase));

			if (existing is not null) {
				if (existing.Type == CustomColumnType.ValueSet && importedColumn.Type == CustomColumnType.ValueSet) {
					foreach (var value in importedColumn.AllowedValues) {
						if (!existing.AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase)) {
							existing.AllowedValues.Add(value);
						}
					}
				}

				continue;
			}

			target.CustomColumns.Add(new CustomColumn(
				CustomColumn.BuildUniqueColumnId(importedColumn.Name, target.CustomColumns),
				importedColumn.Name,
				importedColumn.Type,
				importedColumn.AllowedValues.ToList()));
		}
	}

	private CollectionItem MapImportedItem(Collection target, Collection importedCollection, CollectionItem incomingItem, string importDirectory) {
		var mapped = new CollectionItem {
			Id = string.IsNullOrWhiteSpace(incomingItem.Id) ? Guid.NewGuid().ToString() : incomingItem.Id,
			Name = incomingItem.Name,
			Status = incomingItem.Status,
			Price = incomingItem.Price,
			Rating = incomingItem.Rating,
			Comment = incomingItem.Comment,
			ImagePath = CopyImportedImageIfNeeded(incomingItem.ImagePath, importDirectory, incomingItem.Id)
		};

		foreach (var field in incomingItem.CustomFields) {
			var importedColumn = importedCollection.CustomColumns.FirstOrDefault(c =>
				string.Equals(c.Id, field.ColumnId, StringComparison.OrdinalIgnoreCase));
			if (importedColumn is null) {
				continue;
			}

			var targetColumn = target.CustomColumns.FirstOrDefault(c =>
				string.Equals(c.Name, importedColumn.Name, StringComparison.OrdinalIgnoreCase));
			if (targetColumn is null) {
				continue;
			}

			mapped.CustomFields.Add(new CustomFieldValue {
				ColumnId = targetColumn.Id,
				Value = field.Value
			});
		}

		return mapped;
	}

	private string CopyImportedImageIfNeeded(string imagePath, string importDirectory, string itemId) {
		if (string.IsNullOrWhiteSpace(imagePath)) {
			return string.Empty;
		}

		var sourcePath = imagePath;
		if (!Path.IsPathRooted(sourcePath)) {
			sourcePath = Path.Combine(importDirectory, imagePath.Replace('\\', Path.DirectorySeparatorChar));
		}

		if (!File.Exists(sourcePath)) {
			return string.Empty;
		}

		var extension = Path.GetExtension(sourcePath);
		var targetItemId = string.IsNullOrWhiteSpace(itemId) ? Guid.NewGuid().ToString() : itemId;
		var relativePath = FileStorageService.BuildRelativeImagePath(targetItemId, extension);
		var destinationPath = _storageService.ToAbsolutePath(relativePath);
		Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? _storageService.ImagesPath);
		File.Copy(sourcePath, destinationPath, true);

		return relativePath;
	}

	private static string BuildExportBaseName(string collectionName) {
		var invalid = Path.GetInvalidFileNameChars();
		var chars = collectionName
			.Where(c => !invalid.Contains(c))
			.ToArray();
		var sanitized = new string(chars).Trim();
		if (string.IsNullOrWhiteSpace(sanitized)) {
			sanitized = "Kolekcja";
		}

		return $"{sanitized}_{DateTime.Now:yyyy-MM-dd}";
	}
}
