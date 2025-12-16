# Hý?ng d?n Fix l?i: S?n ph?m s?p h?t hàng không hi?n th? tên

## V?n ð?
ListView "S?p h?t hàng" không hi?n th? tên s?n ph?m.

## Nguyên nhân có th?

### 1. PostgreSQL Functions chýa ðý?c t?o
Function `fn_top5_low_stock()` chýa t?n t?i trong database PostgreSQL.

### 2. Database không có d? li?u
Không có s?n ph?m nào có stock <= 10 trong b?ng `product`.

### 3. L?i k?t n?i database
Connection string ho?c database setup không ðúng.

## Cách ki?m tra & s?a

### Bý?c 1: Ki?m tra PostgreSQL Functions

1. M? **pgAdmin** ho?c công c? PostgreSQL client
2. K?t n?i ð?n database `MyShop`
3. Ch?y script SQL t? file: `SQL/dashboard_functions.sql`

```sql
-- Ki?m tra function có t?n t?i không
SELECT * FROM fn_top5_low_stock();
```

N?u l?i "function does not exist", ch?y l?nh t?o function:

```sql
CREATE OR REPLACE FUNCTION fn_top5_low_stock()
RETURNS TABLE (
    product_id INTEGER,
    name VARCHAR(255),
    stock INTEGER
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        p.product_id,
        p.name,
        p.count as stock
    FROM product p
    WHERE p.count <= 10
    ORDER BY p.count ASC, p.name ASC
    LIMIT 5;
END;
$$ LANGUAGE plpgsql;
```

### Bý?c 2: Ki?m tra d? li?u trong database

```sql
-- Xem t?t c? s?n ph?m và s? lý?ng t?n kho
SELECT product_id, name, count FROM product ORDER BY count ASC;

-- Xem s?n ph?m có stock <= 10
SELECT product_id, name, count FROM product WHERE count <= 10;
```

N?u không có s?n ph?m nào, thêm d? li?u test:

```sql
INSERT INTO product (sku, name, import_price, sale_price, count, category_id, created_at, updated_at)
VALUES 
    ('SKU001', 'S?n ph?m A - S?p h?t', 50000, 80000, 5, NULL, NOW(), NOW()),
    ('SKU002', 'S?n ph?m B - S?p h?t', 60000, 90000, 3, NULL, NOW(), NOW()),
    ('SKU003', 'S?n ph?m C - S?p h?t', 70000, 100000, 8, NULL, NOW(), NOW()),
    ('SKU004', 'S?n ph?m D - S?p h?t', 80000, 120000, 2, NULL, NOW(), NOW()),
    ('SKU005', 'S?n ph?m E - S?p h?t', 90000, 150000, 10, NULL, NOW(), NOW());
```

### Bý?c 3: Test trong ?ng d?ng

1. Ch?y ?ng d?ng MyShop
2. Ðãng nh?p vào Dashboard
3. Click nút **"?? Refresh"**
4. M? **Output Window** trong Visual Studio (View ? Output)
5. Ch?n "Debug" trong dropdown
6. Xem log output:

```
=== TESTING LOW STOCK DATA ===
Returned X low stock products
=== LOW STOCK PRODUCTS (X) ===
  - ProductId: 1, Name: 'S?n ph?m A', Stock: 5
  - ProductId: 2, Name: 'S?n ph?m B', Stock: 3
```

### Bý?c 4: N?u v?n không hi?n th?

Ki?m tra binding trong XAML. File `Views/DashboardPage.xaml` ð? ðúng:

```xaml
<TextBlock Text="{Binding Name}" FontWeight="SemiBold" FontSize="13" TextTrimming="CharacterEllipsis"/>
```

Class `LowStockProduct` c?ng ð? ðúng:

```csharp
public class LowStockProduct
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;  // ? OK
    public int Stock { get; set; }
}
```

## Checklist

- [ ] PostgreSQL function `fn_top5_low_stock()` ð? ðý?c t?o
- [ ] Database có ít nh?t 1 s?n ph?m v?i `count <= 10`
- [ ] ?ng d?ng k?t n?i thành công ð?n database
- [ ] Output window hi?n th? log "LOW STOCK PRODUCTS"
- [ ] S? lý?ng products > 0

## K?t qu? mong ð?i

Sau khi hoàn thành các bý?c trên, ListView "?? S?p h?t hàng" s? hi?n th?:

```
??  S?n ph?m A - S?p h?t           C?n 5
??  S?n ph?m B - S?p h?t           C?n 3
??  S?n ph?m C - S?p h?t           C?n 8
??  S?n ph?m D - S?p h?t           C?n 2
??  S?n ph?m E - S?p h?t           C?n 10
```

## File ð? thay ð?i

1. ? `Views/DashboardPage.xaml.cs` - Thêm debug logging
2. ? `SQL/dashboard_functions.sql` - SQL scripts ð? t?o functions
3. ? `DEBUG_LOW_STOCK.md` - Tài li?u này

## Liên h?

N?u v?n g?p v?n ð?, ki?m tra:
- Connection string trong `MainWindow.xaml.cs`
- PostgreSQL service ðang ch?y
- Database "MyShop" ð? ðý?c t?o
- User "postgres" có quy?n truy c?p
