using System;
using System.Collections.Generic;
using Npgsql;
using LewiStoreOOPSQL.Models;

namespace LewiStoreOOPSQL.Data
{
    public class DatabaseManager
    {
        private readonly string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        public void AddProduct(Product product)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO Products
                    (ProductId, ProductName, Description, PriceExcludingVat, QuantityInStock)
                    VALUES
                    (@ProductId, @ProductName, @Description, @PriceExcludingVat, @QuantityInStock)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description ?? "");
                    cmd.Parameters.AddWithValue("@PriceExcludingVat", product.PriceExclusiveVat);
                    cmd.Parameters.AddWithValue("@QuantityInStock", product.QuantityInStock);

                    cmd.ExecuteNonQuery();
                }
            }
        }



        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT ProductId, ProductName, Description, PriceExcludingVat, QuantityInStock
                    FROM Products
                    ORDER BY ProductId";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product product = new Product(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.IsDBNull(2) ? "" : reader.GetString(2),
                            reader.GetDecimal(3),
                            reader.GetInt32(4)
                        );

                        products.Add(product);
                    }
                }
            }

            return products;
        }

        public Product GetProductById(int productId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT ProductId, ProductName, Description, PriceExcludingVat, QuantityInStock
                    FROM Products
                    WHERE ProductId = @ProductId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product(
                                reader.GetInt32(0),
                                reader.GetString(1),
                                reader.IsDBNull(2) ? "" : reader.GetString(2),
                                reader.GetDecimal(3),
                                reader.GetInt32(4)
                            );
                        }
                    }
                }
            }

            return null;
        }

        public void UpdateProductStock(int productId, int newQuantity)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
                    UPDATE Products
                    SET QuantityInStock = @QuantityInStock
                    WHERE ProductId = @ProductId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@QuantityInStock", newQuantity);
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int AddSale(Sale sale)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
            INSERT INTO Sales
            (ProductId, QuantitySold, Subtotal, VatAmount, TotalAmount, SaleDate)
            VALUES
            (@ProductId, @QuantitySold, @Subtotal, @VatAmount, @TotalAmount, @SaleDate)
            RETURNING SaleId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", sale.ProductId);
                    cmd.Parameters.AddWithValue("@QuantitySold", sale.QuantitySold);
                    cmd.Parameters.AddWithValue("@Subtotal", sale.Subtotal);
                    cmd.Parameters.AddWithValue("@VatAmount", sale.VatAmount);
                    cmd.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                    cmd.Parameters.AddWithValue("@SaleDate", sale.SaleDate);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public Sale ProcessSale(int productId, int quantity)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string selectQuery = @"
                    SELECT ProductId, ProductName, Description, PriceExcludingVat, QuantityInStock
                    FROM Products
                    WHERE ProductId = @ProductId";

                        Product product = null;

                        using (var selectCmd = new NpgsqlCommand(selectQuery, conn, transaction))
                        {
                            selectCmd.Parameters.AddWithValue("@ProductId", productId);

                            using (var reader = selectCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    product = new Product(
                                        reader.GetInt32(0),
                                        reader.GetString(1),
                                        reader.IsDBNull(2) ? "" : reader.GetString(2),
                                        reader.GetDecimal(3),
                                        reader.GetInt32(4)
                                    );
                                }
                            }
                        }

                        if (product == null)
                            throw new Exception("Product not found.");

                        if (quantity <= 0)
                            throw new Exception("Quantity must be greater than zero.");

                        if (quantity > product.QuantityInStock)
                            throw new Exception("Not enough stock available.");

                        decimal vatRate = 0.15m;
                        decimal subtotal = product.PriceExclusiveVat * quantity;
                        decimal vat = subtotal * vatRate;
                        decimal total = subtotal + vat;
                        int newQuantity = product.QuantityInStock - quantity;
                        DateTime saleDate = DateTime.Now;

                        string updateQuery = @"
                    UPDATE Products
                    SET QuantityInStock = @QuantityInStock
                    WHERE ProductId = @ProductId";

                        using (var updateCmd = new NpgsqlCommand(updateQuery, conn, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@QuantityInStock", newQuantity);
                            updateCmd.Parameters.AddWithValue("@ProductId", productId);
                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                                throw new Exception("Stock update failed.");
                        }

                        string insertSaleQuery = @"
                    INSERT INTO Sales
                    (ProductId, QuantitySold, Subtotal, VatAmount, TotalAmount, SaleDate)
                    VALUES
                    (@ProductId, @QuantitySold, @Subtotal, @VatAmount, @TotalAmount, @SaleDate)
                    RETURNING SaleId";

                        int saleId;

                        using (var saleCmd = new NpgsqlCommand(insertSaleQuery, conn, transaction))
                        {
                            saleCmd.Parameters.AddWithValue("@ProductId", productId);
                            saleCmd.Parameters.AddWithValue("@QuantitySold", quantity);
                            saleCmd.Parameters.AddWithValue("@Subtotal", subtotal);
                            saleCmd.Parameters.AddWithValue("@VatAmount", vat);
                            saleCmd.Parameters.AddWithValue("@TotalAmount", total);
                            saleCmd.Parameters.AddWithValue("@SaleDate", saleDate);

                            saleId = Convert.ToInt32(saleCmd.ExecuteScalar());
                        }

                        transaction.Commit();

                        return new Sale(
                            saleId,
                            productId,
                            quantity,
                            subtotal,
                            vat,
                            total,
                            saleDate
                        );
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public List<Sale> GetAllSales()
        {
            List<Sale> sales = new List<Sale>();

            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
                    SELECT SaleId, ProductId, QuantitySold, Subtotal, VatAmount, TotalAmount, SaleDate
                    FROM Sales
                    ORDER BY SaleId";

                using (var cmd = new NpgsqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Sale sale = new Sale(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetDecimal(3),
                            reader.GetDecimal(4),
                            reader.GetDecimal(5),
                            reader.GetDateTime(6)
                        );

                        sales.Add(sale);
                    }
                }
            }

            return sales;
        }

        public void UpdateProduct(Product product)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
            UPDATE Products
            SET ProductName = @ProductName,
                Description = @Description,
                PriceExcludingVat = @PriceExcludingVat,
                QuantityInStock = @QuantityInStock
            WHERE ProductId = @ProductId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description ?? "");
                    cmd.Parameters.AddWithValue("@PriceExcludingVat", product.PriceExclusiveVat);
                    cmd.Parameters.AddWithValue("@QuantityInStock", product.QuantityInStock);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new Exception("Product update failed. Product not found.");
                }
            }
        }

        public bool ProductHasSales(int productId)
{
    using (var conn = GetConnection())
    {
        conn.Open();

        string query = @"
            SELECT COUNT(*)
            FROM Sales
            WHERE ProductId = @ProductId";

        using (var cmd = new NpgsqlCommand(query, conn))
        {
            cmd.Parameters.AddWithValue("@ProductId", productId);

            int count = Convert.ToInt32(cmd.ExecuteScalar());

            return count > 0;
        }
    }
}

        public void DeleteProduct(int productId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();

                string query = @"
            DELETE FROM Products
            WHERE ProductId = @ProductId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                        throw new Exception("Product delete failed. Product not found.");
                }
            }
        }
    }


}