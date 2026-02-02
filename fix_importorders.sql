-- Creates ImportOrders table compatible with the project's EF model
-- Run with: mysql -u root -h 127.0.0.1 -P 3306 warehousex < fix_importorders.sql

CREATE TABLE IF NOT EXISTS `ImportOrders` (
  `ImportOrderId` INT NOT NULL AUTO_INCREMENT,
  `ImportDate` DATETIME NOT NULL,
  `Description` LONGTEXT NOT NULL,
  `SupplierName` LONGTEXT NOT NULL,
  `CreatedDate` DATETIME NOT NULL,
  `ModifiedDate` DATETIME NOT NULL,
  PRIMARY KEY (`ImportOrderId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert seed data matching migrations
INSERT INTO `ImportOrders` (`ImportOrderId`, `CreatedDate`, `Description`, `ImportDate`, `ModifiedDate`, `SupplierName`)
VALUES
  (1, '0001-01-01 00:00:00', 'Initial stock', '2025-06-01 00:00:00', '0001-01-01 00:00:00', 'Acme Supplies'),
  (2, '0001-01-01 00:00:00', 'Restock widgets', '2025-06-10 00:00:00', '0001-01-01 00:00:00', 'Widget World');
