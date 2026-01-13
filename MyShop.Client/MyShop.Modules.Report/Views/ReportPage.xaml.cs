using System;
using System.Linq;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.Core.Enums;
using MyShop.Core.Helpers;
using MyShop.Infrastructure;
using MyShop.Modules.Report.ViewModels;
using Windows.Foundation;
using Windows.UI;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                new { Label = "Doanh số sản phẩm", Value = ReportType.ProductSales }
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
            _isInitialized = true;

            // 3. [QUAN TRỌNG] Lắng nghe sự thay đổi dữ liệu để vẽ lại
            // Lắng nghe danh sách so sánh (Line Chart)
            ViewModel.ProductSeriesList.CollectionChanged -= OnDataChanged; // Unsubscribe để tránh leak bộ nhớ nếu reload page
            ViewModel.ProductSeriesList.CollectionChanged += OnDataChanged;

            // [MỚI] Lắng nghe danh sách doanh thu (Bar Chart) -> Đây là cái thiếu khiến mặc định không vẽ
            ViewModel.DataPoints.CollectionChanged -= OnDataChanged;
            ViewModel.DataPoints.CollectionChanged += OnDataChanged;

            await ViewModel.InitializeAsync();

            // Nếu chưa có dữ liệu thì gọi lệnh tải
            if (!ViewModel.DataPoints.Any() && !ViewModel.ProductSeriesList.Any())
            {
                await ViewModel.LoadReportCommand.ExecuteAsync(null);
            }
            else
            {
                // Nếu đã có dữ liệu (do cache hoặc chuyển trang quay lại), vẽ luôn
                RenderChart();
            }


        }
        private void OnDataChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RenderChart();
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
        private string GetFormattedDateLabel(DateTime date)
        {
            // Dựa vào ViewModel.SelectedInterval để format
            switch (ViewModel.SelectedInterval)
            {
                case ReportTimeInterval.Year:
                    return date.ToString("yyyy"); // Ví dụ: 2025
                case ReportTimeInterval.Month:
                    return date.ToString("MM/yyyy"); // Ví dụ: 12/2025
                case ReportTimeInterval.Week:
                    // Có thể hiển thị ngày đầu tuần
                    return date.ToString("dd/MM");
                case ReportTimeInterval.Day:
                default:
                    return date.ToString("dd/MM"); // Ví dụ: 30/01
            }
        }
        // Helper tạo 1 mục chú thích: [Hình màu] [Tên]
        private StackPanel CreateLegendItem(Color color, string label, bool isCircle)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Margin = new Thickness(0, 0, 15, 0) // Khoảng cách giữa các mục
            };

            // Tạo hình khối màu (Tròn cho Line, Vuông cho Bar)
            Shape shape;
            if (isCircle)
            {
                shape = new Ellipse { Width = 10, Height = 10, Fill = new SolidColorBrush(color) };
            }
            else
            {
                shape = new Rectangle { Width = 10, Height = 10, Fill = new SolidColorBrush(color), RadiusX = 2, RadiusY = 2 };
            }

            // Căn giữa hình theo chiều dọc
            shape.VerticalAlignment = VerticalAlignment.Center;

            // Tạo Text
            var textBlock = new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.DimGray),
                VerticalAlignment = VerticalAlignment.Center
            };

            panel.Children.Add(shape);
            panel.Children.Add(textBlock);

            return panel;
        }
        private void DrawLegend(double margin)
        {
            // Container chính chứa tất cả các chú thích, xếp ngang
            var legendContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            // Đặt vị trí: Cách lề trái bằng margin, cách lề trên 10px
            Canvas.SetLeft(legendContainer, margin);
            Canvas.SetTop(legendContainer, 10);

            // 1. Trường hợp Biểu đồ đường (Product Sales)
            if (ViewModel.SelectedReportType == ReportType.ProductSales)
            {
                foreach (var series in ViewModel.ProductSeriesList)
                {
                    var color = Core.Helpers.ColorHelper.FromHex(series.ColorHex);
                    // Dùng hình tròn cho biểu đồ đường
                    var item = CreateLegendItem(color, series.ProductName, isCircle: true);
                    legendContainer.Children.Add(item);
                }
            }
            // 2. Trường hợp Biểu đồ cột (Doanh thu & Lợi nhuận)
            else
            {
                // Màu cố định giống trong DrawBarChart
                var revenueItem = CreateLegendItem(Colors.CornflowerBlue, "Doanh thu", isCircle: false);
                var profitItem = CreateLegendItem(Colors.SeaGreen, "Lợi nhuận", isCircle: false);

                legendContainer.Children.Add(revenueItem);
                legendContainer.Children.Add(profitItem);
            }

            // Thêm container vào Canvas
            ChartCanvas.Children.Add(legendContainer);
        }
        private void RenderChart()
        {
            if (ChartCanvas == null || ChartCanvas.ActualWidth == 0 || ChartCanvas.ActualHeight == 0) return;

            ChartCanvas.Children.Clear();

            double margin = 40;
            double width = ChartCanvas.ActualWidth - (margin * 2);
            double height = ChartCanvas.ActualHeight - (margin * 2);

            // [MỚI] Vẽ chú thích màu ở phía trên
            DrawLegend(margin);

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

            // [MỚI] Tính step để skip label
            int labelStep = 1;
            if (data.Count > 10)
            {
                labelStep = (int)Math.Ceiling((double)data.Count / 10);
            }

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
                Canvas.SetLeft(rectRev, x); // Vị trí gốc
                Canvas.SetTop(rectRev, margin + h - hRev);
                ChartCanvas.Children.Add(rectRev);

                // [SỬA] Label Trục X
                // Chỉ vẽ nếu thỏa mãn step hoặc là điểm đầu/cuối
                if (i == 0 || i == data.Count - 1 || i % labelStep == 0)
                {
                    // Nếu item.Label từ server rỗng hoặc format xấu, bạn có thể dùng item.Date và hàm helper:
                    // string text = GetFormattedDateLabel(item.Date);
                    // Sử dụng hàm helper đã tạo ở Bước 1
                    string labelText = GetFormattedDateLabel(item.Date.DateTime);

                    var textblock = new TextBlock { 
                        Text = labelText, 
                        FontSize = 10, 
                        Foreground = new SolidColorBrush(Colors.Black) 
                    };

                    // Căn giữa text dưới cột
                    textblock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    double textWidth = textblock.DesiredSize.Width;

                    // x là cạnh trái của slot, cộng nửa slot để ra tâm
                    double centerPos = margin + (i * stepX) + (stepX / 2);

                    Canvas.SetLeft(textblock, centerPos - (textWidth / 2));
                    Canvas.SetTop(textblock, margin + h + 5);
                    ChartCanvas.Children.Add(textblock);
                }

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

            // [MỚI] Tính toán bước nhảy để vẽ Label sao cho không bị chồng chéo
            // Nếu có quá nhiều điểm (vd > 10), ta sẽ bỏ qua bớt nhãn
            int labelStep = 1;
            if (allDates.Count > 10)
            {
                labelStep = (int)Math.Ceiling((double)allDates.Count / 10);
            }

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

                    // [SỬA] Vẽ Label Ngày
                    // Chỉ vẽ label ở series đầu tiên VÀ chỉ vẽ nếu trúng bước nhảy (để tránh chồng chữ)
                    if (series == seriesList.First())
                    {
                        // Logic: Luôn vẽ điểm đầu, điểm cuối, và các điểm chia hết cho step
                        if (i == 0 || i == allDates.Count - 1 || i % labelStep == 0)
                        {
                            // Sử dụng hàm helper đã tạo ở Bước 1
                            string labelText = GetFormattedDateLabel(date.DateTime); // Lưu ý: date là DateTimeOffset nên cần .DateTime hoặc .ToLocalTime()

                            var tb = new TextBlock
                            {
                                Text = labelText,
                                FontSize = 10,
                                Foreground = new SolidColorBrush(Colors.Black) // Nên set màu chữ rõ ràng
                            };

                            // Căn giữa text so với điểm
                            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                            double textWidth = tb.DesiredSize.Width;

                            Canvas.SetLeft(tb, x - (textWidth / 2)); // Căn giữa
                            Canvas.SetTop(tb, margin + h + 5);
                            ChartCanvas.Children.Add(tb);
                        }
                    }
                }
                ChartCanvas.Children.Add(polyline);
            }
        }
    }
}
