using FoodDrinkApp.Services;

namespace FoodDrinkApp;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        ThemePicker.SelectedIndex = 0;
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;

        // Load username
        string savedUsername = Preferences.Get("UserName", "NutriBite User");
        UserNameLabel.Text = savedUsername;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LargeTextSwitch.IsToggled = AccessibilityService.LargeTextEnabled;
        ApplyLargeTextState();
    }

    private async void OnUserNameTapped(object? sender, TappedEventArgs e)
    {
        string currentName = UserNameLabel.Text ?? "NutriBite User";
        string result = await DisplayPromptAsync("Edit username", "Enter your name:", initialValue: currentName, maxLength: 20);

        if (!string.IsNullOrWhiteSpace(result))
        {
            UserNameLabel.Text = result;
            Preferences.Set("UserName", result);
            Announce($"Username changed to {result}");
        }
    }

    private void OnThemeChanged(object? sender, EventArgs e)
    {
        Application.Current!.UserAppTheme = ThemePicker.SelectedIndex switch
        {
            1 => AppTheme.Light,
            2 => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        Announce("Theme updated");
    }

    private void OnLargeTextToggled(object? sender, ToggledEventArgs e)
    {
        AccessibilityService.LargeTextEnabled = e.Value;
        ApplyLargeTextState();

        // Apply font scale to all pages in the current window
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.Page is Shell shell)
        {
            foreach (var page in shell.Navigation.NavigationStack)
            {
                AccessibilityService.ApplyFontScale(page);
            }
        }

        Announce(e.Value ? "Large text mode enabled" : "Large text mode disabled");
    }

    private void ApplyLargeTextState()
    {
        AccessibilityService.ApplyFontScale(this);

        LargeTextPreviewTitle.Text = AccessibilityService.LargeTextEnabled
            ? "Font preview (enlarged)"
            : "Font preview";

        LargeTextPreviewBody.Text = AccessibilityService.LargeTextEnabled
            ? "Text is now larger. Other pages will also use this setting."
            : "Enable the switch above to enlarge text across the app.";
    }

    private void Announce(string message)
    {
        SettingsStatusLabel.Text = message;
        SemanticScreenReader.Announce(message);
    }
}