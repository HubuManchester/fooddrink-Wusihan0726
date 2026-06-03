using System.Runtime.CompilerServices;

namespace FoodDrinkApp.Services;

public static class AccessibilityService
{
    private const double LargeTextScale = 1.22;
    private static readonly ConditionalWeakTable<BindableObject, FontSizeStore> OriginalFontSizes = new();
    private static bool _largeTextEnabled;
    private static bool _isApplying;

    public static bool LargeTextEnabled
    {
        get => _largeTextEnabled;
        set
        {
            if (_largeTextEnabled == value) return;
            _largeTextEnabled = value;
            Preferences.Set("LargeTextEnabled", value);
            OnLargeTextChanged();
        }
    }

    static AccessibilityService()
    {
        _largeTextEnabled = Preferences.Get("LargeTextEnabled", false);
    }

    private static void OnLargeTextChanged()
    {
        var currentPage = GetCurrentPage();
        if (currentPage != null)
        {
            ApplyFontScale(currentPage);

            // Force CollectionView layout refresh
            ForceRefreshCollectionViews();
        }
    }

    private static Page? GetCurrentPage()
    {
        var shell = Application.Current?.Windows?.FirstOrDefault()?.Page as Shell;
        return shell?.CurrentPage;
    }

    public static void ApplyFontScale(Element root)
    {
        if (root == null || _isApplying) return;

        _isApplying = true;

        try
        {
            var scale = LargeTextEnabled ? LargeTextScale : 1.0;

            ResetAllStoredFonts(root);

            if (scale > 1.0)
            {
                ApplyFontScaleToElementTree(root, scale);
            }
        }
        finally
        {
            _isApplying = false;
        }
    }

    private static void ResetAllStoredFonts(Element root)
    {
        ResetElementFont(root);

        var allElements = GetAllElements(root);
        foreach (var element in allElements)
        {
            ResetElementFont(element);
        }
    }

    private static void ResetElementFont(Element element)
    {
        switch (element)
        {
            case Label label:
                if (OriginalFontSizes.TryGetValue(label, out var labelStore) && labelStore.HasValue)
                {
                    label.FontSize = labelStore.Value;
                    OriginalFontSizes.Remove(label);
                }
                break;
            case Button button:
                if (OriginalFontSizes.TryGetValue(button, out var buttonStore) && buttonStore.HasValue)
                {
                    button.FontSize = buttonStore.Value;
                    OriginalFontSizes.Remove(button);
                }
                break;
            case Entry entry:
                if (OriginalFontSizes.TryGetValue(entry, out var entryStore) && entryStore.HasValue)
                {
                    entry.FontSize = entryStore.Value;
                    OriginalFontSizes.Remove(entry);
                }
                break;
            case Editor editor:
                if (OriginalFontSizes.TryGetValue(editor, out var editorStore) && editorStore.HasValue)
                {
                    editor.FontSize = editorStore.Value;
                    OriginalFontSizes.Remove(editor);
                }
                break;
            case Picker picker:
                if (OriginalFontSizes.TryGetValue(picker, out var pickerStore) && pickerStore.HasValue)
                {
                    picker.FontSize = pickerStore.Value;
                    OriginalFontSizes.Remove(picker);
                }
                break;
            case SearchBar searchBar:
                if (OriginalFontSizes.TryGetValue(searchBar, out var searchBarStore) && searchBarStore.HasValue)
                {
                    searchBar.FontSize = searchBarStore.Value;
                    OriginalFontSizes.Remove(searchBar);
                }
                break;
        }
    }

    private static void ApplyFontScaleToElementTree(Element root, double scale)
    {
        ApplyScaleToElement(root, scale);

        var allElements = GetAllElements(root);
        foreach (var element in allElements)
        {
            ApplyScaleToElement(element, scale);
        }
    }

    private static void ApplyScaleToElement(Element element, double scale)
    {
        switch (element)
        {
            case Label label:
                ScaleLabelFont(label, scale);
                break;
            case Button button:
                ScaleButtonFont(button, scale);
                break;
            case Entry entry:
                ScaleEntryFont(entry, scale);
                break;
            case Editor editor:
                ScaleEditorFont(editor, scale);
                break;
            case Picker picker:
                ScalePickerFont(picker, scale);
                break;
            case SearchBar searchBar:
                ScaleSearchBarFont(searchBar, scale);
                break;
        }
    }

