using AutoMapper;
using Microsoft.Extensions.Logging;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Shared.DTOs.Orders;
using MyShop.Shared.Enums;

namespace MyShop.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Order> CreateOrderAsync(AddOrderDTO request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = _mapper.Map<Order>(request);
                await _unitOfWork.Orders.AddAsync(order);

                var productIds = request.OrderItems.Select(oi => oi.ProductId).ToList();
                var products = await _unitOfWork.Products.GetByIdsAsync(productIds);

                foreach (var item in order.OrderItems)
                {
                    var product = products.FirstOrDefault(p => p.Id == item.ProductId);

                    if (product is null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw new Exception($"Sản phẩm ID {item.ProductId} không tồn tại.");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        throw new Exception($"Sản phẩm '{product.Name}' không đủ hàng (Tồn: {product.Stock}).");
                    }

                    item.UnitSalePrice = product.SalePrice;
                    item.UnitCost = product.ImportPrice;
                    item.TotalPrice = item.Quantity * item.UnitSalePrice;
                    item.OrderId = order.Id;

                    product.Stock -= item.Quantity;
                    product.SaleAmount += item.Quantity;
                }

                order.FinalPrice = order.OrderItems.Sum(oi => oi.TotalPrice);

                await _unitOfWork.CommitTransactionAsync();
                return order;
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Order> DeleteOrderAsync(int orderId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.Orders.GetDetailsAsync(orderId);

                if (order is null)
                    throw new Exception("Không tìm thấy đơn hàng");

                if (order.Status != OrderStatus.Canceled)
                {
                    foreach (var item in order.OrderItems)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }               

                _unitOfWork.Orders.Remove(order);
                await _unitOfWork.CommitTransactionAsync();
                return order;
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Order> UpdateStatusAsync(UpdateOrderStatusDTO request)
        {
            var order = await _unitOfWork.Orders.GetDetailsAsync(request.Id);

            if (order is null)
                throw new Exception("Không tìm thấy đơn hàng");

            if (!CanUpdateStatus(order.Status, request.NewStatus))
                throw new Exception($"Không thể cập nhật từ trạng thái {order.Status} sang {request.NewStatus}");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                //if you update a cancelled order, you need to get stock
                if (order.Status == OrderStatus.Canceled)
                {
                    foreach (var item in order.OrderItems)
                    {

                        if (item.Quantity > item.Product.Stock)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            throw new Exception
                                ($"Không thể cập nhật từ trạng thái {order.Status} sang {request.NewStatus} vì sản phảm này đã hết");
                        }

                        item.Product.Stock -= item.Quantity;
                        item.Product.SaleAmount++;
                    }
                }
                //if you update a order to a cancelled one, you need to restock
                else if (request.NewStatus == OrderStatus.Canceled)
                {
                    foreach (var item in order.OrderItems)
                    {
                        item.Product.Stock += item.Quantity;
                        item.Product.SaleAmount--;
                    }
                }

                order.Status = request.NewStatus;

                await _unitOfWork.CommitTransactionAsync();
                return order;
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private bool CanUpdateStatus(OrderStatus oldStatus, OrderStatus newStatus)
        {
            if (oldStatus == newStatus) return false;

            if (oldStatus == OrderStatus.Canceled)
            {
                return true; 
            }

            if (oldStatus == OrderStatus.Paid)
            {
                if (newStatus == OrderStatus.Canceled) return true;
                return false; 
            }

            return true;
        }
    }
}
