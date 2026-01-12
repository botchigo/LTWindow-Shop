using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Modules.Orders.ViewModels
{
    public partial class OrderManagementViewModel : ObservableObject
    {
        private readonly IMyShopClient _client;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<IGetOrders_Orders_Items> Orders { get; } = new();

        //filter
        [ObservableProperty] private DateTimeOffset? _fromDate;
        [ObservableProperty] private DateTimeOffset? _toDate;

        //paging
        [ObservableProperty] private int _currentPage = 1;
        [ObservableProperty] private int _pageSize = 10;
        [ObservableProperty] private int _totalPages = 1;
        [ObservableProperty] private bool _isLoading;

        public OrderManagementViewModel(IMyShopClient client, IDialogService dialogService, INavigationService navigationService)
        {
            _client = client;
            _dialogService = dialogService;
            _navigationService = navigationService;

            InitializeDefaultDates();

            _ = LoadDataAsync();
        }

        private void InitializeDefaultDates()
        {
            var now = DateTimeOffset.Now;

            FromDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);
            ToDate = now;
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var skip = (CurrentPage - 1) * PageSize;

                var order = new List<OrderSortInput>
                {
                    new OrderSortInput
                    {
                        CreatedAt = SortEnumType.Desc
                    }
                };

                var result = await _client.GetOrders.ExecuteAsync(
                    skip: skip,
                    take: PageSize,
                    fromDate: FromDate?.UtcDateTime,
                    toDate: ToDate?.UtcDateTime,
                    order: order);

                if (result.IsErrorResult())
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
                    return;
                }

                var data = result.Data.Orders;

                TotalPages = (int)Math.Ceiling((double)data.TotalCount / PageSize);
                if (TotalPages == 0) TotalPages = 1;

                Orders.Clear();
                foreach (var item in data.Items)
                {
                    Orders.Add(item);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void GoToCreateOrderPage()
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("CreateOrderPage", null));
        }

        [RelayCommand]
        private void GoToDetailsPage(int id)
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("OrderDetailsPage", id));
        }

        //=======PAGINATION========

        private bool CanLoadNextPage() => CurrentPage < TotalPages && IsLoading == false;

        [RelayCommand(CanExecute = nameof(CanLoadNextPage))]
        private async Task LoadNextPageAsync()
        {
            CurrentPage++;
            await LoadDataAsync();
        }

        private bool CanLoadPreviousPage() => CurrentPage > 1 && IsLoading == false;

        [RelayCommand(CanExecute = nameof(CanLoadPreviousPage))]
        private async Task LoadPreviousPageAsync()
        {
            CurrentPage--;
            await LoadDataAsync();
        }
    }
}
