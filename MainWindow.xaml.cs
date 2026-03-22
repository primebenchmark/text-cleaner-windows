using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;

namespace TextCleaner
{
    public sealed partial class MainWindow : Window
    {
        private string _lastClipboardText = string.Empty;
        private readonly DispatcherTimer _clipboardTimer;
        private AppSettings _settings;

        public MainWindow()
        {
            InitializeComponent();

            _settings = AppSettings.Load();

            // Set window icon
            AppWindow.SetIcon(System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico"));

            // Apply saved window size
            AppWindow.Resize(new Windows.Graphics.SizeInt32(
                Math.Max(_settings.WindowWidth, 600),
                Math.Max(_settings.WindowHeight, 400)));

            // Enforce minimum window size
            AppWindow.Changed += (s, args) =>
            {
                if (AppWindow.Size.Width < 600 || AppWindow.Size.Height < 400)
                {
                    AppWindow.Resize(new Windows.Graphics.SizeInt32(
                        Math.Max(AppWindow.Size.Width, 600),
                        Math.Max(AppWindow.Size.Height, 400)));
                }
            };

            // Apply saved theme
            ThemeToggle.IsOn = _settings.IsDarkTheme;
            ApplyTheme(_settings.IsDarkTheme);

            // Apply saved font size
            InputBox.FontSize = _settings.TextFontSize;
            OutputBox.FontSize = _settings.TextFontSize;

            _clipboardTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clipboardTimer.Tick += ClipboardTimer_Tick;
            _clipboardTimer.Start();
        }

        private void ApplyTheme(bool isDark)
        {
            if (Content is FrameworkElement root)
            {
                root.RequestedTheme = isDark ? ElementTheme.Dark : ElementTheme.Light;
            }

            // Customize title bar colors to match the theme
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = AppWindow.TitleBar;
                if (isDark)
                {
                    titleBar.BackgroundColor = ColorHelper.FromArgb(255, 32, 32, 32);
                    titleBar.ForegroundColor = Colors.White;
                    titleBar.InactiveBackgroundColor = ColorHelper.FromArgb(255, 32, 32, 32);
                    titleBar.InactiveForegroundColor = ColorHelper.FromArgb(255, 153, 153, 153);
                    titleBar.ButtonBackgroundColor = ColorHelper.FromArgb(255, 32, 32, 32);
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(255, 51, 51, 51);
                    titleBar.ButtonHoverForegroundColor = Colors.White;
                    titleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(255, 32, 32, 32);
                    titleBar.ButtonInactiveForegroundColor = ColorHelper.FromArgb(255, 153, 153, 153);
                }
                else
                {
                    titleBar.BackgroundColor = ColorHelper.FromArgb(255, 243, 243, 243);
                    titleBar.ForegroundColor = Colors.Black;
                    titleBar.InactiveBackgroundColor = ColorHelper.FromArgb(255, 243, 243, 243);
                    titleBar.InactiveForegroundColor = ColorHelper.FromArgb(255, 153, 153, 153);
                    titleBar.ButtonBackgroundColor = ColorHelper.FromArgb(255, 243, 243, 243);
                    titleBar.ButtonForegroundColor = Colors.Black;
                    titleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(255, 229, 229, 229);
                    titleBar.ButtonHoverForegroundColor = Colors.Black;
                    titleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(255, 243, 243, 243);
                    titleBar.ButtonInactiveForegroundColor = ColorHelper.FromArgb(255, 153, 153, 153);
                }
            }
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            _settings.IsDarkTheme = ThemeToggle.IsOn;
            ApplyTheme(ThemeToggle.IsOn);
            _settings.Save();
        }

        private async void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var rulesListView = new ListView
            {
                SelectionMode = ListViewSelectionMode.Single,
                MaxHeight = 300,
                ItemsSource = _settings.Rules.Select(r => r.ToString()).ToList()
            };

            var findBox = new TextBox { PlaceholderText = "Character(s) to find", Margin = new Thickness(0, 8, 0, 0), CornerRadius = new CornerRadius(6) };
            var replaceBox = new TextBox { PlaceholderText = "Replace with (empty = remove)", Margin = new Thickness(0, 4, 0, 0), CornerRadius = new CornerRadius(6) };

