CREATE DATABASE ShoppingCart
GO

USE [ShoppingCart]
GO

CREATE TABLE [dbo].[ShoppingCart](
  [ID] int IDENTITY(1,1) PRIMARY KEY,
  [UserId] [bigint] NOT NULL,
  CONSTRAINT ShoppingCartUnique UNIQUE([ID], [UserID])
)
GO

CREATE INDEX ShoppingCart_UserId 
ON [dbo].[ShoppingCart] (UserId)
GO

CREATE TABLE [dbo].[ShoppingCartItems](
  [ID] int IDENTITY(1,1) PRIMARY KEY,
	[ShoppingCartId] [int] NOT NULL,
	[ProductCatalogId] [bigint] NOT NULL,
	[ProductName] [nvarchar](100) NOT NULL,
	[ProductDescription] [nvarchar](500) NULL,
	[Amount] [int] NOT NULL,
	[Currency] [nvarchar](5) NOT NULL
)

GO

ALTER TABLE [dbo].[ShoppingCartItems]  WITH CHECK ADD CONSTRAINT [FK_ShoppingCart] FOREIGN KEY([ShoppingCartId])
REFERENCES [dbo].[ShoppingCart] ([Id])
GO

ALTER TABLE [dbo].[ShoppingCartItems] CHECK CONSTRAINT [FK_ShoppingCart]
GO

CREATE INDEX ShoppingCartItems_ShoppingCartId 
ON [dbo].[ShoppingCartItems] (ShoppingCartId)
GO

CREATE TABLE [dbo].[EventStore](
  [ID] int IDENTITY(1,1) PRIMARY KEY,
  [Name] [nvarchar](100)  NOT NULL,
  [OccurredAt] [datetimeoffset] NOT NULL,
  [Content][nvarchar](max) NOT NULL
)
GO

