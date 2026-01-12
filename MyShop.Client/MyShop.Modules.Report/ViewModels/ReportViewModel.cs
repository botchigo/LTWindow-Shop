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

        public ObservableCollection<ReportDataPointDto> DataPoints { get; } = new();

        public ObservableCollection<ProductSeriesDto> ProductSeriesList { get; } = new();

        public ObservableCollection<string> AvailableProductNames { get; } = new();
        public IEnumerable<string> FilteredProductNames =>
            AvailableProductNames.Where(n => !ProductSeriesList.Any(s => s.ProductName == n));

        public ReportViewModel(IMyShopClient client, IDialogService dialogService)
        {
            _client = client;
            _dialogService = dialogService;
        }

        public async Task InitializeAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var result = await _client.GetProductNames.ExecuteAsync("", 0, 20);
                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                }
                else
                {
                    AvailableProductNames.Clear();
                    foreach (var product in result.Data.Products.Items)
                        AvailableProductNames.Add(product.Name);

                    OnPropertyChanged(nameof(FilteredProductNames));
                }
            }
            finally { _semaphore.Release(); }
        }

        [RelayCommand]
        public async Task LoadReportAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                IsLoading = true;
                DataPoints.Clear();

                if (SelectedReportType == ReportType.RevenueProfit)
                {
                    ProductSeriesList.Clear();

                    var input = new ReportBaseParamsInput
                    {
                        Start = StartDate.UtcDateTime,
                        End = EndDate.UtcDateTime,
                        Interval = SelectedInterval
                    };

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
                                Label = item.Date.ToLocalTime().ToString("dd/MM")
                            });
                        }
                    }
                }
                else if (SelectedReportType == ReportType.ProductSales)
                {
                    DataPoints.Clear();

                    var input = new ReportBaseParamsInput
                    {
                        Start = StartDate.UtcDateTime,
                        End = EndDate.UtcDateTime,
                        Interval = SelectedInterval
                    };

                    var result = await _client.GetProductComparisonReport.ExecuteAsync(input);

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
                                Points = item.Points.Select(p => new ReportDataPointDto
                                {
                                    Date = p.Date,
                                    Value = p.Value,
                                    Profit = p.Profit,
                                    Label = p.Label,
                                }).ToList(),
                                ColorHex = item.ColorHex,
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

                var input = new GetSingleProductSeriesDTOInput
                {
                    ProductName = productName,
                    Start = StartDate.UtcDateTime,
                    End = EndDate.UtcDateTime,
                    Interval = SelectedInterval,
                    ColorIndex = ProductSeriesList.Count(),
                };

                var result = await _client.GetSingleProductSeries.ExecuteAsync(input);

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                }
                else if (result.Data.SingleProductSeries != null)
                {
                    var report = result.Data.SingleProductSeries;
                    var series = new ProductSeriesDto
                    {
                        ProductName = report.ProductName,
                        ColorHex = report.ColorHex,
                        Points = report.Points.Select(p => new ReportDataPointDto
                        {
                            Date = p.Date,
                            Value = p.Value,
                            Label = p.Label,
                        }).ToList()
                    };

                    ProductSeriesList.Add(series);
                    OnPropertyChanged(nameof(FilteredProductNames));
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
