using Database.Repositories;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyShop.ViewModels;
using MyShop.Extensions;

namespace MyShop.Views
{
    public sealed partial class DashboardPage : Page
    {
        private DashboardViewModel? _viewModel;

        public DashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string userName)
            {
                // Get ViewModel from DI
                _viewModel = App.GetService<DashboardViewModel>();
                
                // Set DataContext
                this.DataContext = _viewModel;
                
                // Subscribe to events
                _viewModel.ErrorOccurred += OnErrorOccurred;
                _viewModel.LogoutRequested += OnLogoutRequested;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                
                // Initialize with username
                _viewModel.Initialize(userName);
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Redraw chart when MonthlyRevenue changes or when loading completes
            if (e.PropertyName == nameof(DashboardViewModel.MonthlyRevenue) ||
                e.PropertyName == nameof(DashboardViewModel.IsLoading))
            {
                // Only draw when not loading and we have a viewmodel
                if (_viewModel?.IsLoading == false && _viewModel.MonthlyRevenue != null)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        DrawRevenueChart(_viewModel.MonthlyRevenue.ToList());
                    });
                }
            }
        }

        private async void OnErrorOccurred(object? sender, ErrorEventArgs e)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                await ShowErrorAsync(e.Title, e.Message);
            });
        }

        private async void OnLogoutRequested(object? sender, EventArgs e)
        {
            await DispatcherQueue.EnqueueAsync(async () =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Ðãng xu?t",
                    Content = "B?n có ch?c ch?n mu?n ðãng xu?t?",
                    PrimaryButtonText = "Có",
                    CloseButtonText = "Không",
                    XamlRoot = this.XamlRoot
                };

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    // Navigate back to login using MainWindow method
                    var mainWindow = (Application.Current as App)?.m_window as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.NavigateToLogin();
                    }
                    else if (Frame.CanGoBack)
                    {
                        Frame.GoBack();
                    }
                }
            });
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.LoadDashboardDataCommand.CanExecute(null) == true)
            {
                await _viewModel.LoadDashboardDataCommand.ExecuteAsync(null);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.RequestLogout();
        }

        private void DrawRevenueChart(List<DailyRevenue> data)
        {
            System.Diagnostics.Debug.WriteLine($"[DrawRevenueChart] Called with {data?.Count ?? 0} data points");
            
            RevenueChart.Children.Clear();

            if (data == null || data.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[DrawRevenueChart] No data, showing 'no data' message");
                var noDataText = new TextBlock
                {
                    Text = "Không có d? li?u doanh thu trong tháng này",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };
                Canvas.SetLeft(noDataText, 200);
                Canvas.SetTop(noDataText, 90);
                RevenueChart.Children.Add(noDataText);
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[DrawRevenueChart] Drawing chart with {data.Count} points");
            
            const double chartWidth = 660;
            const double chartHeight = 180;
            const double bottomMargin = 25;
            const double leftMargin = 60;

            var maxRevenue = data.Max(d => d.TotalRevenue);
            if (maxRevenue == 0) maxRevenue = 1;

            System.Diagnostics.Debug.WriteLine($"[DrawRevenueChart] Max revenue: {maxRevenue}");

            var pointSpacing = chartWidth / Math.Max(data.Count - 1, 1);

            // Draw horizontal grid lines
            for (int i = 0; i <= 4; i++)
            {
                var y = bottomMargin + (chartHeight - bottomMargin) * (1 - i / 4.0);
                var gridLine = new Line
                {
                    X1 = leftMargin,
                    Y1 = y,
                    X2 = leftMargin + chartWidth,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    StrokeThickness = 1,
                    Opacity = 0.4
                };
                RevenueChart.Children.Add(gridLine);

                var valueText = new TextBlock
                {
                    Text = $"{(maxRevenue * i / 4.0 / 1000):N0}k",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };
                Canvas.SetLeft(valueText, 0);
                Canvas.SetTop(valueText, y - 8);
                RevenueChart.Children.Add(valueText);
            }

            // Draw chart line and points
            var points = new List<Windows.Foundation.Point>();
            for (int i = 0; i < data.Count; i++)
            {
                var x = leftMargin + i * pointSpacing;
                var y = bottomMargin + (chartHeight - bottomMargin) * (1 - (double)data[i].TotalRevenue / (double)maxRevenue);
                points.Add(new Windows.Foundation.Point(x, y));

                // Draw point
                var dot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 123, 92, 214)),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 2
                };
                Canvas.SetLeft(dot, x - 5);
                Canvas.SetTop(dot, y - 5);
                RevenueChart.Children.Add(dot);

                // Date label
                if (data.Count <= 10 || i % 2 == 0 || i == data.Count - 1)
                {
                    var dateText = new TextBlock
                    {
                        Text = data[i].Day.ToString("dd"),
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Colors.Gray)
                    };
                    Canvas.SetLeft(dateText, x - 8);
                    Canvas.SetTop(dateText, chartHeight + 5);
                    RevenueChart.Children.Add(dateText);
                }
            }

            // Draw lines connecting points
            for (int i = 0; i < points.Count - 1; i++)
            {
                var line = new Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 123, 92, 214)),
                    StrokeThickness = 3
                };
                RevenueChart.Children.Add(line);
            }

            // Draw gradient area under the line
            if (points.Count > 0)
            {
                var pathFigure = new PathFigure { StartPoint = new Windows.Foundation.Point(points[0].X, chartHeight) };
                pathFigure.Segments.Add(new LineSegment { Point = points[0] });

                foreach (var point in points.Skip(1))
                {
                    pathFigure.Segments.Add(new LineSegment { Point = point });
                }

                pathFigure.Segments.Add(new LineSegment { Point = new Windows.Foundation.Point(points[points.Count - 1].X, chartHeight) });
                pathFigure.IsClosed = true;

                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                var path = new Microsoft.UI.Xaml.Shapes.Path
                {
                    Data = pathGeometry,
                    Fill = new LinearGradientBrush
                    {
                        StartPoint = new Windows.Foundation.Point(0, 0),
                        EndPoint = new Windows.Foundation.Point(0, 1),
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop { Color = Microsoft.UI.ColorHelper.FromArgb(80, 123, 92, 214), Offset = 0 },
                            new GradientStop { Color = Microsoft.UI.ColorHelper.FromArgb(10, 123, 92, 214), Offset = 1 }
                        }
                    }
                };

                RevenueChart.Children.Insert(0, path);
            }
            
            System.Diagnostics.Debug.WriteLine($"[DrawRevenueChart] Chart drawn successfully with {RevenueChart.Children.Count} elements");
        }

        private async Task ShowErrorAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }

    public class StatusColorConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "pending" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 152, 0)),
                    "paid" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 76, 175, 80)),
                    "canceled" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 244, 67, 54)),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
