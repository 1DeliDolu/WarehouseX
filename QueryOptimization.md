-- Original Query
SELECT p.ProductName, SUM(o.Quantity) AS TotalSold
FROM Orders o
JOIN Products p ON o.ProductID = p.ProductID
WHERE p.Category = 'Electronics'
GROUP BY p.ProductName
ORDER BY TotalSold DESC;

-- ============================================================================
-- QUERY OPTIMIZATION ANALYSIS & RECOMMENDATIONS
-- ============================================================================

/*
POTENTIAL INEFFICIENCIES:
1. Missing indexes on join and filter columns
2. Full table scan on Products table due to lack of index on Category
3. No index on Order.Quantity for the SUM operation
4. Potential for covering index to avoid table lookups
*/

-- ============================================================================
-- 1. INDEXING STRATEGIES
-- ============================================================================

-- Create indexes to support the query
CREATE INDEX IX_Products_Category ON Products(Category);
CREATE INDEX IX_Orders_ProductID ON Orders(ProductID) INCLUDE (Quantity);

-- For even better performance, create a covering index
CREATE INDEX IX_Products_Category_ProductID_ProductName 
ON Products(Category, ProductID) 
INCLUDE (ProductName);

-- ============================================================================
-- 2. QUERY RESTRUCTURING
-- ============================================================================

-- Option 1: Use a CTE for better readability and potential optimization
WITH ElectronicProducts AS (
    SELECT ProductID, ProductName 
    FROM Products 
    WHERE Category = 'Electronics'
)
SELECT 
    p.ProductName, 
    SUM(o.Quantity) AS TotalSold
FROM Orders o
JOIN ElectronicProducts p ON o.ProductID = p.ProductID
GROUP BY p.ProductName
ORDER BY TotalSold DESC;

-- Option 2: Use a subquery if the database engine optimizes it better
SELECT 
    p.ProductName, 
    (SELECT SUM(Quantity) 
     FROM Orders o 
     WHERE o.ProductID = p.ProductID) AS TotalSold
FROM Products p
WHERE p.Category = 'Electronics'
ORDER BY TotalSold DESC;

-- ============================================================================
-- 3. ALTERNATIVE APPROACHES
-- ============================================================================

-- Option 1: Materialized View (if supported) for frequently accessed data
/*
CREATE MATERIALIZED VIEW ElectronicsSales AS
SELECT 
    p.ProductID,
    p.ProductName, 
    SUM(o.Quantity) AS TotalSold
FROM Orders o
JOIN Products p ON o.ProductID = p.ProductID
WHERE p.Category = 'Electronics'
GROUP BY p.ProductID, p.ProductName;
*/

-- Option 2: Pre-aggregate data (for reporting purposes)
/*
CREATE TABLE DailyProductSales (
    SaleDate DATE,
    ProductID INT,
    TotalSold INT,
    PRIMARY KEY (SaleDate, ProductID)
);

-- Schedule this to run daily
INSERT INTO DailyProductSales (SaleDate, ProductID, TotalSold)
SELECT 
    CAST(GETDATE() AS DATE),
    ProductID,
    SUM(Quantity)
FROM Orders
GROUP BY ProductID;
*/

-- ============================================================================
-- 4. ADDITIONAL RECOMMENDATIONS
-- ============================================================================

-- 1. Consider partitioning the Orders table by date if dealing with large datasets
-- 2. Implement query caching for frequently executed reports
-- 3. Consider denormalizing the schema for read-heavy reporting queries
-- 4. Monitor query performance using execution plans and adjust indexes accordingly
-- 5. Consider using a columnstore index for analytical queries on large datasets

-- Example of checking query execution plan
/*
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

-- Run your optimized query here

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
*/

