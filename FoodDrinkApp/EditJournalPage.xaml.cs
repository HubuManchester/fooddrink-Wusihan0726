using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

[QueryProperty(nameof(EntryId), "id")]
public partial class EditJournalPage : ContentPage
{
    private JournalEntry? currentEntry;
    private double? currentLatitude;
    private double? currentLongitude;
    private string? currentLocationAddress;
    private string? currentPhotoPath;

    public string EntryId
    {
        set => _ = LoadEntryAsync(value);
    }

    public EditJournalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async Task LoadEntryAsync(string id)
    {
        currentEntry = await Task.Run(() => JournalService.GetEntryById(id));

        if (currentEntry == null)
        {
            await DisplayAlert("Error", "Journal entry not found", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        MealTypeEntry.Text = currentEntry.MealType;
        currentLatitude = currentEntry.Latitude;
        currentLongitude = currentEntry.Longitude;
        currentLocationAddress = currentEntry.LocationAddress;
        currentPhotoPath = currentEntry.PhotoPath;

        NotesEditor.Text = currentEntry.Notes;

        if (!string.IsNullOrWhiteSpace(currentLocationAddress))
        {
            LocationLabel.Text = currentLocationAddress;
        }

        if (currentLatitude.HasValue && currentLongitude.HasValue)
        {
            CoordinateLabel.Text = $"Lat: {currentLatitude:F5}, Lon: {currentLongitude:F5}";
            CoordinateLabel.IsVisible = true;
        }

        if (!string.IsNullOrWhiteSpace(currentPhotoPath) && File.Exists(currentPhotoPath))
        {
            PhotoPreview.Source = ImageSource.FromFile(currentPhotoPath);
        }
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

            if (!string.IsNullOrWhiteSpace(currentPhotoPath) && File.Exists(currentPhotoPath))
            {
                try
                {
                    File.Delete(currentPhotoPath);
                }
                catch { }
            }

            var localPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
            await using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            await File.WriteAllBytesAsync(localPath, memoryStream.ToArray());

            currentPhotoPath = localPath;
            PhotoPreview.Source = ImageSource.FromFile(localPath);

            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
            await ShowStatusAsync("Photo updated successfully");
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
            await ShowStatusAsync("Location updated successfully");
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
        }

        return null;
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

            if (currentEntry == null) return;

            var mealType = string.IsNullOrWhiteSpace(MealTypeEntry.Text) ? "Lunch" : MealTypeEntry.Text.Trim();

            currentEntry.MealType = mealType;
            currentEntry.Notes = notes;
            currentEntry.LocationAddress = currentLocationAddress;
            currentEntry.Latitude = currentLatitude;
            currentEntry.Longitude = currentLongitude;
            currentEntry.PhotoPath = currentPhotoPath;

            var success = await JournalService.UpdateEntryAsync(currentEntry);

            if (success)
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(300));
                SemanticScreenReader.Announce("Journal entry updated");
                await ShowStatusAsync("Entry updated successfully");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error", "Failed to update entry", "OK");
            }
        }
        catch (Exception ex)
        {
            await ShowStatusAsync($"Save error: {ex.Message}");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task ShowStatusAsync(string message)
    {
        StatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
        await Task.Delay(3000);
        if (StatusLabel.Text == message)
        {
            StatusLabel.Text = "";
        }
    }
}