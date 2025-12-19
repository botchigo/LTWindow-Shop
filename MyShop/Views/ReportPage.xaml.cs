using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Database.Enums;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyShop.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Views
{
    public sealed partial class ReportPage : Page
    {
        public ReportViewModel ViewModel { get; }
        private bool _isInitialized = false;
        public ReportPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<ReportViewModel>();

            // Khởi tạo các ComboBox
            ReportTypeCombo.ItemsSource = Enum.GetValues(typeof(ReportType));
            IntervalCombo.ItemsSource = Enum.GetValues(typeof(ReportTimeInterval));

            // Giá trị mặc định
            ReportTypeCombo.SelectedIndex = 0;
            IntervalCombo.SelectedIndex = 0;
            FromDatePicker.Date = DateTimeOffset.Now.AddMonths(-1);
            ToDatePicker.Date = DateTimeOffset.Now;

            ReportTypeCombo.SelectionChanged += OnFilterChanged;
            IntervalCombo.SelectionChanged += OnFilterChanged;
            FromDatePicker.DateChanged += OnFilterChanged;
            ToDatePicker.DateChanged += OnFilterChanged;

            _isInitialized = true; // Chỉ bật lên sau khi đã cài đặt mọi thứ

            // Load tên sản phẩm vào ComboBox
            this.Loaded += async (s, e) => {
                await ViewModel.InitializeAsync();

                // Cập nhật lại danh sách lọc sau khi load xong tên
                ViewModel.CallNotifyFilteredProductNames();
            };

        }

        private void RenderChart()
        {
            if (ChartCanvas == null || ViewModel == null) return;
            if (ChartCanvas.ActualWidth <= 0 || ChartCanvas.ActualHeight <= 0) return;

            ChartCanvas.Children.Clear();

            // Tính toán kích thước vẽ
            double marginLeft = 60; double marginRight = 120;
            double marginTop = 40; double marginBottom = 60;
            double drawWidth = ChartCanvas.ActualWidth - marginLeft - marginRight;
            double drawHeight = ChartCanvas.ActualHeight - marginTop - marginBottom;

            try
            {
                // 1. Vẽ lưới dựa trên dữ liệu hiện tại
                DrawBackgroundGrid(marginLeft, marginTop, drawWidth, drawHeight);

                // 2. Chọn hàm vẽ phù hợp
                if (ViewModel.SelectedReportType == ReportType.ProductSales)
                {
                    if (ViewModel.ProductSeriesList.Count > 0)
                        DrawMultiLineChart(marginLeft, marginTop, drawWidth, drawHeight);
                }
                else
                {
                    if (ViewModel.DataPoints.Count > 0)
                        DrawBarChart(marginLeft, marginTop, drawWidth, drawHeight);
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Render Error] {ex.Message}"); }
        }
        private void DrawBackgroundGrid(double xOffset, double yOffset, double width, double height)
        {
            int gridCount = 5; // Chia làm 5 khoảng
            double maxVal = 0;
            if (ViewModel.SelectedReportType == ReportType.ProductSales && ViewModel.ProductSeriesList.Any())
                maxVal = ViewModel.ProductSeriesList.SelectMany(s => s.Points).Max(p => p.Value);
            else if (ViewModel.DataPoints.Any())
                maxVal = ViewModel.DataPoints.Max(p => Math.Max(p.Value, p.Profit));
            if (maxVal <= 0) maxVal = 1;

            // --- Vẽ Tiêu đề Trục Y (Đơn vị: Số lượng/Doanh thu) ---
            var yTitle = new TextBlock
            {
                Text = ViewModel.SelectedReportType == ReportType.ProductSales ? "Số lượng" : "Giá trị (VNĐ)",
                FontSize = 12,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.DimGray)
            };
            Canvas.SetLeft(yTitle, 10); // Đặt sát lề trái
            Canvas.SetTop(yTitle, yOffset - 40); // Căn giữa theo chiều cao
            ChartCanvas.Children.Add(yTitle);

            for (int i = 0; i <= gridCount; i++)
            {
                double y = yOffset + height - (i * (height / gridCount));

                // Vẽ đường lưới ngang
                var line = new Line
                {
                    X1 = xOffset,
                    Y1 = y,
                    X2 = xOffset + width,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.LightGray),
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 4 } // Tạo đường đứt nét
                };
                ChartCanvas.Children.Add(line);

                // Vẽ giá trị bên trái (Trục Y)
                double labelVal = (maxVal / gridCount) * i;
                var tb = new TextBlock
                {
                    Text = labelVal.ToString("N0"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
                    Width = xOffset - 10,
                    TextAlignment = TextAlignment.Right
                };
                Canvas.SetLeft(tb, 0);
                Canvas.SetTop(tb, y - 7);
                ChartCanvas.Children.Add(tb);
            }

            // --- Vẽ Tiêu đề Trục X (Thời gian) ---
            var xTitle = new TextBlock
            {
                Text = "Thời gian (" + ViewModel.SelectedInterval.ToString() + ")",
                FontSize = 12,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.DimGray),
                Width = width,
                TextAlignment = TextAlignment.Center
            };
            Canvas.SetLeft(xTitle, xOffset);
            Canvas.SetTop(xTitle, yOffset + height + 35); // Đặt dưới nhãn ngày tháng
            ChartCanvas.Children.Add(xTitle);
        }
        private void DrawBarChart(double xOffset, double yOffset, double width, double height)
        {
            double maxVal = ViewModel.DataPoints.Max(p => Math.Max(p.Value, p.Profit));
            if (maxVal <= 0) maxVal = 1;

            double spacing = width / ViewModel.DataPoints.Count;
            double barWidth = spacing * 0.3;

            // --- Vẽ Chú thích (Legend) cho biểu đồ cột ---
            double legendX = xOffset + width + 20;
            double legendY = yOffset + height / 2;         

            // Chú thích Doanh thu (Màu xanh)
            DrawRectangle(legendX, legendY, 12, 12, Microsoft.UI.Colors.CornflowerBlue);
            AddLabel("Doanh thu", legendX + 18, legendY - 2, false);

            // Chú thích Lợi nhuận (Màu xanh lá)
            DrawRectangle(legendX, legendY + 18, 12, 12, Microsoft.UI.Colors.LightGreen);
            AddLabel("Lợi nhuận", legendX + 18, legendY + 16, false);

            for (int i = 0; i < ViewModel.DataPoints.Count; i++)
            {
                var point = ViewModel.DataPoints[i];
                double xBase = xOffset + (i * spacing) + (spacing / 4);

                // Vẽ Doanh thu
                double revHeight = (point.Value / maxVal) * height;
                DrawRectangle(xBase, yOffset + height - revHeight, barWidth, revHeight, Microsoft.UI.Colors.CornflowerBlue);

                // Vẽ Lợi nhuận
                double profHeight = (point.Profit / maxVal) * height;
                DrawRectangle(xBase + barWidth + 4, yOffset + height - profHeight, barWidth, profHeight, Microsoft.UI.Colors.LightGreen);

                // Nhãn trục X
                AddLabel(point.Label, xBase, yOffset + height + 10);
            }
        }

        private void DrawRectangle(double x, double y, double w, double h, Windows.UI.Color color)
        {
            var rect = new Rectangle { Width = w, Height = h, Fill = new SolidColorBrush(color), RadiusX = 2, RadiusY = 2 };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            ChartCanvas.Children.Add(rect);
        }

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderChart();

        private void DrawMultiLineChart(double xOffset, double yOffset, double width, double height)
        {
            if (ViewModel.ProductSeriesList.Count == 0) return;

            // 1. Tìm MaxValue trên tất cả các sản phẩm để đồng bộ trục Y
            double maxVal = ViewModel.ProductSeriesList.SelectMany(s => s.Points).Max(p => p.Value);
            if (maxVal <= 0) maxVal = 1;

            // 2. Lấy danh sách nhãn trục X (tất cả các mốc thời gian duy nhất)
            var allDates = ViewModel.ProductSeriesList.SelectMany(s => s.Points)
                .Select(p => p.Date).Distinct().OrderBy(d => d).ToList();

            double spacing = width / Math.Max(allDates.Count - 1, 1);

            // 3. Vẽ từng đường sản phẩm
            foreach (var series in ViewModel.ProductSeriesList)
            {
                var points = new List<Point>();
                var colorBrush = new SolidColorBrush(series.Color);

                for (int i = 0; i < allDates.Count; i++)
                {
                    var date = allDates[i];
                    var dataPoint = series.Points.FirstOrDefault(p => p.Date == date);
                    double val = dataPoint?.Value ?? 0; // Nếu ngày đó sản phẩm không bán được -> 0

                    double x = xOffset + (i * spacing);
                    double y = yOffset + height - ((val / maxVal) * height);
                    points.Add(new Point(x, y));

                    // Vẽ điểm nút nhỏ cho từng đường
                    var dot = new Ellipse { Width = 6, Height = 6, Fill = colorBrush };
                    Canvas.SetLeft(dot, x - 3); Canvas.SetTop(dot, y - 3);
                    ChartCanvas.Children.Add(dot);

                    // Vẽ nhãn trục X (chỉ vẽ 1 lần ở đường đầu tiên)
                    if (series == ViewModel.ProductSeriesList.First())
                        AddLabel(date.ToLocalTime().ToString("dd/MM"), x - 15, yOffset + height + 10);
                }

                // Vẽ đường nối
                for (int i = 0; i < points.Count - 1; i++)
                {
                    var line = new Line
                    {
                        X1 = points[i].X,
                        Y1 = points[i].Y,
                        X2 = points[i + 1].X,
                        Y2 = points[i + 1].Y,
                        Stroke = colorBrush,
                        StrokeThickness = 2
                    };
                    ChartCanvas.Children.Add(line);
                }

                // Vẽ chú thích (Legend) ở cuối mỗi đường
                AddLegend(series.ProductName, points.Last().X + 5, points.Last().Y, colorBrush);
            }
        }

        private void AddLegend(string name, double x, double y, Brush brush)
        {
            var tb = new TextBlock { Text = name, FontSize = 10, Foreground = brush, FontWeight = Microsoft.UI.Text.FontWeights.Bold };
            Canvas.SetLeft(tb, x); Canvas.SetTop(tb, y - 10);
            ChartCanvas.Children.Add(tb);
        }

        private void AddLabel(string text, double x, double y, bool isAxisLabel = true)
        {
            if (string.IsNullOrEmpty(text)) return;

            var tb = new TextBlock
            {
                Text = text,
                FontSize = 10,
                Foreground = new SolidColorBrush(isAxisLabel ? Microsoft.UI.Colors.Gray : Microsoft.UI.Colors.Black),
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            ChartCanvas.Children.Add(tb);
        }
        private void ClearChart_Click(object sender, RoutedEventArgs e)
        {
            // Gọi lệnh từ ViewModel thay vì chỉ clear list thủ công
            ViewModel.ClearAllProductsCommand.Execute(null);

            // Vẽ lại biểu đồ trống
            RenderChart();
        }
        // Hàm xử lý chung khi bất kỳ bộ lọc nào thay đổi
        private async void OnFilterChanged(object sender, object e)
        {
            if (!_isInitialized || ViewModel == null) return;

            // Cập nhật dữ liệu từ UI vào ViewModel
            ViewModel.SelectedReportType = (ReportType)ReportTypeCombo.SelectedItem;
            ViewModel.SelectedInterval = (ReportTimeInterval)IntervalCombo.SelectedItem;
            ViewModel.StartDate = FromDatePicker.Date ?? DateTimeOffset.Now.AddMonths(-1);
            ViewModel.EndDate = ToDatePicker.Date ?? DateTimeOffset.Now;

            // Tự động tải dữ liệu
            await ViewModel.LoadReportAsync();

            // Vẽ lại biểu đồ
            RenderChart();
        }
        private async void OnProductSelected(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            if (ProductSelector.SelectedItem is string productName)
            {
                await ViewModel.LoadSingleProductReportAsync(productName);
                RenderChart();
                // Reset lại combo để người dùng có thể chọn tiếp sản phẩm khác
                ProductSelector.SelectedIndex = -1;
            }
        }
    }
}
