IF DB_ID(N'GestionCommerciale') IS NULL
BEGIN
    CREATE DATABASE GestionCommerciale;
END;
GO

USE GestionCommerciale;
GO

IF OBJECT_ID(N'dbo.Clients', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clients
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clients PRIMARY KEY,
        LastName NVARCHAR(100) NOT NULL,
        FirstName NVARCHAR(100) NULL,
        Email NVARCHAR(254) NULL,
        Phone NVARCHAR(30) NULL,
        Address NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Clients_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Reference NVARCHAR(50) NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(1000) NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Products_Reference UNIQUE (Reference),
        CONSTRAINT CK_Products_UnitPrice CHECK (UnitPrice >= 0),
        CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        OrderNumber NVARCHAR(30) NOT NULL,
        ClientId INT NOT NULL,
        OrderDate DATETIME2 NOT NULL
            CONSTRAINT DF_Orders_OrderDate DEFAULT SYSUTCDATETIME(),
        Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Orders_Status DEFAULT N'Brouillon',
        TotalHT DECIMAL(18,2) NOT NULL,
        TotalTTC DECIMAL(18,2) NOT NULL,
        CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber),
        CONSTRAINT CK_Orders_Status
            CHECK (Status IN (N'Brouillon', N'Validée', N'Annulée')),
        CONSTRAINT CK_Orders_Totals CHECK (TotalHT >= 0 AND TotalTTC >= 0),
        CONSTRAINT FK_Orders_Clients FOREIGN KEY (ClientId)
            REFERENCES dbo.Clients (Id)
    );
END;
GO

IF OBJECT_ID(N'dbo.OrderLines', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderLines
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderLines PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        LineTotal AS (CONVERT(DECIMAL(18,2), Quantity * UnitPrice)) PERSISTED,
        CONSTRAINT UQ_OrderLines_Order_Product UNIQUE (OrderId, ProductId),
        CONSTRAINT CK_OrderLines_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderLines_UnitPrice CHECK (UnitPrice >= 0),
        CONSTRAINT FK_OrderLines_Orders FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders (Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId)
            REFERENCES dbo.Products (Id)
    );
END;
GO
