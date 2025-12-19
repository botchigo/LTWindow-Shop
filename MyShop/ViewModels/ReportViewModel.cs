using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Enums;
using Database.Repositories;
using Microsoft.UI.Xaml.Documents;

namespace MyShop.ViewModels
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly IReportRepository _reportRepository;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTimeOffset.Now.AddMonths(-1);
        [ObservableProperty]
        private DateTimeOffset _endDate = DateTimeOffset.Now;
        [ObservableProperty]
        private ReportType _selectedReportType = ReportType.RevenueProfit;
        [ObservableProperty]
        private ReportTimeInterval _selectedInterval = ReportTimeInterval.Day;
        [ObservableProperty]
        private bool _isLoading;
        // Danh sách tất cả sản phẩm có trong DB để chọn
        public ObservableCollection<string> AvailableProductNames { get; } = new();

        // Danh sách các sản phẩm đang được hiển thị trên biểu đồ
        public ObservableCollection<ProductSeries> ProductSeriesList { get; } = new();

        [ObservableProperty]
        private string _selectedProductNames;
        // Thuộc tính này sẽ trả về những sản phẩm CHƯA được thêm vào biểu đồ
        public IEnumerable<string> FilteredProductNames =>
            AvailableProductNames.Where(name =>
                !ProductSeriesList.Any(s => s.ProductName == name));
        public ObservableCollection<ReportDataPoint> DataPoints { get; } = new();
        public ReportViewModel(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }
        // Phương thức khởi tạo dữ liệu ban đầu
        public async Task InitializeAsync()
        {
            await _semaphore.WaitAsync(); // Chờ đến lượt
            try
            {
                var names = await _reportRepository.GetAllProductNamesAsync();
                AvailableProductNames.Clear();
                foreach (var name in names) AvailableProductNames.Add(name);
            }
            finally
            {
                _semaphore.Release(); // Giải phóng khóa
            }
        }
        [RelayCommand]
        public async Task AddProductToChartAsync()
        {
            if (string.IsNullOrWhiteSpace(SelectedProductNames)) return;

            // Chuẩn hóa tên sản phẩm trước khi kiểm tra
            string trimmedName = SelectedProductNames.Trim();

            // Kiểm tra trùng lặp (không phân biệt hoa thường)
            bool isExisted = ProductSeriesList.Any(s =>
                s.ProductName.Trim().Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

            if (isExisted) return;

            await _semaphore.WaitAsync(); // Đảm bảo luồng chạy an toàn
            try
            {
                IsLoading = true;
                // gọi Repository nạp dữ liệu
                var newSeries = await _reportRepository.GetSingleProductSeriesAsync(
                    trimmedName, StartDate.DateTime, EndDate.DateTime, SelectedInterval, ProductSeriesList.Count);

                if (newSeries != null) 
                {
                    ProductSeriesList.Add(newSeries);
                    OnPropertyChanged(nameof(FilteredProductNames));
                }
            }
            finally
            {
                IsLoading = false;
                _semaphore.Release();
            }
        }

        [RelayCommand]
        public async Task LoadSingleProductReportAsync(string productName)
        {
            if (string.IsNullOrEmpty(productName)) return;

            await _semaphore.WaitAsync(); // PHẢI THÊM Semaphore ở đây
            try
            {
                IsLoading = true;

                // Chuẩn hóa thời gian để không mất ngày cuối
                DateTime start = StartDate.Date;
                DateTime end = EndDate.Date.AddDays(1).AddTicks(-1);
                var utcStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                var utcEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);

                var newSeries = await _reportRepository.GetSingleProductSeriesAsync(
                    productName, utcStart, utcEnd, SelectedInterval, ProductSeriesList.Count);

                if (newSeries != null && newSeries.Points.Any())
                {
                    ProductSeriesList.Add(newSeries);
                    OnPropertyChanged(nameof(FilteredProductNames)); // Cập nhật combo
                }
            }
            finally
            {
                IsLoading = false;
                _semaphore.Release();
            }
        }
        [RelayCommand]
        public async Task LoadReportAsync()
        {
            // Đợi nếu có luồng khác đang chạy
            await _semaphore.WaitAsync();

            try
            {
                IsLoading = true;
                DataPoints.Clear();
                ProductSeriesList.Clear();

                // Chuẩn hóa thời gian
                DateTime start = StartDate.Date;
                DateTime end = EndDate.Date.AddDays(1).AddTicks(-1);
                var utcStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
                var utcEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);

                if (SelectedReportType == ReportType.ProductSales)
                {
                    var results = await _reportRepository.GetProductComparisonReportAsync(utcStart, utcEnd, SelectedInterval);
                    foreach (var s in results) ProductSeriesList.Add(s);
                }
                else
                {
                    var data = await _reportRepository.GetRevenueProfitReportAsync(utcStart, utcEnd, SelectedInterval);
                    foreach (var p in data)
                    {
                        p.Label = p.Date.ToLocalTime().ToString("dd/MM");
                        DataPoints.Add(p);
                    }
                }
            }
            finally
            {
                IsLoading = false;
                _semaphore.Release(); // Giải phóng cho yêu cầu tiếp theo
            }
        }

        // Hàm thông báo cập nhật danh sách lọc
        public void CallNotifyFilteredProductNames()
        {
            OnPropertyChanged(nameof(FilteredProductNames));
        }
        [RelayCommand]
        public void ClearAllProducts()
        {
            ProductSeriesList.Clear();
            // Thông báo để ComboBox (FilteredProductNames) nạp lại đầy đủ danh sách ban đầu
            OnPropertyChanged(nameof(FilteredProductNames));
        }
    }
}
