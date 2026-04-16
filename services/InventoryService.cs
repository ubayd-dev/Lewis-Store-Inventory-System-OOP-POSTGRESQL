using System;
using System.Collections.Generic;
using LewiStoreOOPSQL.Models;

namespace LewiStoreOOPSQL.Services
{
    public class InventoryService
    {
        private List<Product> products = new List<Product>();
        private List<Sale> sales = new List<Sale>();
        private int nextSaleId = 1;

        public void AddProduct(Product product)
        {
            if (product == null)
                throw new Exception("Product cannot be null.");

            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new Exception("Product name cannot be empty");

            if (product.PriceExclusiveVat <= 0)
                throw new Exception("Price must be greater than zero");

            if (product.QuantityInStock < 0)
                throw new Exception("Quantity cannot be negative");

            Product existingProduct = products.Find(p => p.ProductId == product.ProductId);

            if (existingProduct != null)
                throw new Exception("Product with this Id already exists");

            products.Add(product);
        }

        public List<Product> GetAllProducts()
        {
            return new List<Product>(products);
        }

        public Product GetProductById(int productId)
        {
            return products.Find(p => p.ProductId == productId);
        }

        public Sale SellProduct(int productId, int quantity)
        {
            Product product = GetProductById(productId);

            if (product == null)
                throw new Exception("Product not found");

            if (quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            if (quantity > product.QuantityInStock)
                throw new Exception("Not enough stock available");

            decimal subTotal = product.PriceExclusiveVat * quantity;
            decimal vat = subTotal * 0.15m;
            decimal totalAmount = subTotal + vat;

            product.QuantityInStock -= quantity;

            Sale sale = new Sale(
                nextSaleId++,
                product.ProductId,
                quantity,
                subTotal,
                vat,
                totalAmount,
                DateTime.Now
            );

            sales.Add(sale);
            return sale;
        }
        public List<Sale> GetAllSales()
        {
            return new List<Sale>(sales);
        }
    }
}