using System.Diagnostics;

namespace CollectionManagementSystem.Services;

public sealed class FileStorageService {
	public FileStorageService() {
		DataRootPath = Path.Combine(FileSystem.AppDataDirectory, "CollectionManagementSystem");
		CollectionsPath = Path.Combine(DataRootPath, "collections");
		ImagesPath = Path.Combine(DataRootPath, "images");
		IndexPath = Path.Combine(DataRootPath, "index.txt");

		EnsureStorage();
		Debug.WriteLine($"CollectionManagementSystem data path: {DataRootPath}");
	}

	public string DataRootPath { get; }
	public string CollectionsPath { get; }
	public string ImagesPath { get; }
	public string IndexPath { get; }

	public string GetCollectionPath(string collectionId) {
		return Path.Combine(CollectionsPath, $"{collectionId}.txt");
	}

	public static async Task<string> ReadTextAsync(string path) {
		if (!File.Exists(path)) return string.Empty;
		return await File.ReadAllTextAsync(path);
	}

	public static async Task WriteTextAsync(string path, string content) {
		var directory = Path.GetDirectoryName(path);
		if (!string.IsNullOrWhiteSpace(directory)) {
			Directory.CreateDirectory(directory);
		}

		await File.WriteAllTextAsync(path, content);
	}

	public static bool FileExists(string path) {
		return File.Exists(path);
	}

	public static void DeleteFile(string path) {
		if (File.Exists(path)) File.Delete(path);
	}

	public static string BuildRelativeImagePath(string itemId, string extension) {
		var cleanExtension = extension.StartsWith('.') ? extension : $".{extension}";
		return Path.Combine("images", $"{itemId}{cleanExtension}").Replace('/', '\\');
	}

	public string ToAbsolutePath(string relativeOrAbsolutePath) {
		if (string.IsNullOrWhiteSpace(relativeOrAbsolutePath)) {
			return string.Empty;
		}

		if (Path.IsPathRooted(relativeOrAbsolutePath)) {
			return relativeOrAbsolutePath;
		}

		var fixedRelative = relativeOrAbsolutePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
		return Path.Combine(DataRootPath, fixedRelative);
	}

	public static async Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = true) {
		var directory = Path.GetDirectoryName(destinationPath);
		if (!string.IsNullOrWhiteSpace(directory)) {
			Directory.CreateDirectory(directory);
		}

		await using var source = File.OpenRead(sourcePath);
		await using var destination = File.Open(destinationPath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None);
		await source.CopyToAsync(destination);
	}

	private void EnsureStorage() {
		Directory.CreateDirectory(DataRootPath);
		Directory.CreateDirectory(CollectionsPath);
		Directory.CreateDirectory(ImagesPath);

		if (!File.Exists(IndexPath)) {
			File.WriteAllText(IndexPath, "[INDEX]\n[/INDEX]\n");
		}
	}
}
