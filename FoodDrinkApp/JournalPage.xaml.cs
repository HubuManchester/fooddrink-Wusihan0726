using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class JournalPage : ContentPage
{
    public JournalPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
        await LoadJournalEntriesAsync();
    }

    private Task LoadJournalEntriesAsync()
    {
        var entries = JournalService.GetAllEntries();
        JournalCollection.ItemsSource = entries;
        return Task.CompletedTask;
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddJournalPage));
    }

    private async void OnViewClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            await Shell.Current.GoToAsync($"{nameof(JournalDetailPage)}?id={Uri.EscapeDataString(id)}");
        }
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            var parameters = new Dictionary<string, object>
            {
                { "id", id }
            };
            await Shell.Current.GoToAsync($"{nameof(EditJournalPage)}", parameters);
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string id)
        {
            var confirm = await DisplayAlert("Delete", "Are you sure you want to delete this journal entry?", "Yes", "No");
            if (!confirm) return;

            try
            {
                var success = await JournalService.DeleteEntryAsync(id);
                if (success)
                {
                    HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                    await LoadJournalEntriesAsync();
                    SemanticScreenReader.Announce("Journal entry deleted");
                }
                else
                {
                    await DisplayAlert("Error", "Failed to delete entry", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadJournalEntriesAsync();
        JournalRefreshView.IsRefreshing = false;
        SemanticScreenReader.Announce("Journal refreshed");
    }
}