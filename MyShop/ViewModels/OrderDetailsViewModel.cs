using Database.Enums;
using Database.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.ViewModels
{
    public class OrderDetailsViewModel 
    {
        public Order Order { get; }
        public List<OrderStatus> OrderStatuses { get; }
        public OrderStatus SelectedOrderStatus { get; set; }

        public OrderDetailsViewModel(Order order)
        {
            Order = order;
            OrderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
            SelectedOrderStatus = order.Status;
        }
    }
}
