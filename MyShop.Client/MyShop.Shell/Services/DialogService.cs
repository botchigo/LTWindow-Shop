using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using MyShop.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace MyShop.Shell.Services
{
    public class DialogService : IDialogService
    {      
        public async Task ShowErrorDialogAsync(string title, string message)
        {
            StackPanel contentPanel = new StackPanel { Spacing = 12 };

            FontIcon warningIcon = new FontIcon
            {
                Glyph = "\uE7BA",
                FontFamily = (FontFamily)Microsoft.UI.Xaml.Application.Current.Resources["SymbolThemeFontFamily"],
                FontSize = 32,
                Foreground = new SolidColorBrush(Colors.OrangeRed),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            TextBlock messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            contentPanel.Children.Add(warningIcon);
            contentPanel.Children.Add(messageText);

            ContentDialog dialog = new ContentDialog();
            ConfigureDialog(dialog);
            dialog.Title = title;
            dialog.Content = contentPanel;
            dialog.CloseButtonText = "Đóng";
            dialog.DefaultButton = ContentDialogButton.Close;

            await dialog.ShowAsync();
        }

        public async Task<bool> ShowConfirmAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Confirm",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close
            };

            ConfigureDialog(dialog);

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }

        public async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };

            ConfigureDialog(dialog);

            await dialog.ShowAsync();
        }

        private void ConfigureDialog(ContentDialog dialog)
        {
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                dialog.XamlRoot = rootElement.XamlRoot;
                dialog.RequestedTheme = rootElement.RequestedTheme;
            }
        }

        public async Task<bool> ShowConfirmationDialogAsync(string title, object content, string primaryButtonText = "Đồng ý", string closeButtonText = "Hủy")
        {
            var mainRoot = (App.MainWindow?.Content as UIElement)?.XamlRoot;
            if (mainRoot == null)
                return false;

            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = mainRoot;
            dialog.MaxWidth = mainRoot.Size.Width;

            // Áp dụng Style nếu có
            if (Microsoft.UI.Xaml.Application.Current.Resources.TryGetValue("LargeContentDialogStyle", out object style))
            {
                dialog.Style = style as Style;
            }

            dialog.Title = title;
            dialog.Content = content;
            dialog.PrimaryButtonText = primaryButtonText;
            dialog.CloseButtonText = closeButtonText;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.HorizontalAlignment = HorizontalAlignment.Stretch;
            dialog.VerticalAlignment = VerticalAlignment.Stretch;

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }
}
