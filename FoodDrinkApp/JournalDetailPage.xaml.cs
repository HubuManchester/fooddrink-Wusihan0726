using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

[QueryProperty(nameof(EntryId), "id")]
public partial class JournalDetailPage : ContentPage
{
    private JournalEntry? currentEntry;

    public JournalDetailPage()
    {
        InitializeComponent();
    }

    public string EntryId
    {
        set => _ = LoadEntryAsync(value);
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

        RenderEntry();
    }

    private void RenderEntry()
    {
        if (currentEntry == null) return;

        MealTypeLabel.Text = currentEntry.MealType;
        DateLabel.Text = currentEntry.Date.ToString("MMMM dd, yyyy - hh:mm tt");
        NotesLabel.Text = currentEntry.Notes;

        if (!string.IsNullOrWhiteSpace(currentEntry.LocationAddress))
        {
            LocationLabel.Text = currentEntry.LocationAddress;
        }

        if (currentEntry.Latitude.HasValue && currentEntry.Longitude.HasValue)
        {
            CoordinateLabel.Text = $"Lat: {currentEntry.Latitude:F5}, Lon: {currentEntry.Longitude:F5}";
        }

        if (!string.IsNullOrWhiteSpace(currentEntry.PhotoPath) && File.Exists(currentEntry.PhotoPath))
        {
            PhotoImage.Source = ImageSource.FromFile(currentEntry.PhotoPath);
        }
    }

    private async void OnReadAloudClicked(object? sender, EventArgs e)
    {
        if (currentEntry == null) return;

        try
        {
            var locationText = string.IsNullOrWhiteSpace(currentEntry.LocationAddress)
                ? ""
                : $" Location: {currentEntry.LocationAddress}. ";

            var speechText = $"Meal: {currentEntry.MealType}. {locationText} Notes: {currentEntry.Notes}";
            await SpeechService.SpeakAsync(speechText);
            await ShowStatusAsync("Reading aloud...");
        }
        catch (Exception ex)
        {
            await ShowStatusAsync($"Speech error: {ex.Message}");
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (currentEntry == null) return;

        var confirm = await DisplayAlert("Delete", "Are you sure you want to delete this journal entry?", "Yes", "No");
        if (!confirm) return;

        try
        {
            await JournalService.DeleteEntryAsync(currentEntry.Id);
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await ShowStatusAsync("Entry deleted");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await ShowStatusAsync($"Delete error: {ex.Message}");
        }
    }

    private async Task ShowStatusAsync(string message)
    {
        StatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
        await Task.Delay(2000);
        if (StatusLabel.Text == message)
        {
            StatusLabel.Text = "";
        }
    }
}