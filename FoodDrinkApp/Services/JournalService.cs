using System.Text.Json;
using FoodDrinkApp.Models;

namespace FoodDrinkApp.Services;

public static class JournalService
{
    private const string JournalKey = "journal_entries";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static List<JournalEntry> cachedEntries = new();

    static JournalService()
    {
        LoadEntries();
    }

    private static void LoadEntries()
    {
        try
        {
            var json = Preferences.Get(JournalKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(json))
            {
                var entries = JsonSerializer.Deserialize<List<JournalEntry>>(json, JsonOptions);
                if (entries != null)
                {
                    cachedEntries = entries.OrderByDescending(e => e.Date).ToList();
                    return;
                }
            }
        }
        catch
        {
            // Fall back to empty list
        }

        cachedEntries = new List<JournalEntry>();
    }

    private static void SaveEntries()
    {
        var json = JsonSerializer.Serialize(cachedEntries, JsonOptions);
        Preferences.Set(JournalKey, json);
    }

    public static IReadOnlyList<JournalEntry> GetAllEntries()
    {
        return cachedEntries.OrderByDescending(e => e.Date).ToList();
    }

    public static IReadOnlyList<JournalEntry> GetEntriesByDate(DateTime date)
    {
        return cachedEntries
            .Where(e => e.Date.Date == date.Date)
            .OrderByDescending(e => e.Date)
            .ToList();
    }

    public static JournalEntry? GetEntryById(string id)
    {
        return cachedEntries.FirstOrDefault(e => e.Id == id);
    }

    public static async Task<JournalEntry> AddEntryAsync(JournalEntry entry)
    {
        await Task.CompletedTask;
        entry.Id = Guid.NewGuid().ToString("N");
        entry.Date = DateTime.Now;
        cachedEntries.Insert(0, entry);
        SaveEntries();
        return entry;
    }

    public static async Task<bool> UpdateEntryAsync(JournalEntry entry)
    {
        await Task.CompletedTask;
        var index = cachedEntries.FindIndex(e => e.Id == entry.Id);
        if (index >= 0)
        {
            cachedEntries[index] = entry;
            SaveEntries();
            return true;
        }
        return false;
    }

    public static async Task<bool> DeleteEntryAsync(string id)
    {
        await Task.CompletedTask;
        var entry = cachedEntries.FirstOrDefault(e => e.Id == id);
        if (entry != null)
        {
            cachedEntries.Remove(entry);
            SaveEntries();

            // Delete photo file if exists
            if (!string.IsNullOrWhiteSpace(entry.PhotoPath))
            {
                try
                {
                    File.Delete(entry.PhotoPath);
                }
                catch
                {
                    // Ignore file deletion errors
                }
            }

            return true;
        }
        return false;
    }

    public static int GetTodayEntryCount()
    {
        return cachedEntries.Count(e => e.Date.Date == DateTime.Now.Date);
    }

    public static int GetTotalPhotoCount()
    {
        return cachedEntries.Count(e => e.HasPhoto);
    }
}