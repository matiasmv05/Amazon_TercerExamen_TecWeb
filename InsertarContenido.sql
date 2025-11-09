-- =============================================
-- Base de datos AmazonDB
-- =============================================
CREATE DATABASE AmazonDB;
GO

USE AmazonDB;
GO

-- =============================================
-- Tabla Users
-- =============================================
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    Billetera DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE()
);
GO

-- =============================================
-- Tabla Products
-- =============================================
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL,
    Category NVARCHAR(100) NOT NULL,
    SellerId INT NOT NULL,
    ImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Products_Seller FOREIGN KEY (SellerId)
        REFERENCES Users(Id)
        ON DELETE CASCADE
);
GO

-- =============================================
-- Tabla Orders
-- =============================================
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    TotalAmount DECIMAL(18,2) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Cart', -- Cart, Paid, Cancelled
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Orders_User FOREIGN KEY (UserId)
        REFERENCES Users(Id)
        ON DELETE CASCADE
);
GO

-- =============================================
-- Tabla Order_Items
-- =============================================
CREATE TABLE Order_Items (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Order FOREIGN KEY (OrderId)
        REFERENCES Orders(Id)
        ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Product FOREIGN KEY (ProductId)
        REFERENCES Products(Id)
        ON DELETE NO ACTION  -- <- Cambiado

);
GO

-- =============================================
-- Tabla Payments
-- =============================================
CREATE TABLE Payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Failed
    TotalAmount DECIMAL(18,2) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Payments_Order FOREIGN KEY (OrderId)
        REFERENCES Orders(Id)
        ON DELETE CASCADE
);
GO

-- =============================================
-- Índices y optimizaciones
-- =============================================

-- Para búsquedas frecuentes por email
CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);

-- Para filtrar productos por categoría
CREATE INDEX IX_Products_Category ON Products(Category);

-- Para obtener rápidamente el carrito de un usuario
CREATE INDEX IX_Orders_UserId_Status ON Orders(UserId, Status);

-- Para obtener productos en orden rápidamente
CREATE INDEX IX_OrderItems_OrderId ON Order_Items(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON Order_Items(ProductId);
GO
