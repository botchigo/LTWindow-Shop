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
            ChartCanvas.Children.Clear();
            if (_viewModel.DataPoints == null || _viewModel.DataPoints.Count == 0) return;

            // Chỉnh ẩn/hiện chú thích lợi nhuận
            ProfitLegend.Visibility = _viewModel.SelectedReportType == ReportType.RevenueProfit ? Visibility.Visible : Visibility.Collapsed;

            if (_viewModel.SelectedReportType == ReportType.ProductSales)
            {
                DrawLineChart(); // Vẽ biểu đồ đường cho sản lượng
            }
            else
            {
                DrawBarChart(); // Vẽ biểu đồ cột cho doanh thu/lợi nhuận
            }
        }

        private void DrawBarChart()
        {
            double canvasWidth = ChartCanvas.ActualWidth;
            double canvasHeight = ChartCanvas.ActualHeight - 60; // Trừ lề dưới cho label
            double maxVal = _viewModel.DataPoints.Max(p => Math.Max(p.Value, p.Profit));
            if (maxVal == 0) maxVal = 1;

            double spacing = canvasWidth / _viewModel.DataPoints.Count;
            double barWidth = spacing * 0.4;

            for (int i = 0; i < _viewModel.DataPoints.Count; i++)
            {
                var point = _viewModel.DataPoints[i];
                double xBase = i * spacing + (spacing / 4);

                // Vẽ cột Doanh thu (Màu xanh)
                double revHeight = (point.Value / maxVal) * canvasHeight;
                DrawRectangle(xBase, canvasHeight - revHeight, barWidth, revHeight, Colors.CornflowerBlue);

                // Vẽ cột Lợi nhuận (Màu xanh lá) - Vẽ cạnh bên
                double profHeight = (point.Profit / maxVal) * canvasHeight;
                DrawRectangle(xBase + barWidth + 2, canvasHeight - profHeight, barWidth, profHeight, Colors.LightGreen);

                // Vẽ nhãn trục X
                AddLabel(point.Label, xBase, canvasHeight + 10);
            }
        }

        private void DrawLineChart()
        {
            double canvasWidth = ChartCanvas.ActualWidth;
            double canvasHeight = ChartCanvas.ActualHeight - 60;
            double maxVal = _viewModel.DataPoints.Max(p => p.Value);
            if (maxVal == 0) maxVal = 1;

            double spacing = canvasWidth / Math.Max(_viewModel.DataPoints.Count - 1, 1);
            var points = new List<Point>();

            for (int i = 0; i < _viewModel.DataPoints.Count; i++)
            {
                double x = i * spacing;
                double y = canvasHeight - ((_viewModel.DataPoints[i].Value / maxVal) * canvasHeight);
                points.Add(new Point { X = x, Y = y });

                // Vẽ điểm nút
                var dot = new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(Colors.CornflowerBlue) };
                Canvas.SetLeft(dot, x - 4);
                Canvas.SetTop(dot, y - 4);
                ChartCanvas.Children.Add(dot);

                AddLabel(_viewModel.DataPoints[i].Label, x - 10, canvasHeight + 10);
            }

            // Nối các điểm bằng đường thẳng
            for (int i = 0; i < points.Count - 1; i++)
            {
                var line = new Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = new SolidColorBrush(Colors.CornflowerBlue),
                    StrokeThickness = 2
                };
                ChartCanvas.Children.Add(line);
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
