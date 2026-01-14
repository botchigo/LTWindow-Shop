using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Core.DTOs;

namespace MyShop.Core.Interfaces
{
    public interface IPrintService
    {
        Task GenerateInvoicePdfAsync(OrderDto order, string filePath);
    }
}
