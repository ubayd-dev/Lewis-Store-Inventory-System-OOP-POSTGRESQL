using System;
namespace LewiStoreOOPSQL.Models
{
    public class Product
    {
        private int productId;
        private string productName;

        private string description;
        private decimal priceExclusiveVat;
        private int quantityInStock;

        public int ProductId { get { return productId; } set { productId = value; } }
        public string ProductName
        { get { return productName; } set { productName = value; } }
        public string Description
        { get { return description; } set { description = value; } }
        public decimal PriceExclusiveVat { get { return priceExclusiveVat; } set { if (value >= 0) priceExclusiveVat = value; } }
        public int QuantityInStock { get { return quantityInStock; } set { if (value >= 0) quantityInStock = value; } }

        public Product(int productId, string productName, string description, decimal priceExclusiveVat, int quantityInStock)
        {
            ProductId = productId;
            ProductName = productName;
            Description = description;
            PriceExclusiveVat = priceExclusiveVat;
            QuantityInStock = quantityInStock;
        }

    }
}
