USE AmazonDB;
GO

-- =============================================
-- Insertar Usuarios
-- =============================================
INSERT INTO Users (Name, Email, Billetera, CreatedAt)
VALUES 
('Juan Pérez', 'juan@example.com', 5000, GETDATE()),
('María López', 'maria@example.com', 3000, GETDATE()),
('Carlos Gómez', 'carlos@example.com', 10000, GETDATE());

-- =============================================
-- Insertar Productos
-- =============================================
INSERT INTO Products (Name, Description, Price, Stock, Category, SellerId, ImageUrl, CreatedAt)
VALUES
('Laptop Gaming Pro', 'Laptop para gaming con RTX 4070', 20000, 15, 'Electrónicos', 1, 'https://example.com/laptop1.jpg', GETDATE()),
('Smartphone X', 'Smartphone de última generación', 15000, 25, 'Electrónicos', 2, 'https://example.com/smartphone1.jpg', GETDATE()),
('Auriculares Inalámbricos', 'Auriculares con cancelación de ruido', 2000, 50, 'Electrónicos', 3, 'https://example.com/headphones1.jpg', GETDATE()),
('Mesa de Oficina', 'Mesa ergonómica de madera', 5000, 10, 'Muebles', 1, 'https://example.com/desk1.jpg', GETDATE()),
('Silla Gamer', 'Silla cómoda para gaming', 3500, 20, 'Muebles', 2, 'https://example.com/chair1.jpg', GETDATE());

-- =============================================
-- Insertar Órdenes (algunas Cart y algunas Paid)
-- =============================================
INSERT INTO Orders (UserId, TotalAmount, Status, UpdatedAt)
VALUES
(1, NULL, 'Cart', GETDATE()),      -- Carrito de Juan
(2, 15000, 'Paid', GETDATE()),     -- Orden pagada de María
(1, 20000, 'Paid', GETDATE()),     -- Orden pagada de Juan
(3, NULL, 'Cart', GETDATE());      -- Carrito de Carlos

-- =============================================
-- Insertar Order_Items
-- =============================================
-- Carrito de Juan
INSERT INTO Order_Items (OrderId, ProductId, Quantity, UnitPrice)
VALUES
(1, 1, 1, 20000),   -- Laptop Gaming Pro
(1, 3, 2, 2000);    -- 2 Auriculares

-- Orden pagada de María
INSERT INTO Order_Items (OrderId, ProductId, Quantity, UnitPrice)
VALUES
(2, 2, 1, 15000);   -- Smartphone X

-- Orden pagada de Juan
INSERT INTO Order_Items (OrderId, ProductId, Quantity, UnitPrice)
VALUES
(3, 1, 1, 20000);   -- Laptop Gaming Pro

-- Carrito de Carlos
INSERT INTO Order_Items (OrderId, ProductId, Quantity, UnitPrice)
VALUES
(4, 4, 1, 5000),    -- Mesa de Oficina
(4, 5, 1, 3500);    -- Silla Gamer

-- =============================================
-- Insertar Pagos
-- =============================================
-- Pago completado de María
INSERT INTO Payments (OrderId, Status, TotalAmount, CreatedAt)
VALUES
(2, 'Completed', 15000, GETDATE());

-- Pago completado de Juan
INSERT INTO Payments (OrderId, Status, TotalAmount, CreatedAt)
VALUES
(3, 'Completed', 20000, GETDATE());

-- Nota: Los carritos activos no tienen pagos aún