    private static void ScaleLabelFont(Label label, double scale)
    {
        if (label.FontSize <= 0 || label.FontSize > 100) return;

        if (!OriginalFontSizes.TryGetValue(label, out var store) || !store.HasValue)
        {
            store = new FontSizeStore { HasValue = true, Value = label.FontSize };
            OriginalFontSizes.Add(label, store);
        }

        label.FontSize = store.Value * scale;
    }

    private static void ScaleButtonFont(Button button, double scale)
    {
        if (button.FontSize <= 0 || button.FontSize > 100) return;

        if (!OriginalFontSizes.TryGetValue(button, out var store) || !store.HasValue)
        {
            store = new FontSizeStore { HasValue = true, Value = button.FontSize };
            OriginalFontSizes.Add(button, store);
        }

        button.FontSize = store.Value * scale;
    }

    private static void ScaleEntryFont(Entry entry, double scale)
    {
        if (entry.FontSize <= 0 || entry.FontSize > 100) return;

        if (!OriginalFontSizes.TryGetValue(entry, out var store) || !store.HasValue)
        {
            store = new FontSizeStore { HasValue = true, Value = entry.FontSize };
            OriginalFontSizes.Add(entry, store);
        }

        entry.FontSize = store.Value * scale;
    }

    private static void ScaleEditorFont(Editor editor, double scale)
    {
        if (editor.FontSize <= 0 || editor.FontSize > 100) return;

        if (!OriginalFontSizes.TryGetValue(editor, out var store) || !store.HasValue)
        {
            store = new FontSizeStore { HasValue = true, Value = editor.FontSize };
            OriginalFontSizes.Add(editor, store);
        }

        editor.FontSize = store.Value * scale;
    }

    private static void ScalePickerFont(Picker picker, double scale)
    {
        if (picker.FontSize <= 0 || picker.FontSize > 100) return;

        if (!OriginalFontSizes.TryGetValue(picker, out var store) || !store.HasValue)
        {
            store = new FontSizeStore { HasValue = true, Value = picker.FontSize };
            OriginalFontSizes.Add(picker, store);
        }

        picker.FontSize = store.Value * scale;
    }

    private static void ScaleSearchBarFont(SearchBar searchBar, double scale)
    {
        if (searchBar.FontSize <= 0 || searchBar.FontSize > 100) return;

        if (!OriginalFontSizes.TryGetValue(searchBar, out var store) || !store.HasValue)
        {
            store = new FontSizeStore { HasValue = true, Value = searchBar.FontSize };
            OriginalFontSizes.Add(searchBar, store);
        }

        searchBar.FontSize = store.Value * scale;
    }

    private static IEnumerable<Element> GetAllElements(Element root)
    {
        var result = new List<Element>();
        var stack = new Stack<Element>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            result.Add(current);

            if (current is IVisualTreeElement visualTreeElement)
            {
                foreach (var child in visualTreeElement.GetVisualChildren())
                {
                    if (child is Element element)
                    {
                        stack.Push(element);
                    }
                }
            }
        }

        return result;
    }

    public static void ResetFontSizes(Element root)
    {
        if (root == null) return;

        var allElements = GetAllElements(root);
        foreach (var element in allElements)
        {
            ResetElementFont(element);
        }
    }

    private static void ForceRefreshCollectionViews()
    {
        Task.Delay(150).ContinueWith(_ =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var shell = Application.Current?.Windows?.FirstOrDefault()?.Page as Shell;
                if (shell?.CurrentPage == null) return;

                void RefreshCollectionView(string name, ContentPage page)
                {
                    var collection = page.FindByName<CollectionView>(name);
                    if (collection?.ItemsSource != null)
                    {
                        var temp = collection.ItemsSource;
                        collection.ItemsSource = null;
                        collection.ItemsSource = temp;
                    }
                }

                if (shell.CurrentPage is MainPage mainPage)
                {
                    RefreshCollectionView("FoodCollection", mainPage);
                }
                else if (shell.CurrentPage is JournalPage journalPage)
                {
                    RefreshCollectionView("JournalCollection", journalPage);
                }
            });
        });
    }

    private sealed class FontSizeStore
    {
        public bool HasValue { get; set; }
        public double Value { get; set; }
    }
}