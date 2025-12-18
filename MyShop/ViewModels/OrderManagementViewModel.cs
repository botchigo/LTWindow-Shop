using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Database.Enums;
using Database.models;
using Database.Repositories;
using MyShop.Services;
using MyShop.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.ViewModels
{
    public partial class OrderManagementViewModel : ObservableObject
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<Order> Orders { get;} = new ObservableCollection<Order>();

        //Filter
        [ObservableProperty]
        private DateTimeOffset? _fromDate;

        [ObservableProperty]
        private DateTimeOffset? _toDate;

        //Paging
        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private bool _isLoading;

        public OrderManagementViewModel(IOrderRepository orderRepository, IDialogService dialogService, 
            INavigationService navigationService)
        {
            _orderRepository = orderRepository;
            _dialogService = dialogService;
            _navigationService = navigationService;

            FromDate = new DateTimeOffset(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1));
            ToDate = DateTimeOffset.Now;
            
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;
            Orders.Clear();

            var result = await _orderRepository.GetPagedOrdersAsync(
                CurrentPage, PageSize, FromDate?.UtcDateTime, ToDate?.UtcDateTime);

            foreach(var order in result.Orders)
            {
                Orders.Add(order);
            }

            TotalPages = (int)Math.Ceiling((double)result.TotalCount / PageSize);
            if(TotalPages == 0)
                TotalPages = 1;

            IsLoading = false;
        }

        [RelayCommand]
        private void GoToCreateOrderPage()
        {
            _navigationService.NavigateTo<CreateOrderPage>();
        }

        [RelayCommand]
        private async Task DeleteOrderAsync(Order order)
        {
            var confirm = await _dialogService.ShowConfirmAsync("Xóa đơn hàng", $"Bạn chắc chắn muốn xóa đơn hàng không?");
            if (confirm is false)
                return;

            try
            {
                await _orderRepository.DeleteOrderAsync(order.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi xóa đơn hàng", ex.Message);
            }
        }

        private bool CanLoadNextPage() => CurrentPage < TotalPages;
        [RelayCommand(CanExecute = nameof(CanLoadNextPage))]
        private async Task NextPage()
        {
            CurrentPage++;
            await LoadDataAsync();
        }

        private bool CanLoadPreviousPage() => CurrentPage > 1;
        [RelayCommand(CanExecute = nameof(CanLoadPreviousPage))]
        private async Task PreviousPage()
        {
            CurrentPage--;
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task ViewDetails(Order order)
        {
            try
            {
                var details = await _orderRepository.GetDetailsAsync(order.Id);
                if(details is null)
                {
                    await _dialogService.ShowErrorDialogAsync("Lỗi", "Không tìm thấy đơn hàng này.");
                    return;
                }

                var viewModel = new OrderDetailsViewModel(details);
                var result = await _dialogService.ShowOrderDetailsAsync(viewModel);

                if(result)
                {
                    if(viewModel.SelectedOrderStatus != details.Status)
                    {
                        await UpdateOrderStatus(details.Id, viewModel.SelectedOrderStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi", ex.Message);
            }
        }

        private async Task UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            try
            {
                await _orderRepository.UpdateStatusAsync(orderId, newStatus);
                await _dialogService.ShowMessageAsync("Thành công", "Đã cập nhật trạng thái đơn hàng.");

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi cập nhật", ex.Message);
            }
        }
    }
}
