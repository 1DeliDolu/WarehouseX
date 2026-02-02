-- Creates missing tables expected by the EF model and seeds data
-- Run with: mysql -u root -h 127.0.0.1 -P 3306 warehousex < fix_missing_tables.sql

use warehousex;
-- Products
CREATE TABLE IF NOT EXISTS `Products` (
  `ProductId` INT NOT NULL AUTO_INCREMENT,
  `Name` LONGTEXT NOT NULL,
  `Description` LONGTEXT NOT NULL,
  `SKU` LONGTEXT NOT NULL,
  `QuantityInStock` INT NOT NULL,
  `CreatedDate` DATETIME NOT NULL,
  `ModifiedDate` DATETIME NOT NULL,
  PRIMARY KEY (`ProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ExportOrders
CREATE TABLE IF NOT EXISTS `ExportOrders` (
  `ExportOrderId` INT NOT NULL AUTO_INCREMENT,
  `ExportDate` DATETIME NOT NULL,
  `Description` LONGTEXT NOT NULL,
  `CustomerName` LONGTEXT NOT NULL,
  `CreatedDate` DATETIME NOT NULL,
  `ModifiedDate` DATETIME NOT NULL,
  PRIMARY KEY (`ExportOrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ExportOrderItems
CREATE TABLE IF NOT EXISTS `ExportOrderItems` (
  `ExportOrderItemId` INT NOT NULL AUTO_INCREMENT,
  `ExportOrderId` INT NOT NULL,
  `ProductId` INT NOT NULL,
  `Quantity` INT NOT NULL,
  PRIMARY KEY (`ExportOrderItemId`),
  INDEX `IX_ExportOrderItems_ExportOrderId` (`ExportOrderId`),
  INDEX `IX_ExportOrderItems_ProductId` (`ProductId`),
  CONSTRAINT `FK_ExportOrderItems_ExportOrders_ExportOrderId` FOREIGN KEY (`ExportOrderId`) REFERENCES `ExportOrders` (`ExportOrderId`) ON DELETE CASCADE,
  CONSTRAINT `FK_ExportOrderItems_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`ProductId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ImportOrderItems (ensure exists)
CREATE TABLE IF NOT EXISTS `ImportOrderItems` (
  `ImportOrderItemId` INT NOT NULL AUTO_INCREMENT,
  `ImportOrderId` INT NOT NULL,
  `ProductId` INT NOT NULL,
  `Quantity` INT NOT NULL,
  PRIMARY KEY (`ImportOrderItemId`),
  INDEX `IX_ImportOrderItems_ImportOrderId` (`ImportOrderId`),
  INDEX `IX_ImportOrderItems_ProductId` (`ProductId`),
  CONSTRAINT `FK_ImportOrderItems_ImportOrders_ImportOrderId` FOREIGN KEY (`ImportOrderId`) REFERENCES `ImportOrders` (`ImportOrderId`) ON DELETE CASCADE,
  CONSTRAINT `FK_ImportOrderItems_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`ProductId`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Seed Products
INSERT INTO `Products` (`ProductId`, `CreatedDate`, `Description`, `ModifiedDate`, `Name`, `QuantityInStock`, `SKU`)
SELECT 1, '0001-01-01 00:00:00', 'Standard widget', '0001-01-01 00:00:00', 'Widget A', 100, 'WIDGET-A'
WHERE NOT EXISTS (SELECT 1 FROM `Products` WHERE `ProductId` = 1);

INSERT INTO `Products` (`ProductId`, `CreatedDate`, `Description`, `ModifiedDate`, `Name`, `QuantityInStock`, `SKU`)
SELECT 2, '0001-01-01 00:00:00', 'Advanced widget', '0001-01-01 00:00:00', 'Widget B', 50, 'WIDGET-B'
WHERE NOT EXISTS (SELECT 1 FROM `Products` WHERE `ProductId` = 2);

INSERT INTO `Products` (`ProductId`, `CreatedDate`, `Description`, `ModifiedDate`, `Name`, `QuantityInStock`, `SKU`)
SELECT 3, '0001-01-01 00:00:00', 'Multi-purpose gadget', '0001-01-01 00:00:00', 'Gadget X', 75, 'GADGET-X'
WHERE NOT EXISTS (SELECT 1 FROM `Products` WHERE `ProductId` = 3);

-- Seed ExportOrders
INSERT INTO `ExportOrders` (`ExportOrderId`, `CreatedDate`, `CustomerName`, `Description`, `ExportDate`, `ModifiedDate`)
SELECT 1, '0001-01-01 00:00:00', 'BestBuy', 'Order for BestBuy', '2025-06-15 00:00:00', '0001-01-01 00:00:00'
WHERE NOT EXISTS (SELECT 1 FROM `ExportOrders` WHERE `ExportOrderId` = 1);

INSERT INTO `ExportOrders` (`ExportOrderId`, `CreatedDate`, `CustomerName`, `Description`, `ExportDate`, `ModifiedDate`)
SELECT 2, '0001-01-01 00:00:00', 'GadgetMart', 'Order for GadgetMart', '2025-06-20 00:00:00', '0001-01-01 00:00:00'
WHERE NOT EXISTS (SELECT 1 FROM `ExportOrders` WHERE `ExportOrderId` = 2);

-- Seed ExportOrderItems
INSERT INTO `ExportOrderItems` (`ExportOrderItemId`, `ExportOrderId`, `ProductId`, `Quantity`)
SELECT 1, 1, 1, 30 WHERE NOT EXISTS (SELECT 1 FROM `ExportOrderItems` WHERE `ExportOrderItemId` = 1);
INSERT INTO `ExportOrderItems` (`ExportOrderItemId`, `ExportOrderId`, `ProductId`, `Quantity`)
SELECT 2, 1, 2, 10 WHERE NOT EXISTS (SELECT 1 FROM `ExportOrderItems` WHERE `ExportOrderItemId` = 2);
INSERT INTO `ExportOrderItems` (`ExportOrderItemId`, `ExportOrderId`, `ProductId`, `Quantity`)
SELECT 3, 2, 3, 20 WHERE NOT EXISTS (SELECT 1 FROM `ExportOrderItems` WHERE `ExportOrderItemId` = 3);

-- Ensure ImportOrderItems seed (only insert if missing)
INSERT INTO `ImportOrderItems` (`ImportOrderItemId`, `ImportOrderId`, `ProductId`, `Quantity`)
SELECT 1, 1, 1, 60 WHERE NOT EXISTS (SELECT 1 FROM `ImportOrderItems` WHERE `ImportOrderItemId` = 1);
INSERT INTO `ImportOrderItems` (`ImportOrderItemId`, `ImportOrderId`, `ProductId`, `Quantity`)
SELECT 2, 1, 3, 40 WHERE NOT EXISTS (SELECT 1 FROM `ImportOrderItems` WHERE `ImportOrderItemId` = 2);
INSERT INTO `ImportOrderItems` (`ImportOrderItemId`, `ImportOrderId`, `ProductId`, `Quantity`)
SELECT 3, 2, 1, 40 WHERE NOT EXISTS (SELECT 1 FROM `ImportOrderItems` WHERE `ImportOrderItemId` = 3);
INSERT INTO `ImportOrderItems` (`ImportOrderItemId`, `ImportOrderId`, `ProductId`, `Quantity`)
SELECT 4, 2, 2, 50 WHERE NOT EXISTS (SELECT 1 FROM `ImportOrderItems` WHERE `ImportOrderItemId` = 4);
