using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

[QueryProperty(nameof(ItemId), "id")]
public partial class FoodDetailPage : ContentPage
{
    private FoodItem? currentItem;

    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    public FoodDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    protected override void OnDisappearing()
    {
        SpeechService.Stop();
        base.OnDisappearing();
    }

    private async Task LoadItemAsync(string id)
    {
        try
        {
            currentItem = await FoodCatalogService.GetByIdAsync(id);

            if (currentItem == null)
            {
                await DisplayAlert("Error", "Food item not found. It may have been deleted.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            RenderItem();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load item: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private void RenderItem()
    {
        if (currentItem is null)
        {
            NameLabel.Text = "Record not found";
            CategoryLabel.Text = "";
            CaloriesLabel.Text = "";
            MacroLabel.Text = "";
            DescriptionLabel.Text = "The selected food or drink could not be loaded.";
            AllergyLabel.Text = "";
            return;
        }

        NameLabel.Text = currentItem.Name;
        CategoryLabel.Text = currentItem.Category;
        CaloriesLabel.Text = currentItem.CaloriesLabel;
        MacroLabel.Text = currentItem.MacroSummary;
        DescriptionLabel.Text = currentItem.Description;
        AllergyLabel.Text = currentItem.AllergyNote;
        SemanticProperties.SetDescription(NameLabel, currentItem.AccessibleSummary);
    }

    private async void OnSpeakClicked(object? sender, EventArgs e)
    {
        if (currentItem is null)
        {
            await DisplayAlert("Missing record", "There is no nutrition summary to read.", "OK");
            return;
        }

        try
        {
            await SpeechService.SpeakAsync(currentItem.AccessibleSummary);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Text to speech unavailable", ex.Message, "OK");
        }
    }

    private void OnStopSpeechClicked(object? sender, EventArgs e)
    {
        SpeechService.Stop();
        SemanticScreenReader.Announce("Reading stopped.");
    }

    private async void OnVibrateClicked(object? sender, EventArgs e)
    {
        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500));
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
            await DisplayAlert("Reminder", "Vibration feedback has been triggered.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Vibration unavailable", ex.Message, "OK");
        }
    }

    private async void OnEditClicked(object? sender, EventArgs e)
    {
        if (currentItem == null)
        {
            await DisplayAlert("Error", "No item to edit", "OK");
            return;
        }

        var parameters = new Dictionary<string, object>
        {
            { "id", currentItem.Id }
        };
        await Shell.Current.GoToAsync($"{nameof(EditFoodPage)}", parameters);
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (currentItem == null) return;

        var confirm = await DisplayAlert("Delete", $"Are you sure you want to delete '{currentItem.Name}'?", "Yes", "No");
        if (!confirm) return;

        try
        {
            var success = await FoodCatalogService.DeleteAsync(currentItem.Id);
            if (success)
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
                SemanticScreenReader.Announce("Food item deleted");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error", "Failed to delete item", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}