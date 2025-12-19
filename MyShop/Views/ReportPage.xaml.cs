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
        private ReportViewModel _viewModel;

        public ReportPage()
        {
            this.InitializeComponent();
            _viewModel = App.GetService<ReportViewModel>();

            // Khởi tạo các ComboBox
            ReportTypeCombo.ItemsSource = Enum.GetValues(typeof(ReportType));
            IntervalCombo.ItemsSource = Enum.GetValues(typeof(ReportTimeInterval));

            // Giá trị mặc định
            ReportTypeCombo.SelectedIndex = 0;
            IntervalCombo.SelectedIndex = 0;
            FromDatePicker.Date = DateTimeOffset.Now.AddMonths(-1);
            ToDatePicker.Date = DateTimeOffset.Now;
        }

        private async void RefreshReport_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectedReportType = (ReportType)ReportTypeCombo.SelectedItem;
            _viewModel.SelectedInterval = (ReportTimeInterval)IntervalCombo.SelectedItem;
            _viewModel.StartDate = FromDatePicker.Date ?? DateTimeOffset.Now.AddMonths(-1);
            _viewModel.EndDate = ToDatePicker.Date ?? DateTimeOffset.Now;

            await _viewModel.LoadReportAsync();
            RenderChart();
        }

        private void RenderChart()
        {
            if (ChartCanvas == null || _viewModel == null) return;
            if (ChartCanvas.ActualWidth <= 0 || ChartCanvas.ActualHeight <= 0) return;

            ChartCanvas.Children.Clear();
            if (_viewModel.DataPoints == null || _viewModel.DataPoints.Count == 0) return;

            // Thiết lập Margin để thu nhỏ Canvas hiển thị (Padding nội bộ)
            double marginLeft = 60;   // Chừa chỗ cho nhãn trục Y
            double marginRight = 40;
            double marginTop = 40;
            double marginBottom = 60; // Chừa chỗ cho nhãn trục X

            double drawWidth = ChartCanvas.ActualWidth - marginLeft - marginRight;
            double drawHeight = ChartCanvas.ActualHeight - marginTop - marginBottom;

            try
            {
                // 1. Vẽ lưới ngang (Grid lines) và Nhãn trục Y
                DrawBackgroundGrid(marginLeft, marginTop, drawWidth, drawHeight);

                // 2. Vẽ dữ liệu chính
                if (_viewModel.SelectedReportType == ReportType.ProductSales)
                    DrawLineChart(marginLeft, marginTop, drawWidth, drawHeight);
                else
                    DrawBarChart(marginLeft, marginTop, drawWidth, drawHeight);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Render Error] {ex.Message}");
            }
        }
        private void DrawBackgroundGrid(double xOffset, double yOffset, double width, double height)
        {
            int gridCount = 5; // Chia làm 5 khoảng
            double maxVal = _viewModel.DataPoints.Max(p => Math.Max(p.Value, p.Profit));
            if (maxVal <= 0) maxVal = 1;

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
                    StrokeDashArray = new DoubleCollection { 4, 4 } // Đường đứt nét
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
        }
        private void DrawBarChart(double xOffset, double yOffset, double width, double height)
        {
            double maxVal = _viewModel.DataPoints.Max(p => Math.Max(p.Value, p.Profit));
            if (maxVal <= 0) maxVal = 1;

            double spacing = width / _viewModel.DataPoints.Count;
            double barWidth = spacing * 0.3;

            for (int i = 0; i < _viewModel.DataPoints.Count; i++)
            {
                var point = _viewModel.DataPoints[i];
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

        private void DrawLineChart(double xOffset, double yOffset, double width, double height)
        {
            double maxVal = _viewModel.DataPoints.Max(p => p.Value);
            if (maxVal <= 0) maxVal = 1;

            // Tính khoảng cách giữa các điểm trên trục X
            double spacing = width / Math.Max(_viewModel.DataPoints.Count - 1, 1);
            var points = new List<Point>();

            // 1. Tính toán tọa độ tất cả các điểm trước
            for (int i = 0; i < _viewModel.DataPoints.Count; i++)
            {
                var point = _viewModel.DataPoints[i];
                double x = xOffset + (i * spacing);
                double ratio = point.Value / maxVal;
                double y = yOffset + height - (ratio * height);

                points.Add(new Point(x, y));
            }

            // 2. Vẽ các đường nối (Polyline hoặc Line)
            for (int i = 0; i < points.Count - 1; i++)
            {
                var line = new Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.CornflowerBlue),
                    StrokeThickness = 3,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                ChartCanvas.Children.Add(line);
            }

            // 3. Vẽ các điểm nút (Dots) đè lên đường nối
            for (int i = 0; i < points.Count; i++)
            {
                // Vẽ vòng tròn điểm
                var dot = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(Microsoft.UI.Colors.White),
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.CornflowerBlue),
                    StrokeThickness = 2
                };
                Canvas.SetLeft(dot, points[i].X - 5);
                Canvas.SetTop(dot, points[i].Y - 5);
                ChartCanvas.Children.Add(dot);

                // Thêm nhãn trục X (Ngày/Tháng)
                AddLabel(_viewModel.DataPoints[i].Label, points[i].X - 20, yOffset + height + 10);
            }
        }

        private void DrawRectangle(double x, double y, double w, double h, Windows.UI.Color color)
        {
            var rect = new Rectangle { Width = w, Height = h, Fill = new SolidColorBrush(color), RadiusX = 2, RadiusY = 2 };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            ChartCanvas.Children.Add(rect);
        }

        private void AddLabel(string text, double x, double y)
        {
            var tb = new TextBlock { Text = text, FontSize = 10, Foreground = new SolidColorBrush(Colors.Gray) };
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            ChartCanvas.Children.Add(tb);
        }

        private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => RenderChart();
    }
}
