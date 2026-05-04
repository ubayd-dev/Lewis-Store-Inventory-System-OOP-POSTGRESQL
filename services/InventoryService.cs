using System;
using System.Collections.Generic;
using System.Linq;
using LewiStoreOOPSQL.Models;
using LewiStoreOOPSQL.Data;


namespace LewiStoreOOPSQL.Services
{

    /// <summary>
    /// Handles business rules and validation for inventory operations.
    /// This layer sits between the UI and the database.
    /// It ensures only clean, valid business data reaches the database.
    /// </summary>
    public class InventoryService
    {
        private readonly DatabaseManager db;
        // private const decimal VatRate = 0.15m;


        /// <summary>
        /// Handles business rules and validation for inventory operations.
        /// This layer sits between the UI and the database.
        /// It ensures only clean, valid business data reaches the database.
        /// </summary>
        public InventoryService(DatabaseManager db)
        {
            this.db = db;
        }

        /// <summary>
        /// Removes leading and trailing spaces from text input
        /// before it is validated or saved to the database.
        /// Prevents messy values like "     Table".
        /// </summary>
        private string CleanText(string value)
        {
            if (value == null)
                return "";

            return value.Trim();
        }


        /// <summary>
        /// Validates product related text fields (name/description).
        /// Protects database from invalid input such as:
        /// empty strings, symbols only, numbers only, or overly long values.
        /// </summary>
        private void ValidateProductText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception($"{fieldName} cannot be empty.");

            if (value.Length > 100)
                throw new Exception($"{fieldName} cannot be longer than 100 characters.");

            bool hasLetter = value.Any(char.IsLetter);

            if (!hasLetter)
                throw new Exception($"{fieldName} must contain at least one letter.");

            bool validCharacters = value.All(c =>
                char.IsLetterOrDigit(c) ||
                c == ' ' ||
                c == '-' ||
                c == '.'
            );

            if (!validCharacters)
                throw new Exception($"{fieldName} contains invalid characters.");
        }

        /// <summary>
        /// Validates and inserts a new product.
        /// Business rules:
        /// - Product must exist
        /// - Text must be cleaned and valid
        /// - Price must be greater than zero
        /// - Quantity cannot be negative
        /// - Product ID must be unique
        /// </summary>
        public void AddProduct(Product product)
        {
            if (product == null)
                throw new Exception("Product cannot be null.");

            product.ProductName = CleanText(product.ProductName);
            product.Description = CleanText(product.Description);

            ValidateProductText(product.ProductName, "Product name");
            ValidateProductText(product.Description, "Description");

            if (product.PriceExclusiveVat <= 0)
                throw new Exception("Price must be greater than zero.");

            if (product.QuantityInStock < 0)
                throw new Exception("Quantity cannot be negative.");

            Product existingProduct = db.GetProductById(product.ProductId);

            if (existingProduct != null)
                throw new Exception("Product with this ID already exists.");

            db.AddProduct(product);
        }

        /// <summary>
        /// Returns all products currently stored in inventory.
        /// Used by inventory display, selling, update, and delete flows.
        /// </summary>
        public List<Product> GetAllProducts()
        {
            return db.GetAllProducts();
        }


        /// <summary>
        /// Returns a single product by ID.
        /// Used for validation, updates, selling, and delete checks.
        /// </summary>
        public Product GetProductById(int productId)
        {
            return db.GetProductById(productId);
        }

        /// <summary>
        /// Validates and updates an existing product.
        /// Ensures updated values are clean and valid before saving.
        /// Prevents invalid stock, price, or text updates.
        /// </summary>
        public void UpdateProduct(Product product)
        {
            if (product == null)
                throw new Exception("Product cannot be null.");

            product.ProductName = CleanText(product.ProductName);
            product.Description = CleanText(product.Description);

            ValidateProductText(product.ProductName, "Product name");
            ValidateProductText(product.Description, "Description");

            if (product.PriceExclusiveVat <= 0)
                throw new Exception("Price must be greater than zero.");

            if (product.QuantityInStock < 0)
                throw new Exception("Quantity cannot be negative.");

            Product existingProduct = db.GetProductById(product.ProductId);

            if (existingProduct == null)
                throw new Exception("Product not found.");

            db.UpdateProduct(product);
        }


        /// <summary>
        /// Deletes a product only if:
        /// - it exists
        /// - it has no sales history
        ///
        /// Products with linked sales are protected to preserve reporting integrity.
        /// </summary>
        public void DeleteProduct(int productId)
        {
            Product existingProduct = db.GetProductById(productId);

            if (existingProduct == null)
                throw new Exception("Product not found.");

            if (db.ProductHasSales(productId))
                throw new Exception("This product cannot be deleted because it has sales history.");

            db.DeleteProduct(productId);
        }


        /// <summary>
        /// Delegates sale processing to the database layer.
        /// Sale logic is handled in a transaction to ensure:
        /// stock update and sale insert succeed together.
        /// </summary>
        public Sale SellProduct(int productId, int quantity)
        {
            return db.ProcessSale(productId, quantity);
        }


        /// <summary>
        /// Returns all recorded sales history.
        /// Used for reporting and sales history display.
        /// </summary>
        public List<Sale> GetAllSales()
        {
            return db.GetAllSales();
        }
    }
}