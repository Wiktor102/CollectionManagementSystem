using CollectionManagementSystem.Services;

namespace CollectionManagementSystem.Helpers;

public static class ImageHelper {
	public static async Task<string> PickAndStoreImageAsync(FileStorageService storageService, string itemId) {
		var result = await FilePicker.Default.PickAsync(new PickOptions {
			PickerTitle = "Wybierz obraz",
			FileTypes = FilePickerFileType.Images
		});

		if (result is null) {
			return string.Empty;
		}

		var extension = Path.GetExtension(result.FileName);
		if (string.IsNullOrWhiteSpace(extension)) {
			extension = ".jpg";
		}

		var relativePath = FileStorageService.BuildRelativeImagePath(itemId, extension);
		var absolutePath = storageService.ToAbsolutePath(relativePath);
		var directory = Path.GetDirectoryName(absolutePath);

		if (!string.IsNullOrWhiteSpace(directory)) {
			Directory.CreateDirectory(directory);
		}

		await using var source = await result.OpenReadAsync();
		await using var destination = File.Open(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None);
		await source.CopyToAsync(destination);

		return relativePath;
	}

	public static string ResolveImagePath(FileStorageService storageService, string imagePath) {
		return storageService.ToAbsolutePath(imagePath);
	}
}
