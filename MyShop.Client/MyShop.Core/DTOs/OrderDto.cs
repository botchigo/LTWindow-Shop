using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Core.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        // Danh sách sản phẩm trong đơn hàng
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}
