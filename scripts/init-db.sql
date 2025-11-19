-- Initialize database schemas for modular monolith

-- Catalog Schema
CREATE SCHEMA IF NOT EXISTS catalog;

-- Orders Schema
CREATE SCHEMA IF NOT EXISTS orders;

-- Customers Schema
CREATE SCHEMA IF NOT EXISTS customers;

-- Inventory Schema
CREATE SCHEMA IF NOT EXISTS inventory;

-- Payments Schema
CREATE SCHEMA IF NOT EXISTS payments;

-- Shipping Schema
CREATE SCHEMA IF NOT EXISTS shipping;

-- Common Schema (for outbox, etc.)
CREATE SCHEMA IF NOT EXISTS common;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA catalog TO ecommerce;
GRANT ALL PRIVILEGES ON SCHEMA orders TO ecommerce;
GRANT ALL PRIVILEGES ON SCHEMA customers TO ecommerce;
GRANT ALL PRIVILEGES ON SCHEMA inventory TO ecommerce;
GRANT ALL PRIVILEGES ON SCHEMA payments TO ecommerce;
GRANT ALL PRIVILEGES ON SCHEMA shipping TO ecommerce;
GRANT ALL PRIVILEGES ON SCHEMA common TO ecommerce;
