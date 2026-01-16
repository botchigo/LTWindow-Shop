using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MyShop.Core.Helpers;
using MyShop.Modules.Dashboard.ViewModels;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Modules.Dashboard.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel ViewModel { get; }
        public DashboardPage()
        {
            InitializeComponent();
            ViewModel = ServiceHelper.GetService<DashboardViewModel>();
            DataContext = ViewModel;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Loaded += (s, e) => DrawRevenueChart();
        }

        private void RefreshButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.LoadDashboardDataCommand.Execute(null);
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.MonthlyRevenue) || e.PropertyName == nameof(ViewModel.IsLoading))
            {
                DispatcherQueue.TryEnqueue(() => DrawRevenueChart());
            }
        }

        private void RevenueChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawRevenueChart();
        }

        private void DrawRevenueChart()
        {
            RevenueChart.Children.Clear();
            if (ViewModel.MonthlyRevenue.Count < 2) return; 

            double width = RevenueChart.ActualWidth > 0 ? RevenueChart.ActualWidth : 700;
            double height = RevenueChart.ActualHeight > 0 ? RevenueChart.ActualHeight : 200;
            double margin = 20;

            var data = ViewModel.MonthlyRevenue;

            double maxVal = (double)data.Max(x => x.TotalRevenue);
            if (maxVal == 0) maxVal = 1;

            double stepX = (width - margin * 2) / (data.Count - 1);

            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Colors.CornflowerBlue),
                StrokeThickness = 3
            };

            for (int i = 0; i < data.Count; i++)
            {
                double val = (double)data[i].TotalRevenue;

                double x = margin + (i * stepX);
                double y = height - margin - ((val / maxVal) * (height - margin * 2));

                polyline.Points.Add(new Point(x, y));

                var dot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Colors.Blue),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 2,
                };

                Canvas.SetLeft(dot, x - 4);
                Canvas.SetTop(dot, y - 4);
                ToolTipService.SetToolTip(dot, new ToolTip { Content = $"{val:N0} ð" });

                RevenueChart.Children.Add(dot);

                if (i == 0 || i == data.Count - 1 || i % 5 == 0)
                {
                    var tb = new TextBlock
                    {
                        Text = data[i].Day.ToString("dd/MM"),
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Colors.Gray)
                    };
                    Canvas.SetLeft(tb, x - 15);
                    Canvas.SetTop(tb, height - 15);
                    RevenueChart.Children.Add(tb);
                }
            }

            RevenueChart.Children.Insert(0, polyline);
        }
    }
}
