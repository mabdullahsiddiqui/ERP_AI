using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace ERP_AI.Desktop.Services
{
    public class ThemeService : IThemeService
    {
        private const string RegistryKey = @"HKEY_CURRENT_USER\Software\ERP_AI\Theme";
        private const string DefaultAccentColor = "#0078D4";
        private const double DefaultFontSize = 14.0;

        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        private Theme _currentTheme;
        private bool _isDarkMode;
        private string _accentColor;
        private double _fontSize;

        public Theme CurrentTheme
        {
            get => _currentTheme;
            private set
            {
                if (_currentTheme != value)
                {
                    var oldTheme = _currentTheme;
                    _currentTheme = value;
                    OnThemeChanged(oldTheme, value);
                }
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            private set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnThemeChanged(_currentTheme, _currentTheme);
                }
            }
        }

        public string AccentColor
        {
            get => _accentColor;
            private set
            {
                if (_accentColor != value)
                {
                    _accentColor = value;
                    OnThemeChanged(_currentTheme, _currentTheme);
                }
            }
        }

        public double FontSize
        {
            get => _fontSize;
            private set
            {
                if (Math.Abs(_fontSize - value) > 0.1)
                {
                    _fontSize = value;
                    OnThemeChanged(_currentTheme, _currentTheme);
                }
            }
        }

        public ThemeService()
        {
            _accentColor = DefaultAccentColor;
            _fontSize = DefaultFontSize;
            _currentTheme = Theme.Light;
            _isDarkMode = false;
            
            LoadSettings();
        }

        public void SetTheme(Theme theme)
        {
            CurrentTheme = theme;
            ApplyTheme();
        }

        public void SetAccentColor(string color)
        {
            if (IsValidColor(color))
            {
                AccentColor = color;
                ApplyAccentColor();
            }
        }

        public void SetFontSize(double size)
        {
            if (size >= 8.0 && size <= 24.0)
            {
                FontSize = size;
                ApplyFontSize();
            }
        }

        public void ToggleDarkMode()
        {
            IsDarkMode = !IsDarkMode;
            ApplyTheme();
        }

        public void ResetToDefault()
        {
            CurrentTheme = Theme.Light;
            AccentColor = DefaultAccentColor;
            FontSize = DefaultFontSize;
            IsDarkMode = false;
            ApplyTheme();
        }

        public void SaveSettings()
        {
            try
            {
                var settings = new ThemeSettings
                {
                    Theme = CurrentTheme,
                    IsDarkMode = IsDarkMode,
                    AccentColor = AccentColor,
                    FontSize = FontSize
                };

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                Registry.SetValue(RegistryKey, "ThemeSettings", json);
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                System.Diagnostics.Debug.WriteLine($"Failed to save theme settings: {ex.Message}");
            }
        }

        public void LoadSettings()
        {
            try
            {
                var json = Registry.GetValue(RegistryKey, "ThemeSettings", null) as string;
                if (!string.IsNullOrEmpty(json))
                {
                    var settings = JsonConvert.DeserializeObject<ThemeSettings>(json);
                    if (settings != null)
                    {
                        _currentTheme = settings.Theme;
                        _isDarkMode = settings.IsDarkMode;
                        _accentColor = settings.AccentColor ?? DefaultAccentColor;
                        _fontSize = settings.FontSize > 0 ? settings.FontSize : DefaultFontSize;
                        
                        ApplyTheme();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but use defaults
                System.Diagnostics.Debug.WriteLine($"Failed to load theme settings: {ex.Message}");
                ResetToDefault();
            }
        }

        private void ApplyTheme()
        {
            try
            {
                // Apply ModernWpfUI theme
                if (CurrentTheme == Theme.Auto)
                {
                    // Use system theme
                    var isSystemDark = IsSystemDarkMode();
                    IsDarkMode = isSystemDark;
                }
                else
                {
                    IsDarkMode = CurrentTheme == Theme.Dark || CurrentTheme == Theme.HighContrast;
                }

                // Apply ModernWpfUI theme
                var theme = IsDarkMode ? 
                    ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Dark :
                    ModernWpf.ThemeManager.Current.ApplicationTheme = ModernWpf.ApplicationTheme.Light;

                // Apply high contrast if needed
                if (CurrentTheme == Theme.HighContrast)
                {
                    ApplyHighContrast();
                }

                ApplyAccentColor();
                ApplyFontSize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme: {ex.Message}");
            }
        }

        private void ApplyAccentColor()
        {
            try
            {
                if (ColorConverter.ConvertFromString(AccentColor) is Color color)
                {
                    ModernWpf.ThemeManager.Current.AccentColor = color;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply accent color: {ex.Message}");
            }
        }

        private void ApplyFontSize()
        {
            try
            {
                // Apply font size to application resources
                Application.Current.Resources["SystemFontSize"] = FontSize;
                Application.Current.Resources["SystemFontSizeSmall"] = FontSize - 2;
                Application.Current.Resources["SystemFontSizeLarge"] = FontSize + 2;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply font size: {ex.Message}");
            }
        }

        private void ApplyHighContrast()
        {
            try
            {
                // Apply high contrast specific settings
                Application.Current.Resources["SystemControlBackgroundBaseLowBrush"] = new SolidColorBrush(Colors.Black);
                Application.Current.Resources["SystemControlForegroundBaseHighBrush"] = new SolidColorBrush(Colors.White);
                Application.Current.Resources["SystemControlBackgroundAccentBrush"] = new SolidColorBrush(Colors.Yellow);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply high contrast: {ex.Message}");
            }
        }

        private bool IsSystemDarkMode()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidColor(string color)
        {
            try
            {
                ColorConverter.ConvertFromString(color);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnThemeChanged(Theme oldTheme, Theme newTheme)
        {
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(newTheme, oldTheme, IsDarkMode, AccentColor, FontSize));
        }

        private class ThemeSettings
        {
            public Theme Theme { get; set; }
            public bool IsDarkMode { get; set; }
            public string AccentColor { get; set; }
            public double FontSize { get; set; }
        }
    }
}