            var addButton = new Button
            {
                Content = "Add Rule",
                Margin = new Thickness(0, 4, 4, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                CornerRadius = new CornerRadius(6),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 37, 99, 235)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
            };
            var updateButton = new Button
            {
                Content = "Update Selected",
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEnabled = false,
                CornerRadius = new CornerRadius(6),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 217, 119, 6)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
            };
            var removeButton = new Button
            {
                Content = "Remove Selected",
                Margin = new Thickness(0, 4, 4, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEnabled = false,
                CornerRadius = new CornerRadius(6),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 220, 38, 38)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
            };
            var resetButton = new Button
            {
                Content = "Reset to Defaults",
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                CornerRadius = new CornerRadius(6),
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    Windows.UI.Color.FromArgb(255, 107, 114, 128)),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
            };

            var topButtonRow = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            topButtonRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topButtonRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(addButton, 0);
            Grid.SetColumn(updateButton, 1);
            topButtonRow.Children.Add(addButton);
            topButtonRow.Children.Add(updateButton);

            var bottomButtonRow = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            bottomButtonRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bottomButtonRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetColumn(removeButton, 0);
            Grid.SetColumn(resetButton, 1);
            bottomButtonRow.Children.Add(removeButton);
            bottomButtonRow.Children.Add(resetButton);

            // --- Window & Font Size settings ---
            var separator = new Border
            {
                Height = 1,
                Margin = new Thickness(0, 12, 0, 8),
                Opacity = 0.3,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray)
            };

            var appearanceHeader = new TextBlock
            {
                Text = "Appearance & Window",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 6)
            };

            var sizeGrid = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Pixel) });
            sizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var widthBox = new NumberBox
            {
                Header = "Window Width (px)",
                Value = _settings.WindowWidth,
                Minimum = 600,
                Maximum = 3840,
                SmallChange = 10,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                CornerRadius = new CornerRadius(6)
            };
            var heightBox = new NumberBox
            {
                Header = "Window Height (px)",
                Value = _settings.WindowHeight,
                Minimum = 400,
                Maximum = 2160,
                SmallChange = 10,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                CornerRadius = new CornerRadius(6)
            };
            Grid.SetColumn(widthBox, 0);
            Grid.SetColumn(heightBox, 2);
            sizeGrid.Children.Add(widthBox);
            sizeGrid.Children.Add(heightBox);

            var fontSizeBox = new NumberBox
            {
                Header = "Text Box Font Size (pt)",
                Value = _settings.TextFontSize,
                Minimum = 8,
                Maximum = 48,
                SmallChange = 1,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact,
                Margin = new Thickness(0, 4, 0, 0),
                CornerRadius = new CornerRadius(6)
            };

            var panel = new StackPanel
            {
                Width = 420,
                Children = { rulesListView, findBox, replaceBox, topButtonRow, bottomButtonRow,
                             separator, appearanceHeader, sizeGrid, fontSizeBox }
            };

            var scrollViewer = new ScrollViewer
            {
                Content = panel,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 500
            };

            void RefreshList()
            {
                rulesListView.ItemsSource = _settings.Rules.Select(r => r.ToString()).ToList();
            }

            rulesListView.SelectionChanged += (s, args) =>
            {
                int idx = rulesListView.SelectedIndex;
                bool hasSelection = idx >= 0 && idx < _settings.Rules.Count;
                updateButton.IsEnabled = hasSelection;
                removeButton.IsEnabled = hasSelection;
                if (hasSelection)
                {
                    findBox.Text = _settings.Rules[idx].Find;
                    replaceBox.Text = _settings.Rules[idx].ReplaceWith;
                }
            };

            addButton.Click += (s, args) =>
            {
                if (!string.IsNullOrEmpty(findBox.Text))
                {
                    _settings.Rules.Add(new CharacterRule
                    {
                        Find = findBox.Text,
                        ReplaceWith = replaceBox.Text ?? string.Empty
                    });
                    findBox.Text = string.Empty;
                    replaceBox.Text = string.Empty;
                    rulesListView.SelectedIndex = -1;
                    RefreshList();
                }
            };

            updateButton.Click += (s, args) =>
            {
                int idx = rulesListView.SelectedIndex;
                if (idx >= 0 && idx < _settings.Rules.Count && !string.IsNullOrEmpty(findBox.Text))
                {
                    _settings.Rules[idx].Find = findBox.Text;
                    _settings.Rules[idx].ReplaceWith = replaceBox.Text ?? string.Empty;
                    RefreshList();
                    rulesListView.SelectedIndex = idx;
                }
            };

            removeButton.Click += (s, args) =>
            {
                int idx = rulesListView.SelectedIndex;
                if (idx >= 0 && idx < _settings.Rules.Count)
                {
                    _settings.Rules.RemoveAt(idx);
                    findBox.Text = string.Empty;
                    replaceBox.Text = string.Empty;
                    RefreshList();
                }
            };

            resetButton.Click += (s, args) =>
            {
                _settings.Rules = AppSettings.GetDefaultRules();
                RefreshList();
            };

            var dialog = new ContentDialog
            {
                Title = "Character Substitution Rules",
                Content = scrollViewer,
                CloseButtonText = "Done",
                CornerRadius = new CornerRadius(12),
                XamlRoot = Content.XamlRoot,
                RequestedTheme = _settings.IsDarkTheme ? ElementTheme.Dark : ElementTheme.Light
            };

            await dialog.ShowAsync();

            // Apply window size if changed
            if (!double.IsNaN(widthBox.Value) && !double.IsNaN(heightBox.Value))
            {
                _settings.WindowWidth = (int)widthBox.Value;
                _settings.WindowHeight = (int)heightBox.Value;
                AppWindow.Resize(new Windows.Graphics.SizeInt32(_settings.WindowWidth, _settings.WindowHeight));
            }

            // Apply font size if changed
            if (!double.IsNaN(fontSizeBox.Value))
            {
                _settings.TextFontSize = fontSizeBox.Value;
                InputBox.FontSize = _settings.TextFontSize;
                OutputBox.FontSize = _settings.TextFontSize;
            }

            _settings.Save();

            // Re-process input with updated rules
            if (!string.IsNullOrEmpty(InputBox.Text))
            {
                OutputBox.Text = CleanText(InputBox.Text, _settings.Rules);
            }
        }

        private void PasteBtn_Click(object sender, RoutedEventArgs e)
        {
            var dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                _ = PasteTextAsync(dataPackageView);
            }
        }

        private async System.Threading.Tasks.Task PasteTextAsync(DataPackageView dataPackageView)
        {
            try
            {
                string text = await dataPackageView.GetTextAsync();
                InputBox.Text = text;
            }
            catch (Exception ex)
            {
                await ShowErrorDialog("Clipboard Error", $"Unable to read from clipboard.\n\n{ex.Message}");
            }
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(OutputBox.Text ?? string.Empty);
                Clipboard.SetContent(dataPackage);
            }
            catch (Exception ex)
            {
                _ = ShowErrorDialog("Clipboard Error", $"Unable to copy to clipboard.\n\n{ex.Message}");
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Text = string.Empty;
            OutputBox.Text = string.Empty;
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = InputBox.Text;

            if (string.IsNullOrEmpty(inputText))
            {
                OutputBox.Text = string.Empty;
                return;
            }

            string processed = CleanText(inputText, _settings.Rules);

            if (!string.Equals(OutputBox.Text, processed, StringComparison.Ordinal))
            {
                OutputBox.Text = processed;

                if (!string.IsNullOrEmpty(processed))
                {
                    try
                    {
                        var dataPackage = new DataPackage();
                        dataPackage.SetText(processed);
                        Clipboard.SetContent(dataPackage);
                        _lastClipboardText = processed;
                    }
                    catch { /* best-effort auto-copy */ }
                }
            }
        }

        private async void ClipboardTimer_Tick(object? sender, object e)
        {
            try
            {
                var dataPackageView = Clipboard.GetContent();
                if (!dataPackageView.Contains(StandardDataFormats.Text))
                    return;

                string text = await dataPackageView.GetTextAsync();
                if (text == _lastClipboardText)
                    return;

                _lastClipboardText = text;
                InputBox.Text = text;
            }
            catch { /* ignore clipboard access errors in timer */ }
        }

        private async System.Threading.Tasks.Task ShowErrorDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private static string CleanText(string? input, List<CharacterRule> rules)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            string result = input;

            // Apply multi-character rules first (longer Find strings first to avoid partial matches)
            foreach (var rule in rules.Where(r => r.Find.Length > 1).OrderByDescending(r => r.Find.Length))
            {
                result = result.Replace(rule.Find, rule.ReplaceWith, StringComparison.OrdinalIgnoreCase);
            }

            // Apply single-character rules via StringBuilder scan
            var singleCharRules = rules.Where(r => r.Find.Length == 1).ToList();
            if (singleCharRules.Count > 0)
            {
                var sb = new StringBuilder(result.Length);
                foreach (char ch in result)
                {
                    var match = singleCharRules.FirstOrDefault(r => r.Find[0] == ch);
                    if (match != null)
                    {
                        if (!string.IsNullOrEmpty(match.ReplaceWith))
                            sb.Append(match.ReplaceWith);
                        // else: remove (append nothing)
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                result = sb.ToString();
            }

            return result;
        }
    }
}
