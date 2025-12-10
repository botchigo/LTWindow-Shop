-- ================================================
--  DATABASE MyShop 2025
--  PostgreSQL Schema
-- ================================================

-- Xóa table cũ nếu tồn tại (để chạy lại nhiều lần)
DROP TABLE IF EXISTS order_item CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS product CASCADE;
DROP TABLE IF EXISTS category CASCADE;
DROP TABLE IF EXISTS app_user CASCADE;
DROP TABLE IF EXISTS app_config CASCADE;

-- ===========================
-- 1. BẢNG NGƯỜI DÙNG (LOGIN)
-- ===========================
CREATE TABLE app_user (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password VARCHAR(100) NOT NULL,
    full_name VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW()
);


-- ===========================
-- 2. BẢNG CẤU HÌNH CHƯƠNG TRÌNH
-- ===========================
-- ===========================
-- 2. BẢNG CẤU HÌNH CHƯƠNG TRÌNH
-- ===========================
CREATE TABLE app_config (
    config_id SERIAL PRIMARY KEY,

    -- Tên cấu hình (vd: db_host, theme, backup_path...)
    config_key VARCHAR(100) UNIQUE NOT NULL,

    -- Giá trị của cấu hình
    config_value TEXT,

    -- Mô tả để hiển thị trên UI
    description TEXT,

    -- Thời gian cập nhật gần nhất
    updated_at TIMESTAMP DEFAULT NOW()
);


-- ===========================
-- 3. CATEGORY (LOẠI SẢN PHẨM)
-- ===========================
CREATE TABLE category (
    category_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT
);

-- ===========================
-- 4. PRODUCT (SẢN PHẨM)
-- ===========================
CREATE TABLE product (
    product_id SERIAL PRIMARY KEY,
    category_id INT REFERENCES category(category_id) ON DELETE SET NULL,

    sku VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,

    import_price INT NOT NULL,       -- giá nhập
    sale_price INT NOT NULL,         -- giá bán (bổ sung để tính lợi nhuận)

    count INT DEFAULT 0,             -- số lượng tồn kho

    description TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- ===========================
-- 5. ORDER (ĐƠN HÀNG)
-- ===========================
CREATE TABLE orders (
    order_id SERIAL PRIMARY KEY,

   
    user_id INT,

    created_time TIMESTAMP NOT NULL DEFAULT NOW(),
    final_price INT NOT NULL DEFAULT 0,
    status VARCHAR(30) DEFAULT 'paid',
    payment_method VARCHAR(30) DEFAULT 'cash'
);

-- ===========================
-- 6. ORDER ITEM (CHI TIẾT ĐƠN HÀNG)
-- ===========================
CREATE TABLE order_item (
    order_item_id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(order_id) ON DELETE CASCADE,
    product_id INT REFERENCES product(product_id) ON DELETE SET NULL,

    quantity INT NOT NULL DEFAULT 1,
    unit_sale_price INT NOT NULL,        -- giá bán tại thời điểm tạo đơn
    unit_cost INT NOT NULL,             -- giá nhập tại thời điểm bán

    total_price INT NOT NULL             -- = quantity * unit_sale_price
);

-- ===========================
-- INDEXES TỐI ƯU TÌM KIẾM
-- ===========================
CREATE INDEX idx_product_name ON product(name);
CREATE INDEX idx_order_created_time ON orders(created_time);
CREATE INDEX idx_order_item_product ON order_item(product_id);




INSERT INTO app_config (config_key, config_value, description)
VALUES
('db_host', '127.0.0.1', 'Địa chỉ máy chủ cơ sở dữ liệu'),
('db_port', '5432', 'Cổng PostgreSQL'),
('db_name', 'myshop2025', 'Tên database'),
('db_user', 'postgres', 'Tài khoản kết nối database'),
('db_password', '123456', 'Mật khẩu kết nối database'),

('theme', 'dark', 'Giao diện chương trình'),
('shop_name', 'MyShop 2025', 'Tên cửa hàng'),
('auto_login', 'true', 'Tự động đăng nhập nếu có thông tin lưu'),
('printer_name', 'MAY_IN_BILL', 'Tên máy in mặc định'),
('version', '1.0.0', 'Phiên bản phần mềm');

INSERT INTO category (name, description)
VALUES
('Đồ uống', 'Các loại nước giải khát'),
('Bánh kẹo', 'Snack và bánh quy'),
('Gia vị', 'Các loại gia vị nấu ăn'),
('Mỹ phẩm', 'Chăm sóc cá nhân'),
('Đồ gia dụng', 'Vật dụng gia đình'),
('Rau củ', 'Rau củ tươi'),
('Thịt cá', 'Sản phẩm tươi sống'),
('Điện tử', 'Thiết bị điện tử nhỏ'),
('Quần áo', 'Thời trang'),
('Văn phòng phẩm', 'Dụng cụ học tập & văn phòng');


INSERT INTO product (category_id, sku, name, import_price, sale_price, count, description)
VALUES
(1, 'DU001', 'Coca Cola lon', 7000, 10000, 120, 'Nước ngọt Coca'),
(1, 'DU002', 'Pepsi lon', 7000, 10000, 100, 'Nước ngọt Pepsi'),
(2, 'BK001', 'Snack Oishi', 3000, 5000, 200, 'Snack vị tôm'),
(2, 'BK002', 'Bánh Oreo', 8000, 12000, 80, 'Bánh kem Oreo'),
(3, 'GV001', 'Nước mắm Nam Ngư', 15000, 20000, 50, 'Chai 500ml'),
(4, 'MP001', 'Dầu gội Dove', 30000, 45000, 40, 'Chai 340ml'),
(5, 'GD001', 'Chảo chống dính Sunhouse', 90000, 120000, 25, 'Chảo size 26cm'),
(8, 'DT001', 'Tai nghe Bluetooth', 120000, 150000, 30, 'Tai nghe mini'),
(9, 'QA001', 'Áo thun nam', 45000, 70000, 60, 'Size M'),
(10,'VP001', 'Bút bi Thiên Long', 2000, 4000, 500, 'Màu xanh');

INSERT INTO orders (user_id, final_price, status, payment_method)
VALUES
(1, 20000, 'paid', 'cash'),
(2, 30000, 'paid', 'cash'),
(3, 15000, 'paid', 'momo'),
(4, 45000, 'paid', 'cash'),
(5, 120000, 'paid', 'bank'),
(6, 30000, 'paid', 'cash'),
(7, 20000, 'pending', 'cash'),
(8, 50000, 'paid', 'momo'),
(9, 70000, 'canceled', 'cash'),
(10,90000, 'paid', 'cash');


INSERT INTO order_item (order_id, product_id, quantity, unit_sale_price, unit_cost, total_price)
VALUES
(1, 1, 2, 10000, 7000, 20000),
(2, 3, 3, 5000, 3000, 15000),
(3, 2, 1, 10000, 7000, 10000),
(4, 6, 1, 45000, 30000, 45000),
(5, 7, 1, 120000, 90000, 120000),
(6, 4, 2, 12000, 8000, 24000),
(7, 10, 5, 4000, 2000, 20000),
(8, 8, 1, 150000, 120000, 150000),
(9, 9, 1, 70000, 45000, 70000),
(10,5, 3, 20000, 15000, 60000);

INSERT INTO app_user (username, password, full_name)
VALUES ('admin', '123456', 'Chủ cửa hàng');

