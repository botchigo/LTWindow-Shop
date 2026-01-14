using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MyShop.Contract;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using MyShop.Infrastructure;
using StrawberryShake;

namespace MyShop.Modules.Orders.ViewModels
{
    public partial class OrderDetailsViewModel : ObservableObject, INavigationAware
    {
        private readonly IMyShopClient _client;
        private readonly INavigationService _navService;
        private readonly IDialogService _dialogService;
        private readonly IPrintService _printService;
        private readonly IFilePickerService _filePickerService;

        [ObservableProperty] private IGetOrderById_OrderById? _order;
        [ObservableProperty] private bool _isLoading;
        // Giả sử đây là đơn hàng đang xem
        [ObservableProperty] private OrderDto _selectedOrder;

        public ObservableCollection<OrderStatus> Statuses { get; } = new(Enum.GetValues<OrderStatus>());
        [ObservableProperty] private OrderStatus? _selectedOrderStatus;

        public OrderDetailsViewModel(IMyShopClient client, INavigationService nav, IDialogService dialog,
            IPrintService printService, IFilePickerService filePickerService)
        {
            _client = client;
            _navService = nav;
            _dialogService = dialog;
            _printService = printService;
            _filePickerService = filePickerService;
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
            try
            {
                var result = await _client.GetOrderById.ExecuteAsync(id);

                if (result.IsErrorResult())
                {
                    // Nếu lỗi thì hiển thị thông báo hoặc log
                    await _dialogService.ShowErrorDialogAsync("Lỗi", "Không thể tải đơn hàng");
                    return;
                }

                // 1. Gán dữ liệu hiển thị lên UI
                Order = result.Data.OrderById;
                SelectedOrderStatus = Order.Status;

                // 2. [QUAN TRỌNG] Gán dữ liệu cho biến SelectedOrder để dùng cho IN ẤN
                // Phải chuyển đổi thủ công từ kiểu GraphQL sang kiểu DTO
                SelectedOrder = new OrderDto
                {
                    Id = Order.Id,
                    TotalPrice = Order.FinalPrice,
                    // Chuyển DateTimeOffset sang DateTime
                    OrderDate = Order.CreatedAt.DateTime,
                    Status = Order.Status.ToString(),

                    // Map danh sách sản phẩm
                    OrderItems = Order.OrderItems.Select(item => new OrderItemDto
                    {
                        ProductId = item.Product.Id,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        Price = item.TotalPrice
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi tải dữ liệu", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
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
        [RelayCommand]
        public async Task PrintOrderAsync()
        {
            if (SelectedOrder == null) return;

            try
            {
                // 1. Mở hộp thoại chọn nơi lưu file
                // (Bạn cần implement hàm SaveFileAsync trong FilePickerService trả về đường dẫn string)
                string filePath = await _filePickerService.PickSaveFileAsync($"HoaDon_{SelectedOrder.Id}.pdf", "PDF Files|*.pdf");

                if (string.IsNullOrEmpty(filePath)) return; // Người dùng hủy chọn

                // 2. Gọi service tạo PDF
                await _printService.GenerateInvoicePdfAsync(SelectedOrder, filePath);

                // 3. Thông báo thành công & Hỏi có muốn mở file không
                // (Logic mở file tùy bạn, có thể dùng Process.Start)
                await _dialogService.ShowMessageAsync("Thành công", "Đã xuất file PDF hóa đơn!");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorDialogAsync("Lỗi in ấn", ex.Message);
            }
        }
    }
}
