using Avalonia.Styling;
using System;

namespace SolRIA.SAFT.Desktop.Services;

public interface IThemeService
{
    string CurrentTheme { get; }
    ThemeVariant CurrentThemeVariant { get; }
    void ApplySavedTheme();
    void SetTheme(string theme);
    void ToggleTheme();
    event Action<string> ThemeChanged;
}
