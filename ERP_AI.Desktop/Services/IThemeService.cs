using System;

namespace ERP_AI.Desktop.Services
{
    public interface IThemeService
    {
        event EventHandler<ThemeChangedEventArgs> ThemeChanged;
        
        Theme CurrentTheme { get; }
        bool IsDarkMode { get; }
        string AccentColor { get; }
        double FontSize { get; }
        
        void SetTheme(Theme theme);
        void SetAccentColor(string color);
        void SetFontSize(double size);
        void ToggleDarkMode();
        void ResetToDefault();
        
        void SaveSettings();
        void LoadSettings();
    }

    public enum Theme
    {
        Light,
        Dark,
        HighContrast,
        Auto
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public Theme NewTheme { get; }
        public Theme OldTheme { get; }
        public bool IsDarkMode { get; }
        public string AccentColor { get; }
        public double FontSize { get; }

        public ThemeChangedEventArgs(Theme newTheme, Theme oldTheme, bool isDarkMode, string accentColor, double fontSize)
        {
            NewTheme = newTheme;
            OldTheme = oldTheme;
            IsDarkMode = isDarkMode;
            AccentColor = accentColor;
            FontSize = fontSize;
        }
    }
}
