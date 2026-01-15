using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MyShop.Infrastructure.Services
{
    public class PrintService : IPrintService
    {
        public async Task GenerateInvoicePdfAsync(OrderDto order, string filePath)
        {
            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // 1. Cấu hình trang
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                        // 2. Các phần của hóa đơn
                        page.Header().Element(ComposeHeader);
                        page.Content().Element(c => ComposeContent(c, order));
                        page.Footer().Element(ComposeFooter);
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        // --- PHẦN HEADER (Thông tin cửa hàng & Khách) ---
        void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                // Cột trái: Thông tin cửa hàng
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("MY SHOP").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                    column.Item().Text("Địa chỉ: Trường Đại học Khoa Học Tự Nhiên (cơ sở 2)");
                });
            });
        }

        // --- PHẦN CONTENT (Bảng danh sách món) ---
        [Obsolete]
        void ComposeContent(IContainer container, OrderDto order)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Tiêu đề
                column.Item().Text($"HÓA ĐƠN: #{order.Id}").FontSize(16).SemiBold();
                column.Item().Text($"Ngày tạo: {order.OrderDate:dd/MM/yyyy HH:mm}");
                column.Item().PaddingBottom(10);

                // Bảng sản phẩm
                column.Item().Table(table =>
                {
                    // Định nghĩa cột
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(25); // STT
                        columns.RelativeColumn(3);  // Tên món
                        columns.RelativeColumn();   // Số lượng
                        columns.RelativeColumn();   // Đơn giá
                        columns.RelativeColumn();   // Thành tiền
                    });

                    // Header bảng
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("#");
                        header.Cell().Element(CellStyle).Text("Sản phẩm");
                        header.Cell().Element(CellStyle).AlignRight().Text("SL");
                        header.Cell().Element(CellStyle).AlignRight().Text("Đơn giá");
                        header.Cell().Element(CellStyle).AlignRight().Text("Tổng");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                        }
                    });

                    // Dữ liệu từng dòng
                    int i = 1;
                    foreach (var item in order.OrderItems) // Giả sử bạn có list OrderItems
                    {
                        table.Cell().Element(CellStyle).Text(i++);
                        table.Cell().Element(CellStyle).Text(item.ProductName);
                        table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.Price:N0}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.Quantity * item.Price:N0}");

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                        }
                    }
                });

                // Tổng tiền
                column.Item().PaddingTop(10).AlignRight().Text($"TỔNG CỘNG: {order.TotalPrice:N0} VNĐ").FontSize(14).SemiBold().FontColor(Colors.Red.Medium);
            });
        }

        // --- PHẦN FOOTER (Lời cảm ơn) ---
        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Cảm ơn quý khách đã mua hàng!");
                x.Element().PaddingTop(5).Text($"Trang {x.CurrentPageNumber}");
            });
        }
    }
}
