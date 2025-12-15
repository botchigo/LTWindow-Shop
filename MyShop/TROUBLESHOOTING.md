# ?? Troubleshooting: "Tài kho?n m?t kh?u không ðúng"

## ? V?n ð? b?n g?p:

B?n th?y dialog **"K?t n?i Database thành công"** nhýng khi ðãng nh?p l?i báo **"Tài kho?n m?t kh?u không ðúng"**.

---

## ? Nguyên nhân và gi?i pháp:

### **Nguyên nhân 1: Dialog "K?t n?i Database" gây nh?m l?n**

**V?n ð?:** 
- Dialog này hi?n ra SAU KHI init database
- Nó KHÔNG liên quan ð?n vi?c login
- Gây nh?m l?n cho user

**? Ð? s?a:**
- Xóa dialog t? ð?ng hi?n sau khi init
- Database s? init âm th?m
- Ch? báo l?i khi th?c s? có v?n ð?

---

### **Nguyên nhân 2: Chýa có d? li?u trong database**

**V?n ð?:**
- Database ðý?c t?o thành công
- Nhýng CHÝA CÓ user nào
- Khi login ? không t?m th?y user ? fail

**? Gi?i pháp:**

#### **Cách 1: Dùng account test t? ð?ng** (Khuy?n ngh?)

Code ð? t? ð?ng t?o 2 accounts khi init l?n ð?u:

```
Username: admin
Password: 123456

HO?C

Username: test  
Password: test123
```

**Th? ðãng nh?p l?i v?i account này!**

#### **Cách 2: Ðãng k? account m?i**

1. Click nút **"Signup"**
2. Nh?p thông tin:
   - Username: `yourname`
   - Password: `yourpass`
   - Name: `Your Full Name`
3. Click **"Ðãng k?"**
4. Sau ðó login v?i account v?a t?o

#### **Cách 3: Ki?m tra database có data chýa**

M? **pgAdmin** ho?c **psql**:

```sql
-- K?t n?i vào database MyShop
\c MyShop

-- Xem t?t c? users
SELECT * FROM app_user;
```

N?u **không có d? li?u**, ch?y:

```sql
INSERT INTO app_user (username, password, full_name) 
VALUES ('admin', '123456', 'Administrator');

INSERT INTO app_user (username, password, full_name) 
VALUES ('test', 'test123', 'Test User');
```

---

### **Nguyên nhân 3: Database connection failed nhýng không báo l?i**

**Ki?m tra:**

1. PostgreSQL có ðang ch?y không?
```powershell
Get-Service -Name "*postgresql*"
# N?u Status = Stopped ? Start-Service postgresql-x64-18
```

2. M?t kh?u database ðúng chýa?

Ki?m tra file `Services/DatabaseManager.cs` và `Database/AppDbContextFactory.cs`:

```csharp
password: "12345"  // ? Ph?i ðúng v?i password PostgreSQL c?a b?n
```

3. Database MyShop ð? t?o chýa?

```powershell
psql -U postgres
\l  # List all databases
```

N?u chýa có MyShop:
```sql
CREATE DATABASE "MyShop";
```

---

## ?? Cách test t?ng bý?c:

### **Test 1: Ki?m tra k?t n?i**

1. M? pgAdmin ho?c psql
2. K?t n?i vào PostgreSQL v?i username `postgres` và password `12345`
3. N?u k?t n?i ðý?c = PostgreSQL OK

### **Test 2: Ki?m tra database**

```sql
\c MyShop
\dt  -- List all tables
```

Ph?i th?y b?ng `app_user`

### **Test 3: Ki?m tra data**

```sql
SELECT * FROM app_user;
```

**N?u k?t qu? tr?ng** ? Ðây là l? do login fail!

**Gi?i pháp:** Insert data nhý ? Cách 3 phía trên.

### **Test 4: Th? ðãng nh?p**

1. Ch?y app (F5)
2. Nh?p:
   - Username: `admin`
   - Password: `123456`
3. Click Login

**K?t qu? mong ð?i:** "Chào m?ng admin!"

---

## ?? Debug nâng cao:

N?u v?n không ðý?c, thêm code debug vào `LoginButton_Click`:

```csharp
// Thêm sau d?ng: await EnsureDatabaseInitializedAsync();

// DEBUG: Ki?m tra có bao nhiêu users
var userCount = await _databaseManager.Context.AppUsers.CountAsync();
System.Diagnostics.Debug.WriteLine($"DEBUG: Có {userCount} users trong database");

// DEBUG: List t?t c? usernames
var allUsers = await _databaseManager.Context.AppUsers
    .Select(u => u.Username)
    .ToListAsync();
System.Diagnostics.Debug.WriteLine($"DEBUG: Users: {string.Join(", ", allUsers)}");
```

Sau ðó xem Output window trong Visual Studio khi ch?y.

---

## ?? Checklist:

? **Trý?c khi login, ð?m b?o:**

- [ ] PostgreSQL ðang ch?y (check Service)
- [ ] Database MyShop ð? ðý?c t?o
- [ ] B?ng app_user t?n t?i
- [ ] Có ít nh?t 1 user trong b?ng
- [ ] Password database ðúng (12345 ho?c password b?n set)
- [ ] Ð? rebuild solution (Ctrl+Shift+B)

? **Khi login:**

- [ ] Nh?p ðúng username: `admin`
- [ ] Nh?p ðúng password: `123456`
- [ ] KHÔNG có kho?ng tr?ng th?a
- [ ] Ki?m tra Caps Lock t?t

---

## ?? Quick Fix:

**Cách nhanh nh?t:**

1. Xóa database c?:
```sql
DROP DATABASE IF EXISTS "MyShop";
```

2. Ch?y l?i app (F5)

3. Click Login (s? t? ð?ng t?o database và seed data)

4. Login v?i:
   - Username: `admin`
   - Password: `123456`

---

## ?? Lýu ?:

?? **N?u b?n th?y dialog xu?t hi?n nhi?u l?n:**

- Dialog "K?t n?i Database" ð? ðý?c XÓA trong version m?i
- Ch? c?n dialog báo l?i khi th?c s? có l?i
- Login s? ch? show 1 dialog: Success ho?c Fail

?? **Password ðang lýu plaintext:**

- Hi?n t?i password lýu d?ng text thý?ng
- Không an toàn cho production
- TODO: Hash b?ng BCrypt trý?c khi release

---

## ?? C?n tr? giúp thêm?

N?u v?n không ðý?c, g?i cho tôi:

1. Screenshot dialog l?i
2. Output t? SQL:
```sql
SELECT * FROM app_user;
```
3. K?t qu? t?:
```powershell
Get-Service -Name "*postgresql*"
```

Tôi s? debug c? th? cho trý?ng h?p c?a b?n! ??
