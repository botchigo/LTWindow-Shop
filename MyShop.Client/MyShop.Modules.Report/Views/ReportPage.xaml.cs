using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.Core.Enums;
using MyShop.Core.Helpers;
using MyShop.Infrastructure;
using MyShop.Modules.Report.ViewModels;
using System;
using System.Linq;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Report.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ReportPage : Page
    {
        public ReportViewModel ViewModel { get; }
        private bool _isInitialized = false;
        public ReportPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<ReportViewModel>();
            DataContext = ViewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //set up report type
            var reportTypes = new[]
            {
                new { Label = "Doanh thu & Lợi nhuận", Value = ReportType.RevenueProfit },
                new { Label = "So sánh sản phẩm", Value = ReportType.ProductSales }
            };
            ReportTypeCombo.ItemsSource = reportTypes;
            ReportTypeCombo.DisplayMemberPath = "Label"; 
            ReportTypeCombo.SelectedValuePath = "Value"; 

            //setup interval
            var intervals = new[]
        {
            new { Label = "Theo Ngày", Value = ReportTimeInterval.Day },
            new { Label = "Theo Tuần", Value = ReportTimeInterval.Week },
            new { Label = "Theo Tháng", Value = ReportTimeInterval.Month },
            new { Label = "Theo Năm", Value = ReportTimeInterval.Year }
        };
            IntervalCombo.ItemsSource = intervals;
            IntervalCombo.DisplayMemberPath = "Label";  
            IntervalCombo.SelectedValuePath = "Value";  

            _isInitialized = false;

            ReportTypeCombo.SelectedValue = ViewModel.SelectedReportType;
            IntervalCombo.SelectedValue = ViewModel.SelectedInterval;

            FromDatePicker.Date = ViewModel.StartDate;
            ToDatePicker.Date = ViewModel.EndDate;

            await ViewModel.InitializeAsync();

            await ViewModel.LoadReportCommand.ExecuteAsync(null);

            ViewModel.ProductSeriesList.CollectionChanged += (s, args) =>
            {
                RenderChart();
            };

            _isInitialized = true;
        }

        private async void OnFilterChanged(object sender, object e)
        {
            if (!_isInitialized) return;

            // Sync UI -> ViewModel
            if (ReportTypeCombo.SelectedValue is ReportType type) ViewModel.SelectedReportType = type;
            if (IntervalCombo.SelectedValue is ReportTimeInterval interval) ViewModel.SelectedInterval = interval;

            if (FromDatePicker.Date.HasValue) ViewModel.StartDate = FromDatePicker.Date.Value;
            if (ToDatePicker.Date.HasValue) ViewModel.EndDate = ToDatePicker.Date.Value;

            // Reload Data
            await ViewModel.LoadReportCommand.ExecuteAsync(null);

            // Redraw
            RenderChart();
        }

        private async void OnProductSelected(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            if (ProductSelector.SelectedItem is string productName)
            {
                await ViewModel.LoadSingleProductReportAsync(productName);
                RenderChart();

                // Reset combo box để chọn tiếp
                ProductSelector.SelectedIndex = -1;
            }
        }

        private void ClearChart_Click(object sender, RoutedEventArgs e)
        {
            // ViewModel đã xử lý clear data, ta chỉ cần vẽ lại
            RenderChart();
        }

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderChart();


        // --- LOGIC VẼ BIỂU ĐỒ (CANVAS) ---

        private void RenderChart()
        {
            if (ChartCanvas == null || ChartCanvas.ActualWidth == 0 || ChartCanvas.ActualHeight == 0) return;

            ChartCanvas.Children.Clear();

            double margin = 40;
            double width = ChartCanvas.ActualWidth - (margin * 2);
            double height = ChartCanvas.ActualHeight - (margin * 2);

            // Vẽ khung & lưới nền
            DrawGrid(margin, width, height);

            if (ViewModel.SelectedReportType == ReportType.ProductSales)
            {
                if (ViewModel.ProductSeriesList.Any())
                    DrawLineChart(margin, width, height);
            }
            else
            {
                if (ViewModel.DataPoints.Any())
                    DrawBarChart(margin, width, height);
            }
        }

        private void DrawGrid(double margin, double w, double h)
        {
            // Tìm Max Value để chia tỉ lệ trục Y
            double maxVal = 100; // Default
            if (ViewModel.SelectedReportType == ReportType.ProductSales && ViewModel.ProductSeriesList.Any())
                maxVal = ViewModel.ProductSeriesList.SelectMany(s => s.Points).Max(p => p.Value);
            else if (ViewModel.DataPoints.Any())
                maxVal = ViewModel.DataPoints.Max(p => Math.Max(p.Value, p.Profit));

            if (maxVal == 0) maxVal = 10;

            // Vẽ 5 đường ngang
            int steps = 5;
            for (int i = 0; i <= steps; i++)
            {
                double y = margin + h - (i * (h / steps));

                // Line
                var line = new Line
                {
                    X1 = margin,
                    Y1 = y,
                    X2 = margin + w,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Colors.LightGray),
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 2, 2 } // Nét đứt
                };
                ChartCanvas.Children.Add(line);

                // Label Trục Y
                double val = (maxVal / steps) * i;
                var tb = new TextBlock
                {
                    Text = val.ToString("N0"),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };
                Canvas.SetLeft(tb, 5); // Sát lề trái
                Canvas.SetTop(tb, y - 8);
                ChartCanvas.Children.Add(tb);
            }
        }

        private void DrawBarChart(double margin, double w, double h)
        {
            var data = ViewModel.DataPoints;
            double maxVal = data.Max(p => Math.Max(p.Value, p.Profit));
            if (maxVal == 0) return;

            double colWidth = (w / data.Count) * 0.6; // Độ rộng cột
            double stepX = w / data.Count;

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                double x = margin + (i * stepX) + (stepX - colWidth) / 2;

                // 1. Cột Doanh Thu (Blue)
                double hRev = (item.Value / maxVal) * h;
                var rectRev = new Rectangle
                {
                    Width = colWidth / 2,
                    Height = hRev,
                    Fill = new SolidColorBrush(Colors.CornflowerBlue)
                };
                Canvas.SetLeft(rectRev, x);
                Canvas.SetTop(rectRev, margin + h - hRev);
                ChartCanvas.Children.Add(rectRev);

                // 2. Cột Lợi Nhuận (Green)
                double hProf = (item.Profit / maxVal) * h;
                var rectProf = new Rectangle
                {
                    Width = colWidth / 2,
                    Height = hProf,
                    Fill = new SolidColorBrush(Colors.SeaGreen)
                };
                Canvas.SetLeft(rectProf, x + colWidth / 2); // Vẽ sát cạnh
                Canvas.SetTop(rectProf, margin + h - hProf);
                ChartCanvas.Children.Add(rectProf);

                // 3. Label Trục X (Ngày)
                var tb = new TextBlock { Text = item.Label, FontSize = 10, Foreground = new SolidColorBrush(Colors.Black) };
                Canvas.SetLeft(tb, x);
                Canvas.SetTop(tb, margin + h + 5);
                ChartCanvas.Children.Add(tb);
            }
        }

        private void DrawLineChart(double margin, double w, double h)
        {
            var seriesList = ViewModel.ProductSeriesList;
            double maxVal = seriesList.SelectMany(s => s.Points).Max(p => p.Value);
            if (maxVal == 0) return;

            // Lấy tập hợp tất cả các ngày (Trục X)
            var allDates = seriesList.SelectMany(s => s.Points).Select(p => p.Date).Distinct().OrderBy(d => d).ToList();
            double stepX = w / Math.Max(1, allDates.Count - 1);

            foreach (var series in seriesList)
            {
                // Convert Hex String -> Color -> Brush
                var color = Core.Helpers.ColorHelper.FromHex(series.ColorHex);
                var brush = new SolidColorBrush(color);

                Polyline polyline = new Polyline { Stroke = brush, StrokeThickness = 2 };

                for (int i = 0; i < allDates.Count; i++)
                {
                    var date = allDates[i];
                    var point = series.Points.FirstOrDefault(p => p.Date == date);
                    double val = point?.Value ?? 0;

                    double x = margin + (i * stepX);
                    double y = margin + h - ((val / maxVal) * h);

                    polyline.Points.Add(new Point(x, y));

                    // Vẽ điểm tròn (Dot)
                    var dot = new Ellipse { Width = 8, Height = 8, Fill = brush };
                    Canvas.SetLeft(dot, x - 4);
                    Canvas.SetTop(dot, y - 4);
                    ChartCanvas.Children.Add(dot);

                    // Vẽ Label Ngày (Chỉ vẽ 1 lần ở vòng lặp đầu tiên của series đầu tiên để đỡ rối)
                    if (series == seriesList.First())
                    {
                        var tb = new TextBlock { Text = date.ToLocalTime().ToString("dd/MM"), FontSize = 10 };
                        Canvas.SetLeft(tb, x - 10);
                        Canvas.SetTop(tb, margin + h + 5);
                        ChartCanvas.Children.Add(tb);
                    }
                }
                ChartCanvas.Children.Add(polyline);
            }
        }
    }
}
