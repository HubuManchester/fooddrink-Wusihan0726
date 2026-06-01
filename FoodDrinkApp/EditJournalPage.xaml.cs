using FoodDrinkApp.Models;
using FoodDrinkApp.Services;
using Microsoft.Maui.Controls;

namespace FoodDrinkApp;

[QueryProperty(nameof(EntryId), "id")]
public partial class EditJournalPage : ContentPage
{
    private JournalEntry? currentEntry;
    private string currentMealType = "Lunch";
    private double? currentLatitude;
    private double? currentLongitude;
    private string? currentLocationAddress;

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

        currentMealType = currentEntry.MealType;
        currentLatitude = currentEntry.Latitude;
        currentLongitude = currentEntry.Longitude;
        currentLocationAddress = currentEntry.LocationAddress;

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

        UpdateMealTypeButtonStyles(currentMealType);
    }

    private void UpdateMealTypeButtonStyles(string mealType)
    {
        var defaultBg = Application.Current?.UserAppTheme == AppTheme.Dark
            ? (Color)Application.Current.Resources["Gray300"]
            : (Color)Application.Current.Resources["Gray200"];
        var defaultTextColor = Application.Current?.UserAppTheme == AppTheme.Dark
            ? Colors.White
            : (Color)Application.Current.Resources["Primary"];
        var selectedBg = (Color)Application.Current.Resources["Primary"];
        var selectedTextColor = Colors.White;

        BreakfastButton.BackgroundColor = defaultBg;
        BreakfastButton.TextColor = defaultTextColor;
        LunchButton.BackgroundColor = defaultBg;
        LunchButton.TextColor = defaultTextColor;
        DinnerButton.BackgroundColor = defaultBg;
        DinnerButton.TextColor = defaultTextColor;
        SnackButton.BackgroundColor = defaultBg;
        SnackButton.TextColor = defaultTextColor;

        switch (mealType)
        {
            case "Breakfast":
                BreakfastButton.BackgroundColor = selectedBg;
                BreakfastButton.TextColor = selectedTextColor;
                break;
            case "Lunch":
                LunchButton.BackgroundColor = selectedBg;
                LunchButton.TextColor = selectedTextColor;
                break;
            case "Dinner":
                DinnerButton.BackgroundColor = selectedBg;
                DinnerButton.TextColor = selectedTextColor;
                break;
            case "Snack":
                SnackButton.BackgroundColor = selectedBg;
                SnackButton.TextColor = selectedTextColor;
                break;
        }
    }

    private void OnMealTypeClicked(object? sender, EventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        currentMealType = button.Text;
        UpdateMealTypeButtonStyles(currentMealType);
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
            // Fall back to coordinate display
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

            currentEntry.MealType = currentMealType;
            currentEntry.Notes = notes;
            currentEntry.LocationAddress = currentLocationAddress;
            currentEntry.Latitude = currentLatitude;
            currentEntry.Longitude = currentLongitude;

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