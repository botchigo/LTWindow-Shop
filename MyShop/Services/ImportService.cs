using Database.models;
using Database.Repositories;
using ExcelDataReader;
using MyShop.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Services
{
    public class ImportService : IImportService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductRepository _productRepository;
        public ImportService(ICategoryRepository categoryRepository, IProductRepository productRepository)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        public async Task<List<Product>> ImportFromExcelAsync(string filePath)
        {
            var categories = await _categoryRepository.GetCategoriesAsync();
            var categoriesDict = categories.ToDictionary(c => c.Name.ToLower(), c => c.Id);

            var allSkus = await _productRepository.GetAllSkuAsync();
            var skuSet = new HashSet<string>(allSkus);

            return await Task.Run(() =>
            {
                var products = new List<Product>();

                try
                {
                    using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            //read file into DataSet
                            var result = reader.AsDataSet(new ExcelDataSetConfiguration
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                                {
                                    UseHeaderRow = true, //The first line is the title line
                                }
                            });

                            //get the first sheet
                            DataTable table = result.Tables[0];

                            foreach (DataRow row in table.Rows)
                            {
                                var product = new Product
                                {
                                    Name = row["Name"]?.ToString()?.Trim() ?? "New product",
                                    ImportPrice = TryParseDecimal(row["ImportPrice"]),
                                    SalePrice = TryParseDecimal(row["SalePrice"]),
                                    SaleAmount = 0,
                                    Stock = TryParseInt(row["Stock"]),
                                    Description = row["Description"]?.ToString()?.Trim() ?? string.Empty
                                };

                                //duplicated sku
                                var baseSku = StringHelper.GenerateSku(product.Name);
                                var sku = baseSku;
                                int count = 1;
                                while (skuSet.Contains(sku))
                                {
                                    sku = $"{baseSku}-{count}";
                                    count++;
                                }
                                product.Sku = sku;
                                skuSet.Add(sku);

                                //existed category
                                var categoryName = row["Category"]?.ToString() ?? string.Empty;                                
                                if(!string.IsNullOrEmpty(categoryName) && 
                                    categoriesDict.TryGetValue(categoryName.ToLower(), out int categoryId))
                                {
                                    product.CategoryId = categoryId;
                                }

                                //add product
                                products.Add(product);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Excel Import Error: {ex.Message}");
                    throw;
                }

                return products;
            });
        }

        public async Task<List<Product>> ImportFromAccessAsync(string filePath)
        {
            var categories = await _categoryRepository.GetCategoriesAsync();
            var categoriesDict = categories.ToDictionary(c => c.Name.ToLower(), c => c.Id);

            var allSkus = await _productRepository.GetAllSkuAsync();
            var skuSet = new HashSet<string>(allSkus);

            return await Task.Run(() =>
            {
                var products = new List<Product>();

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
                                var product = new Product
                                {
                                    Name = reader["Name"]?.ToString()?.Trim() ?? "New product",
                                    ImportPrice = TryParseDecimal(reader["ImportPrice"]),
                                    SalePrice = TryParseDecimal(reader["SalePrice"]),
                                    SaleAmount = 0,
                                    Stock = TryParseInt(reader["Stock"]),
                                    Description = reader["Description"]?.ToString()?.Trim() ?? string.Empty
                                };

                                //duplicated sku
                                var baseSku = StringHelper.GenerateSku(product.Name);
                                var sku = baseSku;
                                int count = 1;
                                while (skuSet.Contains(sku))
                                {
                                    sku = $"{baseSku}-{count}";
                                    count++;
                                }
                                product.Sku = sku;
                                skuSet.Add(sku);

                                //existed category
                                var categoryName = reader["Category"]?.ToString() ?? string.Empty;
                                if (!string.IsNullOrEmpty(categoryName) &&
                                    categoriesDict.TryGetValue(categoryName.ToLower(), out int categoryId))
                                {
                                    product.CategoryId = categoryId;
                                }

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
