using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPostgreSQLFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Function: Total Products
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_total_products()
                RETURNS INTEGER AS $$
                BEGIN
                    RETURN (SELECT COUNT(*) FROM product);
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Function: Top 5 Low Stock
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_top5_low_stock()
                RETURNS TABLE(product_id INTEGER, name VARCHAR(255), stock INTEGER) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT p.product_id, p.name, p.count as stock
                    FROM product p
                    WHERE p.count <= 10
                    ORDER BY p.count ASC, p.name ASC
                    LIMIT 5;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Function: Top 5 Best Sellers
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_top5_best_seller()
                RETURNS TABLE(product_id INTEGER, product_name VARCHAR(255), total_quantity BIGINT) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT p.product_id, p.name as product_name, SUM(oi.quantity)::BIGINT as total_quantity
                    FROM order_item oi
                    INNER JOIN product p ON oi.product_id = p.product_id
                    GROUP BY p.product_id, p.name
                    ORDER BY total_quantity DESC
                    LIMIT 5;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Function: Total Orders Today
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_total_orders_today()
                RETURNS INTEGER AS $$
                BEGIN
                    RETURN (
                        SELECT COUNT(*)
                        FROM orders
                        WHERE DATE(created_time) = CURRENT_DATE
                    );
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Function: Total Revenue Today
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_total_revenue_today()
                RETURNS BIGINT AS $$
                BEGIN
                    RETURN (
                        SELECT COALESCE(SUM(final_price), 0)::BIGINT
                        FROM orders
                        WHERE DATE(created_time) = CURRENT_DATE
                        AND status = 'paid'
                    );
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Function: Latest 3 Orders
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_latest_3_orders()
                RETURNS TABLE(
                    order_id INTEGER,
                    created_time TIMESTAMP,
                    final_price INTEGER,
                    status VARCHAR(30),
                    payment_method VARCHAR(30)
                ) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT o.order_id, o.created_time, o.final_price, o.status, o.payment_method
                    FROM orders o
                    ORDER BY o.created_time DESC
                    LIMIT 3;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Function: Revenue By Day Current Month
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_revenue_by_day_current_month()
                RETURNS TABLE(day DATE, total_revenue BIGINT) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT 
                        DATE(o.created_time) as day,
                        COALESCE(SUM(o.final_price), 0)::BIGINT as total_revenue
                    FROM orders o
                    WHERE EXTRACT(YEAR FROM o.created_time) = EXTRACT(YEAR FROM CURRENT_DATE)
                    AND EXTRACT(MONTH FROM o.created_time) = EXTRACT(MONTH FROM CURRENT_DATE)
                    AND o.status = 'paid'
                    GROUP BY DATE(o.created_time)
                    ORDER BY day ASC;
                END;
                $$ LANGUAGE plpgsql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop all functions
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_total_products();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_top5_low_stock();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_top5_best_seller();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_total_orders_today();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_total_revenue_today();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_latest_3_orders();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS fn_revenue_by_day_current_month();");
        }
    }
}
