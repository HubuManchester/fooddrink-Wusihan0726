using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class AddJournalPage : ContentPage
{
    private string? currentPhotoPath;
    private string currentMealType = "Lunch";
    private double? currentLatitude;
    private double? currentLongitude;
    private string? currentLocationAddress;

    public AddJournalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private void OnMealTypeClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        // Reset all button styles
        ResetMealTypeButtons();

        // Set selected button style
        button.BackgroundColor = Application.Current?.UserAppTheme == AppTheme.Dark
            ? (Color)Application.Current.Resources["Secondary"]
            : (Color)Application.Current.Resources["Primary"];
        button.TextColor = Colors.White;

        // Update current meal type
        currentMealType = button.Text;
    }

    private void ResetMealTypeButtons()
    {
        var defaultBg = Application.Current?.UserAppTheme == AppTheme.Dark
            ? (Color)Application.Current.Resources["Gray300"]
            : (Color)Application.Current.Resources["Gray200"];
        var defaultTextColor = Application.Current?.UserAppTheme == AppTheme.Dark
            ? Colors.White
            : (Color)Application.Current.Resources["Primary"];

        BreakfastButton.BackgroundColor = defaultBg;
        BreakfastButton.TextColor = defaultTextColor;
        LunchButton.BackgroundColor = defaultBg;
        LunchButton.TextColor = defaultTextColor;
        DinnerButton.BackgroundColor = defaultBg;
        DinnerButton.TextColor = defaultTextColor;
        SnackButton.BackgroundColor = defaultBg;
        SnackButton.TextColor = defaultTextColor;
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await ShowStatusAsync("Camera not supported on this device");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                await ShowStatusAsync("Photo capture cancelled");
                return;
            }

            // Save photo to local storage
            var localPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
            await using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            await File.WriteAllBytesAsync(localPath, memoryStream.ToArray());

            currentPhotoPath = localPath;
            PhotoPreview.Source = ImageSource.FromFile(localPath);

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            await ShowStatusAsync("Photo captured successfully");
        }
        catch (PermissionException)
        {
            await ShowStatusAsync("Camera permission denied. Please enable in device settings.");
        }
        catch (Exception ex)
        {
            await ShowStatusAsync($"Camera error: {ex.Message}");
        }
    }

    private async void OnGetLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            await ShowStatusAsync("Getting location...");

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                await ShowStatusAsync("Current location could not be found");
                return;
            }

            currentLatitude = location.Latitude;
            currentLongitude = location.Longitude;

            CoordinateLabel.Text = $"Lat: {location.Latitude:F5}, Lon: {location.Longitude:F5}";
            CoordinateLabel.IsVisible = true;

            currentLocationAddress = await GetAddressFromLocation(location);
            LocationLabel.Text = currentLocationAddress ?? "Address not available";

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            await ShowStatusAsync("Location captured successfully");
        }
        catch (PermissionException)
        {
            await ShowStatusAsync("Location permission denied. Please enable in device settings.");
        }
        catch (Exception ex)
        {
            await ShowStatusAsync($"Location error: {ex.Message}");
        }
    }

    private static async Task<string?> GetAddressFromLocation(Location location)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location);
            var placemark = placemarks?.FirstOrDefault();

            if (placemark != null)
            {
                var parts = new[]
                {
                    placemark.CountryName,
                    placemark.AdminArea,
                    placemark.Locality,
                    placemark.SubLocality,
                    placemark.Thoroughfare
                }.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToArray();

                return parts.Length > 0 ? string.Join(" / ", parts) : null;
            }
        }
        catch
        {
            // Fall back to coordinate display
        }

        return null;
    }

    private async void OnReadAloudClicked(object? sender, EventArgs e)
    {
        try
        {
            var notes = NotesEditor.Text;
            if (string.IsNullOrWhiteSpace(notes))
            {
                await ShowStatusAsync("No notes to read aloud");
                return;
            }

            var locationText = string.IsNullOrWhiteSpace(currentLocationAddress)
                ? ""
                : $" Location: {currentLocationAddress}. ";

            var speechText = $"Meal: {currentMealType}. {locationText} Notes: {notes}";
            await SpeechService.SpeakAsync(speechText);
            await ShowStatusAsync("Reading aloud...");
        }
        catch (Exception ex)
        {
            await ShowStatusAsync($"Speech error: {ex.Message}");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var notes = NotesEditor.Text?.Trim();
            if (string.IsNullOrWhiteSpace(notes))
            {
                await ShowStatusAsync("Please write some notes before saving");
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(200));
                return;
            }

            var entry = new JournalEntry
            {
                MealType = currentMealType,
                Notes = notes,
                PhotoPath = currentPhotoPath,
                LocationAddress = currentLocationAddress,
                Latitude = currentLatitude,
                Longitude = currentLongitude
            };

            await JournalService.AddEntryAsync(entry);

            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));

            SemanticScreenReader.Announce("Journal entry saved");
            await ShowStatusAsync("Entry saved successfully");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await ShowStatusAsync($"Save error: {ex.Message}");
        }
    }

    private async Task ShowStatusAsync(string message)
    {
        StatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
        await Task.Delay(3000);
        if (StatusLabel.Text == message)
        {
            StatusLabel.Text = "Ready";
        }
    }
}