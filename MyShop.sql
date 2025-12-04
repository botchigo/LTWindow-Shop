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
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(100),
    role VARCHAR(50) DEFAULT 'admin',
    last_screen_opened VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW()
);

-- ===========================
-- 2. BẢNG CẤU HÌNH CHƯƠNG TRÌNH
-- ===========================
CREATE TABLE app_config (
    config_id SERIAL PRIMARY KEY,
    config_key VARCHAR(100) UNIQUE NOT NULL,
    config_value TEXT
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

    user_id INT REFERENCES app_user(user_id) ON DELETE SET NULL,

    created_time TIMESTAMP NOT NULL DEFAULT NOW(),
    final_price INT NOT NULL DEFAULT 0,

    status VARCHAR(30) DEFAULT 'paid',       -- pending / paid / canceled
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