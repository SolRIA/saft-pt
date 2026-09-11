using Avalonia;
using Avalonia.Styling;
using SolRIA.SAFT.Desktop.Models;
using System;

namespace SolRIA.SAFT.Desktop.Services;

public class ThemeService : IThemeService
{
    public const string ThemeSystem = "System";
    public const string ThemeLight = "Light";
    public const string ThemeDark = "Dark";

    private string _currentTheme = ThemeSystem;

    public string CurrentTheme => _currentTheme;

    public ThemeVariant CurrentThemeVariant => _currentTheme switch
    {
        ThemeLight => ThemeVariant.Light,
        ThemeDark => ThemeVariant.Dark,
        _ => ThemeVariant.Default
    };

    public event Action<string> ThemeChanged;

    public void ApplySavedTheme()
    {
        var prefs = Preferences.Load();
        _currentTheme = string.IsNullOrWhiteSpace(prefs.Theme) ? ThemeSystem : prefs.Theme;

        var app = Application.Current;
        if (app != null)
        {
            app.RequestedThemeVariant = CurrentThemeVariant;
        }

        ThemeChanged?.Invoke(_currentTheme);
    }

    public void SetTheme(string theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
            theme = ThemeSystem;

        _currentTheme = theme;

        var app = Application.Current;
        if (app != null)
        {
            app.RequestedThemeVariant = CurrentThemeVariant;
        }

        var prefs = Preferences.Load();
        prefs.Theme = _currentTheme;
        Preferences.Save(prefs);

        ThemeChanged?.Invoke(_currentTheme);
    }

    public void ToggleTheme()
    {
        // Cycle: System -> Light -> Dark -> System
        var next = _currentTheme switch
        {
            ThemeSystem => ThemeLight,
            ThemeLight => ThemeDark,
            _ => ThemeSystem
        };

        SetTheme(next);
    }
}
