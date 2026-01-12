using ExcelDataReader;
using MyShop.Core.DTOs;
using MyShop.Core.Interfaces;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace MyShop.Infrastructure.Services
{
    public class ImportService : IImportService
    {
        public ImportService()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public async Task<List<ProductImportDto>> ParseAccessAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var products = new List<ProductImportDto>();

                string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Persist Security Info=False;";

                try
                {
                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();

                        string query = "SELECT * FROM Products";

                        using (OleDbCommand command = new OleDbCommand(query, connection))
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var product = new ProductImportDto
                                {
                                    Name = reader["Name"]?.ToString()?.Trim() ?? "New product",
                                    ImportPrice = TryParseDecimal(reader["ImportPrice"]),
                                    SalePrice = TryParseDecimal(reader["SalePrice"]),
                                    Stock = TryParseInt(reader["Stock"]),
                                    Description = reader["Description"]?.ToString()?.Trim() ?? string.Empty,
                                    CategoryName = reader["Category"]?.ToString()?.Trim() ?? string.Empty
                                };

                                //add product
                                products.Add(product);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Access Import Error: {ex.Message}");
                    throw new Exception("Lỗi đọc file Access. Hãy đảm bảo máy đã cài 'Microsoft Access Database Engine'.");
                }

                return products;
            });
        }

        public async Task<List<ProductImportDto>> ParseExcelAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var resultList = new List<ProductImportDto>();

                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                });

                var table = dataset.Tables[0];
                foreach (DataRow row in table.Rows)
                {
                    // Map DataRow -> ImportProductInput
                    var item = new ProductImportDto
                    {
                        Name = row["Name"]?.ToString()?.Trim() ?? "New Product",
                        SalePrice = TryParseDecimal(row["SalePrice"]),
                        ImportPrice = TryParseDecimal(row["ImportPrice"]),
                        Stock = TryParseInt(row["Stock"]),
                        Description = row["Description"]?.ToString() ?? string.Empty,
                        CategoryName = row["Category"]?.ToString() ?? string.Empty
                    };
                    resultList.Add(item);
                }
                return resultList;
            });
        }

        private decimal TryParseDecimal(object value)
        {
            if (value is null)
                return 0;

            return decimal.TryParse(value.ToString(), out decimal result) ? result : 0;
        }

        private int TryParseInt(object value)
        {
            if (value is null)
                return 0;

            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }
    }
}
