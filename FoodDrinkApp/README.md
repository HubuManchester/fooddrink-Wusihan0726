# NutriBite - Food & Drink Tracking App

A cross-platform mobile application developed with .NET MAUI for the "Food and Drink" themed coursework assignment. NutriBite helps users track food nutrition, maintain a meal journal, and demonstrates various mobile hardware capabilities.

## Repository

**GitHub Repository:** https://github.com/HubuManchester/fooddrink-Wusihan0726/

**Student Name:** Sihan Wu

**Student ID:** 21906375

## Table of Contents

- [Project Overview](#project-overview)
- [Features](#features)
- [Assessment Criteria Coverage](#assessment-criteria-coverage)
- [Project Structure](#project-structure)
- [Hardware Features Implemented](#hardware-features-implemented)
- [Accessibility Features](#accessibility-features)
- [API Configuration](#api-configuration)
- [Build and Run](#build-and-run)
- [Screencast Checklist](#screencast-checklist)
- [GitHub Usage](#github-usage)
- [Technologies Used](#technologies-used)

## Project Overview

| Property | Value |
|----------|-------|
| Application Name | NutriBite |
| Theme | Food and Drink |
| Framework | .NET MAUI |
| Target Platforms | Android, Windows, iOS, MacCatalyst |
| Minimum Android Version | API 21 (Android 5.0) |
| Minimum Windows Version | 10.0.17763.0 |

## Features

### Core Functionality
- Food and beverage catalog with search functionality
- Detailed nutrition information display (calories, protein, carbs, fat)
- Add, edit, and delete food records
- Meal journal with photo attachment
- Location tagging for meal entries
- Text-to-speech for nutrition summaries and journal notes

### Hardware Integration
- Camera for meal photography
- GPS location with reverse geocoding
- Text-to-speech synthesis
- Vibration feedback
- Haptic feedback
- Accelerometer / shake detection for meal inspiration

### User Experience
- Light and dark theme support
- Large text mode for accessibility
- Screen reader support with semantic properties
- Pull-to-refresh on list pages
- Form validation with user-friendly error messages

## Assessment Criteria Coverage

| Criterion | Weight | Implementation Status |
|-----------|--------|----------------------|
| UI/UX Design and Accessibility | 30% | XAML pages, gradient backgrounds, rounded cards, theme switching, semantic properties, screen reader announcements |
| Use of Mobile Hardware | 20% | Camera, GPS/Geocoding, Text-to-Speech, Vibration, Haptic Feedback, Accelerometer |
| Functionality | 20% | Food catalog, search, CRUD operations, meal journal, settings |
| Validation and Error Handling | 10% | Required field validation, numeric validation, permission handling, user-friendly error messages |
| Code Quality | 10% | Models/Services separation, clear naming conventions, reusable services |
| Deployment | 5% | Cross-platform support for Android and Windows |
| GitHub Usage | 5% | Regular commits, meaningful commit messages, repository management |


## Hardware Features Implemented

| No. | Hardware | API Used | Integration Location |
|-----|----------|----------|----------------------|
| 1 | Camera | MediaPicker.Default.CapturePhotoAsync() | AddJournalPage, HardwarePage |
| 2 | GPS / Location | Geolocation.Default.GetLocationAsync() | AddJournalPage, HardwarePage |
| 3 | Geocoding | Geocoding.Default.GetPlacemarksAsync() | AddJournalPage, HardwarePage |
| 4 | Text-to-Speech | TextToSpeech.Default.SpeakAsync() | FoodDetailPage, JournalDetailPage, AddJournalPage, HardwarePage |
| 5 | Vibration | Vibration.Default.Vibrate() | AddItemPage, EditFoodPage, FoodDetailPage, HardwarePage |
| 6 | Haptic Feedback | HapticFeedback.Default.Perform() | JournalDetailPage, AddJournalPage, HardwarePage |
| 7 | Accelerometer / Shake | Accelerometer.Default | AddJournalPage (shake for meal inspiration), HardwarePage |

## Accessibility Features

### WCAG Principles Addressed

| Principle | Implementation |
|-----------|----------------|
| Perceivable (1.4.3 Contrast) | Sufficient contrast ratio between text and background in both light and dark themes |
| Perceivable (1.4.4 Resize Text) | Large text mode scales all UI text by 1.22x |
| Operable (2.5.5 Target Size) | Buttons have minimum height/width of 44 density-independent pixels |
| Understandable (3.3.1 Error Identification) | Clear error messages with visual and haptic feedback |
| Robust (4.1.2 Name, Role, Value) | SemanticProperties for all interactive elements |

### Semantic Properties Used

```csharp
// Headings
SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.Level1);

// Hints for screen readers
SemanticProperties.SetHint(button, "Save this journal entry");

// Active announcements
SemanticScreenReader.Announce("Journal entry saved");
```

### Large Text Mode

The AccessibilityService dynamically scales font sizes across all pages:
- Labels, Buttons, Entries, Editors, Pickers, SearchBars are all supported
- Original font sizes are cached and restored when large text mode is disabled
- Scales text by 1.22x when enabled

## API Configuration

The application uses mockapi.io as a backend for food catalog data.

**API Endpoint:** https://69ef02b69163f839f8934560.mockapi.io/foods

Configuration file: `Services/MockApiConfig.cs`

```csharp
public static class MockApiConfig
{
    public const string EndpointUrl = "https://69ef02b69163f839f8934560.mockapi.io/foods";
    public static bool IsConfigured => !string.IsNullOrWhiteSpace(EndpointUrl);
}
```

If the API is not available, the app falls back to local sample data.

## Build and Run

### Prerequisites

- Visual Studio 2022 with .NET MAUI workload
- .NET 9 SDK
- Android SDK (for Android deployment)
- Windows 10 version 19041 or later (for Windows deployment)

### Build Commands

```powershell
# Build for Windows
dotnet build .\FoodDrinkApp.csproj -f net9.0-windows10.0.19041.0

# Build for Android
dotnet build .\FoodDrinkApp.csproj -f net9.0-android

# Run on Android emulator or device
dotnet build .\FoodDrinkApp.csproj -f net9.0-android -t:Run
```

### Android Permissions

The following permissions are declared in `Platforms/Android/AndroidManifest.xml`:

<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.VIBRATE" />

<uses-feature android:name="android.hardware.camera" android:required="false" />
<uses-feature android:name="android.hardware.location.gps" android:required="false" />
<uses-feature android:name="android.hardware.sensor.accelerometer" android:required="false" />

## Technologies Used

- .NET MAUI - Cross-platform UI framework
- XAML - Declarative UI markup
- C# 12 - Primary programming language
- Microsoft.Maui.Controls - UI controls
- Microsoft.Extensions.Logging - Debug logging
- MediaPicker - Camera integration
- Geolocation - GPS positioning
- Geocoding - Reverse address lookup
- TextToSpeech - Voice synthesis
- Vibration - Haptic feedback
- HapticFeedback - Touch response
- Accelerometer - Motion detection

## Submission Information

| Item | Details |
|------|---------|
| Course | Developing a Cross-Platform Mobile App |
| Assignment | 6G6Z0014_1CWK100 |
| Submission Date | June 3, 2026 |
| Submission Method | GitHub repository + Screencast on Xuexitong |

## Author

**Sihan Wu**

Student ID: 21906375

GitHub: https://github.com/HubuManchester/fooddrink-Wusihan0726/
