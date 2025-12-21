using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Categories
            migrationBuilder.Sql(@"
                INSERT INTO category (name, description) VALUES
                ('Điện thoại', 'Điện thoại di động các loại'),
                ('Laptop', 'Máy tính xách tay'),
                ('Phụ kiện', 'Phụ kiện điện tử')
                ON CONFLICT DO NOTHING;
            ");

            // Seed Test Users
            migrationBuilder.Sql(@"
                INSERT INTO app_user (username, password, full_name) VALUES
                ('admin', '123456', 'Administrator'),
                ('test', 'test123', 'Test User')
                ON CONFLICT (username) DO NOTHING;
            ");

            // Seed Sample Products
            migrationBuilder.Sql(@"
                INSERT INTO product (category_id, sku, name, import_price, sale_price, count, description) VALUES
                (1, 'IP15-PRO-128', 'iPhone 15 Pro 128GB', 25000000, 29000000, 5, 'iPhone 15 Pro màu Titan tự nhiên'),
                (1, 'SS-S24-256', 'Samsung Galaxy S24 256GB', 18000000, 22000000, 8, 'Samsung Galaxy S24 màu đen'),
                (2, 'MBP-M3-14', 'MacBook Pro M3 14 inch', 45000000, 52000000, 3, 'MacBook Pro chip M3'),
                (2, 'DELL-XPS15', 'Dell XPS 15', 35000000, 40000000, 12, 'Dell XPS 15 i7'),
                (3, 'AIRPODS-PRO2', 'AirPods Pro Gen 2', 5000000, 6500000, 2, 'AirPods Pro thế hệ 2')
                ON CONFLICT (sku) DO NOTHING;
            ");

            // Seed Sample Orders
            migrationBuilder.Sql(@"
                -- Get user_id của admin
                WITH admin_user AS (
                    SELECT user_id FROM app_user WHERE username = 'admin' LIMIT 1
                )
                INSERT INTO orders (user_id, final_price, status, payment_method, created_time) 
                SELECT 
                    (SELECT user_id FROM admin_user),
                    29000000,
                    'paid',
                    'cash',
                    CURRENT_DATE - INTERVAL '2 days'
                WHERE EXISTS (SELECT 1 FROM admin_user);

                -- Order 2
                WITH admin_user AS (
                    SELECT user_id FROM app_user WHERE username = 'admin' LIMIT 1
                )
                INSERT INTO orders (user_id, final_price, status, payment_method, created_time)
                SELECT 
                    (SELECT user_id FROM admin_user),
                    44000000,
                    'paid',
                    'bank_transfer',
                    CURRENT_DATE - INTERVAL '1 day'
                WHERE EXISTS (SELECT 1 FROM admin_user);

                -- Order today
                WITH test_user AS (
                    SELECT user_id FROM app_user WHERE username = 'test' LIMIT 1
                )
                INSERT INTO orders (user_id, final_price, status, payment_method, created_time)
                SELECT 
                    (SELECT user_id FROM test_user),
                    6500000,
                    'paid',
                    'cash',
                    CURRENT_TIMESTAMP
                WHERE EXISTS (SELECT 1 FROM test_user);
            ");

            // Seed Order Items
            migrationBuilder.Sql(@"
                -- Order 1 items
                WITH order1 AS (
                    SELECT order_id FROM orders ORDER BY created_time ASC LIMIT 1
                ),
                product1 AS (
                    SELECT product_id, sale_price FROM product WHERE sku = 'IP15-PRO-128'
                )
                INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, unit_cost, total_price)
                SELECT 
                    (SELECT order_id FROM order1),
                    (SELECT product_id FROM product1),
                    1,
                    (SELECT sale_price FROM product1),
                    25000000,
                    (SELECT sale_price FROM product1)
                WHERE EXISTS (SELECT 1 FROM order1) AND EXISTS (SELECT 1 FROM product1);

                -- Order 2 items
                WITH order2 AS (
                    SELECT order_id FROM orders ORDER BY created_time ASC LIMIT 1 OFFSET 1
                ),
                product2 AS (
                    SELECT product_id, sale_price FROM product WHERE sku = 'SS-S24-256'
                )
                INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, unit_cost, total_price)
                SELECT 
                    (SELECT order_id FROM order2),
                    (SELECT product_id FROM product2),
                    2,
                    (SELECT sale_price FROM product2),
                    18000000,
                    (SELECT sale_price FROM product2) * 2
                WHERE EXISTS (SELECT 1 FROM order2) AND EXISTS (SELECT 1 FROM product2);

                -- Order 3 items
                WITH order3 AS (
                    SELECT order_id FROM orders ORDER BY created_time DESC LIMIT 1
                ),
                product3 AS (
                    SELECT product_id, sale_price FROM product WHERE sku = 'AIRPODS-PRO2'
                )
                INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, unit_cost, total_price)
                SELECT 
                    (SELECT order_id FROM order3),
                    (SELECT product_id FROM product3),
                    1,
                    (SELECT sale_price FROM product3),
                    5000000,
                    (SELECT sale_price FROM product3)
                WHERE EXISTS (SELECT 1 FROM order3) AND EXISTS (SELECT 1 FROM product3);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Clean up in reverse order (due to foreign keys)
            migrationBuilder.Sql("DELETE FROM order_item;");
            migrationBuilder.Sql("DELETE FROM orders;");
            migrationBuilder.Sql("DELETE FROM product;");
            migrationBuilder.Sql("DELETE FROM app_user WHERE username IN ('admin', 'test');");
            migrationBuilder.Sql("DELETE FROM category;");
        }
    }
}
