using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectionManagementSystem.Models;

public sealed class CollectionItem : INotifyPropertyChanged {
	private string _id = Guid.NewGuid().ToString();
	private string _name = string.Empty;
	private ItemStatus _status = ItemStatus.Owned;
	private decimal _price;
	private int _rating = 1;
	private string _comment = string.Empty;
	private string _imagePath = string.Empty;
	private List<CustomFieldValue> _customFields = [];

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Id {
		get => _id;
		set => SetProperty(ref _id, value);
	}

	public string Name {
		get => _name;
		set => SetProperty(ref _name, value);
	}

	public ItemStatus Status {
		get => _status;
		set {
			if (SetProperty(ref _status, value)) {
				OnPropertyChanged(nameof(StatusLabel));
				OnPropertyChanged(nameof(StatusColor));
				OnPropertyChanged(nameof(StatusOpacity));
			}
		}
	}
	public string StatusLabel => Status switch {
		ItemStatus.Owned => "Posiadane",
		ItemStatus.Used => "Uzywane",
		ItemStatus.New => "Nowe",
		ItemStatus.ForSale => "Na sprzedaz",
		ItemStatus.Sold => "Sprzedane",
		ItemStatus.WantToBuy => "Chce kupic",
		_ => "Posiadane"
	};
	public string RatingDisplayText => $"Ocena: {Math.Clamp(Rating, 1, 10)}/10";
	public Color StatusColor => Status switch {
		ItemStatus.Owned => ResolveColor("StatusOwnedColor", "#1A6B6B"),
		ItemStatus.Used => ResolveColor("StatusUsedColor", "#D97B29"),
		ItemStatus.New => ResolveColor("StatusNewColor", "#2B7A4B"),
		ItemStatus.ForSale => ResolveColor("StatusForSaleColor", "#E85D4A"),
		ItemStatus.Sold => ResolveColor("StatusSoldColor", "#6B6B6B"),
		ItemStatus.WantToBuy => ResolveColor("StatusWantToBuyColor", "#3B4C8F"),
		_ => Colors.Gray
	};
	public double StatusOpacity => Status == ItemStatus.Sold ? 0.4 : 1.0;

	public decimal Price {
		get => _price;
		set => SetProperty(ref _price, value);
	}

	public int Rating {
		get => _rating;
		set {
			if (SetProperty(ref _rating, value)) {
				OnPropertyChanged(nameof(RatingDisplayText));
			}
		}
	}

	public string Comment {
		get => _comment;
		set => SetProperty(ref _comment, value);
	}

	public string ImagePath {
		get => _imagePath;
		set {
			if (SetProperty(ref _imagePath, value)) {
				OnPropertyChanged(nameof(HasImage));
				OnPropertyChanged(nameof(ImageSourcePath));
			}
		}
	}

	public bool HasImage => ImageSourcePath != null; // Nie mozemy bezposrednio z ImagePath, poniewaz sciezka moze być nieprawidlowa lub nieistniejaca
	public string? ImageSourcePath {
		get {
			if (string.IsNullOrWhiteSpace(ImagePath)) return null;
			if (Path.IsPathRooted(ImagePath)) return ImagePath;

			var fullPath = Path.Combine(
				FileSystem.AppDataDirectory,
				"CollectionManagementSystem",
				ImagePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar)
			);

			return File.Exists(fullPath) ? fullPath : null;
		}
	}
	public List<CustomFieldValue> CustomFields {
		get => _customFields;
		set => SetProperty(ref _customFields, value);
	}

	private static Color ResolveColor(string key, string fallback) {
		if (Application.Current?.Resources.TryGetValue(key, out var resource) == true && resource is Color color) {
			return color;
		}
		return Color.FromArgb(fallback);
	}

	private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "") {
		if (EqualityComparer<T>.Default.Equals(storage, value)) {
			return false;
		}

		storage = value;
		OnPropertyChanged(propertyName);
		return true;
	}

	private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
