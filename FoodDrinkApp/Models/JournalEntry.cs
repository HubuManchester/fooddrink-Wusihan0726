using System.Text.Json.Serialization;

namespace FoodDrinkApp.Models;

public sealed class JournalEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("date")]
    public DateTime Date { get; set; } = DateTime.Now;

    [JsonPropertyName("mealType")]
    public string MealType { get; set; } = "Lunch";

    [JsonPropertyName("photoPath")]
    public string? PhotoPath { get; set; }

    [JsonPropertyName("locationAddress")]
    public string? LocationAddress { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonIgnore]
    public string DisplayTitle => $"{MealType} - {Date:MMM dd, yyyy}";

    [JsonIgnore]
    public string DisplayTime => Date.ToString("hh:mm tt");

    [JsonIgnore]
    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
}