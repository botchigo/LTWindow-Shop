using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Enums;
using Database.Repositories;

namespace MyShop.ViewModels
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly IReportRepository _reportRepository;

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

        public ObservableCollection<ReportDataPoint> DataPoints { get; } = new();

        public ReportViewModel(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        [RelayCommand]
        public async Task LoadReportAsync()
        {
            IsLoading = true;
            DataPoints.Clear();

            // 1. CHUẨN HÓA THỜI GIAN
            // Lấy ngày bắt đầu từ 00:00:00
            DateTime start = StartDate.Date;

            // Lấy ngày kết thúc đến 23:59:59 để bao phủ toàn bộ dữ liệu trong ngày đó
            DateTime end = EndDate.Date.AddDays(1).AddTicks(-1);

            // 2. XỬ LÝ MÚI GIỜ (Fix lỗi lệch 1 ngày)
            // Chuyển sang UTC để khớp với cách lưu trữ của PostgreSQL
            var utcStart = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            var utcEnd = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            List<ReportDataPoint> data;
            if (SelectedReportType == ReportType.ProductSales)
                data = await _reportRepository.GetProductSalesReportAsync(utcStart, utcEnd, SelectedInterval);
            else
                data = await _reportRepository.GetRevenueProfitReportAsync(utcStart, utcEnd, SelectedInterval);

            System.Diagnostics.Debug.WriteLine($"[Report] Range: {utcStart} to {utcEnd} | Loaded: {data.Count} points");

            // 3. HIỂN THỊ (Fix nhãn ngày trên biểu đồ)
            foreach (var p in data)
            {
                // Đảm bảo Label sử dụng giờ Local khi hiển thị
                // Nếu Repository trả về DateTime, hãy chuyển nó về Local trước khi định dạng chuỗi
                p.Label = p.Date.ToLocalTime().ToString("dd/MM");
                DataPoints.Add(p);
            }

            IsLoading = false;
        }
    }
}
