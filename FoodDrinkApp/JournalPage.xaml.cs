using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class JournalPage : ContentPage
{
    private IReadOnlyList<JournalEntry> allEntries = new List<JournalEntry>();

    public JournalPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyFontScale();
        _ = LoadJournalEntriesAsync();
    }

    private void ApplyFontScale()
    {
        AccessibilityService.ApplyFontScale(this);
    }

    private async Task LoadJournalEntriesAsync(string? query = null)
    {
        allEntries = JournalService.GetAllEntries();

        IReadOnlyList<JournalEntry> dataToShow;
        if (string.IsNullOrWhiteSpace(query))
        {
            dataToShow = allEntries;
        }
        else
        {
            var filtered = allEntries.Where(e =>
                e.MealType.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (e.LocationAddress != null && e.LocationAddress.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                e.Notes.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            dataToShow = filtered;
        }

        JournalCollection.ItemsSource = dataToShow;

        await Task.Delay(50);
        ApplyFontScale();
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

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        await LoadJournalEntriesAsync(e.NewTextValue);
    }

    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        await LoadJournalEntriesAsync(SearchJournalBar.Text);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadJournalEntriesAsync(SearchJournalBar.Text);
        JournalRefreshView.IsRefreshing = false;
        ApplyFontScale();
        SemanticScreenReader.Announce("Journal refreshed");
    }
}