using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Modules.Orders.ViewModels
{
    public partial class OrderDetailsViewModel : ObservableObject, INavigationAware
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navService;
        private readonly IDialogService _dialogService;

        [ObservableProperty] private IGetOrderById_OrderById? _order;
        [ObservableProperty] private bool _isLoading;

        public ObservableCollection<OrderStatus> Statuses { get; } = new(Enum.GetValues<OrderStatus>());
        [ObservableProperty] private OrderStatus? _selectedOrderStatus;

        public OrderDetailsViewModel(IMyShopClient client, INavigationService nav, IDialogService dialog)
        {
            _client = client;
            _navService = nav;
            _dialogService = dialog;
        }

        public async void OnNavigatedTo(object parameter)
        {
            if (parameter is int orderId)
            {
                await LoadDataAsync(orderId);
            }
        }

        public void OnNavigationFrom() { }

        private async Task LoadDataAsync(int id)
        {
            IsLoading = true;
            var result = await _client.GetOrderById.ExecuteAsync(id);
            if (!result.IsErrorResult())
            {
                Order = result.Data.OrderById;
                SelectedOrderStatus = Order.Status;
            }
            IsLoading = false;
        }

        [RelayCommand]
        private async Task UpdateStatusAsync()
        {
            if (Order is null || SelectedOrderStatus is null)
                return;

            var input = new UpdateOrderStatusDTOInput
            {
                Id = Order.Id,
                NewStatus = SelectedOrderStatus.Value,
            };

            var result = await _client.UpdateOrderStatus.ExecuteAsync(input);
            if (result.IsErrorResult())
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
            }

            var response = result.Data.UpdateOrderStatus;
            if (response.Errors != null && response.Errors.Count > 0)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi cập nhật", response.Errors[0].Message);
                return;
            }

            await _dialogService.ShowMessageAsync("Thành công", "Cập nhật trạng thái xong.");
            await LoadDataAsync(Order.Id);
        }

        [RelayCommand]
        private async Task DeleteOrderAsync()
        {
            if (Order is null)
                return;

            var result = await _client.DeleteOrder.ExecuteAsync(Order.Id);
            if (result.IsErrorResult())
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi", result.Errors[0].Message);
            }

            var errors = result.Data.DeleteOrder.Errors;
            if (errors is not null)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi", errors[0].Message);
            }

            await _dialogService.ShowMessageAsync("Thành công", "Đơn hàng đã được xóa.");

            GoBack();
        }

        [RelayCommand]
        private void GoBack()
        {
            WeakReferenceMessenger.Default.Send(new NavigateInnerPageMessage("OrderManagementPage", null));
        }
    }
}
