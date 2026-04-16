using System;
namespace LewiStoreOOPSQL.Models
{
    public class Sale
    {
        private int saleId;
        private int productId;
        private int quantitySold;
        private decimal subTotal;
        private decimal vatAmount;
        private decimal totalAmount;
        private DateTime saleDate;

        public int SaleId { get { return saleId; } set { saleId = value; } }
        public int ProductId { get { return productId; } set { productId = value; } }

        public int QuantitySold { get { return quantitySold; } set { if (value > 0) quantitySold = value; } }

        public decimal Subtotal { get { return subTotal; } set { subTotal = value; } }

        public decimal VatAmount { get { return vatAmount; } set { vatAmount = value; } }

        public decimal TotalAmount { get { return totalAmount; } set { totalAmount = value; } }

        public DateTime SaleDate { get { return saleDate; } set { saleDate = value; } }

        public Sale(int saleId, int productId, int quantitySold, decimal subTotal, decimal vatAmount, decimal totalAmount, DateTime saleDate)
        {
            SaleId = saleId;
            ProductId = productId;
            QuantitySold = quantitySold;
            Subtotal = subTotal;
            VatAmount = vatAmount;
            TotalAmount = totalAmount;
            SaleDate = saleDate;
        }

    }
}

