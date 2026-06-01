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

    private async Task LoadJournalEntriesAsync()
    {
        var entries = JournalService.GetAllEntries();
        JournalCollection.ItemsSource = entries;
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

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadJournalEntriesAsync();
        JournalRefreshView.IsRefreshing = false;
        SemanticScreenReader.Announce("Journal refreshed");
    }
}