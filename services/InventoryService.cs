using System;
using System.Collections.Generic;
using System.Linq;
using LewiStoreOOPSQL.Models;
using LewiStoreOOPSQL.Data;


namespace LewiStoreOOPSQL.Services
{
    public class InventoryService
    {
        private readonly DatabaseManager db;
        // private const decimal VatRate = 0.15m;

        public InventoryService(DatabaseManager db)
        {
            this.db = db;
        }
        private string CleanText(string value)
        {
            if (value == null)
                return "";

            return value.Trim();
        }

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

        public List<Product> GetAllProducts()
        {
            return db.GetAllProducts();
        }

        public Product GetProductById(int productId)
        {
            return db.GetProductById(productId);
        }
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

        public void DeleteProduct(int productId)
        {
            Product existingProduct = db.GetProductById(productId);

            if (existingProduct == null)
                throw new Exception("Product not found.");

            if (db.ProductHasSales(productId))
                throw new Exception("This product cannot be deleted because it has sales history.");

            db.DeleteProduct(productId);
        }

        public Sale SellProduct(int productId, int quantity)
        {
            return db.ProcessSale(productId, quantity);
        }

        public List<Sale> GetAllSales()
        {
            return db.GetAllSales();
        }
    }
}