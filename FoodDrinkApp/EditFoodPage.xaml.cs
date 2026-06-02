using FoodDrinkApp.Models;
using FoodDrinkApp.Services;

namespace FoodDrinkApp;

[QueryProperty(nameof(ItemId), "id")]
public partial class EditFoodPage : ContentPage
{
    private FoodItem? currentItem;

    public string ItemId
    {
        set => _ = LoadItemAsync(value);
    }

    public EditFoodPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AccessibilityService.ApplyFontScale(this);
    }

    private async Task LoadItemAsync(string id)
    {
        currentItem = await FoodCatalogService.GetByIdAsync(id);
        if (currentItem == null)
        {
            await DisplayAlert("Error", "Item not found", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        NameEntry.Text = currentItem.Name;
        CategoryEntry.Text = currentItem.Category;
        DescriptionEditor.Text = currentItem.Description;
        CaloriesEntry.Text = currentItem.Calories.ToString();
        ProteinEntry.Text = currentItem.Protein.ToString();
        CarbsEntry.Text = currentItem.Carbs.ToString();
        FatEntry.Text = currentItem.Fat.ToString();
        AllergyEntry.Text = currentItem.AllergyNote;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            var validationMessage = ValidateForm(out int calories, out int protein, out int carbs, out int fat);
            if (validationMessage is not null)
            {
                ShowValidation(validationMessage);
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(250));
                return;
            }

            if (currentItem == null) return;

            currentItem.Name = NameEntry.Text!.Trim();
            currentItem.Category = CategoryEntry.Text!.Trim();
            currentItem.Description = DescriptionEditor.Text!.Trim();
            currentItem.Calories = calories;
            currentItem.Protein = protein;
            currentItem.Carbs = carbs;
            currentItem.Fat = fat;
            currentItem.AllergyNote = string.IsNullOrWhiteSpace(AllergyEntry.Text)
                ? "No allergy note provided."
                : AllergyEntry.Text.Trim();
            currentItem.Tags = $"{currentItem.Name} {currentItem.Category} {currentItem.Description}";

            var success = await FoodCatalogService.UpdateAsync(currentItem);

            if (success)
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                SemanticScreenReader.Announce("Food record updated");
                await DisplayAlert("Success", "The record has been updated successfully.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await DisplayAlert("Error", "Failed to update record. The API may not support updates.", "OK");
            }
        }
        catch (Exception ex)
        {
            ShowValidation($"Update failed: {ex.Message}");
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private string? ValidateForm(out int calories, out int protein, out int carbs, out int fat)
    {
        calories = protein = carbs = fat = 0;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            return "Please enter a food or drink name.";
        }

        if (string.IsNullOrWhiteSpace(CategoryEntry.Text))
        {
            return "Please enter a category.";
        }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        {
            return "Please add a short description.";
        }

        return TryReadNumber(CaloriesEntry.Text, "calories", out calories)
            ?? TryReadNumber(ProteinEntry.Text, "protein", out protein)
            ?? TryReadNumber(CarbsEntry.Text, "carbs", out carbs)
            ?? TryReadNumber(FatEntry.Text, "fat", out fat);
    }

    private static string? TryReadNumber(string? value, string fieldName, out int number)
    {
        if (int.TryParse(value, out number) && number >= 0)
        {
            return null;
        }

        return $"Please enter a valid non-negative number for {fieldName}.";
    }

    private void ShowValidation(string message)
    {
        ValidationLabel.Text = message;
        ValidationPanel.IsVisible = true;
        SemanticScreenReader.Announce(message);

        // Auto hide validation panel after 3 seconds
        Task.Run(async () =>
        {
            await Task.Delay(3000);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ValidationPanel.IsVisible = false;
            });
        });
    }
}