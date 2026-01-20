using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.DTOs;
using MyShop.Core.Enums;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyShop.Modules.Report.ViewModels
{
    public partial class ReportViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IDialogService _dialogService;

        [ObservableProperty] private DateTimeOffset _startDate = DateTimeOffset.Now.AddMonths(-1);
        [ObservableProperty] private DateTimeOffset _endDate = DateTimeOffset.Now;
        [ObservableProperty] private ReportType _selectedReportType = ReportType.RevenueProfit;
        [ObservableProperty] private ReportTimeInterval _selectedInterval = ReportTimeInterval.Day;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty]
        private bool _isProductFilterVisible;

        public ObservableCollection<ReportDataPointDto> DataPoints { get; } = new();

        public ObservableCollection<ProductSeriesDto> ProductSeriesList { get; } = new();

        public ObservableCollection<string> AvailableProductNames { get; } = new();
        public IEnumerable<string> FilteredProductNames =>
            AvailableProductNames;

        public ReportViewModel(IMyShopClient client, IDialogService dialogService)
        {
            _client = client;
            _dialogService = dialogService;
        }
        partial void OnSelectedReportTypeChanged(ReportType value)
        {
            // Nếu là ProductSales thì hiện (true), còn lại (RevenueProfit) thì ẩn (false)
            IsProductFilterVisible = value == ReportType.ProductSales;

            // 2. Xóa dữ liệu biểu đồ cũ
            DataPoints.Clear();
            ProductSeriesList.Clear();

            // Cập nhật lại danh sách lọc (để hiện đầy đủ sản phẩm trở lại)
            OnPropertyChanged(nameof(FilteredProductNames));

            if (value == ReportType.RevenueProfit)
            {
                LoadReportCommand.ExecuteAsync(null);
            }
        }
        public async Task InitializeAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var result = await _client.GetProductNamesUpdated.ExecuteAsync();
                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                }
                else
                {
                    AvailableProductNames.Clear();
                    foreach (var order in result.Data.Orders.Items)
                    {
                        foreach(var orderItem in order.OrderItems)
                        {
                            if (!AvailableProductNames.Contains(orderItem.Product.Name))
                            {
                                AvailableProductNames.Add(orderItem.Product.Name);
                            }
                        }
                    }

                    OnPropertyChanged(nameof(FilteredProductNames));
                }
            }
            finally { _semaphore.Release(); }
        }

        [RelayCommand]
        public async Task LoadReportAsync()
        {
            // Nếu đang load thì không cho chạy tiếp để tránh spam nút
            if (IsLoading) return;

            await _semaphore.WaitAsync();
            try
            {
                IsLoading = true;

                // Clear dữ liệu cũ trước khi tải mới
                DataPoints.Clear();
                if (SelectedReportType == ReportType.RevenueProfit)
                {
                    // Nếu chuyển tab báo cáo thì clear luôn list bên kia cho sạch
                    ProductSeriesList.Clear();
                }

                // --- 1. LOGIC TÍNH TOÁN NGÀY (QUAN TRỌNG) ---

                // Lấy giá trị Start/End từ DatePicker (Lấy phần .Date để về 00:00:00 giờ Local)
                // TUYỆT ĐỐI KHÔNG DÙNG .UtcDateTime Ở ĐÂY VÌ SẼ BỊ TRỪ 7 TIẾNG
                DateTime startInput = StartDate.Date;
                DateTime endInput = EndDate.Date;
                DateTime finalEnd;

                // Tự động mở rộng thời gian kết thúc dựa trên chế độ xem
                if (SelectedInterval == ReportTimeInterval.Year)
                {
                    // Nếu xem Năm: Lấy đến hết ngày 31/12 23:59:59
                    finalEnd = new DateTime(endInput.Year, 12, 31, 23, 59, 59);

                    // (Tùy chọn) Nếu muốn start luôn là đầu năm:
                    // startInput = new DateTime(startInput.Year, 1, 1);
                }
                else if (SelectedInterval == ReportTimeInterval.Month)
                {
                    // Nếu xem Tháng: Lấy đến ngày cuối cùng của tháng 23:59:59
                    var daysInMonth = DateTime.DaysInMonth(endInput.Year, endInput.Month);
                    finalEnd = new DateTime(endInput.Year, endInput.Month, daysInMonth, 23, 59, 59);

                    // (Tùy chọn) Nếu muốn start luôn là đầu tháng:
                    // startInput = new DateTime(startInput.Year, startInput.Month, 1);
                }
                else
                {
                    // Nếu xem Ngày/Tuần: Lấy đến hết ngày hiện tại (23:59:59)
                    finalEnd = new DateTime(endInput.Year, endInput.Month, endInput.Day, 23, 59, 59);
                }

                // Tạo object Input chung
                var input = new ReportBaseParamsInput
                {
                    Start = startInput,
                    End = finalEnd,
                    Interval = SelectedInterval
                };

                var compar = new ProductComparisonDTOInput
                {
                    Start = startInput,
                    End = finalEnd,
                    Interval = SelectedInterval,
                    ProductNames = AvailableProductNames.ToList()
                };

                // --- 2. GỌI API THEO LOẠI BÁO CÁO ---

                if (SelectedReportType == ReportType.RevenueProfit)
                {
                    var result = await _client.GetRevenueProfitReport.ExecuteAsync(input);

                    if (result.IsErrorResult())
                    {
                        await _dialogService.ShowErrorDialogAsync("Lỗi tải báo cáo", result.Errors[0].Message);
                    }
                    else
                    {
                        foreach (var item in result.Data.RevenueProfitReport)
                        {
                            DataPoints.Add(new ReportDataPointDto
                            {
                                Date = item.Date,
                                Value = item.Value,
                                Profit = item.Profit,
                                // Lưu ý: Nếu ở View đã dùng Helper FormatDate thì Label ở đây không quan trọng lắm
                                // Bỏ .ToLocalTime() để giữ nguyên giá trị ngày nhận được từ Server
                                Label = item.Date.ToString("dd/MM")
                            });
                        }
                    }
                }
                else if (SelectedReportType == ReportType.ProductSales)
                {
                    ProductSeriesList.Clear(); // Clear trước khi add

                    var result = await _client.GetProductComparisonReport.ExecuteAsync(compar);

                    if (result.IsErrorResult())
                    {
                        await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                    }
                    else
                    {
                        foreach (var item in result.Data.ProductComparisonReport)
                        {
                            ProductSeriesList.Add(new ProductSeriesDto
                            {
                                ProductName = item.ProductName,
                                ColorHex = item.ColorHex,
                                Points = item.Points.Select(p => new ReportDataPointDto
                                {
                                    Date = p.Date,
                                    Value = p.Value,
                                    Profit = p.Profit,
                                    Label = p.Label,
                                }).ToList()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi hệ thống", ex.Message);
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

            await _semaphore.WaitAsync();
            try
            {
                IsLoading = true;

                DateTime startInput = StartDate.Date;
                DateTime endInput = EndDate.Date;
                DateTime finalEnd;

                // Tự động mở rộng thời gian kết thúc dựa trên chế độ xem
                if (SelectedInterval == ReportTimeInterval.Year)
                {
                    // Nếu xem Năm: Lấy đến hết ngày 31/12 23:59:59
                    finalEnd = new DateTime(endInput.Year, 12, 31, 23, 59, 59);

                    // (Tùy chọn) Nếu muốn start luôn là đầu năm:
                    // startInput = new DateTime(startInput.Year, 1, 1);
                }
                else if (SelectedInterval == ReportTimeInterval.Month)
                {
                    // Nếu xem Tháng: Lấy đến ngày cuối cùng của tháng 23:59:59
                    var daysInMonth = DateTime.DaysInMonth(endInput.Year, endInput.Month);
                    finalEnd = new DateTime(endInput.Year, endInput.Month, daysInMonth, 23, 59, 59);

                    // (Tùy chọn) Nếu muốn start luôn là đầu tháng:
                    // startInput = new DateTime(startInput.Year, startInput.Month, 1);
                }
                else
                {
                    // Nếu xem Ngày/Tuần: Lấy đến hết ngày hiện tại (23:59:59)
                    finalEnd = new DateTime(endInput.Year, endInput.Month, endInput.Day, 23, 59, 59);
                }

                var input = new GetSingleProductSeriesDTOInput
                {
                    ProductName = productName,
                    Start = StartDate.UtcDateTime,
                    End = EndDate.UtcDateTime,
                    Interval = SelectedInterval,
                    ColorIndex = ProductSeriesList.Count(),
                };

                var productNames = new List<string>();
                productNames.AddRange(ProductSeriesList.Select(p => p.ProductName));
                ProductSeriesList.Clear(); 
                if(!productNames.Any(p => p == productName)) productNames.Add(productName);


                var compar = new ProductComparisonDTOInput
                {
                    Start = startInput,
                    End = finalEnd,
                    Interval = SelectedInterval,
                    ProductNames = productNames,
                };

                var result = await _client.GetProductComparisonReport.ExecuteAsync(compar);

                //if (result.IsErrorResult())
                //{
                //    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                //}
                //else if (result.Data.ProductComparisonReport != null)
                //{
                //    var report = result.Data.SingleProductSeries;
                //    var series = new ProductSeriesDto
                //    {
                //        ProductName = report.ProductName,
                //        ColorHex = report.ColorHex,
                //        Points = report.Points.Select(p => new ReportDataPointDto
                //        {
                //            Date = p.Date,
                //            Value = p.Value,
                //            Profit = p.Profit,
                //            Label = p.Label,
                //        }).ToList()
                //    };

                //    ProductSeriesList.Add(series);
                //    OnPropertyChanged(nameof(FilteredProductNames));
                //}

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                }
                else
                {
                    foreach (var item in result.Data.ProductComparisonReport)
                    {
                        ProductSeriesList.Add(new ProductSeriesDto
                        {
                            ProductName = item.ProductName,
                            ColorHex = item.ColorHex,
                            Points = item.Points.Select(p => new ReportDataPointDto
                            {
                                Date = p.Date,
                                Value = p.Value,
                                Profit = p.Profit,
                                Label = p.Label,
                            }).ToList()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi", ex.Message);
            }
            finally
            {
                IsLoading = false;
                _semaphore.Release();
            }
        }

        [RelayCommand]
        public void ClearAllProducts()
        {
            ProductSeriesList.Clear();
            OnPropertyChanged(nameof(FilteredProductNames));
        }

        public void CallNotifyFilteredProductNames() => OnPropertyChanged(nameof(FilteredProductNames));
    }
}
