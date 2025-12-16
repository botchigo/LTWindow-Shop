using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Repositories;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyShop.Services;

namespace MyShop.Views
{
    // DTO class cho Low Stock Product - ð?m b?o binding ho?t ð?ng
    public class LowStockItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    public sealed partial class DashboardPage : Page
    {
        private DatabaseManager? _dbManager;
        private DashboardRepository? _dashboardRepo;
        private string _currentUserName = "User";

        public DashboardPage()
        {
            this.InitializeComponent();
            DateText.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
            MonthYearText.Text = DateTime.Now.ToString("MMMM yyyy");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<DatabaseManager, string> parameters)
            {
                _dbManager = parameters.Item1;
                _currentUserName = parameters.Item2;
                _dashboardRepo = new DashboardRepository(_dbManager.Context);

                WelcomeText.Text = $"Xin chào, {_currentUserName}!";

                _ = LoadDashboardDataAsync();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDashboardDataAsync();
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
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
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            if (_dashboardRepo == null) return;

            try
            {
                var data = await _dashboardRepo.GetDashboardDataAsync();

                // Update statistics cards
                TotalProductsText.Text = data.TotalProducts.ToString("N0");
                TodayOrdersText.Text = data.TodayOrders.ToString("N0");
                TodayRevenueText.Text = $"{data.TodayRevenue:N0} ð";
                LowStockCountText.Text = data.LowStockProducts.Count.ToString();

                // Debug: Log low stock products
                System.Diagnostics.Debug.WriteLine($"=== LOW STOCK PRODUCTS ({data.LowStockProducts.Count}) ===");
                foreach (var item in data.LowStockProducts)
                {
                    System.Diagnostics.Debug.WriteLine($"  - ProductId: {item.ProductId}, Name: '{item.Name}', Stock: {item.Stock}");
                }

                // Convert to local DTO ð? ð?m b?o binding ho?t ð?ng
                var lowStockItems = data.LowStockProducts.Select(p => new LowStockItem
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Stock = p.Stock
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"=== CONVERTED LOW STOCK ITEMS ({lowStockItems.Count}) ===");
                foreach (var item in lowStockItems)
                {
                    System.Diagnostics.Debug.WriteLine($"  - ProductId: {item.ProductId}, Name: '{item.Name}', Stock: {item.Stock}");
                }

                // Update lists
                TopProductsList.ItemsSource = data.BestSellerProducts;
                LowStockList.ItemsSource = lowStockItems;  // S? d?ng local DTO
                RecentOrdersList.ItemsSource = data.RecentOrders;

                // Draw chart
                DrawRevenueChart(data.MonthlyRevenue);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoadDashboardDataAsync] Error: {ex.Message}");
                await ShowErrorAsync("L?i t?i d? li?u", $"Không th? t?i d? li?u dashboard:\n{ex.Message}");
            }
        }

        private void DrawRevenueChart(List<DailyRevenue> data)
        {
            RevenueChart.Children.Clear();

            if (data == null || data.Count == 0)
            {
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

            const double chartWidth = 660;
            const double chartHeight = 180;
            const double bottomMargin = 25;
            const double leftMargin = 60;

            var maxRevenue = data.Max(d => d.TotalRevenue);
            if (maxRevenue == 0) maxRevenue = 1;

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

        private async Task TestLowStockData()
        {
            if (_dashboardRepo == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine("=== TESTING LOW STOCK DATA ===");
                var lowStockProducts = await _dashboardRepo.GetTop5LowStockAsync();
                System.Diagnostics.Debug.WriteLine($"Returned {lowStockProducts.Count} low stock products");
                
                if (lowStockProducts.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: No low stock products found!");
                    System.Diagnostics.Debug.WriteLine("Possible reasons:");
                    System.Diagnostics.Debug.WriteLine("1. fn_top5_low_stock() function not created in PostgreSQL");
                    System.Diagnostics.Debug.WriteLine("2. No products in database");
                    System.Diagnostics.Debug.WriteLine("3. All products have stock > 10");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR testing low stock: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
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
