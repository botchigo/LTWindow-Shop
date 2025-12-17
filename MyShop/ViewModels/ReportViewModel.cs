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

            List<ReportDataPoint> data;
            if (SelectedReportType == ReportType.ProductSales)
                data = await _reportRepository.GetProductSalesReportAsync(StartDate.DateTime, EndDate.DateTime, SelectedInterval);
            else
                data = await _reportRepository.GetRevenueProfitReportAsync(StartDate.DateTime, EndDate.DateTime, SelectedInterval);

            System.Diagnostics.Debug.WriteLine($"[Report] Loaded {data.Count} points");

            foreach (var p in data) DataPoints.Add(p);
            IsLoading = false;
        }
    }
}
